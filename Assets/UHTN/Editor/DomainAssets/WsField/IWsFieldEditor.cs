using System;
using UnityEngine.UIElements;

namespace UHTN.Editor.DomainAssets
{
    public interface IWsFieldEditor
    {
        VisualElement CreateGUI(int defaultValue, Action<int> onValueChanged);
    }
}
