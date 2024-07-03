using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidpointDisplacementGen : MonoBehaviour
{
    // Variables
    public int widthExponent = 7;
    [ReadOnly]
    public int mapWidth = 129;
    public int heightExponent = 7;
    [ReadOnly]
    public int mapHeight = 129;
    public float displacement = 10f;

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
        mapWidth = (int)Mathf.Pow(2, widthExponent) + 1;
        mapHeight = (int)Mathf.Pow(2, heightExponent) + 1;
    }

    public TileType[,] Generate(int gridWidth, int gridHeight, bool useDelay)
    {
        Debug.LogWarning("Not fully implemented");
        return null;

        /* TileType[,] tileData = new TileType[mapWidth, mapHeight];
         float[,] sampleData = new float[mapWidth, mapHeight];

         float minSample = 0f;
         float maxSample = 0f;

        tileHandler.isGenerating = true;

         var stopwatch = Glob.StartStopWatch(); // Put this before any calculations

         heightMap = GenerateHeightMap();

         for (int x = 0; x < mapWidth; x++)
         {
             for (int y = 0; y < mapHeight; y++)
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
         for (int x = 0; x < mapWidth; x++)
         {
             for (int y = 0; y < mapHeight; y++)
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
        float[,] map = new float[mapWidth, mapHeight];

        // Initialize corners
        map[0, 0] = Random.value * displacement;
        map[0, mapHeight - 1] = Random.value * displacement;
        map[mapWidth - 1, 0] = Random.value * displacement;
        map[mapWidth - 1, mapHeight - 1] = Random.value * displacement;

        int stepSize = mapWidth - 1;
        float scale = displacement;

        while (stepSize > 1)
        {
            int halfStep = stepSize / 2;

            // Square step
            for (int x = 0; x < mapWidth - 1; x += stepSize)
            {
                for (int y = 0; y < mapHeight - 1; y += stepSize)
                {
                    float avg = (map[x, y] + map[x + stepSize, y] + map[x, y + stepSize] + map[x + stepSize, y + stepSize]) / 4f;
                    map[x + halfStep, y + halfStep] = avg + (Random.value * 2f - 1f) * scale;
                }
            }

            // Diamond step
            for (int x = 0; x < mapWidth; x += halfStep)
            {
                for (int y = (x + halfStep) % stepSize; y < mapHeight; y += stepSize)
                {
                    float avg = (map[(x - halfStep + mapWidth) % mapWidth, y] +
                                 map[(x + halfStep) % mapWidth, y] +
                                 map[x, (y + halfStep) % mapHeight] +
                                 map[x, (y - halfStep + mapHeight) % mapHeight]) / 4f;
                    avg += (Random.value * 2f - 1f) * scale;
                    map[x, y] = avg;

                    // Wrap edges
                    if (x == 0) map[mapWidth - 1, y] = avg;
                    if (y == 0) map[x, mapHeight - 1] = avg;
                }
            }

            stepSize /= 2;
            scale /= 2f;
        }

        return map;
    }
}
