# Release Notes

## July 6, 2026 (Version 1.2.1)

### Added

- Added a function (Delete2) to the API that deletes data using only mainId.
- Added clustering result output functionality to the Python package.
- Added a feature to the Python package that deletes data using only mainId.

### Changed

- Added support for file update and deletion features in the sample application for Python.
- Fixed an issue in the output process of detailed information in search results.



## June 29, 2026 (Version 1.2)

### Added

- Added a Python package  
- Added APIs associated with the Python packaging  
- Added a sample application for Python

### Changed

- Fixed an issue where the result was incorrect in some cases during inference  
- Fixed an issue where searching with vectors having zero norm produced incorrect results



## May 11, 2026 (Version 1.1)

### Added

- Added a MNIST sample application for WinUI
- Added Semantic Context Clustering results
- Added an API to refine the entire DB at once
- Added Linux (Ubuntu) libraries

### Changed

- Improved error message output
- Revised return values for some APIs
  - `Load` / `Save` now return `false` when DB loading or saving fails
- Revised the storage location of DLL and `.so` files
  - Library binaries are now consolidated under the library directory
- Updated related documentation to reflect these changes

## 2026.4.1 (Version 1.0)

- Initial release

