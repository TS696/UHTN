using UHTN.PlanViewer;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UHTN.Editor.PlanViewer
{
    public class PlanViewerWindow : EditorWindow
    {
        [SerializeField]
        private TreeViewState _planRunnerTreeViewState;

        [SerializeField]
        private TreeViewState _planTreeViewState;

        [SerializeField]
        private bool _useAutoReload;

        private PlanRunnerTreeView _planRunnerTreeView;
        private PlanTreeView _planTreeView;

        private PlanRunner _selectedPlanRunner;
        private ITask _selectedTask;

        private float _leftViewWidth = 200;
        private float _centerViewWidth = 200;
        private float _rightViewWidth = 200;
        private float _headerHeight = 30;

        [MenuItem("Window/UHTN/Plan Viewer")]
        public static void ShowWindow()
        {
            var window = GetWindow();
            window.Show();
        }

        public static void ShowWindow(PlanRunner planRunner)
        {
            if (planRunner.State is not PlanRunner.RunnerState.Running)
            {
                Debug.LogWarning($"PlanRunner {planRunner.Name} is not running.");
                return;
            }
            
            var window = GetWindow();
            window.Show();

            PlanViewerRegister.Register(planRunner);
            window.OnSelectRunner(planRunner);
            window.Reload();
        }

        private static PlanViewerWindow GetWindow()
        {
            return GetWindow<PlanViewerWindow>("Plan Viewer");
        }


        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
            Initialize();
        }

        private void Initialize()
        {
            PlanViewerRegister.EnablePlanViewer = true;

            if (_planRunnerTreeViewState == null)
            {
                _planRunnerTreeViewState = new TreeViewState();
            }

            if (_planTreeViewState == null)
            {
                _planTreeViewState = new TreeViewState();
            }

            var planRunners = PlanViewerRegister.PlanRunners;
            _planRunnerTreeView = new PlanRunnerTreeView(planRunners, _planRunnerTreeViewState);
            _planRunnerTreeView.Reload();
            _planRunnerTreeView.OnSelect += OnSelectRunner;

            OnSelectRunner(null);
        }

        private void PlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    Initialize();
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    Clear();
                    break;
            }
        }

        private void OnDisable()
        {
            PlanViewerRegister.EnablePlanViewer = false;
            Clear();
        }

        void OnGUI()
        {
            _leftViewWidth = _centerViewWidth = _rightViewWidth = position.width / 3;

            DrawHeader();
            LeftView();
            CenterView();
            RightView();
        }

        void Update()
        {
            if (Time.frameCount % 10 == 0)
            {
                Repaint();

                if (_useAutoReload)
                {
                    Reload();
                }
            }
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Reload", EditorStyles.toolbarButton))
                    {
                        Reload();
                    }

                    if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
                    {
                        Clear();
                    }
                }

                _useAutoReload = GUILayout.Toggle(_useAutoReload, "AutoReload");
            }

            if (Event.current.type == EventType.Repaint)
            {
                _headerHeight = GUILayoutUtility.GetLastRect().height + 5;
            }
        }

        private void Clear()
        {
            PlanViewerRegister.Clear();
            OnSelectRunner(null);
            Reload();
        }

        private void Reload()
        {
            _planRunnerTreeView.Reload();
            _planTreeView.Reload();
        }

        private void LeftView()
        {
            _planRunnerTreeView.OnGUI(new Rect(0, _headerHeight, _leftViewWidth,
                position.height - _headerHeight));
        }

        private void CenterView()
        {
            _planTreeView.OnGUI(new Rect(_leftViewWidth, _headerHeight, _centerViewWidth,
                position.height - _headerHeight));
        }

        private void RightView()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                var width = GUILayout.Width(_rightViewWidth - 10);
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField($"PlanRunner : {_selectedPlanRunner?.Name}", width);
                    if (_selectedPlanRunner != null)
                    {
                        EditorGUI.indentLevel++;
                        DrawPlanRunner(_selectedPlanRunner, width);
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField($"Task : {_selectedTask?.Name}", width);
                    if (_selectedPlanRunner != null && _selectedTask != null)
                    {
                        EditorGUI.indentLevel++;
                        DrawTask(_selectedPlanRunner, _selectedTask, width);
                        EditorGUI.indentLevel--;
                    }
                }
            }
        }

        private static void DrawPlanRunner(PlanRunner planRunner, GUILayoutOption width)
        {
            EditorGUILayout.LabelField("WorldState", width);

            var worldState = planRunner.WorldState;

            EditorGUI.indentLevel++;
            for (var i = 0; i < worldState.StateLength; i++)
            {
                var stateName = worldState.Description.GetStateName(i);
                var stateType = worldState.Description.GetStateType(i);
                EditorGUILayout.LabelField($"{stateName} : {stateType.ToDisplayString(worldState.Values[i])}", width);
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawTask(PlanRunner planRunner, ITask task, GUILayoutOption width)
        {
            var worldStateDesc = planRunner.WorldState.Description;
            switch (task)
            {
                case IPrimitiveTask primitiveTask:
                    EditorGUILayout.LabelField("Preconditions", width);

                    EditorGUI.indentLevel++;

                    foreach (var condition in primitiveTask.Preconditions)
                    {
                        var stateName = worldStateDesc.GetStateName(condition.StateIndex);
                        var stateType = worldStateDesc.GetStateType(condition.StateIndex);
                        EditorGUILayout.LabelField(
                            $"{stateName}: {condition.Value.Operator} {stateType.ToDisplayString(condition.Value.Value)}",
                            width);
                    }

                    EditorGUI.indentLevel--;

                    EditorGUILayout.LabelField("Effects", width);

                    EditorGUI.indentLevel++;
                    foreach (var effect in primitiveTask.Effects)
                    {
                        var stateName = worldStateDesc.GetStateName(effect.StateIndex);
                        var stateType = worldStateDesc.GetStateType(effect.StateIndex);
                        EditorGUILayout.LabelField(
                            $"{stateName}: {effect.Value.Operator} {stateType.ToDisplayString(effect.Value.Value)}",
                            width);
                    }

                    EditorGUI.indentLevel--;
                    break;
            }
        }

        private void OnSelectRunner(PlanRunner select)
        {
            if (_selectedPlanRunner != select)
            {
                _selectedTask = null;
            }

            _selectedPlanRunner = select;

            _planTreeView = new PlanTreeView(select, _planTreeViewState);
            _planTreeView.Reload();
            _planTreeView.OnSelect += OnSelectTask;
        }

        private void OnSelectTask(ITask select)
        {
            _selectedTask = select;
        }
    }
}
