using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerationMethods : MonoBehaviour
{
    // Variables
    public GenerationMethod generationMethod;

    [Condition("generationMethodIsPerlinNoise", true)]
    public float perlinScale = 20f;
    [Condition("generationMethodIsPerlinNoise", true)]
    public Vector2 perlinOffset = Vector2.zero;

    // [Condition("generationMethodIsWaveFunctionCollapse", true)]

    [HideInInspector]
    public bool generationMethodIsPerlinNoise = false;
    [HideInInspector]
    public bool generationMethodIsWaveFunctionCollapse = false;

    private int chunkSize;

    // References
    private GenerationChunks generationChunks;
    private WaveFunctionCollapse waveFunctionCollapse;

    void Start()
    {
        // Cache
        generationChunks = GetComponent<GenerationChunks>();
        waveFunctionCollapse = GetComponent<WaveFunctionCollapse>();
    }

    void OnValidate()
    {
        generationMethodIsPerlinNoise = generationMethod == GenerationMethod.PerlinNoise;
        generationMethodIsWaveFunctionCollapse = generationMethod == GenerationMethod.WaveFunctionCollapse;
    }

    public TileType[,] GenerateTiles(Vector2Int chunkPosition)
    {
        TileType[,] generatedTileData;
        bool successfullyGenerated = false;

        chunkSize = generationChunks.chunkSize;

        switch (generationMethod)
        {
            case GenerationMethod.PerlinNoise:
                if (GeneratePerlinNoise(out generatedTileData, chunkPosition))
                {
                    successfullyGenerated = true;
                }
                break;
            case GenerationMethod.WaveFunctionCollapse:
                if (waveFunctionCollapse.Generate(out generatedTileData))
                {
                    successfullyGenerated = true;
                }
                break;
            default: // GenerationMethod.Random
                if (GenerateRandom(out generatedTileData))
                {
                    successfullyGenerated = true;
                }
                break;
        }

        if (!successfullyGenerated)
        {
            Debug.LogError("Failed to generate chunk using generation method: " + generationMethod + ", Position: " + chunkPosition + ". Generated a random chunk instead!");

            GenerateRandom(out generatedTileData);
        }

        return generatedTileData;
    }

    bool GenerateRandom(out TileType[,] generatedTileData)
    {
        generatedTileData = new TileType[chunkSize, chunkSize];

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                generatedTileData[x, y] = (TileType)UnityEngine.Random.Range(0, Enum.GetNames(typeof(TileType)).Length);
            }
        }

        return true;
    }

    bool GeneratePerlinNoise(out TileType[,] generatedTileData, Vector2Int chunkPosition)
    {
        generatedTileData = new TileType[chunkSize, chunkSize];
        float[,] sampleData = new float[chunkSize, chunkSize];

        float minSample = 0f;
        float maxSample = 0f;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                float sample;

                Vector2 tilePos = new Vector2((float)chunkPosition.x * chunkSize + x + perlinOffset.x, (float)chunkPosition.y * chunkSize + y + perlinOffset.y);

                sample = Mathf.PerlinNoise(tilePos.x * perlinScale, tilePos.y * perlinScale); // ! Return value might be slightly below 0 and slightly above 1

                if (x == 0 && y == 0)
                {
                    minSample = sample;
                    maxSample = sample;
                }

                if (sample < 0)
                {
                    sample = 0;
                }

                if (sample > 1)
                {
                    sample = 1;
                }

                if (sample > maxSample)
                {
                    maxSample = sample;
                }

                if (sample < minSample)
                {
                    minSample = sample;
                }

                sampleData[x, y] = sample;
            }
        }

        // Set tile data
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                generatedTileData[x, y] = GetTileFromPerlinSample(sampleData[x, y], minSample, maxSample);
            }
        }

        return true;
    }

    TileType GetTileFromPerlinSample(float sample, float min, float max)
    {
        float normalizedSample = (sample - min) / (max - min);

        int enumLength = Enum.GetNames(typeof(TileType)).Length;

        int mappedSamples = Mathf.FloorToInt(normalizedSample * enumLength);

        mappedSamples = Mathf.Clamp(mappedSamples, 0, enumLength - 1);

        return (TileType)mappedSamples;
    }
}
