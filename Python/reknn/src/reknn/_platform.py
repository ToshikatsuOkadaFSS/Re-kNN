# src/reknn/_platform.py

from __future__ import annotations

import platform
from dataclasses import dataclass
from pathlib import Path


class ReKNNPlatformError(RuntimeError):
    """Raised when the current platform is not supported."""


@dataclass(frozen=True)
class NativeLibraryInfo:
    system: str
    architecture: str
    library_path: Path


def get_native_library_info() -> NativeLibraryInfo:
    """
    現在の実行環境に対応する native library の情報を返す。

    ここでは DLL/so のロードは行わない。
    ctypes.CDLL() は _native.py 側で行う。
    """

    system = _normalize_system(platform.system())
    architecture = _normalize_architecture(platform.machine())

    library_path = resolve_native_library_path(system, architecture)

    return NativeLibraryInfo(
        system=system,
        architecture=architecture,
        library_path=library_path,
    )


def resolve_native_library_path(system: str, architecture: str) -> Path:
    """
    正規化済みの system / architecture から DLL/so のパスを解決する。
    """

    base_dir = Path(__file__).resolve().parent
    lib_dir = base_dir / "lib"

    if system == "windows" and architecture == "x64":
        path = lib_dir / "windows-x64" / "ReKnnCore.dll"

    elif system == "linux" and architecture == "x64":
        path = lib_dir / "linux-x64" / "libReKnnCore.so"

    else:
        raise ReKNNPlatformError(
            f"Unsupported platform: system={system}, architecture={architecture}"
        )

    if not path.exists():
        raise ReKNNPlatformError(
            "Native library was not found. "
            f"Expected path: {path}"
        )

    return path


def _normalize_system(value: str) -> str:
    name = value.strip().lower()

    if name == "windows":
        return "windows"

    if name == "linux":
        return "linux"

    if name == "darwin":
        return "macos"

    raise ReKNNPlatformError(f"Unsupported operating system: {value}")


def _normalize_architecture(value: str) -> str:
    arch = value.strip().lower()

    if arch in ("x86_64", "amd64"):
        return "x64"

    if arch in ("aarch64", "arm64"):
        return "arm64"

    raise ReKNNPlatformError(f"Unsupported CPU architecture: {value}")


    