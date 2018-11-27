using BenchmarkDotNet.Attributes;
using System;
using System.Linq;

namespace Foundatio.ConsoleTest
{
    [MemoryDiagnoser]
    public class SpanGetVsArrayGet
    {
        protected const int Loops = 100;
        protected const int Count = 1000;

        protected byte[] arrayField;

        [GlobalSetup]
        public void Setup() => arrayField = Enumerable.Repeat(1, Count).Select((val, index) => (byte)index).ToArray();

        [Benchmark(OperationsPerInvoke = Loops * Count, Baseline = true)]
        public byte SpanIndexer_Get()
        {
            Span<byte> local = arrayField;
            byte result = 0;
            for (int _ = 0; _ < Loops; _++)
            {
                for (int j = 0; j < local.Length; j++)
                {
                    result = local[j];
                }
            }
            return result;
        }

        [Benchmark(OperationsPerInvoke = Loops * Count)]
        public byte ArrayIndexer_Get()
        {
            var local = arrayField;
            byte result = 0;
            for (int _ = 0; _ < Loops; _++)
            {
                for (int j = 0; j < local.Length; j++)
                {
                    result = local[j];
                }
            }
            return result;
        }
    }
}
