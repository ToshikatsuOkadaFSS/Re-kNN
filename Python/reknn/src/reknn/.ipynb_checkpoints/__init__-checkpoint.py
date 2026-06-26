# src/reknn/__init__.py

from .model import ReKNN
from .result import Evidence, SearchResult
from .exceptions import ReKNNError, ReKNNNativeError

__version__ = "0.1.0"

# つまりは、公開するクラス一覧
__all__ = [
    "ReKNN",
    "SearchResult",
    "Evidence",
    "ReKNNError",
    "ReKNNNativeError",
]