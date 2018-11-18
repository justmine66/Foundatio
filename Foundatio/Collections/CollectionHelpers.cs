﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Foundatio.Collections
{
    internal static class CollectionHelpers
    {
        private sealed class NongenericCollectionWrapper<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
        {
            private readonly ICollection _collection;

            public int Count => _collection.Count;

            public NongenericCollectionWrapper(ICollection collection)
            {
                if (collection == null)
                {
                    throw new ArgumentNullException("collection");
                }
                _collection = collection;
            }

            public IEnumerator<T> GetEnumerator()
            {
                foreach (T item in this._collection)
                {
                    yield return item;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _collection.GetEnumerator();
            }
        }

        private sealed class CollectionWrapper<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
        {
            private readonly ICollection<T> _collection;

            public int Count => _collection.Count;

            public CollectionWrapper(ICollection<T> collection)
            {
                if (collection == null)
                {
                    throw new ArgumentNullException("collection");
                }
                _collection = collection;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _collection.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _collection.GetEnumerator();
            }
        }

        public static IReadOnlyCollection<T> ReifyCollection<T>(IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            IReadOnlyCollection<T> result;
            if ((result = (source as IReadOnlyCollection<T>)) != null)
            {
                return result;
            }
            ICollection<T> collection;
            if ((collection = (source as ICollection<T>)) != null)
            {
                return new CollectionWrapper<T>(collection);
            }
            ICollection collection2;
            if ((collection2 = (source as ICollection)) != null)
            {
                return new NongenericCollectionWrapper<T>(collection2);
            }
            return new List<T>(source);
        }
    }
}