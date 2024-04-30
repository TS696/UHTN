using System;

namespace UHTN.Builder
{
    [AttributeUsage(AttributeTargets.Field)]
    public class WsFieldHintAttribute : Attribute
    {
        public Type Type { get; }

        public WsFieldHintAttribute(Type type)
        {
            if (!typeof(IWsFieldType).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Type must implement {nameof(IWsFieldType)}");
            }

            Type = type;
        }
    }
}
