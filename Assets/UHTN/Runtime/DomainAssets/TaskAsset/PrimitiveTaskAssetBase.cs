using UnityEngine;

namespace UHTN.DomainAssets
{
    public abstract class PrimitiveTaskAssetBase : TaskAsset
    {
        [SerializeField]
        private StateConditionField[] _preconditions;

        public StateConditionField[] Preconditions => _preconditions;

        [SerializeField]
        private StateEffectField[] _effects;

        public StateEffectField[] Effects => _effects;

        public override void Resolve(DomainAsset.DomainAssetResolver resolver)
        {
            var primitiveTask = new PrimitiveTask(name);

            foreach (var precondition in Preconditions)
            {
                var index = resolver.WorldStateDescription.GetStateIndex(precondition.StateName);
                primitiveTask.Preconditions.Add(
                    new ConditionToDecompose(index, new StateCondition(precondition.Operator, precondition.Value)));
            }

            foreach (var effect in Effects)
            {
                var index = resolver.WorldStateDescription.GetStateIndex(effect.StateName);
                primitiveTask.Effects.Add(new EffectToDecompose(index, new StateEffect(effect.Operator, effect.Value, effect.Type)));
            }

            primitiveTask.SetOperator(CreateOperator(resolver.UserData));

            resolver.AddResolvedTask(name, primitiveTask);
        }

        protected abstract IOperator CreateOperator(object userData);
    }
}
