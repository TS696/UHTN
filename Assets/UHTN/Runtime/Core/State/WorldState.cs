using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

namespace UHTN
{
    public class WorldState : IEquatable<WorldState>
    {
        public enum DirtyReason
        {
            WorldChanged,
            PlanRunner
        }

        private readonly int[] _values;
        public IReadOnlyList<int> Values => _values;
        public int StateLength => _values.Length;
        public event Action<DirtyReason> OnValueChanged;

        public WorldState(int length)
        {
            _values = new int[length];
        }

        public NativeArray<int> ToNativeArray(Allocator allocator)
        {
            var nativeWorldState = new NativeArray<int>(_values.Length, allocator);
            NativeArray<int>.Copy(_values, nativeWorldState, _values.Length);
            return nativeWorldState;
        }

        public void SetValue(int index, int value, DirtyReason dirtyReason = DirtyReason.WorldChanged)
        {
            if (_values[index] != value)
            {
                _values[index] = value;
                OnValueChanged?.Invoke(dirtyReason);
            }
        }

        public void CopyTo(WorldState other)
        {
            Array.Copy(_values, other._values, _values.Length);
        }

        public bool Equals(WorldState other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return _values.SequenceEqual(other._values);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((WorldState)obj);
        }

        public override int GetHashCode()
        {
            return _values != null ? _values.GetHashCode() : 0;
        }
    }
}
