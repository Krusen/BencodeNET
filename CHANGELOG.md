# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]


## [2.3.0] - 2019-02-11
### Added
- `BNumber` casting operators to `int?` and `long?`


## [2.2.9] - 2017-08-05
### Added
- Tolerant parse mode for torrents, which skips validation
- Save original info hash when parsing torrent

### Changed
- Try to guess and handle timestamps in milliseconds in 'created' field

### Fixed
- Handle invalid unix timestamps in 'created' field


## [2.2.2] - 2017-04-03
### Added
- `BList.AsNumbers()` method
- `Torrent.PiecesAsHexString` property
- Attempt to use .torrent file encoding when parsing torrent itself

### Changed
- `Torrent.Pieces` type changed to `byte[]`

### Fixed
- `Torrent.Pieces` property


## [2.1.0] - 2016-10-07
API has been more or less completely rewritten for better use with dependency injectiom 
and generally better usability; albeit a bit more complex.

### Added
- .NET Standard support


## [1.3.1] - 2016-06-27
### Added
- Some XML documentation (intellisense)

### Changed
- Better handling of `CreationDate` in torrent files


## [1.2.1] - 2015-09-26
### Changed
- Further performance improvements when decoding strings and numbers (up to ~30% for a standard torrent file)
- XML documentation now included in nuget package


## [1.2.0] - 2015-09-21
### Changed
- Big performance improvements when decoding

### Removed
- BencodeStream.BaseStream property has been removed


## [1.1.0] - 2015-09-21
### Added
- Torrent file abstractions including method to calculate info hash of a torrent file


## [1.0.0] - 2015-09-19


[Unreleased]: ../../compare/v2.3.0...HEAD
[2.3.0]: ../../compare/v2.2.9...v2.3.0
[2.2.9]: ../../compare/v2.2.0...v2.2.9
[2.2.2]: ../../compare/v2.1.0...v2.2.2
[2.1.0]: ../../compare/v1.3.1...v2.1.0
[1.3.1]: ../../compare/v1.3.0...v1.3.1
[1.3.0]: ../../compare/v1.2.1...v1.3.0
[1.2.1]: ../../compare/v1.2.0...v1.2.1
[1.2.0]: ../../compare/v1.1.0...v1.2.0
[1.1.0]: ../../compare/v1.0.0...v1.1.0
[1.0.0]: ../../releases/tag/v1.0.0