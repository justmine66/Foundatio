using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO.Pipelines;
using System.Buffers.Text;

namespace Foundatio.ConsoleTest
{
    public class MemorySamples
    {
        public void AllocateMangedMemory()
        {
            var mangedMemory = new Student();
        }

        public void AllocateStackMemory()
        {
            unsafe
            {
                var stackMemory = stackalloc byte[100];
            }
        }

        public void AllocateNativeMemory()
        {
            IntPtr nativeMemory0 = default(IntPtr), nativeMemory1 = default(IntPtr);
            try
            {
                unsafe
                {
                    nativeMemory0 = Marshal.AllocHGlobal(256);
                    nativeMemory1 = Marshal.AllocCoTaskMem(256);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(nativeMemory0);
                Marshal.FreeCoTaskMem(nativeMemory1);
            }
        }

        public void Run()
        {

        }
    }

    public class Student
    {

    }

    public class IntParser : IIntParser
    {
        public int Parse(Span<char> managedMemory)
        {
            throw new NotImplementedException();
        }

        public int Parse(Span<char> , int startIndex, int length)
        {
            ArraySegment<byte>
            throw new NotImplementedException();
        }
    }

    public interface IIntParser
    {
        int Parse(Span<char> managedMemory);
        int Parse(Span<char>, int startIndex, int length);
    }

    public interface MemoryblockCopier
    {
        void Copy<T>(Span<T> source, Span<T> destination);
        void Copy<T>(Span<T> source, int sourceStartIndex, Span<T> destination, int destinationStartIndex, int elementsCount);
    }
}
