using System;
using System.Linq;
using System.Reflection;
using UHTN.DomainAssets;
using UnityEditor.IMGUI.Controls;

namespace UHTN.Editor.DomainAssets
{
    public class SensorDropdown : AdvancedDropdown
    {
        private class SensorDropdownItem : AdvancedDropdownItem
        {
            public Type Type { get; }

            public SensorDropdownItem(string name, Type type) : base(name)
            {
                Type = type;
            }
        }

        public event Action<Type> OnItemSelected;
        private Type _wsFieldType;

        public SensorDropdown(AdvancedDropdownState state, Type wsFieldType) : base(state)
        {
            minimumSize = new UnityEngine.Vector2(200, 300);
            _wsFieldType = wsFieldType;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Sensor");

            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(x => x.GetCustomAttribute<SensorCreatorAttribute>() != null)
                .Select(x => (attr: x.GetCustomAttribute<SensorCreatorAttribute>(), type: x))
                .Where(x => x.attr.WsFieldType == _wsFieldType)
                .ToList()
                .ForEach(tuple =>
                {
                    var parent = root;
                    var splitName = tuple.attr.Name.Split('/');
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
                            item = new SensorDropdownItem(str, tuple.type);
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
            OnItemSelected?.Invoke((item as SensorDropdownItem).Type);
        }
    }
}
