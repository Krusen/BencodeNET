BencodeNET
==========
A .NET library for encoding and decoding bencode.

- http://en.wikipedia.org/wiki/Bencode
- https://wiki.theory.org/BitTorrentSpecification#Bencoding

Usage
-----
### Decoding
Simple decoding of a bencoded string:

```C#
// "Hello World!"
BString bstring = Bencode.DecodeString("12:Hello World!"); 

// 42
BNumber bnumber = Bencode.DecodeNumber("i42e");

// { "foo", "bar", 42 }
BList blist = Bencode.DecodeList("l3:foo3:bari42ee");

// { { "foo", 42 }, { "Hello", "World" } }
BDictionary bdictionary = Bencode.DecodeDictionary("d3:fooi42e5:Hello6:World!e");
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
bstring.Encode(); // "12:Hello World!"

var bnumber = new BNumber(42);
bnumber.Encode(); // "i42e"

var blist = new BList { "foo", 42, "bar" };
blist.Encode(); // "l3:fooi42e3:bare"

var bdictionary = new BDictionary { { "foo", 42 }, { "Hello", "World" } };
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

TODO: Examples

If you want to use another encoding than UTF8 and don't want to specify it for each call you can instead change `Bencode.DefaultEncoding` to your desired encoding.

```C#
Bencode.DefaultEncoding = Encoding.ASCII;
```

> **Note:** `Bencode.DefaultEncoding` is a static property and is used by all static methods of the `Bencode` class when no encoding is supplied.
