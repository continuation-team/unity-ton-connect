# Just-CRC32C
[![License: LGPL](https://img.shields.io/github/license/bartimaeusnek/Just-CRC32C)](https://opensource.org/licenses/LGPL-3.0) [![nuget](https://img.shields.io/nuget/v/JustCRC32C.svg)](https://www.nuget.org/packages/JustCRC32C/)

Just a simple and fast CRC32C Wrapper with hardware acceleration.

The library overall is licensed under LGPLv3, so you can use it in a propriatary project, as long as you do not change the library itsef!

Software-fallback is taken from: [here](https://github.com/force-net/Crc32.NET/blob/26c5a818a5c7a3d6a622c92d3cd08dba586c263c/Crc32.NET/SafeProxy.cs#L38)
and Licensed under [MIT License](https://github.com/force-net/Crc32.NET/blob/26c5a818a5c7a3d6a622c92d3cd08dba586c263c/LICENSE) and improved by a tiny bit.

The netstandard2.0 variation for .netFramework uses p/invoke for the ANSI-C compatible JustCRC32C.Native library to archive hardware acceleration.

Benchmark results:
``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 10 (10.0.19045.3086/22H2/2022Update)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK=8.0.100-preview.1.23115.2
  [Host]     : .NET 6.0.18 (6.0.1823.26907), X64 RyuJIT AVX2
  Job-LBWJTK : .NET 8.0.0 (8.0.23.11008), X64 RyuJIT AVX2

Runtime=.NET 8.0  RunStrategy=Throughput  

```
|                 Method |               Arrays |             Mean |         Error |        StdDev |           Median | Ratio | RatioSD |
|----------------------- |--------------------- |-----------------:|--------------:|--------------:|-----------------:|------:|--------:|
| **JustCrc32C_HardwareX64** | **big a(...)ytes) [41]** |   **318,356.546 μs** |   **959.0297 μs** |   **748.7473 μs** |   **318,351.500 μs** |  **1.00** |    **0.00** |
|    JustCrc32C_Hardware | big a(...)ytes) [41] |   614,984.617 μs | 1,256.5669 μs |   981.0448 μs |   614,961.000 μs |  1.93 |    0.01 |
|    JustCrc32C_Software | big a(...)ytes) [41] | 1,203,940.725 μs | 8,988.4146 μs | 7,017.5629 μs | 1,201,179.900 μs |  3.78 |    0.03 |
|          Crc32_dot_NET | big a(...)ytes) [41] | 1,404,042.087 μs | 8,606.4555 μs | 8,050.4838 μs | 1,403,310.000 μs |  4.41 |    0.03 |
|                        |                      |                  |               |               |                  |       |         |
| **JustCrc32C_HardwareX64** | **mediu(...)ytes) [44]** |    **25,509.060 μs** |   **308.8947 μs** |   **288.9403 μs** |    **25,313.297 μs** |  **1.00** |    **0.00** |
|    JustCrc32C_Hardware | mediu(...)ytes) [44] |    48,946.426 μs |    47.9768 μs |    40.0628 μs |    48,952.936 μs |  1.92 |    0.02 |
|    JustCrc32C_Software | mediu(...)ytes) [44] |    96,605.061 μs | 1,044.4672 μs |   925.8930 μs |    96,954.150 μs |  3.79 |    0.05 |
|          Crc32_dot_NET | mediu(...)ytes) [44] |   112,126.815 μs | 1,285.5934 μs | 1,202.5449 μs |   111,274.080 μs |  4.40 |    0.07 |
|                        |                      |                  |               |               |                  |       |         |
| **JustCrc32C_HardwareX64** | **small(...)ytes) [37]** |       **243.729 μs** |     **0.2736 μs** |     **0.2285 μs** |       **243.614 μs** |  **1.00** |    **0.00** |
|    JustCrc32C_Hardware | small(...)ytes) [37] |       486.916 μs |     0.5105 μs |     0.4263 μs |       486.761 μs |  2.00 |    0.00 |
|    JustCrc32C_Software | small(...)ytes) [37] |       961.411 μs |     4.4316 μs |     4.1454 μs |       961.110 μs |  3.94 |    0.02 |
|          Crc32_dot_NET | small(...)ytes) [37] |     1,121.784 μs |    10.2630 μs |     9.6001 μs |     1,121.731 μs |  4.60 |    0.04 |
|                        |                      |                  |               |               |                  |       |         |
| **JustCrc32C_HardwareX64** | **small(...)ytes) [49]** |     **2,519.386 μs** |    **28.7737 μs** |    **26.9149 μs** |     **2,502.841 μs** |  **1.00** |    **0.00** |
|    JustCrc32C_Hardware | small(...)ytes) [49] |     4,980.498 μs |    89.1571 μs |    83.3976 μs |     4,947.656 μs |  1.98 |    0.05 |
|    JustCrc32C_Software | small(...)ytes) [49] |     9,880.705 μs |   194.7273 μs |   182.1480 μs |     9,932.087 μs |  3.92 |    0.09 |
|          Crc32_dot_NET | small(...)ytes) [49] |    11,428.893 μs |   227.2975 μs |   287.4585 μs |    11,432.837 μs |  4.53 |    0.13 |
|                        |                      |                  |               |               |                  |       |         |
| **JustCrc32C_HardwareX64** | **small(...)ytes) [38]** |        **24.642 μs** |     **0.4474 μs** |     **0.4185 μs** |        **24.575 μs** |  **1.00** |    **0.00** |
|    JustCrc32C_Hardware | small(...)ytes) [38] |        49.555 μs |     0.9786 μs |     1.1269 μs |        49.009 μs |  2.02 |    0.06 |
|    JustCrc32C_Software | small(...)ytes) [38] |        99.483 μs |     1.9473 μs |     2.9147 μs |       100.555 μs |  4.07 |    0.14 |
|          Crc32_dot_NET | small(...)ytes) [38] |       113.458 μs |     2.2429 μs |     3.4919 μs |       111.205 μs |  4.64 |    0.16 |
|                        |                      |                  |               |               |                  |       |         |
| **JustCrc32C_HardwareX64** | **tiny (...)ytes) [27]** |         **1.903 μs** |     **0.0293 μs** |     **0.0274 μs** |         **1.911 μs** |  **1.00** |    **0.00** |
|    JustCrc32C_Hardware | tiny (...)ytes) [27] |         3.530 μs |     0.0283 μs |     0.0265 μs |         3.518 μs |  1.86 |    0.04 |
|    JustCrc32C_Software | tiny (...)ytes) [27] |        11.966 μs |     0.2357 μs |     0.2205 μs |        11.943 μs |  6.29 |    0.14 |
|          Crc32_dot_NET | tiny (...)ytes) [27] |        13.929 μs |     0.1878 μs |     0.1757 μs |        13.931 μs |  7.32 |    0.13 |
