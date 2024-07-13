using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour
{
    // Variables
    public int newPathBranchChance = 20;

    private List<Chunk> chunks = new();

    // References
    private GenerationChunks generationChunks;

    void Start()
    {
        // Cache
        generationChunks = GetComponent<GenerationChunks>();
    }

    public bool Generate(out TileType[,] generatedTileData)
    {
        generatedTileData = new TileType[generationChunks.chunkSize, generationChunks.chunkSize];

        return true;
    }
}

[Serializable]
public class Chunk
{
    public Vector2Int position;
    public Cell[,] cells;
    public TileType[,] tiles; // TMP
}

[Serializable]
public class ChunkJson
{
    public Vector2Int position;
    public Cell[,] cells;
    public TileType[,] tiles; // TMP
}

[Serializable]
public class Cell
{
    public List<TileType> tileTypes;
}
