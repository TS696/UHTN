using System;
using UnityEngine.UIElements;

namespace UHTN.Editor.DomainAssets
{
    [WsFieldEditor(typeof(WsFieldBool))]
    public class WsFieldBoolEditor : IWsFieldEditor
    {
        public VisualElement CreateGUI(int defaultValue, Action<int> onValueChanged)
        {
            var toggle = new Toggle();
            toggle.value = defaultValue == 1;
            toggle.RegisterValueChangedCallback(e => onValueChanged(e.newValue ? 1 : 0));
            return toggle;
        }
    }
}
