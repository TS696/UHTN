using UHTN.DomainAssets;
using UnityEditor;
using UnityEngine.UIElements;

namespace UHTN.Editor.DomainAssets
{
    [CustomEditor(typeof(DomainAsset))]
    public class DomainAssetEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var worldStatesGUI = new WorldStateListGUI(serializedObject);
            root.Add(worldStatesGUI);

            var sensorContainerGUI = new SensorListGUI(serializedObject);
            root.Add(sensorContainerGUI);


            var taskListGUI = new TaskListGUI(target as DomainAsset, serializedObject);
            root.Add(taskListGUI);

            if (taskListGUI.TaskCount <= 0 && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(target)))
            {
                taskListGUI.AddTask(typeof(RootCompoundTaskAsset), "RootTask");
            }

            return root;
        }
    }
}
