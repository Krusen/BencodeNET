BencodeNET 
==========
[![AppVeyor](https://ci.appveyor.com/api/projects/status/pikvrmie3ibsk6pt/branch/master?svg=true)](https://ci.appveyor.com/project/Krusen/bencodenet)
[![Coverage](https://coveralls.io/repos/github/Krusen/BencodeNET/badge.svg?branch=master)](https://coveralls.io/github/Krusen/BencodeNET?branch=master)
[![NuGet](https://buildstats.info/nuget/bencodenet?includePreReleases=false)](https://www.nuget.org/packages/BencodeNET/)

A .NET library for encoding and decoding bencode.

- http://en.wikipedia.org/wiki/Bencode
- https://wiki.theory.org/BitTorrentSpecification#Bencoding

Overview
--------

- [Usage](#usage)
  - [Torrents](#torrents)
    - [File modes](#file-modes)
    - [Non-standard fields](#non-standard-fields)
  - [Parsing](#parsing)
  - [Encoding](#encoding)
- [String Character Encoding](#string-character-encoding)
- [Upgrading from 1.x to 2.0](#upgrading-from-version-1x-to-20)


Usage
-----
### Torrents
Working with torrent files:

```C#
// Parse torrent by specifying the file path
var parser = new BencodeParser(); // Default encoding is Encoding.UT8F, but you can specify another if you need to
var torrent = parse.Parse<Torrent>("C:\ubuntu.torrent");

// Alternatively, handle the stream yourself
using (var stream = File.OpenRead("C:\ubuntu.torrent"))
{
    torrent = parse.Parse<Torrent>(stream);
}

// Calculate the info hash
string infoHash = torrent.GetInfoHash();
// "B415C913643E5FF49FE37D304BBB5E6E11AD5101"

// or as bytes instead of a string
byte[] infoHashBytes = torrent.GetInfoHashBytes();

// Get Magnet link
string magnetLink = torrent.GetMagnetLink();
// magnet:?xt=urn:btih:1CA512A4822EDC7C1B1CE354D7B8D2F84EE11C32&dn=ubuntu-14.10-desktop-amd64.iso&tr=http://torrent.ubuntu.com:6969/announce&tr=http://ipv6.torrent.ubuntu.com:6969/announce

// Convert Torrent to it's BDictionary representation
BDictionary bencode = torrent.ToBDictionary();
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

### Parsing
Simple parsing of a bencoded string:

```C#
var parser = new BencodeParser();
BString bstring = parser.ParseString("12:Hellow World!");
// "Hello World!" (BString)

// If you know the type of the bencode you are parsing, you can use the generic version of `ParseString()` instead.
var bstring2 = parser.ParseString<BString>("12:Hello World!");
// "Hello World!" (BString)

var bnumber = parser.ParseString<BNumber>("i42e");
// 42 (BNumber)

var blist = parser.ParseString<BList>("l3:foo3:bari42ee");
// { "foo", "bar", 42 } (BList)

var bdictionary = parser.ParseString<BDictionary>("d3:fooi42e5:Hello6:World!e");
// { { "foo", 42 }, { "Hello", "World" } } (BDictionary)
```

If you are unsure of the type you can just use the non-generic version:

```C#
IBObject bobject = parser.ParseString("12:Hello World!");

if (bobject is BString)
{
    // The parsed object is a string
}
```

It is also possible to decode directly from a stream instead, for example a `FileStream` or a `MemoryStream`:

```C#
using (var stream = File.OpenRead("Ubuntu.torrent"))
{
    var bdictionary = parser.Parse<BDictionary>(stream);
}
```

### Encoding
You have the option to encode `BObject`s either as a `string`, a `byte[]`, to a `Stream` or directly to a file path.

```C#
var bstring = new BString("Hello World!");
bstring.EncodeAsString();    // "12:Hello World!"
bstring.EncodeAsBytes();     // [ 49, 50, 58, 72, ... ]
bstring.EncodeTo("C:\\data.bencode"); // Writes "12:Hello World!" to the specified file
bstring.EncodeTo(new MemoryStream());

var bnumber = new BNumber(42);
bnumber.EncodeAsString();    // "i42e"

var blist = new BList { "foo", 42, "bar" };
blist.EncodeAsString();      // "l3:fooi42e3:bare"

var bdictionary = new BDictionary { { "foo", 42 }, { "Hello", "World!" } };
bdictionary.EncodeAsString() // "d3:fooi42e5:Hello6:World!e"
```

String Character Encoding
-------------------------
By default `Encoding.UTF8` is used when rendering strings. 

When parsing a string directly the encoding is used to convert the string to an array of bytes.

If no encoding is passed to `ToString` it will use the encoding the `BString` was created/decoded with.

```C#
// Using the default encoding from Bencode.DefaultEncoding (UTF8)
var bstring = Bencode.DecodeString("21:æøå äö èéê ñ");
bstring.ToString()              // "æøå äö èéê ñ"
bstring.ToString(Encoding.UTF8) // "æøå äö èéê ñ"

// Using ISO-8859-1
bstring = Bencode.DecodeString("12:æøå äö èéê ñ", Encoding.GetEncoding("ISO-8859-1"));
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
parser = new BencodeParser(Encoding.GetEncoding("ISO-8859-1"));
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

Upgrading from version 1.x to 2.0
---------------------------------
The API has changed quite a bit in version 2.0, but mostly naming wise and the usage is more or less
the same with some added functionality and ease of use.

Probably the biggest difference is that in 1.x you would use the static class `Bencode` and the methods
`DecodeString(string)`, `DecodeNumber(string)` etc. In 2.0 you have to create an instance of `BencodeParser`
and use the methods on that.

Use `BencodeParser.ParseString(string)` for parsing strings directly or `BencodeParser.Parse(...)` 
for parsing `Stream`, `byte[]` or a file by file path (`string`) without opening af stream yourself.

```C#
// 1.x - Parsing strings directly
BString bstring = Bencode.DecodeString("12:Hello World!");
BNumber bnumber = Bencode.DecodeNumber("i42e");
BList blist = Bencode.DecodeList("l3:foo3:bari42ee");
BDictionary bdictionary = Bencode.DecodeDictionary("d3:fooi42e5:Hello6:World!e");

// 2.0 - Parsing strings directly
var parser = new BencodeParser();
BString bstring = parser.ParseString<BString>("12:Hello World!");
BNumber bnumber = parser.ParseString<BNumber>("i42e");
BList blist = parser.ParseString<BList>("l3:foo3:bari42ee");
BDictionary bdictionary = parser.ParseString<BDictionary>("d3:fooi42e5:Hello6:World!e");

// If you don't know the type you are parsing, you can use the non-generic method
IBObject bobject = parser.ParseString("12:Hellow World!");
```
