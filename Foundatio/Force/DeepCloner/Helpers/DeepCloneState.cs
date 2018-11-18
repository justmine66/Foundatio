using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Foundatio.Force.DeepCloner.Helpers
{
    internal class DeepCloneState
    {
        private class CustomEqualityComparer : IEqualityComparer<object>, IEqualityComparer
        {
            bool IEqualityComparer<object>.Equals(object x, object y)
            {
                return x == y;
            }

            bool IEqualityComparer.Equals(object x, object y)
            {
                return x == y;
            }

            public int GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }

        private static readonly CustomEqualityComparer Instance = new CustomEqualityComparer();

        private Dictionary<object, object> _loops;

        private readonly object[] _baseFromTo = new object[6];

        private int _idx;

        public object GetKnownRef(object from)
        {
            object[] baseFromTo = _baseFromTo;
            if (from == baseFromTo[0])
            {
                return baseFromTo[3];
            }
            if (from == baseFromTo[1])
            {
                return baseFromTo[4];
            }
            if (from == baseFromTo[2])
            {
                return baseFromTo[5];
            }
            if (_loops == null)
            {
                return null;
            }
            if (_loops.TryGetValue(from, out object value))
            {
                return value;
            }
            return null;
        }

        public void AddKnownRef(object from, object to)
        {
            if (_idx < 3)
            {
                _baseFromTo[_idx] = from;
                _baseFromTo[_idx + 3] = to;
                _idx++;
            }
            else
            {
                if (_loops == null)
                {
                    _loops = new Dictionary<object, object>(Instance);
                }
                _loops[from] = to;
            }
        }
    }
}
