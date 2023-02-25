using FPSCounter;
using UnityEngine;

namespace PerformanceTracker.Util.Helpers;

public class UIHelper
{
    #region UI

    const int MAX_STRING_SIZE = 499;

    private static readonly GUIStyle _style = new();
    private static Rect _screenRect;
    private const int ScreenOffset = 10;

    internal static MutableString fString = new(MAX_STRING_SIZE, true);
    internal static string _frameOutputText;


    internal static void DrawCounter()
    {
        if (PerformanceTrackerPlugin._counterColor.Value == PerformanceTrackerPlugin.CounterColors.Outline)
            ShadowAndOutline.DrawOutline(_screenRect, _frameOutputText, _style, Color.black, Color.white, 1.5f);
        else
            GUI.Label(_screenRect, _frameOutputText, _style);
    }

    internal static void UpdateLooks()
    {
        if (PerformanceTrackerPlugin._counterColor.Value == PerformanceTrackerPlugin.CounterColors.White)
            _style.normal.textColor = Color.white;
        if (PerformanceTrackerPlugin._counterColor.Value == PerformanceTrackerPlugin.CounterColors.Black)
            _style.normal.textColor = Color.black;

        int w = Screen.width, h = Screen.height;
        _screenRect = new Rect(ScreenOffset, ScreenOffset, w - ScreenOffset * 2, h - ScreenOffset * 2);

        _style.alignment = PerformanceTrackerPlugin._position.Value;
        _style.fontSize = h / 65;
    }

    #endregion
}