using System;
using System.Reflection;

namespace UHTN.Builder
{
    public class EnumWorldState<T> : WorldState where T : Enum
    {
        public EnumWorldState(WorldStateDescription description) : base(description)
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

        public static WorldStateDescription CreateDescription()
        {
            var names = Enum.GetNames(typeof(T));
            var fieldDescList = new WorldStateDescription.FieldDesc[names.Length];

            for (var i = 0; i < names.Length; i++)
            {
                IWsFieldType stateType = WsFieldInt.Instance;

                var fieldInfo = typeof(T).GetField(names[i]);
                var hint = fieldInfo.GetCustomAttribute<WsFieldHintAttribute>();
                if (hint != null)
                {
                    stateType = (IWsFieldType)Activator.CreateInstance(hint.Type);
                }

                fieldDescList[i] = new WorldStateDescription.FieldDesc(names[i], stateType);
            }

            return new WorldStateDescription(fieldDescList);
        }
    }
}
