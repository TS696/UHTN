using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UHTN.Editor.DomainAssets
{
    public static class WsFieldEditorExtensions
    {
        private static readonly Dictionary<Type, Type> _typeCache = new();

        public static IWsFieldEditor GetEditor<T>(this T self) where T : IWsFieldType
        {
            var fieldType = self.GetType();
            if (!_typeCache.TryGetValue(fieldType, out var result))
            {
                result = AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .Where(x =>
                    {
                        var attribute = x.GetCustomAttribute<WsFieldEditorAttribute>();
                        var match = attribute != null && attribute.WsFieldType.IsAssignableFrom(fieldType);
                        var genericMatch = attribute != null && attribute.IsGeneric && fieldType.IsDerivedFromGenericType(attribute.WsFieldType);
                        return match || genericMatch;
                    })
                    .Select(x =>
                    {
                        var attribute = x.GetCustomAttribute<WsFieldEditorAttribute>();
                        if (attribute.IsGeneric)
                        {
                            while (fieldType.BaseType != null)
                            {
                                fieldType = fieldType.BaseType;
                                if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == attribute.WsFieldType)
                                {
                                    break;
                                }
                            }

                            var genericArgument = fieldType.GetGenericArguments()[0];
                            return x.MakeGenericType(genericArgument);
                        }

                        return x;
                    })
                    .FirstOrDefault();
                _typeCache[fieldType] = result;
            }

            return (IWsFieldEditor)Activator.CreateInstance(result);
        }

        private static bool IsDerivedFromGenericType(this Type type, Type genericType)
        {
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
                    return true;
                type = type.BaseType;
            }

            return false;
        }
    }
}
