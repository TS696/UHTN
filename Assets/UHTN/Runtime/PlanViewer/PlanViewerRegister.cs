using System;
using System.Collections.Generic;

namespace UHTN.PlanViewer
{
    public static class PlanViewerRegister
    {
        private static bool _enablePLanViewer;

        public static bool EnablePlanViewer
        {
            get => _enablePLanViewer;
            set
            {
                _enablePLanViewer = value;
                if (!_enablePLanViewer)
                {
                    _planRunners.Clear();
                }
            }
        }

        private static readonly List<PlanRunner> _planRunners = new();
        public static IReadOnlyList<PlanRunner> PlanRunners => _planRunners;

        public static void Register(PlanRunner runner)
        {
            if (_planRunners.Contains(runner))
            {
                return;
            }
            _planRunners.Add(runner);
        }

        public static void UnRegister(PlanRunner runner)
        {
            _planRunners.Remove(runner);
        }

        public static void Clear()
        {
            _planRunners.Clear();
        }
    }
}
