using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour
{
    // Variables
    public int newPathBranchChance = 20;

    private List<Chunk> chunks = new();


}

[Serializable]
public class Chunk
{
    public Vector2 position;
    public Cell[,] cells;
    public TileType[,] tiles; // TMP
}

[Serializable]
public class Cell
{
    public List<TileType> tileTypes;
}
