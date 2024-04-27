using System;

namespace UHTN
{
    public class WsFieldEnum<T> : IWsFieldType where T : Enum
    {
        public string ToDisplayString(int value)
        {
            return Enum.GetName(typeof(T), value);
        }
    }
}
