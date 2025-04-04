using System.Linq;
using UHTN.DomainAssets;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UHTN.Editor.DomainAssets
{
    [CustomPropertyDrawer(typeof(StateEffectField))]
    public class StateEffectFieldDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var domainAsset = (property.serializedObject.targetObject as IDomainAssetContent)?.Domain;
            if (domainAsset == null)
            {
                return base.CreatePropertyGUI(property);
            }

            var worldStateNames = domainAsset.WorldStates.Select(x => x.Name).ToList();

            var rootElement = new VisualElement();
            rootElement.style.flexDirection = FlexDirection.Row;

            var stateNameProperty = property.FindPropertyRelative("_stateName");
            var operatorProperty = property.FindPropertyRelative("_operator");
            var valueProperty = property.FindPropertyRelative("_value");
            var effectTypeProperty = property.FindPropertyRelative("_type");

            var stateNamePopup = new PopupField<string>();
            stateNamePopup.choices = worldStateNames;
            stateNamePopup.BindProperty(stateNameProperty);
            rootElement.Add(stateNamePopup);

            var operatorPopup = new PopupField<StateEffectOperator>();
            operatorPopup.formatSelectedValueCallback = op => op.ToDisplayString();
            operatorPopup.formatListItemCallback = op => op.ToDisplayString();
            rootElement.Add(operatorPopup);

            var wsFieldElement = new WsFieldElement();
            rootElement.Add(wsFieldElement);

            var effectTypeField = new EnumField(StateEffectType.PlanAndExecute);
            effectTypeField.BindProperty(effectTypeProperty);
            effectTypeField.style.marginLeft = 20;
            rootElement.Add(effectTypeField);

            stateNamePopup.RegisterValueChangedCallback(evt =>
            {
                stateNameProperty.stringValue = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();
                OnUpdateStateName(evt.newValue);
            });

            operatorPopup.RegisterValueChangedCallback(evt =>
            {
                operatorProperty.enumValueIndex = (int)evt.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });

            wsFieldElement.OnValueChanged += v =>
            {
                valueProperty.intValue = v;
                property.serializedObject.ApplyModifiedProperties();
            };

            OnUpdateStateName(stateNameProperty.stringValue);

            return rootElement;

            void OnUpdateStateName(string stateName)
            {
                var stateField = domainAsset.WorldStates.FirstOrDefault(x => x.Name == stateName);
                if (stateField == null)
                {
                    return;
                }

                operatorPopup.choices = stateField.FieldType.ApplicableEffectOperators.ToList();
                operatorPopup.index =
                    operatorPopup.choices.IndexOf((StateEffectOperator)operatorProperty.enumValueIndex);

                var fieldEditor = stateField.FieldType.GetEditor();
                wsFieldElement.SetEditor(valueProperty.intValue, fieldEditor);
            }
        }
    }
}
