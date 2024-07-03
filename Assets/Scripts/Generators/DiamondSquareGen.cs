using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondSquareGen : MonoBehaviour
{
    // Variables
    public int sizeExponent = 7;
    [ReadOnly]
    public int size;
    public float roughness = 1f;

    private float[,] heightMap;

    [Header("Options")]
    public bool logElapsedTime = false;

    // References
    private TileHandler tileHandler;

    void Start()
    {
        // Cache
        tileHandler = GetComponent<TileHandler>();
    }

    void OnValidate()
    {
        size = (int)Mathf.Pow(2, sizeExponent) + 1;
    }

    public TileType[,] Generate(int gridWidth, int gridHeight, bool useDelay)
    {
        Debug.LogWarning("Not fully implemented");
        return null;

        /*
        TileType[,] tileData = new TileType[size, size];
        float[,] sampleData = new float[size, size];

        float minSample = 0f;
        float maxSample = 0f;

        tileHandler.isGenerating = true;

        var stopwatch = Glob.StartStopWatch(); // Put this before any calculations

        heightMap = GenerateHeightMap();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float sample = heightMap[x, y];

                // Set the first sample for min and max, making sure they aren't 0
                if (x == 0 && y == 0)
                {
                    minSample = sample;
                    maxSample = sample;
                }

                // Check and set the smallest sample
                if (sample < minSample)
                {
                    minSample = sample;
                }

                // Check and set the largest sample
                if (sample > maxSample)
                {
                    maxSample = sample;
                }

            }
        }

        // Set tile data
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                tileData[x, y] = Glob.GetTileFromSample(sampleData[x, y], minSample, maxSample);
            }
        }

        // Log out elapsed time
        if (logElapsedTime)
        {
            Debug.Log(Glob.FormatMilliseconds(Glob.StopStopWatch(stopwatch)));
        }

        tileHandler.isGenerating = false;

        return tileData;*/
    }

    float[,] GenerateHeightMap()
    {
        float[,] map = new float[size, size];
        int stepSize = size - 1;
        float scale = roughness;

        // Initialize corners
        map[0, 0] = Random.value;
        map[0, stepSize] = Random.value;
        map[stepSize, 0] = Random.value;
        map[stepSize, stepSize] = Random.value;

        while (stepSize > 1)
        {
            int halfStep = stepSize / 2;

            // Diamond step
            for (int x = 0; x < size - 1; x += stepSize)
            {
                for (int y = 0; y < size - 1; y += stepSize)
                {
                    float avg = (map[x, y] + map[x + stepSize, y] + map[x, y + stepSize] + map[x + stepSize, y + stepSize]) / 4f;
                    map[x + halfStep, y + halfStep] = avg + (Random.value * 2f - 1f) * scale;
                }
            }

            // Square step
            for (int x = 0; x < size; x += halfStep)
            {
                for (int y = (x + halfStep) % stepSize; y < size; y += stepSize)
                {
                    float avg = (map[(x - halfStep + size) % size, y] +
                                 map[(x + halfStep) % size, y] +
                                 map[x, (y + halfStep) % size] +
                                 map[x, (y - halfStep + size) % size]) / 4f;
                    avg += (Random.value * 2f - 1f) * scale;
                    map[x, y] = avg;

                    // Wrap edges
                    if (x == 0) map[size - 1, y] = avg;
                    if (y == 0) map[x, size - 1] = avg;
                }
            }

            stepSize /= 2;
            scale /= 2f;
        }

        return map;
    }
}
