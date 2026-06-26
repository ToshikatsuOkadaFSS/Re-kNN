# src/reknn/__init__.py

from .model import ReKNN
#from .result import Evidence, SearchResult
from .exceptions import RekNNException

__version__ = "0.1.0"

# つまりは、公開するクラス一覧
__all__ = [
    "ReKNN",
    "RekNNException",
]
