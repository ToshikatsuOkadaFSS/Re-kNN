# src/reknn/_native.py

from __future__ import annotations

import atexit
import ctypes
import platform
import threading
from pathlib import Path
from typing import ClassVar

import numpy as np


class ReKNNNativeError(RuntimeError):
    pass


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

        以下は仮の C API 名。
        実際の DLL/so の export 名に合わせて変更する。
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
                rc = self._lib.ReKNN_Shutdown()
                self._check(rc)
            finally:
                self._closed = True

    # ---------------------------------------------------------------------
    # error handling
    # ---------------------------------------------------------------------

    # ここは廃止予定
    def _check(self, rc: int) -> None:
        if rc == 0:
            return

        message = self._get_last_error()
        raise ReKNNNativeError(message)

    # 改めて実装の必要あり
    def _get_last_error(self) -> str:
        try:
            raw = self._lib.ReKNN_GetLastError()
            if not raw:
                return "Unknown native error"
            return raw.decode("utf-8", errors="replace")
        except Exception:
            return "Unknown native error"

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
            self.lib.InitializeInstance(target, mode)

        return target

    def Clear(self, instance_no, mode) -> None:
        if instance_no is None:
            return

        self._ensure_open()

        with self._lock:
            self._lib.InitializeInstance(instance_no, mode)
    
    # ---------------------------------------------------------------------
    # operations
    # ---------------------------------------------------------------------

    def Add(
        self,
        instanceNo : ctypes.c_int,
        vec : ctypes.POINTER(ctypes.c_float),
        length : ctypes.c_int,
        mainId : ctypes.c_int,
        subId : ctypes.c_int,
        searchMax : ctypes.c_int,
        threshold : ctypes.c_double
    ) -> ctypes.c_bool:
        self._ensure_open()

        with self._lock:
            rc = self._lib.Add(instanceNo, vec, length, mainId, subId, searchMax, threshold)
        
        return rc


    def Clear(
        self,
        instanceNo : ctypes.c_int
    ) -> None:
        self._ensure_open()

        with self._lock:
            self._lib.Clear(instanceNo)

        

    def Initialize(
        self,
        mode : ctypes.c_int,
        instanceNum : ctypes.c_int
    ) -> None:
        self._ensure_open()

        with self._lock:
            self._lib.Initialize(mode, instanceNo)
        





















    
    def search(
        self,
        handle: ctypes.c_void_p,
        query: np.ndarray,
        k: int,
    ) -> list[tuple[int, float]]:
        self._ensure_open()

        if k <= 0:
            raise ValueError("k must be positive.")

        query = self._as_float32_vector(query)
        dimension = query.shape[0]

        result_handle = ctypes.c_void_p()

        with self._lock:
            rc = self._lib.ReKNN_Search(
                handle,
                query.ctypes.data_as(ctypes.POINTER(ctypes.c_float)),
                ctypes.c_uint32(dimension),
                ctypes.c_uint32(k),
                ctypes.byref(result_handle),
            )
            self._check(rc)

            if not result_handle:
                return []

            try:
                return self._copy_search_result(result_handle)
            finally:
                self._free_result(result_handle)

    # ---------------------------------------------------------------------
    # result handling
    # ---------------------------------------------------------------------

    def _copy_search_result(
        self,
        result_handle: ctypes.c_void_p,
    ) -> list[tuple[int, float]]:
        count = ctypes.c_uint32()

        rc = self._lib.ReKNN_ResultCount(
            result_handle,
            ctypes.byref(count),
        )
        self._check(rc)

        items: list[tuple[int, float]] = []

        for i in range(count.value):
            label = ctypes.c_int64()
            score = ctypes.c_float()

            rc = self._lib.ReKNN_ResultGetItem(
                result_handle,
                ctypes.c_uint32(i),
                ctypes.byref(label),
                ctypes.byref(score),
            )
            self._check(rc)

            items.append((int(label.value), float(score.value)))

        return items

    def _free_result(self, result_handle: ctypes.c_void_p) -> None:
        rc = self._lib.ReKNN_FreeResult(result_handle)
        self._check(rc)

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

    def _as_int64_vector(self, value: np.ndarray) -> np.ndarray:
        arr = np.asarray(value, dtype=np.int64)

        if arr.ndim != 1:
            raise ValueError("labels must be a 1D array.")

        return np.ascontiguousarray(arr)

        