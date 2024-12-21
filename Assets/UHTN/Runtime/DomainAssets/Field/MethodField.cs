using System;
using UnityEngine;

namespace UHTN.DomainAssets
{
    [Serializable]
    public class MethodField
    {
        [SerializeField]
        private StateConditionField[] _preconditions;
        public StateConditionField[] Preconditions => _preconditions;

        [SerializeField]
        private SubTaskField[] _subTasks;
        public SubTaskField[] SubTasks => _subTasks;
    }
}
