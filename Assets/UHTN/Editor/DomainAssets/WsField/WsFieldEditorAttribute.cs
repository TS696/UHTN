using System;

namespace UHTN.Editor.DomainAssets
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WsFieldEditorAttribute : Attribute
    {
        public Type WsFieldType { get; }
        public bool IsGeneric { get; }
        
        public WsFieldEditorAttribute(Type wsFieldType, bool isGeneric = false)
        {
            WsFieldType = wsFieldType;
            IsGeneric = isGeneric;
        }
    }
}
