using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerationChunks : MonoBehaviour
{
    // Variables
    public int chunkSize = 4;

    [Header("Grid")]
    public bool useCustomGridSize = true;

    [Condition("useCustomGridSize", true, true)]
    [ReadOnly]
    public int gridWidth;
    [Condition("useCustomGridSize", true, true)]
    [ReadOnly]
    public int gridHeight;

    [Condition("useCustomGridSize", true, true)]
    public bool addExtraChunksToBorder = true;

    [Condition("useCustomGridSize", true)]
    public int customGridWidth = 10;
    [Condition("useCustomGridSize", true)]
    public int customGridHeight = 10;

    [Header("Debug")]
    public bool reCalcSizes = false;

    [HideInInspector]
    public int halfGridWidth = 0;
    [HideInInspector]
    public int halfGridHeight = 0;

    [HideInInspector]
    public Vector2Int currentChunkPos;
    [HideInInspector]
    public HashSet<Vector2Int> loadedChunks = new HashSet<Vector2Int>();

    // References
    private GenerationManager generationManager;
    private GenerationTiles generationTiles;
    private GenerationMethods generationMethods;

    void Start()
    {
        // Cache
        generationManager = GetComponent<GenerationManager>();
        generationTiles = GetComponent<GenerationTiles>();
        generationMethods = GetComponent<GenerationMethods>();
    }

    void OnValidate()
    {
        if (reCalcSizes)
        {
            reCalcSizes = false;

            Glob.Validate(() =>
            {
                CalcSizes();
            }, this);
        }
    }

    public void CalcSizes()
    {
        if (useCustomGridSize)
        {
            gridWidth = customGridWidth;
            gridHeight = customGridHeight;
        }
        else
        {
            gridWidth = Mathf.CeilToInt((float)Screen.width / generationManager.pixelPerfectCamera.pixelRatio / (generationManager.pixelPerfectCamera.assetsPPU * chunkSize)); ;
            gridHeight = Mathf.CeilToInt((float)Screen.height / generationManager.pixelPerfectCamera.pixelRatio / (generationManager.pixelPerfectCamera.assetsPPU * chunkSize)); ;

            if (addExtraChunksToBorder)
            {
                gridWidth += 2;
                gridHeight += 2;
            }
        }

        if (gridWidth % 2 == 1)
        {
            gridWidth++;
        }

        if (gridHeight % 2 == 1)
        {
            gridHeight++;
        }

        halfGridWidth = gridWidth / 2;
        halfGridHeight = gridHeight / 2;

        generationManager.chunkCount = gridWidth * gridHeight;

        generationManager.tileCount = generationManager.chunkCount * (chunkSize * chunkSize);
    }

    public void UpdateChunks()
    {
        HashSet<Vector2Int> newLoadedChunks = new HashSet<Vector2Int>();

        for (int x = -halfGridWidth; x < halfGridWidth; x++)
        {
            for (int y = -halfGridHeight; y < halfGridHeight; y++)
            {
                Vector2Int chunkPosition = new Vector2Int(currentChunkPos.x + x, currentChunkPos.y + y);

                newLoadedChunks.Add(chunkPosition);

                if (!loadedChunks.Contains(chunkPosition))
                {
                    if (!ChunkContainsPos(chunkPosition))
                    {
                        if (!loadedChunks.Contains(chunkPosition))
                        {
                            LoadChunk(chunkPosition);
                        }
                    }
                    else
                    {
                        LoadSavedChunk(chunkPosition);
                    }
                }
            }
        }

        foreach (Vector2Int chunk in loadedChunks)
        {
            if (!newLoadedChunks.Contains(chunk))
            {
                UnloadChunk(chunk);
            }
        }

        loadedChunks = newLoadedChunks;
    }

    // Creates a new chunk, saves it to memory and loads into the tilemap
    void LoadChunk(Vector2Int chunkPosition)
    {
        Chunk newChunk = new()
        {
            position = chunkPosition,
            tiles = generationMethods.GenerateTiles(chunkPosition)
        };

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int tilePosition = new Vector3Int(chunkPosition.x * chunkSize + x, chunkPosition.y * chunkSize + y, 0);

                generationTiles.LoadTile(tilePosition, newChunk.tiles[x, y]);
            }
        }

        generationManager.chunks.Add(newChunk);
    }

    // Load saved chunk to the tilemap from memory
    void LoadSavedChunk(Vector2Int chunkPosition)
    {
        Chunk foundChunk = null;

        foreach (Chunk chunk in generationManager.chunks)
        {
            if (chunk.position == chunkPosition)
            {

                foundChunk = chunk;

                break;
            }
        }

        if (foundChunk != null)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    Vector3Int tilePosition = new Vector3Int(chunkPosition.x * chunkSize + x, chunkPosition.y * chunkSize + y, 0);

                    generationTiles.LoadTile(tilePosition, foundChunk.tiles[x, y]);
                }
            }
        }
    }

    // Unload unused chunk from the tilemap
    // This doesn't remove it from memory 
    void UnloadChunk(Vector2Int chunkPosition)
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int tilePosition = new Vector3Int(chunkPosition.x * chunkSize + x, chunkPosition.y * chunkSize + y, 0);

                generationTiles.UnloadTile(tilePosition);
            }
        }
    }

    bool ChunkContainsPos(Vector2Int chunkPosition)
    {
        foreach (Chunk chunk in generationManager.chunks)
        {
            if (chunk.position == chunkPosition)
            {
                return true;
            }
        }

        return false;
    }

    public Vector2Int GetChunkPosFromWorldPos(Vector3 position)
    {
        // Floor if positive, Ceil if negative

        int chunkX = position.x > 0 ? Mathf.FloorToInt(position.x / chunkSize) : Mathf.CeilToInt(position.x / chunkSize);
        int chunkY = position.y > 0 ? Mathf.FloorToInt(position.y / chunkSize) : Mathf.CeilToInt(position.y / chunkSize);

        return new Vector2Int(chunkX, chunkY);
    }
}

public enum GenerationMethod
{
    Random,
    PerlinNoise,
    WaveFunctionCollapse
}