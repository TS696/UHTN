using System;
using System.Linq;
using UHTN.DomainAssets;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace UHTN.Editor.DomainAssets
{
    public class TaskListGUI : VisualElement
    {
        private readonly DomainAsset _target;
        private readonly SerializedObject _serializedObject;
        private SerializedProperty _taskAssetsProperty;
        public int TaskCount => _taskAssetsProperty.arraySize;

        public TaskListGUI(DomainAsset target, SerializedObject serializedObject)
        {
            _target = target;
            _serializedObject = serializedObject;

            var tasksFoldout = new Foldout();
            tasksFoldout.text = "Tasks";
            Add(tasksFoldout);

            _taskAssetsProperty = serializedObject.FindProperty("_taskAssets");
            var taskAssetsRoot = new VisualElement();
            tasksFoldout.Add(taskAssetsRoot);

            RefreshTaskAssets(taskAssetsRoot, _taskAssetsProperty);

            var addTaskButton = new Button();
            addTaskButton.style.marginTop = 5;
            addTaskButton.style.width = 200;
            addTaskButton.style.alignSelf = Align.Center;
            addTaskButton.text = "Add Task";
            addTaskButton.clicked += () =>
            {
                var dropdown = new TaskDropdown(new AdvancedDropdownState());
                dropdown.Show(addTaskButton.worldBound);
                dropdown.OnItemSelected += taskType =>
                {
                    AddTask(taskType, taskType.Name);
                    RefreshTaskAssets(taskAssetsRoot, _taskAssetsProperty);
                };
            };
            tasksFoldout.Add(addTaskButton);

            this.TrackPropertyValue(_taskAssetsProperty, _ =>
            {
                RefreshTaskAssets(taskAssetsRoot, _taskAssetsProperty);
            });
        }

        public void AddTask(Type taskType, string name)
        {
            var taskAsset = ScriptableObject.CreateInstance(taskType) as TaskAsset;
            taskAsset.name = name;
            taskAsset.SetDomain(_target);

            AssetDatabase.AddObjectToAsset(taskAsset, _target);
            EditorUtility.SetDirty(_target);
            AssetDatabase.SaveAssets();
        }

        private void RefreshTaskAssets(VisualElement root, SerializedProperty taskAssetsProperty)
        {
            root.Clear();
            var assetPath = AssetDatabase.GetAssetPath(_target);
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<TaskAsset>()
                .OrderByDescending(x => x is RootCompoundTaskAsset)
                .ToArray();
            taskAssetsProperty.arraySize = assets.Length;

            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                var property = taskAssetsProperty.GetArrayElementAtIndex(i);
                property.objectReferenceValue = asset;

                var taskControlRoot = new VisualElement();
                taskControlRoot.style.flexDirection = FlexDirection.Row;

                var taskNameField = new TextField();
                taskNameField.isDelayed = true;
                taskNameField.style.minWidth = 100;
                taskNameField.value = asset.name;
                taskNameField.SetEnabled(asset is not RootCompoundTaskAsset);
                taskControlRoot.Add(taskNameField);
                taskNameField.RegisterValueChangedCallback(evt =>
                {
                    asset.name = evt.newValue;
                    EditorUtility.SetDirty(_target);
                    AssetDatabase.SaveAssets();
                });

                var taskField = new ObjectField();
                taskField.style.flexShrink = 1;
                taskField.style.flexGrow = 1;
                taskField.SetEnabled(false);
                taskField.objectType = typeof(TaskAsset);
                taskField.value = asset;
                taskControlRoot.Add(taskField);

                var removeButton = new Button(() =>
                {
                    AssetDatabase.RemoveObjectFromAsset(asset);
                    EditorUtility.SetDirty(_target);
                    AssetDatabase.SaveAssets();

                    RefreshTaskAssets(root, taskAssetsProperty);
                });
                removeButton.text = "Remove";
                removeButton.SetEnabled(asset is not RootCompoundTaskAsset);
                taskControlRoot.Add(removeButton);

                root.Add(taskControlRoot);
            }

            _serializedObject.ApplyModifiedProperties();
        }
    }
}
