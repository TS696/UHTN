using System.Collections.Generic;
using System.Linq;
using UHTN.Agent;
using UnityEngine;

namespace UHTN.DomainAssets
{
    [CreateAssetMenu(fileName = "DomainAsset", menuName = "UHTN/DomainAsset")]
    public class DomainAsset : ScriptableObject, IDomainAssetContent
    {
        DomainAsset IDomainAssetContent.Domain => this;

        [SerializeField]
        private WorldStateField[] _worldStates;

        public WorldStateField[] WorldStates => _worldStates;

        [SerializeField]
        private TaskAsset[] _taskAssets;

        public TaskAsset[] TaskAssets => _taskAssets;

        public Domain ResolveDomain(object userData = null)
        {
            if (TaskAssets.Length == 0)
            {
                Debug.LogError("No tasks found in domain asset.");
                return null;
            }

            var wsFieldDescList = WorldStates.Select(x => new WorldStateDescription.FieldDesc(x.Name, x.FieldType));
            var worldStateDesc = new WorldStateDescription(wsFieldDescList.ToArray());
            var domainBuilder = new DomainBuilderCore(worldStateDesc);

            var resolver = new DomainAssetResolver(_taskAssets, worldStateDesc, domainBuilder, userData);
            var rootTask = TaskAssets[0];

            rootTask.Resolve(resolver);
            return domainBuilder.Resolve();
        }

        public void ResolveSensor(SensorContainer sensorContainer, object userData)
        {
            for (var i = 0; i < _worldStates.Length; i++)
            {
                sensorContainer.AddSensor(i, _worldStates[i].SensorCreator.CreateSensor(userData));
            }
        }

        public class DomainAssetResolver
        {
            public WorldStateDescription WorldStateDescription { get; }
            public object UserData { get; }

            private readonly Dictionary<string, ITask> _resolvedTasks = new();
            private readonly DomainBuilderCore _domainBuilder;
            private readonly TaskAsset[] _taskAssets;

            public DomainAssetResolver(TaskAsset[] taskAssets, WorldStateDescription worldStateDescription, DomainBuilderCore domainBuilder,
                object userData)
            {
                _taskAssets = taskAssets;
                WorldStateDescription = worldStateDescription;
                _domainBuilder = domainBuilder;
                UserData = userData;
            }

            public ITask ResolveSubTask(SubTaskField subTask)
            {
                if (TryGetResolvedTask(subTask.TaskName, out var task))
                {
                    return task;
                }

                var taskAsset = _taskAssets.FirstOrDefault(x => x.name == subTask.TaskName);
                if (taskAsset == null)
                {
                    Debug.LogError($"Task {subTask.TaskName} not found in domain asset.");
                    return null;
                }

                taskAsset.Resolve(this);
                return _resolvedTasks[subTask.TaskName];
            }

            public void AddResolvedTask(string name, ITask task)
            {
                _domainBuilder.AddTask(task);
                _resolvedTasks.Add(name, task);
            }

            public bool TryGetResolvedTask(string name, out ITask task)
            {
                return _resolvedTasks.TryGetValue(name, out task);
            }
        }
    }
}
