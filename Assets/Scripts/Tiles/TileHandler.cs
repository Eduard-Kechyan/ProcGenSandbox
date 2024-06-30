using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TileHandler : MonoBehaviour
{
    // Variables
    public TileBehaviour tileBehaviour;
    public UIDocument tilesDoc;
    public Tilemap tilemap;
    public PixelPerfectCamera pixelPerfectCamera;
    public int newPathBranchChance = 20;
    public bool calcGridAutomatically = true;
    [Condition("calcGridAutomatically", true, true)]
    public int gridWidth = 10;
    [Condition("calcGridAutomatically", true, true)]
    public int gridHeight = 10;

    [Header("Debug")]
    public bool generate = false;

    private TileType[,] tilesData;

    private int halfGridWidth;
    private int halfGridHeight;

    // UI
    private VisualElement root;
    private Button generateButton;

    void Start()
    {
        // UI
        root = tilesDoc.rootVisualElement;
        generateButton = root.Q<Button>("GenerateButton");

        // UI Taps
        generateButton.clicked += () => GeneratesTiles();
    }

    void OnValidate()
    {
        if (generate)
        {
            generate = false;

            Validate(() =>
            {
                GeneratesTiles();
            }, this);
        }

        MakeGridSizeEven();
    }

    public void GeneratesTiles()
    {
        // tilesData = new TileType[gridWidth, gridHeight];

        CalcGridSize();

        tilemap.ClearAllTiles();

        for (int x = -halfGridWidth; x < halfGridWidth; x++)
        {
            for (int y = -halfGridHeight; y < halfGridHeight; y++)
            {
                int chosenTile = UnityEngine.Random.Range(0, Enum.GetNames(typeof(TileType)).Length);

                tilemap.SetTile(new Vector3Int(x, y, 0), tileBehaviour.ruleTiles[chosenTile].tile);
            }
        }
    }

    void MakeGridSizeEven()
    {
        if (gridWidth % 2 == 1)
        {
            gridWidth++;
        }

        if (gridHeight % 2 == 1)
        {
            gridHeight++;
        }
    }

    void CalcGridSize()
    {
        if (calcGridAutomatically)
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
                tempScreenWidth = Handles.GetMainGameViewSize().x;
                tempScreenHeight = Handles.GetMainGameViewSize().y;
            }

            halfGridWidth = Mathf.CeilToInt(tempScreenWidth / pixelPerfectCamera.pixelRatio / pixelPerfectCamera.assetsPPU / 2);
            halfGridHeight = Mathf.CeilToInt(tempScreenHeight / pixelPerfectCamera.pixelRatio / pixelPerfectCamera.assetsPPU / 2);
        }
        else
        {
            halfGridWidth = gridWidth / 2;
            halfGridHeight = gridHeight / 2;
        }
    }

    void Validate(Action callback, params UnityEngine.Object[] newObjects)
    {
        void NextUpdate()
        {
            EditorApplication.update -= NextUpdate;

            if (newObjects.Any(c => !c))
            {
                return;
            }

            if (newObjects.All(c => !EditorUtility.IsDirty(c)))
            {
                return;
            }

            callback?.Invoke();

            foreach (UnityEngine.Object component in newObjects)
            {
                EditorUtility.SetDirty(component);
            }
        }

        EditorApplication.update += NextUpdate;
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
    Grass,
    GrassFlowers,
    GrassBush,
    GrassRock,
    Desert,
    WaterShallow,
    Water,
    WaterDeep,
    MountainFoot,
    Mountain,
    MountainTop,
    // Path
}
