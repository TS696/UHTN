using System;
using UnityEngine.UIElements;

namespace UHTN.Editor.DomainAssets
{
    public class WsFieldElement : VisualElement
    {
        public Action<int> OnValueChanged { get; set; }
        
        public void SetEditor(int value, IWsFieldEditor editor)
        {
            Clear();
            var element = editor.CreateGUI(value, OnValueChanged);
            Add(element);
        }
    }
}
