using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionCollapseGen : MonoBehaviour
{
    // Variables

    [Header("Options")]
    public bool logElapsedTime = false;

    // References
    private TileHandler tileHandler;

    void Start()
    {
        // Cache
        tileHandler = GetComponent<TileHandler>();
    }

    public TileType[,] Generate(int gridWidth, int gridHeight, bool useDelay)
    {
        tileHandler.isGenerating = true;

        var stopwatch = Glob.StartStopWatch();

        Debug.LogWarning("WIP");

        Debug.Log(Glob.FormatMilliseconds(Glob.StopStopWatch(stopwatch)));

        tileHandler.isGenerating = false;

        return null;
    }
}
