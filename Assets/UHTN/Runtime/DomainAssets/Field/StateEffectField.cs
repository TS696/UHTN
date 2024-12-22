using System;
using UnityEngine;

namespace UHTN.DomainAssets
{
    [Serializable]
    public class StateEffectField
    {
        [SerializeField]
        private string _stateName;
        public string StateName => _stateName;

        [SerializeField]
        private StateEffectOperator _operator;
        public StateEffectOperator Operator => _operator;

        [SerializeField]
        private int _value;
        public int Value => _value;
        
        [SerializeField]
        private StateEffectType _type;
        public StateEffectType Type => _type;
    }
}
