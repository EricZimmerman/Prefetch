# Prefetch
Windows Prefetch parser. Supports all known versions from Windows XP to Windows 11.

You can get a command line tool that uses this libary here:

https://github.com/EricZimmerman/PECmd

Open a file with `PrefetchFile.Open(path)` (or `PrefetchFile.Open(stream, name)`) to get an `IPrefetch` with the header, run times and run count, loaded files, and volume information. Windows 10/11 compressed (MAM) files are decompressed with a built-in managed XPRESS Huffman decoder, so every version can be parsed on any OS.

`Open` throws `PrefetchParseException` when the data is not a prefetch file or can't be decompressed. Corruption after that does not throw: the damaged sections are left empty, and `ParseErrors` lists what went wrong.
