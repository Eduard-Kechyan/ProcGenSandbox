using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimplexNoise;

public class NoiseGen : MonoBehaviour
{
    // Variables
    [Header("Perlin")]
    public float scale = 20f;
    public Vector2 offset = new(0f, 0f);

    [Header("Simplex")]
    public float simplexScale = 20f;
    public Vector2 simplexOffset = new(0f, 0f);

    [Header("Fractal")]
    public float fractalScale = 20f;
    public Vector2 fractalOffset = new(0f, 0f);
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    [Header("Options")]
    public bool useRandomOffset = false;
    public bool logElapsedTime = false;

    // References
    private TileHandler tileHandler;

    void Start()
    {
        // Cache
        tileHandler = GetComponent<TileHandler>();

        if (useRandomOffset)
        {
            float x = UnityEngine.Random.Range(-9999, 9999);
            float y = UnityEngine.Random.Range(-9999, 9999);

            offset = new(x, y);
            simplexOffset = new(x, y);
            fractalOffset = new(x, y);
        }
    }

    public TileType[,] Generate(int gridWidth, int gridHeight, bool useDelay, bool useSimplexNoise = false, bool useFractalNoise = false)
    {
        TileType[,] tileData = new TileType[gridWidth, gridHeight];
        float[,] sampleData = new float[gridWidth, gridHeight];

        float[,] simplexSample = default;

        float minSample = 0f;
        float maxSample = 0f;

        tileHandler.isGenerating = true;

        var stopwatch = Glob.StartStopWatch(); // Put this before any calculations

        if (useSimplexNoise)
        {
            simplexSample = Noise.Calc2D(gridWidth + (int)simplexOffset.x, gridHeight + (int)simplexOffset.y, simplexScale);
        }

        // Set sample data
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                float sample;

                if (useSimplexNoise) // Simplex
                {
                    sample = simplexSample[x, y];
                }
                else if (useFractalNoise) // Fractal
                {
                    float xCoord = (float)x / gridWidth * fractalScale + fractalOffset.x;
                    float yCoord = (float)y / gridHeight * fractalScale + fractalOffset.y;

                    sample = FractalNoise(xCoord, yCoord);
                }
                else // Perlin
                {
                    float xCoord = (float)x / gridWidth * scale + offset.x;
                    float yCoord = (float)y / gridHeight * scale + offset.y;

                    sample = Mathf.PerlinNoise(xCoord, yCoord); // ! Return value might be slightly below 0 and slightly above 1

                    if (sample < 0)
                    {
                        sample = 0;
                    }

                    if (sample > 1)
                    {
                        sample = 1;
                    }
                }

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

                sampleData[x, y] = sample;
            }
        }

        // Set tile data
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
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

        return tileData;
    }

    float FractalNoise(float xCoord, float yCoord)
    {
        float amplitude = 1f;
        float frequency = 1f;
        float noiseHeight = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = xCoord * frequency;
            float sampleY = yCoord * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

            noiseHeight = perlinValue * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return noiseHeight;
    }
}
