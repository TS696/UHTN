using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UHTN.Editor.DomainAssets
{
    public class WorldStateListGUI : VisualElement
    {
        private readonly SerializedProperty _worldStatesProperty;

        public WorldStateListGUI(SerializedObject serializedObject)
        {
            var worldStatesProperty = serializedObject.FindProperty("_worldStates");

            var foldout = new Foldout();
            foldout.text = "World States";

            var listView = new ListView();
            listView.BindProperty(worldStatesProperty);
            listView.reorderable = true;
            listView.allowAdd = true;
            listView.allowRemove = true;
            listView.showBoundCollectionSize = false;
            listView.showAddRemoveFooter = true;

            listView.onAdd += _ =>
            {
                worldStatesProperty.arraySize++;
                var property = worldStatesProperty.GetArrayElementAtIndex(worldStatesProperty.arraySize - 1);
                property.FindPropertyRelative("_name").stringValue = "New State";
                property.FindPropertyRelative("_fieldType").managedReferenceValue = null;
                property.FindPropertyRelative("_sensorCreator").managedReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
            };
            
            foldout.Add(listView);

            Add(foldout);
        }
    }
}
