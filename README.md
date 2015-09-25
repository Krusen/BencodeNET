BencodeNET 
==========
[![AppVeyor](https://img.shields.io/appveyor/ci/Krusen/bencodenet.svg)](https://ci.appveyor.com/project/Krusen/bencodenet)
[![Travis](https://img.shields.io/travis/Krusen/BencodeNET.svg)](https://travis-ci.org/Krusen/BencodeNET) [![NuGet Version](http://img.shields.io/nuget/v/BencodeNET.svg)](https://www.nuget.org/packages/BencodeNET/) [![NuGet Downloads](http://img.shields.io/nuget/dt/BencodeNET.svg)](https://www.nuget.org/packages/BencodeNET/)

A .NET library for encoding and decoding bencode.

- http://en.wikipedia.org/wiki/Bencode
- https://wiki.theory.org/BitTorrentSpecification#Bencoding

Usage
-----
### Torrents
Working with torrent files:

```C#
// Decode torrent by specifying the file path
TorrentFile torrent = Bencode.DecodeTorrentFile("C:\ubuntu.torrent");

// Alternatively, handle the stream yourself
using (var stream = File.OpenRead("C:\ubuntu.torrent"))
{
    torrent = Bencode.DecodeTorrentFile(stream);
}

// Calculate the info hash
string infoHash = torrent.CalculateInfoHash();
// "B415C913643E5FF49FE37D304BBB5E6E11AD5101"

// You can also calculate the info hash with this static method
TorrentFile.CalculateInfoHash(BDictionary info)
```

The following fields are available directly on the `TorrentFile` itself and parsed to a default .NET type where applicable:

- Announce : `string`
- AnnounceList : `BList`
- Comment : `string`
- CreatedBy : `string`
- CreationDate : `DateTime`
- Encoding : `string`
- Info : `BDictionary`

If you need to access other fields you can access them by their key:

```C#
BString keyWithStringValue = torrent["key with string value"];

// The default fields are also accessible here
BDictionary info = torrent["info"];
BString announce = torrent["announce"]
BList announceList = torrent["announce-list"]
```

### Decoding
Simple decoding of a bencoded string:

```C#
BString bstring = Bencode.DecodeString("12:Hello World!");
// "Hello World!"

BNumber bnumber = Bencode.DecodeNumber("i42e");
// 42

BList blist = Bencode.DecodeList("l3:foo3:bari42ee");
// { "foo", "bar", 42 }

BDictionary bdictionary = Bencode.DecodeDictionary("d3:fooi42e5:Hello6:World!e");
// { { "foo", 42 }, { "Hello", "World" } }
```

If you are unsure of the type you can just use the generic `Bencode.Decode`:

```C#
IBObject bobject = Bencode.Decode("12:Hello World!");

if (bobject is BString)
{
    // The decoded object is a string
}
```

It is also possible to decode directly from a stream instead, for example a `FileStream`:

```C#
using (var fs = File.OpenRead("Ubuntu.torrent"))
{
    BDictionary bdictionary = Bencode.DecodeDictionary(fs);
}
```

### Encoding

```C#
var bstring = new BString("Hello World!");
bstring.Encode();    // "12:Hello World!"

var bnumber = new BNumber(42);
bnumber.Encode();    // "i42e"

var blist = new BList { "foo", 42, "bar" };
blist.Encode();      // "l3:fooi42e3:bare"

var bdictionary = new BDictionary { { "foo", 42 }, { "Hello", "World!" } };
bdictionary.Encode() // "d3:fooi42e5:Hello6:World!e"
```

It is also possible to encode directly to a stream:

```C#
using (var fs = File.OpenWrite("MyTorrent.torrent"))
{
    bobject.EncodeToStream(fs);
}
```

String Character Encoding
-------------------------
By default `Encoding.UTF8` is used when rendering strings. 

When decoding a string directly the encoding is used to convert the string to an array of bytes.

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

If you decode a bencoded stream that is not using UTF8 and you don't specify the encoding, then `ToString` without parameters will use `Encoding.UTF8` to try to render the `BString` and you will not get the expected result.

```C#
var bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes("12:æøå äö èéê ñ");
using (var ms = new MemoryStream(bytes))
{
    // When not specifying an encoding, ToString will use Bencode.DefaultEncoding (UTF8)
    var bstring = Bencode.DecodeString(ms);
    bstring.ToString();
    // "??? ?? ??? ?"
    
    // Pass your desired encoding to ToString to override the encoding used to render the string
    bstring.ToString(Encoding.GetEncoding("ISO-8859-1"));
    // "æøå äö èéê ñ"

    // If you specify an encoding when decoding, ToString will use that as the default when rendering the string
    bstring = Bencode.DecodeString(ms, Encoding.GetEncoding("ISO-8859-1"));
    bstring.ToString();
    // "æøå äö èéê ñ"
}
```

When you encode an object directly to a stream (`IBObject.EncodeToStream`) the encoding is irrelevant.

However, when encoding to a string (`IBObject.Encode`) you can specify the encoding used to render the string. `BString.Encode` without specifying an encoding will use the encoding the `BString` was created with. For all the other types `Bencode.DefaultEncoding` will be used.

> **Note:** Using `BList.Encode` and `BDictionary.Encode` will render all contained `BString` using `Bencode.DefaultEncoding` irregardless of the encoding of the `BString` itself.

```C#
var blist = new BList();
blist.Add(new BString("æøå äö èéê ñ", Encoding.GetEncoding("ISO-8859-1")));
blist.Encode();                                   // "l12:??? ?? ??? ?e"
blist.Encode(Encoding.UTF8);                      // "l12:??? ?? ??? ?e
blist.Encode(Encoding.GetEncoding("ISO-8859-1")); // "l12:æøå äö èéê ñe""
```

If you want to use another encoding than UTF8 as the default encoding you can set `Bencode.DefaultEncoding` to your desired encoding.

```C#
Bencode.DefaultEncoding = Encoding.ASCII;
```

> **Note:** `Bencode.DefaultEncoding` is a static property and is used by all static methods of the `Bencode` class when no encoding is supplied.
