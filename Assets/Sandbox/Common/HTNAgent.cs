using System;
using UHTN;
using UnityEngine;

#if UNITY_EDITOR
using UHTN.Editor.PlanViewer;
using UnityEditor;
#endif

namespace Sandbox
{
    public class HtnAgent : MonoBehaviour
    {
        [SerializeField]
        private PlannerExecutionType _executionType;

        public bool IsRunning => _planner.IsRunning;

        private Planner _planner;
        public Planner Planner => _planner;

        public void Initialize(Domain domain, WorldState worldState)
        {
            _planner = new Planner(domain, worldState);
            _planner.ExecutionType = _executionType;
        }

        private void OnValidate()
        {
            if (_planner == null)
            {
                return;
            }

            _planner.ExecutionType = _executionType;
        }

        public void Run()
        {
            ThrowIfNotInitialized();
            _planner.Begin();
        }

        public void Stop()
        {
            ThrowIfNotInitialized();
            _planner.Stop();
        }

        private void ThrowIfNotInitialized()
        {
            if (_planner == null)
            {
                throw new InvalidOperationException("HTNAgent is not initialized.");
            }
        }

        private void OnDisable()
        {
            _planner?.Pause();
        }

        private void Update()
        {
            if (_planner == null || !_planner.IsRunning)
            {
                return;
            }

            _planner?.Tick();
        }

        private void OnDestroy()
        {
            _planner?.Dispose();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(HtnAgent))]
    public class HtnAgentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var agent = (HtnAgent)target;
            var planner = agent.Planner;
            if (planner == null)
            {
                return;
            }

            EditorGUILayout.LabelField("-----WorldStates-----");
            for (var i = 0; i < planner.WorldState.Values.Count; i++)
            {
                var state = planner.WorldState.Values[i];
                var stateName = planner.WorldState.Description.GetStateName(i);
                var stateType = planner.WorldState.Description.GetStateType(i);
                EditorGUILayout.LabelField($"{stateName} : {stateType.ToDisplayString(state)}");
            }

            if (GUILayout.Button("OpenPlanViewer"))
            {
                PlanViewerWindow.ShowWindow(planner.PlanRunner);
            }
        }
    }
#endif
}
