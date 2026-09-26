# YoutubeDLSharp fork — Newtonsoft.Json → System.Text.Json port

Forked from [Bluegrams/YoutubeDLSharp](https://github.com/Bluegrams/YoutubeDLSharp) (BSD-3-Clause,
license retained in `LICENSE.txt`). Ported September 2026.

## Status

- Based on upstream v1.2.0. Builds clean for `net8.0` and `netstandard2.0`.
- Upstream's `net45` and `net6.0` targets were dropped (`netstandard2.0;net45;net6.0;net8.0` ->
  `netstandard2.0;net8.0`): System.Text.Json doesn't meaningfully support net45, and net6.0 is
  out of support. See the comment in `YoutubeDLSharp/YoutubeDLSharp.csproj`.
- No Newtonsoft.Json package or code references remain (a few comments mention it to explain the port).
- Verified: upstream's offline test suite passes 32/32 once `SerializationTests.cs` is ported to
  the new types (the five failures before that were the test file still using Newtonsoft, not the
  library), and a separate 12-case harness covers the trickiest converters (`[EnumMember]`
  snake_case enum mapping, the float-truncating nullable-int converter). A real
  `RunVideoDataFetch` against yt-dlp also parsed correctly.
- **Not run:** the network-dependent tests (`DownloadTests.cs`, `MetadataTests.cs`), which need
  yt-dlp/FFmpeg binaries and real network calls.

## What changed, file by file

- `Converters/DateTimeConverter.cs` — `UnixTimestampConverter` and `CustomDateTimeConverter`
  rewritten against `System.Text.Json.Serialization.JsonConverter<T>` (`Read`/`Write` over
  `Utf8JsonReader`/`Utf8JsonWriter`, not `ReadJson`/`WriteJson` over `JsonReader`/`JsonWriter`).
- `Converters/StringConverters.cs` — same API shift. `StringToEnumConverter<T>` now resolves
  `[EnumMember(Value = "...")]` via reflection directly (System.Text.Json's built-in
  `JsonStringEnumConverter` doesn't understand `[EnumMember]` at all, so this replaces both
  Newtonsoft's `StringEnumConverter` *and* the `[EnumMember]` attributes it was reading).
- `Metadata/*.cs` (6 files) — `[JsonProperty("x")]` → `[JsonPropertyName("x")]` (~140 occurrences,
  mechanical rename, same string values).
- `Metadata/VideoData.cs` line ~139 — `[JsonConverter(typeof(StringEnumConverter))]` on
  `Availability` (Newtonsoft's built-in) → `[JsonConverter(typeof(StringToEnumConverter<Availability>))]`
  (the custom one above) — `Availability`'s JSON values are snake_case (`premium_only` etc.) via
  `[EnumMember]`, which the built-in STJ converter can't map.
- `Utils.cs`, `YoutubeDL.cs` — `JsonConvert.DeserializeObject<T>(json)` → `JsonSerializer.Deserialize<T>(json)`;
  `catch (JsonSerializationException)` → `catch (JsonException)`.
- `Metadata/VideoData.cs` `ToString()` override — `JsonConvert.SerializeObject(this, Formatting.Indented)`
  → `System.Text.Json.JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true })`.
- `YoutubeDLSharp.csproj` — Newtonsoft.Json `PackageReference` removed; `System.Text.Json` added
  as a package reference for the `netstandard2.0` target only (net8.0 already ships it).

## Building

```
cd YoutubeDLSharp
dotnet build -f net8.0
dotnet build -f netstandard2.0
```

## Using this in your own project

Build `YoutubeDLSharp.dll` for whichever target framework your project consumes (`netstandard2.0`
or `net8.0`), reference it the same way you'd reference any local/vendored DLL, and drop
`Newtonsoft.Json` entirely if nothing else in your project needs it - this fork has zero
dependency on it.
