﻿using BenchmarkDotNet.Attributes;
using System;
using System.Linq;

namespace Foundatio.ConsoleTest
{
    [MemoryDiagnoser]
    public class SubstringVsSubslice
    {
        private string Text;

        [Params(10, 1000)]
        public int CharactersCount { get; set; }

        [GlobalSetup]
        public void Setup() => Text = new string(Enumerable.Repeat('a', CharactersCount).ToArray());

        [Benchmark]
        public string Substring() => Text.Substring(0, Text.Length / 2);

        [Benchmark(Baseline = true)]
        public ReadOnlySpan<char> Slice() => Text.AsSpan().Slice(0, Text.Length / 2);
    }
}
