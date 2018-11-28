``` ini

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.407 (1803/April2018Update/Redstone4)
Intel Core i5-4210U CPU 1.70GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=2.1.500
  [Host] : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT


```
|           Method |           Job |     Toolchain | Mean | Error |
|----------------- |-------------- |-------------- |-----:|------:|
| ArrayIndexer_Get |      .NET 4.6 |         net46 |   NA |    NA |
| ArrayIndexer_Set |      .NET 4.6 |         net46 |   NA |    NA |
|  SpanIndexer_Get |      .NET 4.6 |         net46 |   NA |    NA |
|  SpanIndexer_Set |      .NET 4.6 |         net46 |   NA |    NA |
| ArrayIndexer_Get | .NET Core 2.0 | .NET Core 2.0 |   NA |    NA |
| ArrayIndexer_Set | .NET Core 2.0 | .NET Core 2.0 |   NA |    NA |
|  SpanIndexer_Get | .NET Core 2.0 | .NET Core 2.0 |   NA |    NA |
|  SpanIndexer_Set | .NET Core 2.0 | .NET Core 2.0 |   NA |    NA |

Benchmarks with issues:
  SpanVsArray_Indexer.ArrayIndexer_Get: .NET 4.6(Toolchain=net46)
  SpanVsArray_Indexer.ArrayIndexer_Set: .NET 4.6(Toolchain=net46)
  SpanVsArray_Indexer.SpanIndexer_Get: .NET 4.6(Toolchain=net46)
  SpanVsArray_Indexer.SpanIndexer_Set: .NET 4.6(Toolchain=net46)
  SpanVsArray_Indexer.ArrayIndexer_Get: .NET Core 2.0(Toolchain=.NET Core 2.0)
  SpanVsArray_Indexer.ArrayIndexer_Set: .NET Core 2.0(Toolchain=.NET Core 2.0)
  SpanVsArray_Indexer.SpanIndexer_Get: .NET Core 2.0(Toolchain=.NET Core 2.0)
  SpanVsArray_Indexer.SpanIndexer_Set: .NET Core 2.0(Toolchain=.NET Core 2.0)
