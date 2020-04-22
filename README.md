# Introduction

This is a CRC32C implementation in C#. The implementation has support for SSE4.2 CRC32 intrinsics.

# Examples

You can use this Library like this:

```csharp
CRC32C crc = new CRC32C();

crc.Calculate(bytes, from, count);

Console.WriteLine($"CRC: {crc.CRC.ToString("X08")}");
```

There are other variants of `Calculate` accepting `byte*` or single bytes.

The library will do hardware support detection. You can force the library to use the software implementation like this:

```csharp
CRC32C.InitializeSoftwareTable();
CRC32C.Acceleration = HardwareAcceleration.Software;
```