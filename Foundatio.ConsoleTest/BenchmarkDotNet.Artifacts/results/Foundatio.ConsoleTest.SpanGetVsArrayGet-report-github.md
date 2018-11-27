``` ini

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.407 (1803/April2018Update/Redstone4)
Intel Core i5-4210U CPU 1.70GHz (Haswell), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=2.1.500
  [Host]     : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT


```
|           Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|----------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
|  SpanIndexer_Get | 0.7568 ns | 0.0084 ns | 0.0075 ns |  1.00 |    0.00 |           - |           - |           - |                   - |
| ArrayIndexer_Get | 0.7445 ns | 0.0160 ns | 0.0277 ns |  1.00 |    0.05 |           - |           - |           - |                   - |
