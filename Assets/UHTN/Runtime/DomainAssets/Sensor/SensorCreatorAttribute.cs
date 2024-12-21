using System;

namespace UHTN.DomainAssets
{
    public class SensorCreatorAttribute : Attribute
    {
        public string Name { get; }
        public Type WsFieldType { get; }

        public SensorCreatorAttribute(string name, Type wsFieldType)
        {
            Name = name;
            WsFieldType = wsFieldType;
        }
    }
}
