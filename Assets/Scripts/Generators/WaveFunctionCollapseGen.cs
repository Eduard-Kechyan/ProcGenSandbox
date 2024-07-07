using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionCollapseGen : MonoBehaviour
{
    // Variables
    public int loopThreshold = 1000;


    [Header("Options")]
    public bool logElapsedTime = false;

    private List<TileType> allTiles;
    private GridCell[,] grid;

    private int innerThreshold;

    // References
    private TileHandler tileHandler;

    void Start()
    {
        // Cache
        tileHandler = GetComponent<TileHandler>();

        InitializeTileTypes();
    }

    // Map all tiles types to a list
    // Needs to be run only once
    void InitializeTileTypes()
    {
        TileType[] allTimesArray = (TileType[])Enum.GetValues(typeof(TileType));

        allTiles = new List<TileType>(allTimesArray);
    }

    public TileType[,] Generate(int gridWidth, int gridHeight, bool useDelay)
    {
        // Initialize generation
        TileType[,] tileData = new TileType[gridWidth, gridHeight];
        grid = InitializeGrid(gridWidth, gridHeight);

        // ! Generation started
        tileHandler.isGenerating = true;
        var stopwatch = Glob.StartStopWatch();

        // Generating data
        GenerateTileData(gridWidth, gridHeight);

        // Set tile data
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                tileData[x, y] = grid[x, y].possibleTiles[0];
            }
        }

        // ! Generation finished
        if (logElapsedTime)
        {
            Debug.Log(Glob.FormatMilliseconds(Glob.StopStopWatch(stopwatch)));
        }
        tileHandler.isGenerating = false;

        // Return the calculated tiles
        return tileData;
    }

    // Initialize a grid with all possible tile types in each cell
    GridCell[,] InitializeGrid(int width, int height)
    {
        GridCell[,] grid = new GridCell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new GridCell(allTiles);
            }
        }

        return grid;
    }

    void GenerateTileData(int width, int height)
    {
        innerThreshold = loopThreshold;

        int count = 0;

        while (innerThreshold > 0)
        {
            int minOptions = int.MaxValue;
            int collapseX = -1;
            int collapseY = -1;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y].possibleTiles.Count > 1 && grid[x, y].possibleTiles.Count < minOptions)
                    {
                        minOptions = grid[x, y].possibleTiles.Count;

                        collapseX = x;
                        collapseY = y;

                        count++;

                        break;
                    }
                }
            }

            innerThreshold--;

            if (collapseX == -1 || collapseY == -1)
            {
                Debug.Log("Finished!");
                break;
            }

            CollapseCell(collapseX, collapseY);
        }

        Debug.Log(count);
    }

    void CollapseCell(int x, int y)
    {
        GridCell cell = grid[x, y];

        TileType chosenTile = cell.possibleTiles[UnityEngine.Random.Range(0, cell.possibleTiles.Count)];

        grid[x, y].possibleTiles = new List<TileType> { chosenTile };

        // Right
        if (x < grid.GetLength(0) - 1)
        {
            grid[x + 1, y].possibleTiles = ApplyRulesToNeighbor(chosenTile);
        }

        // Left
        if (x > 0)
        {
            grid[x - 1, y].possibleTiles = ApplyRulesToNeighbor(chosenTile);
        }

        // Top
        if (y < grid.GetLength(1) - 1)
        {
            grid[x, y + 1].possibleTiles = ApplyRulesToNeighbor(chosenTile);
        }

        // Bottom
        if (y > 0)
        {
            grid[x, y - 1].possibleTiles = ApplyRulesToNeighbor(chosenTile);
        }
    }

    List<TileType> ApplyRulesToNeighbor(TileType chosenTile)
    {
        List<TileType> filteredTileTypes = new List<TileType>();

        for (int i = 0; i < tileHandler.tileBehaviourWFC.ruleTiles.Length; i++)
        {
            if (tileHandler.tileBehaviourWFC.ruleTiles[i].tileType == chosenTile)
            {
                foreach (TileNeighbor tileNeighbor in tileHandler.tileBehaviourWFC.ruleTiles[i].neighboringTiles)
                {
                    filteredTileTypes.Add(tileNeighbor.tileType);
                }

                if (tileHandler.tileBehaviourWFC.ruleTiles[i].canBeNextToItself)
                {
                    filteredTileTypes.Add(chosenTile);
                }

                break;
            }
        }

        return filteredTileTypes;
    }
}

[Serializable]
public class GridCell
{
    public List<TileType> possibleTiles;

    public GridCell(List<TileType> newPossibleTiles)
    {
        possibleTiles = new List<TileType>(newPossibleTiles);
    }
}