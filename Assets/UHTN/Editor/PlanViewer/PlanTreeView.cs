using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UHTN.Editor.PlanViewer
{
    public class PlanTreeView : TreeView
    {
        public event Action<ITask> OnSelect;

        private readonly PlanRunner _planRunner;
        private readonly Dictionary<int, ITask> _taskDictionary = new();

        public PlanTreeView(PlanRunner planRunner, TreeViewState state) : base(state)
        {
            _planRunner = planRunner;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            useScrollView = true;
        }

        protected override TreeViewItem BuildRoot()
        {
            _taskDictionary.Clear();

            var root = new TreeViewItem(0, -1, "");
            var header = new TreeViewItem(1, 0, "Plan");
            SetExpanded(1, true);

            root.AddChild(header);
            if (_planRunner == null || _planRunner.Processes.Count <= 0)
            {
                return root;
            }

            var id = 2;

            var current = header;
            var lastDepth = 0;
            foreach (var (task, depth, isCompleted) in _planRunner.IterateSchedule())
            {
                var iconName = isCompleted ? "TestPassed" : "TestNormal";
                var icon = EditorGUIUtility.Load(iconName) as Texture2D;
                var item = new TreeViewItem(id, depth, task.Name);
                item.icon = icon;

                _taskDictionary.Add(id, task);
                id++;

                if (lastDepth > depth)
                {
                    for (var i = 0; i < lastDepth - depth; i++)
                    {
                        current = current.parent;
                    }
                }

                current.AddChild(item);

                if (lastDepth < depth)
                {
                    current = item;
                }

                lastDepth = depth;
            }

            return root;
        }

        protected override bool CanChangeExpandedState(TreeViewItem item)
        {
            if (item.id <= 1)
            {
                return false;
            }

            return base.CanChangeExpandedState(item);
        }

        protected override void SingleClickedItem(int id)
        {
            if (id <= 1)
            {
                return;
            }

            var task = _taskDictionary[id];
            OnSelect?.Invoke(task);
        }
    }
}
