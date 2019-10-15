![Icon](Assets/icon_64.png)

BencodeNET 
==========
[![license](https://img.shields.io/badge/license-Unlicense-blue.svg)](https://github.com/Krusen/BencodeNET/blob/master/LICENSE.md)
[![AppVeyor](https://ci.appveyor.com/api/projects/status/pikvrmie3ibsk6pt/branch/master?svg=true)](https://ci.appveyor.com/project/Krusen/bencodenet)
[![Coverage](https://coveralls.io/repos/github/Krusen/BencodeNET/badge.svg?branch=master)](https://coveralls.io/github/Krusen/BencodeNET?branch=master)
[![CodeFactor](https://www.codefactor.io/repository/github/krusen/bencodenet/badge)](https://www.codefactor.io/repository/github/krusen/bencodenet)
[![NuGet](https://buildstats.info/nuget/bencodenet?includePreReleases=false)](https://www.nuget.org/packages/BencodeNET/)
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bhttps%3A%2F%2Fgithub.com%2FKrusen%2FBencodeNET.svg?type=shield)](https://app.fossa.io/projects/git%2Bhttps%3A%2F%2Fgithub.com%2FKrusen%2FBencodeNET?ref=badge_shield)

A lightweight and fast .NET library for encoding and decoding bencode (e.g. torrent files and BitTorrent client/tracker communication).

- http://en.wikipedia.org/wiki/Bencode
- https://wiki.theory.org/BitTorrentSpecification#Bencoding

The main focus of this library is on supporting the bencode format; torrent file reading/manipulation is secondary.

Contents
--------

- [Project status](#project-status)
- [Installation](#installation)
- [Getting started](#getting-started)
  - [Parsing](#parsing)
  - [Encoding](#encoding)
  - [String character encoding](#string-character-encoding)
  - [Torrents](#torrents)
    - [File modes](#file-modes)
    - [Non-standard fields](#non-standard-fields)
- [Changelog](#changelog)
- [Roadmap](#roadmap)
- [Building the project](#building-the-project)
- [Contributing](#contributing)
- [Support](#support)

Project status
--------------
The project is in maintenance mode and only getting updated if issues are being reported or if new features are requested.

So, while I can't promise anything, go ahead and report any issues or feature requests by creating a new issue.


Installation
------------
Install the package **BencodeNET** from [NuGet](https://www.nuget.org/packages/BencodeNET/), using `<PackageReference>` or from the command line:

```
// .csproj using PackageReference
<PackageReference Include="BencodeNET" Version="2.3.0" />

// .NET CLI
> dotnet add package BencodeNET
```


Getting started
---------------
### Parsing
Here are some simple examples for parsing bencode strings directly.

```C#
using BencodeNET.Parsing;
using BencodeNET.Objects;

var parser = new BencodeParser();

// Parse unknown type
IBObject bstring = parser.ParseString("12:Hellow World!");
// "Hello World!" (BString)

// If you know the type of the bencode you are parsing, you can use the generic version of `ParseString()` instead.
BString bstring = parser.ParseString<BString>("12:Hello World!");
// "Hello World!" (BString)

BNumber bnumber = parser.ParseString<BNumber>("i42e");
// 42 (BNumber)

BList blist = parser.ParseString<BList>("l3:foo3:bari42ee");
// { "foo", "bar", 42 } (BList)

BDictionary bdictionary = parser.ParseString<BDictionary>("d3:fooi42e5:Hello6:World!e");
// { { "foo", 42 }, { "Hello", "World" } } (BDictionary)
```

Usually you would probably either parse a `Stream` of some kind or a `PipeReader` if using .NET Core.

```C#
BDictionary bdictionary = parser.Parse<BDictionary>(stream);
BDictionary bdictionary = await parser.ParseAsync<BDictionary>(stream);
BDictionary bdictionary = await parser.ParseAsync<BDictionary>(pipeReader);
```

### Encoding
Encoding an object is simple and can be done in the following ways:

```C#
var bstring = new BString("Hello World!");

bstring.EncodeAsString();    // "12:Hello World!"
bstring.EncodeAsBytes();     // [ 49, 50, 58, 72, ... ]

bstring.EncodeTo("C:\\data.bencode"); // Writes "12:Hello World!" to the specified file
bstring.EncodeTo(stream);
await bstring.EncodeToAsync(stream);
bstring.EncodeTo(pipeWriter);
await bstring.EncodeToAsync(pipeWriter);
```

### String character encoding

By default `Encoding.UTF8` is used when rendering strings. 

When parsing a string directly the encoding is used to convert the string to an array of bytes.

If no encoding is passed to `ToString` it will use the encoding the `BString` was created/decoded with.

```C#
// Using the default encoding from Bencode.DefaultEncoding (UTF8)
var parser = new BencodeParser();
var bstring = parser.ParseString("21:æøå äö èéê ñ");
bstring.ToString()              // "æøå äö èéê ñ"
bstring.ToString(Encoding.UTF8) // "æøå äö èéê ñ"

// Using ISO-8859-1
var parser = new BencodeParser(Encoding.GetEncoding("ISO-8859-1"));
bstring = parser.ParseString("12:æøå äö èéê ñ");
bstring.ToString();              // "æøå äö èéê ñ"
bstring.ToString(Encoding.UTF8); // "??? ?? ??? ?"
```

If you parse bencoded data that is not encoded using UTF8 and you don't specify the encoding, then `EncodeAsString`, 
`EncodeAsBytes`, `EncodeTo` and `ToString` without parameters will use `Encoding.UTF8` to try to render the `BString` 
and you will not get the expected result.

```C#
var bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes("12:æøå äö èéê ñ");

// When not specifying an encoding, ToString will use Encoding.UTF8
var parser = new BencodeParser();

var bstring = parser.Parse<BString>(bytes);
bstring.ToString();
// "??? ?? ??? ?"

// Pass your desired encoding to ToString to override the encoding used to render the string
bstring.ToString(Encoding.GetEncoding("ISO-8859-1"));
// "æøå äö èéê ñ"

// You have to specify the used encoding when creating the parser
// BStrings will then use that as the default when encoding the string
var parser = new BencodeParser(Encoding.GetEncoding("ISO-8859-1"));
bstring = parser.Parse<BString>(bytes);
bstring.ToString();
// "æøå äö èéê ñ"
```

The default encoding, UTF8, should be fine in almost all cases.

When you encode an object directly to a stream (`IBObject.EncodeTo`) the encoding is irrelevant as 
the `BString`s are converted to bytes when created, using the specified encoding at the time.

However, when encoding to a string (`IBObject.EncodeAsString`) you can specify the encoding used to render the string.
`BString.EncodeAsString` without specifying an encoding will use the encoding the `BString` was created with.
For all the other types `Encoding.UTF8` will be used.

> **Note:** Using `EncodeAsString` of `BList` and `BDictionary` will encode all contained `BString` using the specified encoding or `Encoding.UTF8` if no encoding is specified.

```C#
var blist = new BList();
blist.Add(new BString("æøå äö èéê ñ", Encoding.GetEncoding("ISO-8859-1")));
blist.EncodeAsString();                                   // "l12:??? ?? ??? ?e"
blist.EncodeAsString(Encoding.UTF8);                      // "l12:??? ?? ??? ?e
blist.EncodeAsString(Encoding.GetEncoding("ISO-8859-1")); // "l12:æøå äö èéê ñe""
```

### Torrents

Working with torrent files:

```C#
using BencodeNET.Objects;
using BencodeNET.Parsing;
using BencodeNET.Torrents;

// Parse torrent by specifying the file path
var parser = new BencodeParser(); // Default encoding is Encoding.UTF8, but you can specify another if you need to
Torrent torrent = parser.Parse<Torrent>("C:\\ubuntu.torrent");

// Or parse a stream
Torrent torrent = parser.Parse<Torrent>(stream);

// Calculate the info hash
string infoHash = torrent.GetInfoHash();
// "B415C913643E5FF49FE37D304BBB5E6E11AD5101"

// or as bytes instead of a string
byte[] infoHashBytes = torrent.GetInfoHashBytes();

// Get Magnet link
string magnetLink = torrent.GetMagnetLink();
// magnet:?xt=urn:btih:1CA512A4822EDC7C1B1CE354D7B8D2F84EE11C32&dn=ubuntu-14.10-desktop-amd64.iso&tr=http://torrent.ubuntu.com:6969/announce&tr=http://ipv6.torrent.ubuntu.com:6969/announce

// Convert Torrent to its BDictionary representation
BDictionary bdictinoary = torrent.ToBDictionary();
```

#### File modes
The property `FileMode` indicates if the torrent is single-file or multi-file. 

For single-file torrents the `File` property contains the relevant file info. 
The `Files` property is null.

For multi-file torrents the `Files` property contains a list of file info and the directory name.
The `File` property is null.

####  Non-standard fields
The `ExtraFields` property is for any non-standard fields which are not accessible through any other property.
Data set on this property will overwrite any data from the `Torrent` itself when encoding it. This way you are able to add to or owerwrite fields.


Changelog
---------
See [CHANGELOG.md](CHANGELOG.md) for any yet unreleased changes and changes made between different version.

https://keepachangelog.com


Roadmap
-------
The project is in maintenance mode and no new features are currently planned.
Feel free to request new features by creating a new issue.


Building the project
---------------------------------
Requirements:
- .NET Core 3.0 SDK
- Visual Studio 2019 (or other IDE support .NET Core 3.0)

Simply checkout the project, restore nuget packages and build the project.


Contributing
------------
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update or add tests as appropriate.


Support
-------
If you use this or one of my other libraries and want to say thank you or support the development feel free to buy me a Red Bull through [buymeacoffee.com](https://www.buymeacoffee.com/UCkS2tw) or through [ko-fi.com](https://ko-fi.com/krusen).

<a href="https://www.buymeacoffee.com/UCkS2tw" target="_blank"><img src="https://bmc-cdn.nyc3.digitaloceanspaces.com/BMC-button-images/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: auto !important;width: auto !important;" ></a>

<a href="https://ko-fi.com/krusen" target="_blank"><img height="37" src="https://az743702.vo.msecnd.net/cdn/kofi1.png?v=2"/></a>


## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bhttps%3A%2F%2Fgithub.com%2FKrusen%2FBencodeNET.svg?type=large)](https://app.fossa.io/projects/git%2Bhttps%3A%2F%2Fgithub.com%2FKrusen%2FBencodeNET?ref=badge_large)
