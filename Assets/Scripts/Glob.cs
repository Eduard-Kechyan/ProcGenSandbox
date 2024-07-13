using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Glob : MonoBehaviour
{
    public static Glob Instance;

    // TODO - Fix this function 
    public static string FormatMilliseconds(long milliSeconds)
    {
        long totalSeconds = milliSeconds / 1000;
        long remainingMilliSeconds = milliSeconds % 1000;

        long minutes = totalSeconds / 60;
        long seconds = totalSeconds % 60;

        string formatedString = string.Format("{0}m {1}s {2}ms", minutes, seconds, remainingMilliSeconds);

        return "Generated tilemap in: " + formatedString;
    }

    public static void Validate(Action callback, params UnityEngine.Object[] newObjects)
    {
        void NextUpdate()
        {
            EditorApplication.update -= NextUpdate;

            if (newObjects.Any(c => !c))
            {
                return;
            }

            if (newObjects.All(c => !EditorUtility.IsDirty(c)))
            {
                return;
            }

            callback?.Invoke();

            foreach (UnityEngine.Object component in newObjects)
            {
                EditorUtility.SetDirty(component);
            }
        }

        EditorApplication.update += NextUpdate;
    }

    //// STOPWATCH ////

    public static Stopwatch StartStopWatch()
    {
        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();

        return stopwatch;
    }

    public static long StopStopWatch(Stopwatch stopwatch)
    {
        stopwatch.Stop();

        return stopwatch.ElapsedMilliseconds;
    }
}
