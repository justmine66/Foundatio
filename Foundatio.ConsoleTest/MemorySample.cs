using System;
using System.Collections.Generic;
using System.Text;

namespace Foundatio.ConsoleTest
{
    public class MemorySample
    {
        internal unsafe void BufferRented(
            int bufferId,
            int bufferSize,
            int poolId,
            int bucketId)
        {
            var payload = stackalloc EventData[4];
            payload[0].Size = sizeof(int);
            payload[0].DataPointer = ((IntPtr)(&bufferId));
            payload[1].Size = sizeof(int);
            payload[1].DataPointer = ((IntPtr)(&bufferSize));
            payload[2].Size = sizeof(int);
            payload[2].DataPointer = ((IntPtr)(&poolId));
            payload[3].Size = sizeof(int);
            payload[3].DataPointer = ((IntPtr)(&bucketId));
        }

        public struct EventData
        {
            public int Size { get; internal set; }
            public IntPtr DataPointer { get; set; }
        }

        public interface IMemorySegment
        {
            void Copy<T>(T[] source, T[] destination);
            void Copy<T>(T[] source, int sourceStartIndex, T[] destination, int destinationStartIndex, int elementsCount);
            unsafe void Copy<T>(void* source, void* destination, int elementsCount);
            unsafe void Copy<T>(void* source, int sourceStartIndex, void* destination, int destinationStartIndex, int elementsCount);
            unsafe void Copy<T>(void* source, int sourceLength, T[] destination);
            unsafe void Copy<T>(void* source, int sourceStartIndex, T[] destination, int destinationStartIndex, int elementsCount);
        }

        public void UnManagedMemory()
        {
            unsafe
            {
                var pointerToStack = stackalloc byte[256];
                var stackMemory = new Span<byte>(pointerToStack, 256);
            }
        }
    }
}
