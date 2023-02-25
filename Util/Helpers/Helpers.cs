using UnityEngine;

namespace PerformanceTracker.Util.Helpers;

public class Helpers
{
    private static readonly MonoBehaviour[] _helpers = new MonoBehaviour[2];

    internal static void SetCapturingEnabled(bool enableCapturing)
    {
        if (!enableCapturing)
        {
            PluginCounter.Stop();
            Object.Destroy(_helpers[0]);
            Object.Destroy(_helpers[1]);
        }
        else
        {
            if (_helpers[0] == null) _helpers[0] = PerformanceTrackerPlugin.pluginGO.AddComponent<FrameCounterHelper>();
            if (_helpers[1] == null)
                _helpers[1] = PerformanceTrackerPlugin.pluginGO.AddComponent<FrameCounterHelper.FrameCounterHelper2>();

            if (PerformanceTrackerPlugin._showPluginStats.Value == PerformanceTrackerPlugin.Toggle.On)
                PluginCounter.Start(_helpers[0], PerformanceTrackerPlugin.ModContext);
            else
                PluginCounter.Stop();
        }
    }

    internal static void OnEnable()
    {
        if (PerformanceTrackerPlugin._shown is not { Value: PerformanceTrackerPlugin.Toggle.On }) return;
        UIHelper.UpdateLooks();
        SetCapturingEnabled(true);
    }

    internal static void OnDisable()
    {
        SetCapturingEnabled(false);
    }
}