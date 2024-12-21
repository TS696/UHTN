using UHTN.DomainAssets;
using UHTN.Editor.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UHTN.Editor.DomainAssets
{
    [CustomPropertyDrawer(typeof(WorldStateField))]
    public class WorldStateFieldDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var rootElement = new VisualElement();
            rootElement.style.flexDirection = FlexDirection.Row;

            var nameProperty = property.FindPropertyRelative("_name");
            var fieldTypeProperty = property.FindPropertyRelative("_fieldType");

            var nameField = new TextField();
            nameField.style.minWidth = 100;
            nameField.BindProperty(nameProperty);
            rootElement.Add(nameField);

            var fieldTypeField =
                SerializeReferenceHelper.CreateTypePopupField<IWsFieldType>("", typeof(WsFieldInt), fieldTypeProperty);
            rootElement.Add(fieldTypeField);

            return rootElement;
        }
    }
}
