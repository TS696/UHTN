using System;
using UnityEngine;

namespace UHTN.DomainAssets
{
    [Serializable]
    public class SubTaskField
    {
        [SerializeField]
        private string _taskName;
        public string TaskName => _taskName;

        [SerializeField]
        private DecompositionTiming _decompositionTiming;
        public DecompositionTiming DecompositionTiming => _decompositionTiming;
    }
}
