``` ini

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.407 (1803/April2018Update/Redstone4)
Intel Core i5-4210U CPU 1.70GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=2.1.500
  [Host]     : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT


```
|    Method | CharactersCount |       Mean |     Error |     StdDev |     Median |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|---------- |---------------- |-----------:|----------:|-----------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
| **Substring** |              **10** |  **22.141 ns** | **0.8948 ns** |  **2.5673 ns** |  **21.404 ns** |   **9.77** |    **1.47** |      **0.0254** |           **-** |           **-** |                **40 B** |
|     Slice |              10 |   2.270 ns | 0.0912 ns |  0.1447 ns |   2.242 ns |   1.00 |    0.00 |           - |           - |           - |                   - |
|           |                 |            |           |            |            |        |         |             |             |             |                     |
| **Substring** |            **1000** | **208.534 ns** | **6.2942 ns** | **18.1601 ns** | **198.993 ns** | **109.43** |   **11.43** |      **0.6557** |           **-** |           **-** |              **1032 B** |
|     Slice |            1000 |   2.110 ns | 0.0706 ns |  0.0589 ns |   2.104 ns |   1.00 |    0.00 |           - |           - |           - |                   - |
