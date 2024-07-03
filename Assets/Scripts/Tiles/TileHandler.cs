using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TileHandler : MonoBehaviour
{
    // Variables
    public TileBehaviour tileBehaviour;
    public UIDocument tilesDoc;
    public Tilemap tilemap;
    public Tile emptyTile;
    public PixelPerfectCamera pixelPerfectCamera;

    [Header("Options")]
    public bool useVisualDelay = false;
    [Condition("useVisualDelay", true)]
    public float visualDelay = 0.3f;
    public int newPathBranchChance = 20;
    public GenType generationMethod;

    [Header("Grid")]
    public bool useCustomGridSize = true;

    [Condition("useCustomGridSize", true, true)]
    [ReadOnly]
    public int calculatedGridWidth = 10;
    [Condition("useCustomGridSize", true, true)]
    [ReadOnly]
    public int calculatedGridHeight = 10;

    [Condition("useCustomGridSize", true)]
    public int customGridWidth = 10;
    [Condition("useCustomGridSize", true)]
    public int customGridHeight = 10;
    [ReadOnly]
    public int tileCount = 0;

    [Header("Debug")]
    public bool generate = false;
    public bool clear = false;
    public bool runInUpdate = false;

    private TileType[,] tileData;

    private int gridWidth;
    private int gridHeight;
    private int halfGridWidth;
    private int halfGridHeight;

    [HideInInspector]
    public bool isGenerating = false;

    // References
    private NoiseGen noiseGen;
    private DiamondSquareGen diamondSquareGen;
    private MidpointDisplacementGen midpointDisplacementGen;
    private WaveFunctionCollapseGen waveFunctionCollapseGen;

    // UI
    private VisualElement root;
    private Button generateButton;
    private Button clearButton;

    void Start()
    {
        // Cache
        noiseGen = GetComponent<NoiseGen>();
        diamondSquareGen = GetComponent<DiamondSquareGen>();
        midpointDisplacementGen = GetComponent<MidpointDisplacementGen>();
        waveFunctionCollapseGen = GetComponent<WaveFunctionCollapseGen>();

        // UI
        root = tilesDoc.rootVisualElement;
        generateButton = root.Q<Button>("GenerateButton");
        clearButton = root.Q<Button>("ClearButton");

        // UI Taps
        generateButton.clicked += () => GenerateAndSetTiles();
        clearButton.clicked += () => ClearTiles();
    }

    void OnValidate()
    {
        if (generate)
        {
            generate = false;

            Glob.Validate(() =>
            {
                GenerateAndSetTiles();
            }, this);
        }

        if (clear)
        {
            clear = false;

            Glob.Validate(() =>
            {
                ClearTiles();
            }, this);
        }

        if (useCustomGridSize)
        {
            MakeGridSizeEven();

            tileCount = customGridWidth * customGridHeight;
        }
        else
        {
            tileCount = calculatedGridWidth * calculatedGridHeight;
        }
    }

    void Update()
    {
        if (runInUpdate && !isGenerating)
        {
            GenerateAndSetTiles();
        }
    }

    void ClearTiles()
    {
        tilemap.ClearAllTiles();
    }

    void GenerateAndSetTiles()
    {
        CalculatedGridSize();

        // Setup tile data
        tileData = new TileType[gridWidth, gridHeight];

        // Generate tile data
        switch (generationMethod)
        {
            case GenType.PerlinNoise:
                tileData = noiseGen.Generate(gridWidth, gridHeight, useVisualDelay);
                break;
            case GenType.SimplexNoise:
                tileData = noiseGen.Generate(gridWidth, gridHeight, useVisualDelay, true);
                break;
            case GenType.FractalNoise:
                tileData = noiseGen.Generate(gridWidth, gridHeight, useVisualDelay, false, true);
                break;
            case GenType.DiamondSquare:
                tileData = diamondSquareGen.Generate(gridWidth, gridHeight, useVisualDelay);
                break;
            case GenType.MidpointDisplacement:
                tileData = midpointDisplacementGen.Generate(gridWidth, gridHeight, useVisualDelay);
                break;
            case GenType.WaveFunctionCollapse:
                tileData = waveFunctionCollapseGen.Generate(gridWidth, gridHeight, useVisualDelay);
                break;
            default: // GenType.Random
                RandomGen();
                break;
        }

        if (tileData == null)
        {
            Debug.LogWarning("Generation method returned null! Try using a different generation method or fix the method!");
            return;
        }

        ClearTiles();

        // Set tilemap data
        for (int x = -halfGridWidth; x < halfGridWidth; x++)
        {
            for (int y = -halfGridHeight; y < halfGridHeight; y++)
            {
                Tile foundTile = GetTileFromType(tileData[x + halfGridWidth, y + halfGridHeight]);

                tilemap.SetTile(new Vector3Int(x, y, 0), foundTile);
            }
        }
    }

    void RandomGen()
    {
        isGenerating = true;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                tileData[x, y] = (TileType)UnityEngine.Random.Range(0, Enum.GetNames(typeof(TileType)).Length);
            }
        }

        isGenerating = false;
    }

    Tile GetTileFromType(TileType tileType)
    {
        for (int i = 0; i < tileBehaviour.ruleTiles.Length; i++)
        {
            if (tileBehaviour.ruleTiles[i].tileType == tileType)
            {
                return tileBehaviour.ruleTiles[i].tile;
            }
        }

        return emptyTile;
    }

    void MakeGridSizeEven()
    {
        if (customGridWidth % 2 == 1)
        {
            customGridWidth++;
        }

        if (customGridHeight % 2 == 1)
        {
            customGridHeight++;
        }
    }

    void CalculatedGridSize()
    {
        if (useCustomGridSize)
        {
            gridWidth = customGridWidth;
            gridHeight = customGridHeight;
        }
        else
        {
            float tempScreenWidth;
            float tempScreenHeight;

            if (Application.isPlaying)
            {
                tempScreenWidth = Screen.width;
                tempScreenHeight = Screen.height;
            }
            else
            {
                pixelPerfectCamera.runInEditMode = true;

                tempScreenWidth = Handles.GetMainGameViewSize().x;
                tempScreenHeight = Handles.GetMainGameViewSize().y;
            }

            gridWidth = Mathf.CeilToInt(tempScreenWidth / pixelPerfectCamera.pixelRatio / pixelPerfectCamera.assetsPPU);
            gridHeight = Mathf.CeilToInt(tempScreenHeight / pixelPerfectCamera.pixelRatio / pixelPerfectCamera.assetsPPU);

            if (gridWidth % 2 == 1)
            {
                gridWidth++;
            }

            if (gridHeight % 2 == 1)
            {
                gridHeight++;
            }

            calculatedGridWidth = gridWidth;
            calculatedGridHeight = gridHeight;
        }

        halfGridWidth = gridWidth / 2;
        halfGridHeight = gridHeight / 2;
    }
}

[Serializable]
public class TileData
{
    [HideInInspector]
    public string name;
    public Tile tile;
    public TileType tileType;
    public bool canBeNextToItself = true;
    [Condition("canBeNextToItself", true)]
    [ReadOnly]
    public int nextSelfChance = 100;
    [Condition("canBeNextToItself", true, true)]
    [ReadOnly]
    public int cumulativeChance = 0;
    public TileNext[] nextTiles;
}

[Serializable]
public class TileNext
{
    public TileType surroundingTiles;
    public int chance;
}

public enum TileType
{
    MountainTop,
    Mountain,
    MountainFoot,
    GrassRock,
    GrassBush,
    GrassFlowers,
    Grass,
    Desert,
    WaterShallow,
    Water,
    WaterDeep,
    // Path
}

public enum GenType
{
    Random,
    PerlinNoise,
    SimplexNoise,
    FractalNoise,
    DiamondSquare,
    MidpointDisplacement,
    WaveFunctionCollapse,
}