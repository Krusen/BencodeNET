# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

...

## [5.0.0] - 2023-03-30
### Changed
- Added target framework .NET 6
- Removed the following target frameworks and any associated conditional code:
  - .NET Standard 2.0
  - .NET Standard 2.1
  - .NET Core App 2.1
  - .NET 5.0
- Upgraded dependency System.IO.Pipelines from 5.0.1 to 6.0.3
- Switched a use of MemoryPool to ArrayPool
- Merged PR #56
  - Always set `private` field in torrent. If `IsPrivate` is false then `0` is output instead of `1`
- Merged PR #60
  - Escape tracker URLs in magnet links

## [4.0.0] - 2021-01-23
### Changed
- Changed supported frameworks to:
  - .NET Standard 2.0
  - .NET Standard 2.1
  - .NET Core App 2.1
  - .NET 5.0
- `Torrent.Pieces` can now only be set to an array with a length which is a multiple of 20.

## [3.1.4] - 2020-03-06
### Fixed
- Issue parsing torrents without both `name` and `name.utf-8` field ([#47])
- Exception when accessing properties `FullPath` and `FullPathUtf8` on `MultiFileInfo` if `Path`/`PathUtf8` is null ([#47])

## [3.1.3] - 2020-03-03
### Added
- Added `Torrent.DisplayNameUtf8` and `MultiFileInfoList.DirectoryNameUtf8`, both mapped to the `name.utf-8` field

### Changed
- New UTF-8 fields are now also added to `BDictionary` created by `Torrent.ToBDictionary` (and used by encode methods)

### Fixed
- `Torrent.NumberOfPieces` is now correctly calculated by dividing by 20 instead of `Pieces.Length` (introduced in 3.1.0) ([#48])

## [3.1.0] - 2020-02-28
### Added
- Added `FileNameUtf8` and `PathUtf8` and `FullPathUtf8` properties to `SingleFileInfo`/`MultiFileInfo` ([#47]) 
  - These properties reads from the `name.utf-8` and `path.utf-8` fields.

### Changed
- `Torrent.NumberOfPieces` now uses `Pieces.Length` instead of `TotalSize` for the calculation ([#48])

## [3.0.1] - 2019-10-17
### Fixed
- Fixed missing parser for `Torrent` ([#44])


## [3.0.0] - 2019-10-13
There is a few changes to the public API, but most people shouldn't be affected by this unless they have extended/overriden functionality.
Basic usage should not see any or only minor changes compared to v2.3.0.

Implemented async support for parsing/encoding.

Added build targets for .NET Core 2.1 and .NET Core 3.0 to take advantage of performance improvements
in `Stream` and `Encoding` APIs for .NET Core 2.1 or later.

Rewrite of internal parsing for better performance, taking advantage of new `Span<T>`/`Memory<T>`
types - faster parsing and less memory allocation.

Removed support for .NET Framework 4.5 and .NET Standard 1.3.
Lowest supported versions are now .NET Framework 4.6.1 (4.7.2 highly recommended) and .NET Standard 2.0.


### Added
- Implemented parsing/encoding using `PipeReader`/`PipeWriter`
- Added `BencodeReader` as replacement for `BencodeStream`
- Added `IBObject.GetSizeInBytes()` method, returning the size of the object in number of bytes.

### Changed
- Improved parsing/encoding performance
- Reduced memory allocation on parsing/encoding
- Made `BString`, `BNumber`, `BList` and `BDictionary` classes `sealed`
- Made parse methods of `BencodeParser` virtual so it can be overriden if needed by anyone
- Constructor `BString(IEnumerable<byte> bytes, Encoding encoding = null)` changed to `BString(byte[] bytes, Encoding encoding = null)`
- Exposed value type of `BString` changed from `IReadOnlyList<byte>` (`byte[]` internally) to `ReadOnlyMemory<byte>`
- Removed parse method overloads on `IBencodeParser` and added matching extension methods instead
- Removed encode method overloads on `IBObject` and added matching extension methods instead
- Torrent parse mode now default to `TorrentParserMode.Tolerant` instead of `TorrentParserMode.Strict`
- Torrent related classes moved to `BencodeNET.Torrents` namespace

### Removed
- Removed `BencodeStream` and replaced with `BencodeReader`
- Dropped .NET Standard 1.3 support; .NET Standard 2.0 is now lowest supported version
- Dropped .NET Framework 4.5 support; .NET Framework 4.6.1 is now lowest supported version (but 4.7.2 is highly recommended)
- Removed most constructors on `BencodeParser` leaving only `BencodeParser(Encoding encoding = null)` and
  added `BencodeParser.Encoding` property to enable changing encoding. Parsers can still be added/replaced/removed
  through `BencodeParser.Parsers` property.

### Fixed
- Parsing from non-seekable `Stream`s is now possible
- Fixed issue parsing torrent files with non-standard 'announce-list' ([#39])


## [2.3.0] - 2019-02-11
### Added
- Added `BNumber` casting operators to `int?` and `long?`


## [2.2.9] - 2017-08-05
### Added
- Added tolerant parse mode for torrents, which skips validation
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


[Unreleased]: ../../compare/v4.0.0...HEAD
[4.0.0]: ../../compare/v3.1.4...v4.0.0
[3.1.4]: ../../compare/v3.1.3...v3.1.4
[3.1.3]: ../../compare/v3.1.0...v3.1.3
[3.1.0]: ../../compare/v3.0.1...v3.1.0
[3.0.1]: ../../compare/v3.0.0...v3.0.1
[3.0.0]: ../../compare/v2.3.0...v3.0.0
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

[#48]: https://github.com/Krusen/BencodeNET/issues/48
[#47]: https://github.com/Krusen/BencodeNET/issues/47
[#44]: https://github.com/Krusen/BencodeNET/issues/44
[#39]: https://github.com/Krusen/BencodeNET/issues/39