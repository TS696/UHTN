using System.Linq;
using UHTN.DomainAssets;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UHTN.Editor.DomainAssets
{
    [CustomPropertyDrawer(typeof(SubTaskField))]
    public class SubTaskFieldDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var domainAsset = (property.serializedObject.targetObject as IDomainAssetContent)?.Domain;
            if (domainAsset == null)
            {
                return base.CreatePropertyGUI(property);
            }
            var taskNames = domainAsset.TaskAssets.Select(x => x.name).ToList();

            var rootElement = new VisualElement();

            var taskNameProperty = property.FindPropertyRelative("_taskName");
            var decompositionTimingProperty = property.FindPropertyRelative("_decompositionTiming");

            var taskNameField = new PopupField<string>();
            taskNameField.choices = taskNames;
            taskNameField.BindProperty(taskNameProperty);
            rootElement.Add(taskNameField);

            var decompositionTimingField = new EnumField("DecompositionTiming", DecompositionTiming.Immediate);
            decompositionTimingField.BindProperty(decompositionTimingProperty);
            rootElement.Add(decompositionTimingField);

            taskNameField.RegisterValueChangedCallback(evt =>
            {
                OnTaskNameChanged(evt.newValue);
            });
            
            OnTaskNameChanged(taskNameProperty.stringValue);

            return rootElement;

            void OnTaskNameChanged(string taskName)
            {
                var taskAsset = domainAsset.TaskAssets.FirstOrDefault(x => x.name == taskName);
                if (taskAsset == null)
                {
                    decompositionTimingField.style.display = DisplayStyle.None;
                    return;
                }
                
                decompositionTimingField.style.display = taskAsset is CompoundTaskAssetBase? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
