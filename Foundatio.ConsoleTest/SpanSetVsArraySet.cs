using BenchmarkDotNet.Attributes;
using System;
using System.Linq;

namespace Foundatio.ConsoleTest
{
    [MemoryDiagnoser]
    public class SpanSetVsArraySet
    {
        protected const int Loops = 100;
        protected const int Count = 1000;

        protected byte[] arrayField;

        [GlobalSetup]
        public void Setup() => arrayField = Enumerable.Repeat(1, Count).Select((val, index) => (byte)index).ToArray();

        [Benchmark(OperationsPerInvoke = Loops * Count, Baseline = true)]
        public void SpanIndexer_Set()
        {
            Span<byte> local = arrayField;
            for (int _ = 0; _ < Loops; _++)
            {
                for (int j = 0; j < local.Length; j++)
                {
                    local[j] = byte.MaxValue;
                }
            }
        }

        [Benchmark(OperationsPerInvoke = Loops * Count)]
        public void ArrayIndexer_Set()
        {
            var local = arrayField;
            for (int _ = 0; _ < Loops; _++)
            {
                for (int j = 0; j < local.Length; j++)
                {
                    local[j] = byte.MaxValue;
                }
            }
        }
    }
}
