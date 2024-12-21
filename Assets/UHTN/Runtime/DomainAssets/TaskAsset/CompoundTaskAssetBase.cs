using UnityEngine;

namespace UHTN.DomainAssets
{
    public class CompoundTaskAssetBase : TaskAsset
    {
        [SerializeField]
        private MethodField[] _methods;

        public override void Resolve(DomainAsset.DomainAssetResolver resolver)
        {
            var compoundTask = new CompoundTask(name);
            resolver.AddResolvedTask(name, compoundTask);

            foreach (var method in _methods)
            {
                var methodInstance = new Method();

                foreach (var precondition in method.Preconditions)
                {
                    var index = resolver.WorldStateDescription.GetStateIndex(precondition.StateName);
                    methodInstance.Preconditions.Add(new ConditionToDecompose(index,
                        new StateCondition(precondition.Operator, precondition.Value)));
                }

                foreach (var subTask in method.SubTasks)
                {
                    var subTaskInstance = resolver.ResolveSubTask(subTask);
                    methodInstance.SubTasks.Add(new SubTask(subTaskInstance, subTask.DecompositionTiming));
                }

                compoundTask.Methods.Add(methodInstance);
            }
        }
    }
}
