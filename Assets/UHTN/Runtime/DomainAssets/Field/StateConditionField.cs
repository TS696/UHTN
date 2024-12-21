using System;
using UnityEngine;

namespace UHTN.DomainAssets
{
    [Serializable]
    public class StateConditionField
    {
        [SerializeField]
        private string _stateName;
        public string StateName => _stateName;
        
        [SerializeField]
        private StateComparisonOperator _operator;
        public StateComparisonOperator Operator => _operator;

        [SerializeField]
        private int _value;
        public int Value => _value;
    }
}
