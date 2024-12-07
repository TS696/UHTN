using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace UHTN.Editor.PlanViewer
{
    public class PlanRunnerTreeView : TreeView
    {
        public event Action<PlanRunner> OnSelect;

        private readonly IReadOnlyList<PlanRunner> _planRunners;

        public PlanRunnerTreeView(IReadOnlyList<PlanRunner> planRunners, TreeViewState state) : base(state)
        {
            _planRunners = planRunners;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(0, -1, "");
            var header = new TreeViewItem(1, 0, "PlanRunners");
            SetExpanded(1, true);
            root.AddChild(header);

            for (var i = 0; i < _planRunners.Count; i++)
            {
                var treeViewItem = new TreeViewItem(i + 2, 1, _planRunners[i].Name);
                header.AddChild(treeViewItem);
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

            var planRunner = _planRunners[id - 2];
            OnSelect?.Invoke(planRunner);
        }
    }
}
