# src/reknn/_native.py

from __future__ import annotations

import atexit
import ctypes
import platform
import threading
from pathlib import Path
from typing import ClassVar

import numpy as np


# public struct ResultItemMain
# {
#     public int MainId;
#     public double Score;
# }
class ResultItemMain(ctypes.Structure):
    _pack_ = 0
    _fields_ = [
        ("MainId", ctypes.c_int),
        ("Score", ctypes.c_double)
    ]

# public unsafe struct ResultItemMainDetail
# {
#     public int key;
#     public int valueNum;
#     public ResultItemMain* values;
# }
class ResultItemMainDetail(ctypes.Structure):
    _pack_ = 0
    _fields_ = [
        ("key", ctypes.c_int),
        ("valueNum", ctypes.c_int),
        ("values", ctypes.POINTER(ResultItemMain)),
    ]

# public struct ResultItemMainAndSub
# {
#     public int MainId;
#     public int SubId;
#     public double Score;
# }
class ResultItemMainAndSub(ctypes.Structure):
    _pack_ = 0
    _fields_ = [
        ("MainId", ctypes.c_int),
        ("SubId", ctypes.c_int),
        ("Score", ctypes.c_double)
    ]

# public unsafe struct ResultItemMainAndSubDetail
# {
#     public int key;
#     public int valueNum;
#     public ResultItemMainAndSub* values;
# }
class ResultItemMainAndSubDetail(ctypes.Structure):
    _pack_ = 0
    _fields_ = [
        ("key", ctypes.c_int),
        ("valueNum", ctypes.c_int),
        ("values", ctypes.POINTER(ResultItemMainAndSub)),
    ]

# public unsafe struct PredictResult
# {
#     public int ScoreListMainNum;
#     public ResultItemMain* ScoreListMain;
#     public int ScoreListMainAndSubNum;
#     public ResultItemMainAndSub* ScoreListMainAndSub;
#     public int PredictedLabel;
# }
class PredictResult(ctypes.Structure):
    _pack_ = 0
    _fields_ = [
        ("ScoreListMainNum", ctypes.c_int),
        ("ScoreListMains", ctypes.POINTER(ResultItemMain)),
        ("ScoreListMainAndSubNum", ctypes.c_int),
        ("ScoreListMainAndSubs", ctypes.POINTER(ResultItemMainAndSub)),
        ("PredictedLabel", ctypes.c_int),
    ]

# public unsafe struct SearchResult
# {
#     public nuint Size;
#     public int ResultItemMainNum;
#     public ResultItemMain* ResultItemMains;
#     public int ResultItemMainAndSubNum;
#     public ResultItemMainAndSub* ResultItemMainAndSubs;
#     public int ResultItemMainDetailNum;
#     public ResultItemMainDetail* ResultItemMainDetails;
#     public int ResultItemMainAndSubDetailNum;
#     public ResultItemMainAndSubDetail* ResultItemMainAndSubDetails;
# }

class SearchResult(ctypes.Structure):
    _pack_ = 0
    _fields_ = [
        ("Size", ctypes.c_ulonglong),
        ("ResultItemMainNum", ctypes.c_int),
        ("ResultItemMains", ctypes.POINTER(ResultItemMain)),
        ("ResultItemMainAndSubNum", ctypes.c_int),
        ("ResultItemMainAndSubs", ctypes.POINTER(ResultItemMainAndSub)),
        ("ResultItemMainDetailNum", ctypes.c_int),
        ("ResultItemMainDetails", ctypes.POINTER(ResultItemMainDetail)),
        ("ResultItemMainAndSubDetailNum", ctypes.c_int),
        ("ResultItemMainAndSubDetails", ctypes.POINTER(ResultItemMainAndSubDetail)),
    ]


class _NativeRuntime:
    """
    DLL/so を扱う低レベルラッパー。

    - プロセス内で1つだけ存在する
    - DLL/so のロードを一元管理する
    - ctypes の関数定義を一箇所に閉じ込める
    - native 側の allocate/free を一元管理する
    """

    _instance: ClassVar[_NativeRuntime | None] = None
    _instance_lock: ClassVar[threading.Lock] = threading.Lock()

    @classmethod
    def instance(cls) -> "_NativeRuntime":
        if cls._instance is None:
            with cls._instance_lock:
                if cls._instance is None:
                    cls._instance = cls()
        return cls._instance

    def __init__(self) -> None:
        # instance() 経由でのみ生成される想定
        if self.__class__._instance is not None:
            raise RuntimeError("_NativeRuntime is a singleton. Use _NativeRuntime.instance().")

        self._lock = threading.RLock()
        self._closed = False

        self.max_instance = 20

        self._lib_path = self._resolve_library_path()
        self._lib = ctypes.CDLL(str(self._lib_path))

        self._configure_functions()

        # native 側にグローバル初期化がある場合
        self._init_native()

        self.allocate_info = {}

        # Python 終了時の保険
        atexit.register(self.close)

    # ---------------------------------------------------------------------
    # library loading
    # ---------------------------------------------------------------------

    def _resolve_library_path(self) -> Path:
        base_dir = Path(__file__).resolve().parent

        system = platform.system().lower()
        machine = platform.machine().lower()

        if system == "windows":
            if machine not in ("amd64", "x86_64"):
                raise RuntimeError(f"Unsupported Windows architecture: {machine}")
            return base_dir / "lib" / "windows-x64" / "FuutaSystemSvcVectorLibrary.dll"

        if system == "linux":
            if machine not in ("x86_64", "amd64"):
                raise RuntimeError(f"Unsupported Linux architecture: {machine}")
            return base_dir / "lib" / "linux-x64" / "FuutaSystemSvcVectorLibrary.so"

        raise RuntimeError(f"Unsupported platform: {system} {machine}")

    # ---------------------------------------------------------------------
    # function signatures
    # ---------------------------------------------------------------------

    def _configure_functions(self) -> None:
        """
        C API の関数シグネチャをここで定義する。
        """

        # [DllImport("FuutaSystemSvcVectorLibrary")]
        # private static extern bool Initialize(ModeEnum mode, int instanceNum);
        self._lib.Initialize.argtypes = [
            ctypes.c_int,
            ctypes.c_int
        ]
        self._lib.Initialize.restype = None
        
        # [DllImport("FuutaSystemSvcVectorLibrary")]
        # private static extern unsafe bool Load(int instanceNo, byte* utf8Text, int textLength);
        self._lib.Load.argtypes = [
            ctypes.c_int,
            ctypes.c_char_p,
            ctypes.c_int
        ]
        self._lib.Load.restype = ctypes.c_bool
        
        # [DllImport("FuutaSystemSvcVectorLibrary")]
        # private static extern unsafe bool Add(int instanceNo, float* vec, int length, int mainId, int subId, int searchMax, double threshold);
        self._lib.Add.argtypes = [
            ctypes.c_int,
            ctypes.POINTER(ctypes.c_float),
            ctypes.c_int,
            ctypes.c_int,
            ctypes.c_int,
            ctypes.c_int,
            ctypes.c_double
        ]
        self._lib.Add.restype = ctypes.c_bool
        
        # [DllImport("FuutaSystemSvcVectorLibrary")]
        # private static extern unsafe bool AddBulk(int instanceNo, int count, float* vec, int* size, int* mainId, int* subId, int searchMax, double threshold);
        self._lib.AddBulk.argtypes = [
            ctypes.c_int,
            ctypes.c_int,
            ctypes.POINTER(ctypes.c_float),
            ctypes.POINTER(ctypes.c_int),
            ctypes.POINTER(ctypes.c_int),
            ctypes.POINTER(ctypes.c_int),
            ctypes.c_int,
            ctypes.c_double,
        ]
        self._lib.AddBulk.restype = ctypes.c_bool
        
        # [DllImport("FuutaSystemSvcVectorLibrary")]
        # private static extern unsafe bool Refine(int instanceNo, float* vec, int length, int mainId, int subId, int searchMax, double threshold);
        self._lib.Refine.argtypes = [
            ctypes.c_int,
            ctypes.POINTER(ctypes.c_float),
            ctypes.c_int,
            ctypes.c_int,
            ctypes.c_int,
            ctypes.c_int,
            ctypes.c_double,
        ]
        self._lib.Refine.restype = ctypes.c_bool
        
        # [DllImport("FuutaSystemSvcVectorLibrary")]
        # private static extern int Delete(int instanceNo, int mainId, int subId);
        self._lib.Delete.argtypes = [
            ctypes.c_int,
            ctypes.c_int,
            ctypes.c_int,
        ]
        self._lib.Delete.restype = ctypes.c_int
        
        # [DllImport("FuutaSystemSvcVectorLibrary")]
        # private static extern unsafe SearchResult* Search(int instanceNo, float* vec, int length, int kValue);
        self._lib.Search.argtypes = [
            ctypes.c_int,
            ctypes.POINTER(ctypes.c_float),
            ctypes.c_int,
            ctypes.c_int,
        ]
        self._lib.Search.restype = ctypes.POINTER(SearchResult)
        
        # [DllImport("FuutaSystemSvcVectorLibrary")]
        # private static extern unsafe double GetTotalVector(int instanceNo);
        self._lib.GetTotalVector.argtypes = [
            ctypes.c_int,
        ]
        self._lib.GetTotalVector.restype = ctypes.c_double
        
        # [DllImport("FuutaSystemSvcVectorLibrary")]
        # private static extern unsafe bool Save(int instanceNo, byte* utf8Text, int textLength);
        self._lib.Save.argtypes = [
            ctypes.c_int,
            ctypes.c_char_p,
            ctypes.c_int,
        ]
        self._lib.Save.restype = ctypes.c_bool
        
        # [DllImport("FuutaSystemSvcVectorLibrary")]
        # private static extern unsafe bool SaveWithCount(int instanceNo, byte* utf8Text, int textLength, int count);
        self._lib.SaveWithCount.argtypes = [
            ctypes.c_int,
            ctypes.c_char_p,
            ctypes.c_int,
            ctypes.c_int,
        ]
        self._lib.SaveWithCount.restype = ctypes.c_bool
        
        # [DllImport("FuutaSystemSvcVectorLibrary")]
        # private static extern unsafe SearchResult* SimpleClustering(int instanceNo);
        self._lib.SimpleClustering.argtypes = [
            ctypes.c_int,
        ]
        self._lib.SimpleClustering.restype = ctypes.POINTER(SearchResult)
        
        # [DllImport("FuutaSystemSvcVectorLibrary")]
        # private static extern unsafe PredictResult* Predict(int instanceNo, float* vec, int length, int kValue, double detectThreshold);
        self._lib.Predict.argtypes = [
            ctypes.c_int,
            ctypes.POINTER(ctypes.c_float),
            ctypes.c_int,
            ctypes.c_int,
            ctypes.c_double,
        ]
        self._lib.Predict.restype = ctypes.POINTER(PredictResult)
        
        # [DllImport("FuutaSystemSvcVectorLibrary")]
        # private static extern unsafe PredictResult* Predict2(int instanceNo, float* vec, int length, int kValue, double detectThreshold, double minThreshold);
        self._lib.Predict2.argtypes = [
            ctypes.c_int,
            ctypes.POINTER(ctypes.c_float),
            ctypes.c_int,
            ctypes.c_int,
            ctypes.c_double,
            ctypes.c_double,
        ]
        self._lib.Predict2.restype = ctypes.POINTER(PredictResult)
        
        # [DllImport("FuutaSystemSvcVectorLibrary")]
        # private static extern unsafe bool IsNeedRefine(int instanceNo, float* vec, int length, int searchMax, int mainId, int subId);
        self._lib.IsNeedRefine.argtypes = [
            ctypes.c_int,
            ctypes.POINTER(ctypes.c_float),
            ctypes.c_int,
            ctypes.c_int,
            ctypes.c_int,
            ctypes.c_int,
        ]
        self._lib.IsNeedRefine.restype = ctypes.c_bool
        
        # [DllImport("FuutaSystemSvcVectorLibrary")]
        # private static extern unsafe StatusDetailEnum GetStatusDetail();
        self._lib.GetStatusDetail.argtypes = [
        ]
        self._lib.GetStatusDetail.restype = ctypes.c_int
        
        # [DllImport("FuutaSystemSvcVectorLibrary")]
        # private static extern bool RefineAll(int instanceNo, int limit);
        self._lib.RefineAll.argtypes = [
            ctypes.c_int,
            ctypes.c_int,
        ]
        self._lib.RefineAll.restype = ctypes.c_bool
        
        # [DllImport("DLL\\FuutaSystemSvcVectorLibrary")]
        # private static extern void SetDebugMode(int mode);
        self._lib.SetDebugMode.argtypes = [
            ctypes.c_int,
        ]
        self._lib.SetDebugMode.restype = None
        
        # [DllImport("DLL\\FuutaSystemSvcVectorLibrary")]
        # private static extern unsafe void FreeNativeMemory(void* ptr);
        self._lib.FreeNativeMemory.argtypes = [
            ctypes.c_void_p,
        ]
        self._lib.FreeNativeMemory.restype = None

        # [DllImport("DLL\\FuutaSystemSvcVectorLibrary")]
        # private static extern unsafe void Clear(int instanceNo);
        self._lib.Clear.argtypes = [
            ctypes.c_int,
        ]
        self._lib.Clear.restype = None

        # [DllImport("DLL\\FuutaSystemSvcVectorLibrary")]
        # private static extern unsafe void InitializeInstance(int instanceNo, ModeEnum mode);
        self._lib.InitializeInstance.argtypes = [
            ctypes.c_int,
            ctypes.c_int,
        ]
        self._lib.InitializeInstance.restype = None


    # ---------------------------------------------------------------------
    # lifecycle
    # ---------------------------------------------------------------------

    def _init_native(self) -> None:
        rc = self._lib.Initialize(1, self.max_instance)

    def close(self) -> None:
        """
        native 側に shutdown が必要な場合だけ呼ぶ。

        注意:
        ctypes.CDLL 自体を明示的に unload する設計にはしない。
        ここでは native ライブラリ内部のリソース解放だけを行う。
        """
        with self._lock:
            if self._closed:
                return

            try:
                for i in self.allocate_info.keys():
                    self.Clear(i)
            finally:
                self._closed = True

    # ---------------------------------------------------------------------
    # error handling
    # ---------------------------------------------------------------------

    def _ensure_open(self) -> None:
        if self._closed:
            raise ReKNNNativeError("Native runtime is already closed.")

    # ---------------------------------------------------------------------
    # database handle
    # ---------------------------------------------------------------------

    # インスタンスの初期化
    def Initialize(self, mode):
        self._ensure_open()

        if len(self.allocate_info) >= self.max_instance:
            raise RekNNException("no free instance : current = {0}, max = {1}".format(len(self.allocate_info), self.max_instance))

        target = None
        for i in range(self.max_instance):
            if not i in self.allocate_info.keys():
                target = i
                break

        if target is None:
            raise RekNNException("bug?? : no free instance : current = {0}, max = {1}".format(len(self.allocate_info), self.max_instance))

        with self._lock:
            self._lib.InitializeInstance(target, mode)
            self.allocate_info[target] = mode

        return target


    # ---------------------------------------------------------------------
    # operations
    # ---------------------------------------------------------------------

    def Add(
        self,
        instanceNo : ctypes.c_int,
        vec : np.ndarray,
        length : ctypes.c_int,
        mainId : ctypes.c_int,
        subId : ctypes.c_int,
        searchMax : ctypes.c_int,
        threshold : ctypes.c_double
    ) -> ctypes.c_bool:
        self._ensure_open()

        vec = self._as_float32_matrix(vec)

        with self._lock:
            v = vec.ctypes.data_as(ctypes.POINTER(ctypes.c_float))
            rc = self._lib.Add(instanceNo, v, length, mainId, subId, searchMax, threshold)
        
        return rc


    def AddBulk(
        self,
        instanceNo : ctypes.c_int,
        count : ctypes.c_int,
        vec : np.ndarray,
        length : np.ndarray,
        mainId : np.ndarray,
        subId : np.ndarray,
        searchMax : ctypes.c_int,
        threshold : ctypes.c_double
    ) -> ctypes.c_bool:
        self._ensure_open()

        #print("--- at native AddBulk ---")
        #print(f"count={count}")
        #print(f"vec={vec}")
        #print(f"length={length}")
        #print(f"mainId={mainId}")
        #print(f"subId={subId}")

        # private static extern unsafe bool AddBulk(int instanceNo, int count, float* vec, int* size, int* mainId, int* subId, int searchMax, double threshold);
        with self._lock:
            vp = self._as_float32_matrix(vec).ctypes.data_as(ctypes.POINTER(ctypes.c_float))
            lp = self._as_int32_vector(length).ctypes.data_as(ctypes.POINTER(ctypes.c_int))
            mp = self._as_int32_vector(mainId).ctypes.data_as(ctypes.POINTER(ctypes.c_int))
            sp = self._as_int32_vector(subId).ctypes.data_as(ctypes.POINTER(ctypes.c_int))
            rc = self._lib.AddBulk(instanceNo, count, vp, lp, mp, sp, searchMax, threshold)

        return rc


    def Clear(
        self,
        instanceNo : ctypes.c_int
    ) -> None:
        self._ensure_open()

        with self._lock:
            self._lib.Clear(instanceNo)

    def Dispose(
        self,
        instanceNo : ctypes.c_int
    ) -> None:
        self._ensure_open()

        with self._lock:
            self.Clear(instanceNo)
            del self.allocate_info[instanceNo]


    def Predict2(
        self,
        instanceNo : ctypes.c_int,
        vec : np.ndarray,
        length : ctypes.c_int,
        kValue : ctypes.c_int,
        detectThreshold : ctypes.c_double,
        minThreshold : ctypes.c_double
    ) -> ctypes.POINTER(PredictResult):
        self._ensure_open()

        vec = self._as_float32_matrix(vec)

        results = None

        with self._lock:
            rc = None
            try:
                v = vec.ctypes.data_as(ctypes.POINTER(ctypes.c_float))
                result = self._lib.Predict2(instanceNo, v, length, kValue, detectThreshold, minThreshold)
                 
                results = {}
                results['ScoreListMainNum'] = result.contents.ScoreListMainNum
                results['ScoreListMains'] = {
                    k:v for k,v in [
                        [
                            result.contents.ScoreListMains[x].MainId,
                            result.contents.ScoreListMains[x].Score
                        ]
                        for x in range(result.contents.ScoreListMainNum)
                    ]
                }
                results['ScoreListMainAndSubNum'] = result.contents.ScoreListMainAndSubNum
                results['ScoreListMainAndSubs'] = {
                    k:v for k,v in [
                        [
                            result.contents.ScoreListMainAndSubs[x].MainId,
                            [
                                result.contents.ScoreListMainAndSubs[x].SubId,
                                result.contents.ScoreListMainAndSubs[x].Score
                            ]
                        ]
                        for x in range(result.contents.ScoreListMainAndSubNum)
                    ]
                }
                results['PredictedLabel'] = result.contents.PredictedLabel

            finally:
                self._lib.FreeNativeMemory(rc)

        return results


        # [DllImport("FuutaSystemSvcVectorLibrary")]
        # private static extern unsafe SearchResult* Search(int instanceNo, float* vec, int length, int kValue);
        self._lib.Search.argtypes = [
            ctypes.c_int,
            ctypes.POINTER(ctypes.c_float),
            ctypes.c_int,
            ctypes.c_int,
        ]
        self._lib.Search.restype = ctypes.POINTER(SearchResult)

    def Search(
        self,
        instanceNo : ctypes.c_int,
        vec : np.ndarray,
        length : ctypes.c_int,
        kValue : ctypes.c_int,
    ):
        self._ensure_open()

        results = None

        vec = self._as_float32_matrix(vec)

        with self._lock:
            rc = None
            try:
                v = vec.ctypes.data_as(ctypes.POINTER(ctypes.c_float))
                result = self._lib.Search(instanceNo, v, length, kValue)

                results = {}
                results['Size'] = result.contents.Size
                results['ResultItemMainNum'] = result.contents.ResultItemMainNum
                results['ResultItemMains'] = {
                    k:v for k,v in [
                        [
                            result.contents.ResultItemMains[x].MainId,
                            result.contents.ResultItemMains[x].Score
                        ]
                        for x in range(result.contents.ResultItemMainNum)
                    ]
                }
                results['ResultItemMainAndSubNum'] = result.contents.ResultItemMainAndSubNum
                results['ResultItemMainAndSubs'] = {
                    k:v for k,v in [
                        [
                            result.contents.ResultItemMainAndSubs[x].MainId,
                            [
                                result.contents.ResultItemMainAndSubs[x].SubId,
                                result.contents.ResultItemMainAndSubs[x].Score,
                            ]
                        ]
                        for x in range(result.contents.ResultItemMainAndSubNum)
                    ]
                }
                results['ResultItemMainDetailNum'] = result.contents.ResultItemMainDetailNum
                results['ResultItemMainDetails'] = {
                    k:v for k,v in [
                        [
                            result.contents.ResultItemMainDetails[x].key,
                            {
                                k:v for k,v in [
                                    [
                                        result.contents.ResultItemMainDetails[x].values[y].MainId,
                                        result.contents.ResultItemMainDetails[x].values[y].Score,
                                    ]
                                    for y in range(result.contents.ResultItemMainDetails[x].valueNum)
                                ]
                            }
                        ]
                        for x in range(result.contents.ResultItemMainDetailNum)
                    ]
                }
                results['ResultItemMainAndSubDetailNum'] = result.contents.ResultItemMainAndSubDetailNum
                results['ResultItemMainAndSubDetails'] = {
                    k:v for k,v in [
                        [
                            result.contents.ResultItemMainAndSubDetails[x].key,
                            {
                                k:v for k,v in [
                                    [
                                        result.contents.ResultItemMainAndSubDetails[x].values[y].MainId,
                                        [
                                            result.contents.ResultItemMainAndSubDetails[x].values[y].SubId,
                                            result.contents.ResultItemMainAndSubDetails[x].values[y].Score,
                                        ]
                                    ]
                                    for y in range(result.contents.ResultItemMainAndSubDetails[x].valueNum)
                                ]
                            }
                        ]
                        for x in range(result.contents.ResultItemMainDetailNum)
                    ]
                }

            finally:
                self._lib.FreeNativeMemory(rc)

        return results


    def Save(self, db_path : str, target_instance : int) -> ctypes.c_bool:
        self._ensure_open()
        db_path_pt = db_path.encode('utf-8')
        db_path_len = len(db_path_pt)
        return self._lib.Save(0, db_path_pt, db_path_len)


    def Load(self, db_path : str, target_instance : int) -> ctypes.c_bool:
        self._ensure_open()
        db_path_pt = db_path.encode('utf-8')
        db_path_len = len(db_path_pt)
        return self._lib.Load(0, db_path_pt, db_path_len)


    def RefineAll(self, limit : int, target_instance : int) -> ctypes.c_bool:
        self._ensure_open()
        return self._lib.RefineAll(target_instance, limit)
    

    def GetStatusDetail(self):
        self._ensure_open()
        return self._lib.GetStatusDetail()


    def GetTotalVector(self, target_instance : int ) -> ctypes.c_double:
        self._ensure_open()
        return self._lib.GetTotalVector(target_instance)


    def SetDebugMode(self, mode : int) -> None:
        self._ensure_open()
        return self._lib.SetDebugMode(mode)


    # ---------------------------------------------------------------------
    # numpy validation
    # ---------------------------------------------------------------------

    def _as_float32_matrix(self, value: np.ndarray) -> np.ndarray:
        arr = np.asarray(value, dtype=np.float32)

        if arr.ndim != 2:
            raise ValueError("vectors must be a 2D array.")

        if arr.shape[0] == 0:
            raise ValueError("vectors must not be empty.")

        if arr.shape[1] == 0:
            raise ValueError("vector dimension must not be zero.")

        if not np.all(np.isfinite(arr)):
            raise ValueError("vectors must not contain NaN or Inf.")

        return np.ascontiguousarray(arr)

    def _as_float32_vector(self, value: np.ndarray) -> np.ndarray:
        arr = np.asarray(value, dtype=np.float32)

        if arr.ndim != 1:
            raise ValueError("query must be a 1D array.")

        if arr.shape[0] == 0:
            raise ValueError("query dimension must not be zero.")

        if not np.all(np.isfinite(arr)):
            raise ValueError("query must not contain NaN or Inf.")

        return np.ascontiguousarray(arr)

    def _as_int32_vector(self, value: np.ndarray) -> np.ndarray:
        arr = np.asarray(value, dtype=np.int32)

        if arr.ndim != 1:
            raise ValueError("labels must be a 1D array.")

        return np.ascontiguousarray(arr)

        
