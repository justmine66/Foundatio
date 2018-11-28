using BenchmarkDotNet.Running;
using System;

namespace Foundatio.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.Run<SubstringVsSubslice>();
            //BenchmarkRunner.Run<SpanGetVsArrayGet>();
            //BenchmarkRunner.Run<SpanSetVsArraySet>();
            BenchmarkRunner.Run<SpanVsArray_Indexer>();

            Console.Read();   
        }
    }
}
