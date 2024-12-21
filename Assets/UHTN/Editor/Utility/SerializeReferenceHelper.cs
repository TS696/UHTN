using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace UHTN.Editor.Utility
{
    internal static class SerializeReferenceHelper
    {
        private static readonly Dictionary<Type, List<Type>> typeCache = new();

        public static PopupField<Type> CreateTypePopupField<T>(string label, Type defaultValue, SerializedProperty bindTarget)
        {
            var types = GetTypeList<T>();
            var popupField = new PopupField<Type>(label, types, defaultValue, x =>
            {
                var name = x.Name;
                var index = name.LastIndexOf('.');
                return index == -1 ? name : name.Substring(index + 1);
            }, x => x.FullName);

            if (bindTarget.managedReferenceValue == null)
            {
                bindTarget.managedReferenceValue = Activator.CreateInstance(defaultValue);
                bindTarget.serializedObject.ApplyModifiedProperties();
            }
            popupField.index = types.IndexOf(bindTarget.managedReferenceValue.GetType());
            
            popupField.RegisterValueChangedCallback(evt =>
            {
                bindTarget.managedReferenceValue = Activator.CreateInstance(evt.newValue);
                bindTarget.serializedObject.ApplyModifiedProperties();
            });
            
            return popupField;
        }

        private static List<Type> GetTypeList<T>()
        {
            if (typeCache.TryGetValue(typeof(T), out var types))
            {
                return types;
            }

            var typeList = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(T).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface && !x.IsGenericType)
                .ToList();

            typeCache[typeof(T)] = typeList;
            return typeList;
        }
    }
}
