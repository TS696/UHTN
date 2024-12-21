using System;

namespace UHTN.DomainAssets
{
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class TaskAssetAttribute : Attribute
    {
        public string Name { get; }

        protected TaskAssetAttribute(string name)
        {
            Name = name;
        }
    }
}
