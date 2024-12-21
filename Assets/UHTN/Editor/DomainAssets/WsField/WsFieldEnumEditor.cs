using System;
using UnityEngine.UIElements;

namespace UHTN.Editor.DomainAssets
{
    [WsFieldEditor(typeof(WsFieldEnum<>), true)]
    public class WsFieldEnumEditor<T> : IWsFieldEditor where T : Enum
    {
        public VisualElement CreateGUI(int defaultValue, Action<int> onValueChanged)
        {
            var field = new EnumField(Enum.GetValues(typeof(T)).GetValue(0) as Enum);
            field.value = (T)(object)defaultValue;
            field.RegisterValueChangedCallback(e => onValueChanged((int)(object)e.newValue));
            return field;
        }
    }
}
