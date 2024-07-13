using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;

public class GenerationTiles : MonoBehaviour
{
    // Variables
    public Tilemap tilemap;
    public Tile emptyTile;

    [Header("Debug")]
    public bool clearTilemap = false;

    public bool debugTileLoading = false;
    [Condition("debugTileLoading", true)]
    public Color tileLoadingColorNull;
    [Condition("debugTileLoading", true)]
    public Color tileLoadingColorFull;
    [Condition("debugTileLoading", true)]
    public float tileLoadingFadeDuration = 0.5f;

    // References
    private GenerationManager generationManager;
    private GenerationChunks generationChunks;

    // UI
    private VisualElement root;
    private Button clearButton;

    void Start()
    {
        // Cache
        generationManager = GetComponent<GenerationManager>();
        generationChunks = GetComponent<GenerationChunks>();

        // UI
        root = generationManager.uiDoc.rootVisualElement;
        clearButton = root.Q<Button>("ClearButton");

        // UI Taps
        clearButton.clicked += () => ClearTilemap();
    }

    void OnValidate()
    {
        if (clearTilemap)
        {
            clearTilemap = false;

            Glob.Validate(() =>
            {
                ClearTilemap();
            }, this);
        }
    }

    public void ClearTilemap()
    {
        tilemap.ClearAllTiles();

        generationManager.chunks = new();
        generationChunks.loadedChunks = new HashSet<Vector2Int>();
    }

    public void LoadTile(Vector3Int position, TileType tileType)
    {
        TileBase tile = GetTileFromType(tileType);

        tilemap.SetTile(position, tile);

        if (debugTileLoading)
        {
            StartCoroutine(FadeInTile(position));
        }
    }

    public void UnloadTile(Vector3Int position)
    {
        tilemap.SetTile(position, null);
    }

    IEnumerator FadeInTile(Vector3Int tilePosition)
    {
        tilemap.SetTileFlags(tilePosition, TileFlags.LockTransform);

        tilemap.SetColor(tilePosition, tileLoadingColorNull);

        float elapseTime = 0f;

        while (elapseTime < tileLoadingFadeDuration)
        {
            tilemap.SetColor(tilePosition, Color.Lerp(tileLoadingColorNull, tileLoadingColorFull, elapseTime / tileLoadingFadeDuration));

            elapseTime += Time.deltaTime;

            yield return null;
        }

        tilemap.SetColor(tilePosition, tileLoadingColorFull);
    }

    Tile GetTileFromType(TileType tileType)
    {
        foreach (TileData rule in generationManager.tileBehaviour.ruleTiles)
        {
            if (rule.tileType == tileType)
            {
                return rule.tile;
            }
        }

        return emptyTile;
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
    public TileNeighbor[] neighboringTiles;
}

[Serializable]
public class TileNeighbor
{
    public TileType tileType;
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