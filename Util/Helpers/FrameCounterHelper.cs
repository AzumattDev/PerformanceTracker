using System;
using System.Collections;
using System.Diagnostics;
using FPSCounter;
using UnityEngine;

namespace PerformanceTracker.Util.Helpers;

/// <summary>
/// Code that actually captures the frame times
/// int.MinValue makes all events on this script execute first in the scene
/// </summary>
[DefaultExecutionOrder(int.MinValue)]
internal sealed class FrameCounterHelper : MonoBehaviour
{
    #region Measurements

    /// <summary>
    /// https://docs.unity3d.com/Manual/ExecutionOrder.html
    /// Measure order:
    /// 1 FixedUpdate (all iterations this frame, includes physics)
    /// 2 Update
    /// 3 Yield null (includes physics and yield null / waitseconds)
    /// 4 LateUpdate
    /// 5 Scene rendering (from last LateUpdate to first OnGUI call)
    /// 6 OnGUI (measured from first redraw until first WaitForEndOfFrame)
    /// </summary>
    private static readonly MovingAverage _fixedUpdateTime = new();

    private static readonly MovingAverage _updateTime = new();
    private static readonly MovingAverage _yieldTime = new();
    private static readonly MovingAverage _lateUpdateTime = new();
    private static readonly MovingAverage _renderTime = new();
    private static readonly MovingAverage _onGuiTime = new();
    private static readonly MovingAverage _gcAddedSize = new(60);

    /// <summary>
    /// Measure frame time separately to get the true value
    /// TODO measure vsync sleeping?
    /// </summary>
    private static readonly MovingAverage _frameTime = new();

    #endregion

    #region Timing

    private static Stopwatch _measurementStopwatch;

    private static long TakeMeasurement()
    {
        var result = _measurementStopwatch.ElapsedTicks;
        _measurementStopwatch.Reset();
        _measurementStopwatch.Start();
        return result;
    }

    #endregion

    #region Capture

    internal static bool CanProcessOnGui;
    private static bool _onGuiHit;

    private static readonly WaitForEndOfFrame WaitForEndOfFrame = new();
    private static readonly KVPluginDataComparer Comparer = new();

    private IEnumerator Start()
    {
        _measurementStopwatch = new Stopwatch();
        var totalStopwatch = new Stopwatch();
        var nanosecPerTick = (float)(1000 * 1000 * 100) / Stopwatch.Frequency;
        var msScale = 1f / (nanosecPerTick * 1000f);
        var gcPreviousAmount = 0L;

        while (true)
        {
            // Waits until right after last Update
            yield return null;

            _updateTime.Sample(TakeMeasurement());

            // Waits until right after last OnGUI
            yield return WaitForEndOfFrame;

            // If no OnGUI was executed somehow, make sure to log the render time
            if (!_onGuiHit)
            {
                _renderTime.Sample(TakeMeasurement());
                _onGuiHit = true;
            }

            CanProcessOnGui = false;

            _onGuiTime.Sample(TakeMeasurement());
            // Stop until FixedUpdate so it gets counted accurately (skip other end of frame stuff)
            _measurementStopwatch.Reset();

            // Get actual frame round-time
            _frameTime.Sample(totalStopwatch.ElapsedTicks);
            totalStopwatch.Reset();
            totalStopwatch.Start();

            // Calculate only once at end of frame so all data is from a single frame
            var avgFrame = _frameTime.GetAverage();
            var fps = 1000000f / (avgFrame / nanosecPerTick);


            UIHelper.fString.Append(fps, 2, 2).Append(" FPS");

            if (PerformanceTrackerPlugin._showUnityMethodStats.Value == PerformanceTrackerPlugin.Toggle.On)
            {
                var avgFixed = _fixedUpdateTime.GetAverage();
                var avgUpdate = _updateTime.GetAverage();
                var avgYield = _yieldTime.GetAverage();
                var avgLate = _lateUpdateTime.GetAverage();
                var avgRender = _renderTime.GetAverage();
                var avgGui = _onGuiTime.GetAverage();

                var totalCapturedTicks = avgFixed + avgUpdate + avgYield + avgLate + avgRender + avgGui;
                var otherTicks = avgFrame - totalCapturedTicks;

                // Print floats with 1 decimal precision i.e. XX.X and padding of 2,
                // meaning we assume we always get XX.X value
                UIHelper.fString.Append(", ").Append(avgFrame * msScale, 2, 2);
                UIHelper.fString.Append("ms\nFixed: ").Append(avgFixed * msScale, 2, 2);
                UIHelper.fString.Append("ms\nUpdate: ").Append(avgUpdate * msScale, 2, 2);
                UIHelper.fString.Append("ms\nYield/anim: ").Append(avgYield * msScale, 2, 2);
                UIHelper.fString.Append("ms\nLate: ").Append(avgLate * msScale, 2, 2);
                UIHelper.fString.Append("ms\nRender/VSync: ").Append(avgRender * msScale, 2, 2);
                UIHelper.fString.Append("ms\nOnGUI: ").Append(avgGui * msScale, 2, 2);
                UIHelper.fString.Append("ms\nOther: ").Append(otherTicks * msScale, 2, 2).Append("ms");
            }

            if (PerformanceTrackerPlugin._measureMemory != null && PerformanceTrackerPlugin._measureMemory.Value ==
                PerformanceTrackerPlugin.Toggle.On)
            {
                var procMem = MemoryInfo.QueryProcessMemStatus();
                var currentMem = procMem.WorkingSetSize / 1024 / 1024;

                var memorystatus = MemoryInfo.QuerySystemMemStatus();
                var freeMem = memorystatus.ullAvailPhys / 1024 / 1024;

                UIHelper.fString.Append("\nRAM: ").Append((uint)currentMem).Append("MB used, ");
                UIHelper.fString.Append((uint)freeMem).Append("MB free");

                var totalGcMemBytes = GC.GetTotalMemory(false);
                if (totalGcMemBytes != 0)
                {
                    var gcDelta = totalGcMemBytes - gcPreviousAmount;
                    var totalGcMem = totalGcMemBytes / 1024 / 1024;
                    _gcAddedSize.Sample(gcDelta);

                    UIHelper.fString.Append("\nGC: ").Append((int)totalGcMem).Append("MB (");
                    UIHelper.fString.Append(_gcAddedSize.GetAverageFloat() / 1024, 2, 4).Append("KB/s)");
                    //fString.Append(Mathf.RoundToInt(_gcAddedSize.GetAverage() * fps / 1024)).Append("KB/s)");

                    gcPreviousAmount = totalGcMemBytes;
                }

                // Check if current GC supports generations
                var gcGens = GC.MaxGeneration;
                if (gcGens > 0)
                {
                    UIHelper.fString.Append("\nGC hits:");
                    for (var g = 0; g < gcGens; g++)
                    {
                        var collections = GC.CollectionCount(g);
                        UIHelper.fString.Append(' ').Append(g).Append(':').Append(collections);
                    }
                }
            }

            var plugList = PluginCounter.SlowPlugins;
            if (plugList != null)
            {
                if (plugList.Count > 0)
                {
                    plugList.Sort(Comparer);
                    int len = plugList.Count;
                    for (int i = 0; i < len && i < 5; i++)
                    {
                        var kvav = plugList[i];
                        var maxName = kvav.Key.Length > 20 ? 20 : kvav.Key.Length;
                        UIHelper.fString.Append("\n[").Append(kvav.Key, 0, maxName).Append(": ")
                            .Append(kvav.Value * msScale, 1, 2).Append("ms]");
                    }
                }
                else
                {
                    UIHelper.fString.Append("\nNo slow plugins");
                }
            }

            UIHelper._frameOutputText = UIHelper.fString.Finalize();
            _measurementStopwatch.Reset();
        }
    }

    private void FixedUpdate()
    {
        // If fixed doesn't run at all this frame, stopwatch won't get started and tick count will stay at 0
        _measurementStopwatch.Start();
    }

    private void Update()
    {
        _fixedUpdateTime.Sample(TakeMeasurement());
    }

    private void LateUpdate()
    {
        _yieldTime.Sample(TakeMeasurement());
    }

    /// <summary>
    /// Needed to measure LateUpdate, int.MaxValue makes it run as the last LateUpdate call in scene.
    /// It's the last possible time to do it without listening for render events on a Camera, which is less reliable
    /// </summary>
    [DefaultExecutionOrder(int.MaxValue)]
    internal sealed class FrameCounterHelper2 : MonoBehaviour
    {
        private void LateUpdate()
        {
            _lateUpdateTime.Sample(TakeMeasurement());

            _onGuiHit = false;
            CanProcessOnGui = true;
        }
    }

    private void OnGUI()
    {
        // Dragging the mouse will mess with event ordering and give bad results, so we have to reset this flag at the last point before OnGUI we can (end of lateupdate)
        if (!_onGuiHit)
        {
            _renderTime.Sample(TakeMeasurement());
            _onGuiHit = true;
        }

        if (Event.current.type == EventType.Repaint)
            UIHelper.DrawCounter();
    }

    #endregion
}