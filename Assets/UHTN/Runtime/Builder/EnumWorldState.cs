using System;

namespace UHTN.Builder
{
    public class EnumWorldState<T> : WorldState where T : Enum
    {
        public EnumWorldState(int stateLength) : base(stateLength)
        {
        }

        public void SetInt(T state, int value)
        {
            SetValue((int)(object)state, value);
        }

        public void SetBool(T state, bool value)
        {
            SetValue((int)(object)state, value ? 1 : 0);
        }

        public void SetEnum<TU>(T state, TU value) where TU : Enum
        {
            SetValue((int)(object)state, (int)(object)value);
        }

        public int GetInt(T state)
        {
            return Values[(int)(object)state];
        }

        public TU GetEnum<TU>(T state) where TU : Enum
        {
            return (TU)(object)GetInt(state);
        }

        public bool GetBool(T state)
        {
            return GetInt(state) != 0;
        }
    }
}
