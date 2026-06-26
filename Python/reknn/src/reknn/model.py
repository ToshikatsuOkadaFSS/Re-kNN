# src/reknn/model.py

from __future__ import annotations

import ctypes

import numpy as np

from ._native import _NativeRuntime


class ReKNN:
    def __init__(self,
                 mode='bert',
                 k=2,
                 search_k=5,
                 add_k=5,
                 min_similarity=0.9,
                 predict_similarity=0.6,
                 ignore_similarity=0.3,
                 unknown_label=-1) -> None:
        self._native = _NativeRuntime.instance()

        if mode == 'bert':
            self.mode = 1
            self.dimension = 768
        elif mode == 'mnist':
            self.mode = 2
            self.dimension = 784
        elif mode == 'cifar10':
            self.mode = 3
            self.dimension = 3072            
        elif mode == 'vec300':
            self.mode = 4
            self.dimension = 300
        else:
            raise RekNNException("bad mode = {0}".format(mode))

        self.k = k
        self.search_k = search_k
        self.add_k = add_k
        self.min_similarity = min_similarity
        self.predict_similarity = predict_similarity
        self.ignore_similarity = ignore_similarity
        self.unknown_label = unknown_label

        self.current_instance = self._native.Initialize(self.mode)
        
        self._closed = False


    
    def clear(self, target_instance=None):
        self._ensure_open()

        target_instance = self.current_instance if target_instance is None else target_instance
        self._native.Clear(target_instance)


    def fit(self, X, y, item_ids=None, target_instance=None, verbose=False):
        self._ensure_open()
        self.clear()
        self.add(X, y, item_ids, target_instance, verbose)

    def add(self, X: np.ndarray, y: np.ndarray, item_ids=None, target_instance=None, verbose=False) -> None:
        self._ensure_open()

        if X.shape[1] != self.dimension:
            raise ValueError(
                f"vector dimension mismatch: expected {self.dimension}, "
                f"got {vectors.shape[1]}"
            )

        target_instance = self.current_instance if target_instance is None else target_instance
         
        vec = np.array(X)
        main = np.array(y)

        length = vec.shape[0]
        if item_ids is None:
            item_ids = np.array([-1 for x in range(length)])
        else:
            item_ids = np.array(item_ids)

        if length != len(main):
            raise ValueError(
                f"Vector data mismatch: X:{length}, item_ids:{len(main)}")

        if length != len(item_ids):
            raise ValueError(
                f"Vector data mismatch: X:{length}, y:{len(main)}")

        if length > 0:
            length_info = np.full(length, 1, dtype=np.int64)
            #print(f"vec.shape={vec.shape}")
            self._native.AddBulk(target_instance, length, vec, length_info, y, item_ids, self.add_k, self.min_similarity)

        #if verbose:
        #    for i in tqdm(range(length)):
        #        #print(vec[i][0:3])
        #        self._native.Add(target_instance, vec[i].reshape(-1, self.dimension), 1, y[i], item_ids[i], self.add_k, self.min_similarity)
        #else:
        #    for i in range(length):
        #        #print(vec[i][0:3])
        #        self._native.Add(target_instance, vec[i].reshape(-1, self.dimension), 1, y[i], item_ids[i], self.add_k, self.min_similarity)
    
    def predict(self,
        X : np.ndarray,
        k=None,
        predict_similarity=None,
        ignore_similarity=None, target_instance=None
    ):
        self._ensure_open()

        target_instance = self.current_instance if target_instance is None else target_instance
        k = self.k if k is None else k
        predict_similarity = self.predict_similarity if predict_similarity is None else predict_similarity
        ignore_similarity = self.ignore_similarity if ignore_similarity is None else ignore_similarity

        if X.shape[1] != self.dimension:
            raise ValueError(
                f"query dimension mismatch: expected {self.dimension}, "
                f"got {vectors.shape[1]}"
            )

        vec = np.array(X)
        length = vec.shape[0]

        result = self._native.Predict2(
            target_instance,
            vec, 
            length, 
            k, 
            predict_similarity,
            ignore_similarity)

        return result['PredictedLabel']
         
         
    def explain(self,
        X : np.ndarray,
        k=None,
        predict_similarity=None,
        ignore_similarity=None,
        target_instance=None
    ):
        self._ensure_open()

        target_instance = self.current_instance if target_instance is None else target_instance
        k = self.k if k is None else k
        predict_similarity = self.predict_similarity if predict_similarity is None else predict_similarity
        ignore_similarity = self.ignore_similarity if ignore_similarity is None else ignore_similarity

        if X.shape[1] != self.dimension:
            raise ValueError(
                f"query dimension mismatch: expected {self.dimension}, "
                f"got {vectors.shape[1]}"
            )

        vec = np.array(X)
        length = vec.shape[0]

        results = self._native.Predict2(
            target_instance,
            vec, 
            length, 
            k, 
            predict_similarity,
            ignore_similarity)

        return results

        
    def search(self,
        X : np.ndarray, 
        k=None,
        target_instance=None
    ):
        self._ensure_open()

        target_instance = self.current_instance if target_instance is None else target_instance
        k = self.search_k if k is None else k

        if X.shape[1] != self.dimension:
            raise ValueError(
                f"query dimension mismatch: expected {self.dimension}, "
                f"got {vectors.shape[1]}"
            )

        vec = np.array(X)
        length = vec.shape[0]

        results = self._native.Search(
            target_instance,
            vec,
            length, 
            k)

        return results        
        
        
    def save(self, db_path, target_instance:int=None):
        self._ensure_open()
        target_instance = self.current_instance if target_instance is None else target_instance
        return self._native.Save(db_path, target_instance)

    def load(self, db_path, target_instance:int=None):
        self._ensure_open()
        target_instance = self.current_instance if target_instance is None else target_instance
        return self._native.Load(db_path, target_instance)

    def refine(self, limit, target_instance:int=None):
        self._ensure_open()
        target_instance = self.current_instance if target_instance is None else target_instance
        return self._native.RefineAll(limit, target_instance)        

    def get_total_vector(self, target_instance:int=None):
        self._ensure_open()
        target_instance = self.current_instance if target_instance is None else target_instance
        return self._native.GetTotalVector(target_instance)        
        
    def set_debug_mode(self, mode:int)->None:
        self._ensure_open()
        return self._native.SetDebugMode(mode)        
        
    def close(self) -> None:
        if self._closed:
            return

        if self.current_instance is not None:
            self._native.Disppse(self.current_instance)
            self.current_instance = None

        self._closed = True

    def _ensure_open(self) -> None:
        if self._closed or self.current_instance is None:
            raise RuntimeError("ReKNN instance is already closed.")

    def __enter__(self) -> "ReKNN":
        self._ensure_open()
        return self

    def __exit__(self, exc_type, exc, tb) -> None:
        self.close()

    def __del__(self) -> None:
        try:
            self.close()
        except Exception:
            # __del__ では例外を外に出さない
            pass
