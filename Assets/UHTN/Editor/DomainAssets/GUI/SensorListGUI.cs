using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UHTN.Editor.DomainAssets
{
    public class SensorListGUI : VisualElement
    {
        private readonly SerializedProperty _worldStatesProperty;
        private readonly Foldout _foldout;

        public SensorListGUI(SerializedObject serializedObject)
        {
            _worldStatesProperty = serializedObject.FindProperty("_worldStates");

            _foldout = new Foldout();
            _foldout.text = "Sensors";
            _foldout.BindProperty(_worldStatesProperty);
            Add(_foldout);

            Refresh();
            this.TrackPropertyValue(_worldStatesProperty, _ => Refresh());
        }

        private void Refresh()
        {
            _foldout.Clear();

            for (var i = 0; i < _worldStatesProperty.arraySize; i++)
            {
                var worldStateProperty = _worldStatesProperty.GetArrayElementAtIndex(i);
                var worldStateName = worldStateProperty.FindPropertyRelative("_name").stringValue;
                var wsFieldType = worldStateProperty.FindPropertyRelative("_fieldType").managedReferenceValue;
                var sensorCreatorProperty = worldStateProperty.FindPropertyRelative("_sensorCreator");

                var worldStateFoldout = new Foldout();
                worldStateFoldout.text = worldStateName;
                worldStateFoldout.style.marginBottom = 5;
                _foldout.Add(worldStateFoldout);

                var sensorGUIContainer = new IMGUIContainer(() =>
                {
                    if (sensorCreatorProperty.managedReferenceValue == null)
                    {
                        return;
                    }

                    using var verticalScope = new EditorGUILayout.VerticalScope(GUI.skin.box);
                    EditorGUILayout.PropertyField(sensorCreatorProperty, new GUIContent(sensorCreatorProperty.managedReferenceValue.GetType().Name),
                        true);
                    sensorCreatorProperty.serializedObject.ApplyModifiedProperties();
                });
                sensorGUIContainer.style.display = sensorCreatorProperty.managedReferenceValue != null ? DisplayStyle.Flex : DisplayStyle.None;
                worldStateFoldout.Add(sensorGUIContainer);

                var addButton = new Button();
                addButton.text = "Add Sensor";
                addButton.style.width = 200;
                addButton.style.alignSelf = Align.Center;
                addButton.style.display = sensorCreatorProperty.managedReferenceValue != null ? DisplayStyle.None : DisplayStyle.Flex;
                addButton.clicked += () =>
                {
                    var dropdown = new SensorDropdown(new AdvancedDropdownState(), wsFieldType.GetType());
                    dropdown.Show(addButton.worldBound);
                    dropdown.OnItemSelected += (sensorType) =>
                    {
                        var sensorCreatorInstance = Activator.CreateInstance(sensorType);
                        sensorCreatorProperty.managedReferenceValue = sensorCreatorInstance;
                        sensorCreatorProperty.serializedObject.ApplyModifiedProperties();
                    };
                };
                worldStateFoldout.Add(addButton);

                var removeButton = new Button(() =>
                {
                    sensorCreatorProperty.managedReferenceValue = null;
                    sensorCreatorProperty.serializedObject.ApplyModifiedProperties();
                });
                removeButton.text = "Remove";
                worldStateFoldout.Add(removeButton);
                removeButton.style.alignSelf = Align.FlexEnd;
                removeButton.style.display = sensorCreatorProperty.managedReferenceValue != null ? DisplayStyle.Flex : DisplayStyle.None;

                worldStateFoldout.TrackPropertyValue(sensorCreatorProperty, prop =>
                {
                    sensorGUIContainer.style.display = prop.managedReferenceValue != null ? DisplayStyle.Flex : DisplayStyle.None;
                    addButton.style.display = prop.managedReferenceValue != null ? DisplayStyle.None : DisplayStyle.Flex;
                    removeButton.style.display = prop.managedReferenceValue != null ? DisplayStyle.Flex : DisplayStyle.None;
                });
            }

            var spacer = new VisualElement();
            spacer.style.height = 10;
            _foldout.Add(spacer);
        }
    }
}
