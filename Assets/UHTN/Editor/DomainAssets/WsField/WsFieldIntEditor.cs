using System;
using UnityEngine.UIElements;

namespace UHTN.Editor.DomainAssets
{
    [WsFieldEditor(typeof(WsFieldInt))]
    public class WsFieldIntEditor : IWsFieldEditor
    {
        public VisualElement CreateGUI(int defaultValue, Action<int> onValueChanged)
        {
            var intField = new IntegerField();
            intField.style.minWidth = 50;
            intField.value = defaultValue;
            intField.RegisterValueChangedCallback(e => onValueChanged(e.newValue));
            return intField;
        }
    }
}
