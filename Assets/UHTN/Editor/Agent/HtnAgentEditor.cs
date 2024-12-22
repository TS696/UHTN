using UHTN.Agent;
using UHTN.Editor.PlanViewer;
using UnityEditor;
using UnityEngine;

namespace UHTN.Editor.Agent
{
    [CustomEditor(typeof(HtnAgentBase), true)]
    public class HtnAgentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
 
            var agent = (HtnAgentBase)target;
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
}
