using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using SimplexNoise;

public class NoiseGen : MonoBehaviour
{
    public bool useRandomOffset = false;

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

    void Start()
    {
        if (useRandomOffset)
        {
            float x = Random.Range(-9999, 9999);
            float y = Random.Range(-9999, 9999);

            offset = new(x, y);
            simplexOffset = new(x, y);
            fractalOffset = new(x, y);
        }
    }

    public TileType[,] Generate(int gridWidth, int gridHeight, bool useDelay, bool useSimplexNoise = false, bool useFractalNoise = false)
    {
        Stopwatch stopwatch = new Stopwatch();

        TileType[,] tileData = new TileType[gridWidth, gridHeight];

        float[,] simplexSample = default;

        if (useSimplexNoise)
        {
            simplexSample = Noise.Calc2D(gridWidth + (int)simplexOffset.x, gridHeight + (int)simplexOffset.y, simplexScale);
        }

        stopwatch.Start();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (useSimplexNoise) // Simplex
                {
                    float sample = simplexSample[x, y];
                    tileData[x, y] = GetTileFormSimplexSample(sample);
                }
                else if (useFractalNoise) // Fractal
                {
                    float xCoord = (float)x / gridWidth * fractalScale + fractalOffset.x;
                    float yCoord = (float)y / gridHeight * fractalScale + fractalOffset.y;

                    float sample = FractalNoise(xCoord, yCoord);

                    UnityEngine.Debug.Log(sample);
                    tileData[x, y] = GetTileFormFractalSample(sample);
                }
                else // Perlin
                {
                    float xCoord = (float)x / gridWidth * scale + offset.x;
                    float yCoord = (float)y / gridHeight * scale + offset.y;

                    float sample = Mathf.PerlinNoise(xCoord, yCoord);

                    tileData[x, y] = GetTileFormSample(sample);
                }
            }
        }

        stopwatch.Stop();

        UnityEngine.Debug.Log("Generated noise in: " + stopwatch.ElapsedMilliseconds);

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

    TileType GetTileFormSample(float sample)
    {
        if (sample < 0.1f)
        {
            return TileType.Grass;
        }
        if (sample < 0.2f)
        {
            return TileType.GrassFlowers;
        }
        if (sample < 0.3f)
        {
            return TileType.GrassBush;
        }
        if (sample < 0.4f)
        {
            return TileType.Desert;
        }
        if (sample < 0.5f)
        {
            return TileType.WaterShallow;
        }
        if (sample < 0.6f)
        {
            return TileType.Water;
        }
        if (sample < 0.7f)
        {
            return TileType.WaterDeep;
        }
        if (sample < 0.8f)
        {
            return TileType.MountainFoot;
        }
        if (sample < 0.9f)
        {
            return TileType.Mountain;
        }
        if (sample < 1f)
        {
            return TileType.MountainTop;
        }

        return TileType.Grass;
    }

    TileType GetTileFormSimplexSample(float sample)
    {
        if (sample < 22f)
        {
            return TileType.Grass;
        }
        if (sample < 50f)
        {
            return TileType.GrassFlowers;
        }
        if (sample < 77f)
        {
            return TileType.GrassBush;
        }
        if (sample < 105f)
        {
            return TileType.Desert;
        }
        if (sample < 133f)
        {
            return TileType.WaterShallow;
        }
        if (sample < 161f)
        {
            return TileType.Water;
        }
        if (sample < 188f)
        {
            return TileType.WaterDeep;
        }
        if (sample < 216f)
        {
            return TileType.MountainFoot;
        }
        if (sample < 244f)
        {
            return TileType.Mountain;
        }
        if (sample < 273f)
        {
            return TileType.MountainTop;
        }

        return TileType.Grass;
    }

    TileType GetTileFormFractalSample(float sample)
    {
        if (sample < -0.5f)
        {
            return TileType.Grass;
        }
        if (sample < -0.4f)
        {
            return TileType.GrassFlowers;
        }
        if (sample < -0.3f)
        {
            return TileType.GrassBush;
        }
        if (sample < -0.2f)
        {
            return TileType.Desert;
        }
        if (sample < -0.1f)
        {
            return TileType.WaterShallow;
        }
        if (sample < 0.1f)
        {
            return TileType.Water;
        }
        if (sample < 0.2f)
        {
            return TileType.WaterDeep;
        }
        if (sample < 0.3f)
        {
            return TileType.MountainFoot;
        }
        if (sample < 0.4f)
        {
            return TileType.Mountain;
        }
        if (sample < 0.5f)
        {
            return TileType.MountainTop;
        }

        return TileType.Grass;
    }

}
