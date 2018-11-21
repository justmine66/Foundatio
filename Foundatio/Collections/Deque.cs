using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Foundatio.Collections
{
    [DebuggerDisplay("Count = {Count}, Capacity = {Capacity}")]
    [DebuggerTypeProxy(typeof(Deque<>.DebugView))]
    public sealed class Deque<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>, IList, ICollection
    {
        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly Deque<T> _deque;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public T[] Items => _deque.ToArray();

            public DebugView(Deque<T> deque)
            {
                _deque = deque;
            }
        }

        private const int DefaultCapacity = 8;

        private T[] _buffer;

        private int _offset;

        bool ICollection<T>.IsReadOnly => false;

        public T this[int index]
        {
            get
            {
                CheckExistingIndexArgument(Count, index);
                return DoGetItem(index);
            }
            set
            {
                CheckExistingIndexArgument(Count, index);
                DoSetItem(index, value);
            }
        }

        bool IList.IsFixedSize => false;

        bool IList.IsReadOnly => false;

        object IList.this[int index]
        {
            get => this[index];
            set
            {
                if (value == null && default(T) != null)
                {
                    throw new ArgumentNullException(nameof(value), "Value cannot be null.");
                }
                if (!IsT(value))
                {
                    throw new ArgumentException("Value is of incorrect type.", nameof(value));
                }
                this[index] = (T)value;
            }
        }

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => this;

        private bool IsEmpty => Count == 0;

        private bool IsFull => Count == Capacity;

        private bool IsSplit => _offset > Capacity - Count;

        public int Capacity
        {
            get => _buffer.Length;
            set
            {
                if (value < Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Capacity cannot be set to a value less than Count");
                }
                if (value != _buffer.Length)
                {
                    T[] array = new T[value];
                    CopyToArray(array, 0);
                    _buffer = array;
                    _offset = 0;
                }
            }
        }

        public int Count
        {
            get;
            private set;
        }

        public Deque(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity may not be negative.");
            }
            _buffer = new T[capacity];
        }

        public Deque(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }
            var readOnlyCollection = CollectionHelpers.ReifyCollection<T>(collection);
            var count = readOnlyCollection.Count;
            if (count > 0)
            {
                _buffer = new T[count];
                DoInsertRange(0, readOnlyCollection);
            }
            else
            {
                _buffer = new T[8];
            }
        }

        public Deque()
            : this(8)
        {
        }

        public void Insert(int index, T item)
        {
            CheckNewIndexArgument(Count, index);
            DoInsert(index, item);
        }

        public void RemoveAt(int index)
        {
            CheckExistingIndexArgument(Count, index);
            DoRemoveAt(index);
        }

        public int IndexOf(T item)
        {
            var @default = EqualityComparer<T>.Default;
            var num = 0;
            using (var enumerator = GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (@default.Equals(item, current))
                    {
                        return num;
                    }
                    num++;
                }
            }
            return -1;
        }

        void ICollection<T>.Add(T item)
        {
            DoInsert(Count, item);
        }

        bool ICollection<T>.Contains(T item)
        {
            var @default = EqualityComparer<T>.Default;
            using (var enumerator = GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    if (@default.Equals(item, current))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            var count = Count;
            CheckRangeArguments(array.Length, arrayIndex, count);
            CopyToArray(array, arrayIndex);
        }

        private void CopyToArray(Array array, int arrayIndex = 0)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (IsSplit)
            {
                var num = Capacity - _offset;
                Array.Copy(_buffer, _offset, array, arrayIndex, num);
                Array.Copy(_buffer, 0, array, arrayIndex + num, Count - num);
            }
            else
            {
                Array.Copy(_buffer, _offset, array, arrayIndex, Count);
            }
        }

        public bool Remove(T item)
        {
            var num = IndexOf(item);
            if (num == -1)
            {
                return false;
            }
            DoRemoveAt(num);
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var count = this.Count;
            int num;
            for (var i = 0; i != count; i = num)
            {
                yield return this.DoGetItem(i);
                num = i + 1;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static bool IsT(object value)
        {
            if (value is T)
            {
                return true;
            }
            if (value != null)
            {
                return false;
            }
            return default(T) == null;
        }

        int IList.Add(object value)
        {
            if (value == null && default(T) != null)
            {
                throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            }
            if (!IsT(value))
            {
                throw new ArgumentException("Value is of incorrect type.", nameof(value));
            }
            AddToBack((T)value);
            return Count - 1;
        }

        bool IList.Contains(object value)
        {
            if (!IsT(value))
            {
                return false;
            }
            return ((ICollection<T>)this).Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            if (!IsT(value))
            {
                return -1;
            }
            return IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            if (value == null && default(T) != null)
            {
                throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            }
            if (!IsT(value))
            {
                throw new ArgumentException("Value is of incorrect type.", nameof(value));
            }
            Insert(index, (T)value);
        }

        void IList.Remove(object value)
        {
            if (IsT(value))
            {
                Remove((T)value);
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array), "Destination array cannot be null.");
            }
            CheckRangeArguments(array.Length, index, Count);
            try
            {
                CopyToArray(array, index);
            }
            catch (ArrayTypeMismatchException innerException)
            {
                throw new ArgumentException("Destination array is of incorrect type.", nameof(array), innerException);
            }
            catch (RankException innerException2)
            {
                throw new ArgumentException("Destination array must be single dimensional.", nameof(array), innerException2);
            }
        }

        private static void CheckNewIndexArgument(int sourceLength, int index)
        {
            if (index < 0 || index > sourceLength)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Invalid new index " + index + " for source length " + sourceLength);
            }
        }

        private static void CheckExistingIndexArgument(int sourceLength, int index)
        {
            if (index < 0 || index >= sourceLength)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Invalid existing index " + index + " for source length " + sourceLength);
            }
        }

        private static void CheckRangeArguments(int sourceLength, int offset, int count)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "Invalid offset " + offset);
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Invalid count " + count);
            }
            if (sourceLength - offset < count)
            {
                throw new ArgumentException("Invalid offset (" + offset + ") or count + (" + count + ") for source length " + sourceLength);
            }
        }

        private int DequeIndexToBufferIndex(int index)
        {
            return (index + _offset) % Capacity;
        }

        private T DoGetItem(int index)
        {
            return _buffer[DequeIndexToBufferIndex(index)];
        }

        private void DoSetItem(int index, T item)
        {
            _buffer[DequeIndexToBufferIndex(index)] = item;
        }

        private void DoInsert(int index, T item)
        {
            EnsureCapacityForOneElement();
            if (index == 0)
            {
                DoAddToFront(item);
            }
            else if (index == Count)
            {
                DoAddToBack(item);
            }
            else
            {
                DoInsertRange(index, (IReadOnlyCollection<T>)new T[1]
                {
                item
                });
            }
        }

        private void DoRemoveAt(int index)
        {
            if (index == 0)
            {
                DoRemoveFromFront();
            }
            else if (index == Count - 1)
            {
                DoRemoveFromBack();
            }
            else
            {
                DoRemoveRange(index, 1);
            }
        }

        private int PostIncrement(int value)
        {
            var offset = _offset;
            _offset += value;
            _offset %= Capacity;
            return offset;
        }

        private int PreDecrement(int value)
        {
            _offset -= value;
            if (_offset < 0)
            {
                _offset += Capacity;
            }
            return _offset;
        }

        private void DoAddToBack(T value)
        {
            _buffer[DequeIndexToBufferIndex(Count)] = value;
            var num = ++Count;
        }

        private void DoAddToFront(T value)
        {
            _buffer[PreDecrement(1)] = value;
            var num = ++Count;
        }

        private T DoRemoveFromBack()
        {
            T result = _buffer[DequeIndexToBufferIndex(Count - 1)];
            var num = --Count;
            return result;
        }

        private T DoRemoveFromFront()
        {
            var num = --Count;
            return _buffer[PostIncrement(1)];
        }

        private void DoInsertRange(int index, IReadOnlyCollection<T> collection)
        {
            var count = collection.Count;
            if (index < Count / 2)
            {
                var num = Capacity - count;
                for (var i = 0; i != index; i++)
                {
                    _buffer[DequeIndexToBufferIndex(num + i)] = _buffer[DequeIndexToBufferIndex(i)];
                }
                PreDecrement(count);
            }
            else
            {
                var num2 = Count - index;
                var num3 = index + count;
                for (var num4 = num2 - 1; num4 != -1; num4--)
                {
                    _buffer[DequeIndexToBufferIndex(num3 + num4)] = _buffer[DequeIndexToBufferIndex(index + num4)];
                }
            }
            var num5 = index;
            foreach (var item in collection)
            {
                _buffer[DequeIndexToBufferIndex(num5)] = item;
                num5++;
            }
            Count += count;
        }

        private void DoRemoveRange(int index, int collectionCount)
        {
            if (index == 0)
            {
                PostIncrement(collectionCount);
                Count -= collectionCount;
            }
            else if (index == Count - collectionCount)
            {
                Count -= collectionCount;
            }
            else
            {
                if (index + collectionCount / 2 < Count / 2)
                {
                    for (var num = index - 1; num != -1; num--)
                    {
                        _buffer[DequeIndexToBufferIndex(collectionCount + num)] = _buffer[DequeIndexToBufferIndex(num)];
                    }
                    PostIncrement(collectionCount);
                }
                else
                {
                    var num2 = Count - collectionCount - index;
                    var num3 = index + collectionCount;
                    for (var i = 0; i != num2; i++)
                    {
                        _buffer[DequeIndexToBufferIndex(index + i)] = _buffer[DequeIndexToBufferIndex(num3 + i)];
                    }
                }
                Count -= collectionCount;
            }
        }

        private void EnsureCapacityForOneElement()
        {
            if (IsFull)
            {
                Capacity = ((Capacity == 0) ? 1 : (Capacity * 2));
            }
        }

        public void AddToBack(T value)
        {
            EnsureCapacityForOneElement();
            DoAddToBack(value);
        }

        public void AddToFront(T value)
        {
            EnsureCapacityForOneElement();
            DoAddToFront(value);
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            CheckNewIndexArgument(Count, index);
            var readOnlyCollection = CollectionHelpers.ReifyCollection<T>(collection);
            var count = readOnlyCollection.Count;
            if (count > Capacity - Count)
            {
                Capacity = checked(Count + count);
            }
            if (count != 0)
            {
                DoInsertRange(index, readOnlyCollection);
            }
        }

        public void RemoveRange(int offset, int count)
        {
            CheckRangeArguments(Count, offset, count);
            if (count != 0)
            {
                DoRemoveRange(offset, count);
            }
        }

        public T RemoveFromBack()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("The deque is empty.");
            }
            return DoRemoveFromBack();
        }

        public T RemoveFromFront()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("The deque is empty.");
            }
            return DoRemoveFromFront();
        }

        public void Clear()
        {
            _offset = 0;
            Count = 0;
        }

        public T[] ToArray()
        {
            var array = new T[Count];
            ((ICollection<T>)this).CopyTo(array, 0);
            return array;
        }
    }
}
