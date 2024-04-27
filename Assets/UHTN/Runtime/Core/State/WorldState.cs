using System;
using System.Collections.Generic;
using Unity.Collections;

namespace UHTN
{
    public class WorldState
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
        public WorldStateDescription Description { get; }

        public WorldState(WorldStateDescription description)
        {
            _values = new int[description.StateLength];
            Description = description;
        }

        public NativeArray<int> ToNativeArray(Allocator allocator)
        {
            var nativeWorldState = new NativeArray<int>(_values.Length, allocator);
            NativeArray<int>.Copy(_values, nativeWorldState, _values.Length);
            return nativeWorldState;
        }

        public void SetValue(int index, int value, DirtyReason dirtyReason = DirtyReason.WorldChanged)
        {
            if (!Description.GetStateType(index).Validate(value))
            {
                throw new ArgumentException(
                    $"Invalid value '{value}' for the '{Description.GetStateName(index)}' state of the '{Description.GetStateType(index).GetType()}' type.");
            }

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
    }
}
