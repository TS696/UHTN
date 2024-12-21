using System;
using System.Linq;
using System.Reflection;
using UHTN.DomainAssets;
using UnityEditor.IMGUI.Controls;

namespace UHTN.Editor.DomainAssets
{
    public class TaskDropdown : AdvancedDropdown
    {
        private class TaskDropdownItem : AdvancedDropdownItem
        {
            public Type Type { get; }

            public TaskDropdownItem(string name, Type type) : base(name)
            {
                Type = type;
            }
        }

        public event Action<Type> OnItemSelected;

        public TaskDropdown(AdvancedDropdownState state) : base(state)
        {
            minimumSize = new UnityEngine.Vector2(200, 300);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Task");

            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(x => x.GetCustomAttribute<TaskAssetAttribute>() != null)
                .Select(x => (attr: x.GetCustomAttribute<TaskAssetAttribute>(), type: x))
                .ToList()
                .ForEach(tuple =>
                {
                    var parent = root;
                    var splitName = tuple.attr.Name.Split('/');
                    switch (tuple.attr)
                    {
                        case PrimitiveTaskAssetAttribute _:
                            splitName = splitName.Prepend("PrimitiveTask").ToArray();
                            break;
                        case CompoundTaskAssetAttribute _:
                            splitName = splitName.Prepend("CompoundTask").ToArray();
                            break;
                    }
                    
                    for (var i = 0; i < splitName.Length; i++)
                    {
                        var str = splitName[i];
                        var foundChild = parent.children.FirstOrDefault(x => x.name == str);
                        if (foundChild != null)
                        {
                            parent = foundChild;
                            continue;
                        }

                        AdvancedDropdownItem item = null;
                        if (i == splitName.Length - 1)
                        {
                            item = new TaskDropdownItem(str, tuple.type);
                        }
                        else
                        {
                            item = new AdvancedDropdownItem(str);
                        }

                        parent.AddChild(item);
                        parent = item;
                    }
                });

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            OnItemSelected?.Invoke((item as TaskDropdownItem).Type);
        }
    }
}
