using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

public class TilemapManager : MonoBehaviour
{
    // Variables
    // Variables
    public int chunkSize = 4;
    public GameObject dummyPrefab;

    [Space(20)]
    public TileBehaviour tileBehaviour;
    public UIDocument tilesDoc;
    public Tilemap tilemap;
    public PixelPerfectCamera pixelPerfectCamera;
    public Tile emptyTile;

    [Header("Grid")]
    public bool useCustomGridSize = true;

    [Condition("useCustomGridSize", true, true)]
    [ReadOnly]
    public int gridWidth;
    [Condition("useCustomGridSize", true, true)]
    [ReadOnly]
    public int gridHeight;

    [Condition("useCustomGridSize", true)]
    public int customGridWidth = 10;
    [Condition("useCustomGridSize", true)]
    public int customGridHeight = 10;

    [Header("Debug")]
    public bool clearTilemap = false;
    public bool reCalcSizes = false;
    public bool useUpdateForMovementChecking = false;

    [Header("Stats")]
    [ReadOnly]
    public int chunkCount = 0;
    [ReadOnly]
    public int tileCount = 0;

    private int calculatedGridWidth = 10;
    private int calculatedGridHeight = 10;

    private float halfGridWidth = 0f;
    private float halfGridHeight = 0f;

    [HideInInspector]
    public bool initialChunksGenerated = false;

    private Chunk[,] chunks;

    private Vector2Int currentChunk;
    private HashSet<Vector2Int> loadedChunks = new HashSet<Vector2Int>();

    // UI
    private VisualElement root;
    private Button generateButton;
    private Button clearButton;

    void Start()
    {
        // UI
        root = tilesDoc.rootVisualElement;
        generateButton = root.Q<Button>("GenerateButton");
        clearButton = root.Q<Button>("ClearButton");

        // UI Taps
        generateButton.clicked += () => GenerateChunks();
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

        if (reCalcSizes)
        {
            reCalcSizes = false;

            Glob.Validate(() =>
            {
                CalcSizes(true);
            }, this);
        }

        CalcSizes();
    }

    void ClearTilemap()
    {
        tilemap.ClearAllTiles();
    }

    void CalcSizes(bool isAlt = false)
    {
        if (useCustomGridSize)
        {
            gridWidth = customGridWidth;
            gridHeight = customGridHeight;
        }
        else
        {
            if (isAlt)
            {
                CalcGridSizes();

                gridWidth = calculatedGridWidth + 2;
                gridHeight = calculatedGridHeight + 2;
            }
        }

        halfGridWidth = (float)gridWidth / 2;
        halfGridHeight = (float)gridHeight / 2;

        chunkCount = gridWidth * gridHeight;

        tileCount = chunkCount * (chunkSize * chunkSize);
    }

    void CalcGridSizes()
    {
        calculatedGridWidth = Mathf.CeilToInt((float)Screen.width / pixelPerfectCamera.pixelRatio / (pixelPerfectCamera.assetsPPU * chunkSize));
        calculatedGridHeight = Mathf.CeilToInt((float)Screen.height / pixelPerfectCamera.pixelRatio / (pixelPerfectCamera.assetsPPU * chunkSize));
    }

    void GenerateChunks()
    {
        CalcSizes(true);

        ClearTilemap();

        currentChunk = GetChunkFromPosition(Vector3.zero);

        UpdateChunks();

        initialChunksGenerated = true;
    }

    void Update()
    {
        if (useUpdateForMovementChecking && initialChunksGenerated)
        {
            WorldMoved(transform.parent.position);
        }
    }

    public void WorldMoved(Vector2 worldPos)
    {
        Vector2 invertedPos = new Vector2(-worldPos.x, -worldPos.y);

        Vector2Int newChunk = GetChunkFromPosition(invertedPos);
        if (newChunk != currentChunk)
        {
            currentChunk = newChunk;
            UpdateChunks();
        }
    }

    Vector2Int GetChunkFromPosition(Vector3 position)
    {
        int chunkX = Mathf.FloorToInt(position.x / chunkSize);
        int chunkY = Mathf.FloorToInt(position.y / chunkSize);
        return new Vector2Int(chunkX, chunkY);
    }

    void UpdateChunks()
    {
        HashSet<Vector2Int> newLoadedChunks = new HashSet<Vector2Int>();

        for (int x = -gridWidth / 2; x <= gridWidth / 2; x++)
        {
            for (int y = -gridHeight / 2; y <= gridHeight / 2; y++)
            {
                Vector2Int chunkPosition = new Vector2Int(currentChunk.x + x, currentChunk.y + y);
                newLoadedChunks.Add(chunkPosition);

                if (!loadedChunks.Contains(chunkPosition))
                {
                    LoadChunk(chunkPosition);
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

    void LoadChunk(Vector2Int chunkPosition)
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int tilePosition = new Vector3Int(chunkPosition.x * chunkSize + x, chunkPosition.y * chunkSize + y, 0);
                TileBase tile = GetTileFromType((TileType)UnityEngine.Random.Range(0, Enum.GetNames(typeof(TileType)).Length));
                tilemap.SetTile(tilePosition, tile);
            }
        }
    }

    void UnloadChunk(Vector2Int chunkPosition)
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int tilePosition = new Vector3Int(chunkPosition.x * chunkSize + x, chunkPosition.y * chunkSize + y, 0);
                tilemap.SetTile(tilePosition, null);
            }
        }
    }

    Tile GetTileFromType(TileType tileType)
    {
        foreach (TileData rule in tileBehaviour.ruleTiles)
        {
            if (rule.tileType == tileType)
            {
                return rule.tile;
            }
        }

        return emptyTile;
    }










    /* // Variables
     public int chunkSize = 4;
     public GameObject dummyPrefab;

     [Space(20)]
     public TileBehaviour tileBehaviour;
     public UIDocument tilesDoc;
     public Tilemap tilemap;
     public PixelPerfectCamera pixelPerfectCamera;
     public Tile emptyTile;

     [Header("Grid")]
     public bool useCustomGridSize = true;

     [Condition("useCustomGridSize", true, true)]
     [ReadOnly]
     public int gridWidth;
     [Condition("useCustomGridSize", true, true)]
     [ReadOnly]
     public int gridHeight;

     [Condition("useCustomGridSize", true)]
     public int customGridWidth = 10;
     [Condition("useCustomGridSize", true)]
     public int customGridHeight = 10;

     [Header("Debug")]
     public bool clearTilemap = false;
     public bool reCalcSizes = false;

     [Header("Stats")]
     [ReadOnly]
     public int chunkCount = 0;
     [ReadOnly]
     public int tileCount = 0;

     private int calculatedGridWidth = 10;
     private int calculatedGridHeight = 10;

     private float halfGridWidth = 0f;
     private float halfGridHeight = 0f;

     private Chunk[,] chunks;

     private Vector4 lastChunkPos = Vector4.zero;

     // UI
     private VisualElement root;
     private Button generateButton;
     private Button clearButton;*/

    /*  void Start()
      {
          // UI
          root = tilesDoc.rootVisualElement;
          generateButton = root.Q<Button>("GenerateButton");
          clearButton = root.Q<Button>("ClearButton");

          // UI Taps
          generateButton.clicked += () => GenerateChunks();
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

          if (reCalcSizes)
          {
              reCalcSizes = false;

              Glob.Validate(() =>
              {
                  CalcSizes(true);
              }, this);
          }

          CalcSizes();
      }

      void ClearTilemap()
      {
          tilemap.ClearAllTiles();
      }

      void CalcSizes(bool isAlt = false)
      {
          if (useCustomGridSize)
          {
              gridWidth = customGridWidth;
              gridHeight = customGridHeight;
          }
          else
          {
              if (isAlt)
              {
                  CalcGridSizes();

                  gridWidth = calculatedGridWidth + 2;
                  gridHeight = calculatedGridHeight + 2;
              }
          }

          halfGridWidth = (float)gridWidth / 2;
          halfGridHeight = (float)gridHeight / 2;

          chunkCount = gridWidth * gridHeight;

          tileCount = chunkCount * (chunkSize * chunkSize);
      }

      void CalcGridSizes()
      {
          calculatedGridWidth = Mathf.CeilToInt((float)Screen.width / pixelPerfectCamera.pixelRatio / (pixelPerfectCamera.assetsPPU * chunkSize));
          calculatedGridHeight = Mathf.CeilToInt((float)Screen.height / pixelPerfectCamera.pixelRatio / (pixelPerfectCamera.assetsPPU * chunkSize));
      }
  */
    //// INITIAL GENERATION ////

    /* void GenerateChunks()
     {
         ClearTilemap();

         CalcSizes(true);

         chunks = new Chunk[gridWidth, gridHeight];

         for (int x = 0; x < gridWidth; x++)
         {
             for (int y = 0; y < gridHeight; y++)
             {
                 chunks[x, y] = new Chunk()
                 {
                     tiles = GenerateRandomTiles(),
                     position = new Vector2(x * chunkSize, y * chunkSize)
                 };
             }
         }

         PopulateTilemap();
     }

     void PopulateTilemap()
     {
         Vector2Int offset = new Vector2Int((gridWidth * chunkSize) / 2, (gridHeight * chunkSize) / 2);

         for (int x = 0; x < gridWidth; x++)
         {
             for (int y = 0; y < gridHeight; y++)
             {
                 SetTiles(chunks[x, y], offset);
             }
         }
     }
 */
    //// MOVEMENT HANDLING ////

    //  public void WorldMoved(Vector2 worldPos)
    // {


    /* Vector4 currentChunkPos = Vector4.zero;
     Vector2 currentChunkPosAlt = Vector2.zero;
     bool isXPositive = true;
     bool isYPositive = true;
     float xPos = worldPos.x;
     float yPos = worldPos.y;

     Vector2Int offset = new Vector2Int((gridWidth * chunkSize) / 2, (gridHeight * chunkSize) / 2);

     // Round the positions
     if (xPos < 0)
     {
         xPos = Mathf.Ceil(worldPos.x / chunkSize);

         isXPositive = false;
     }
     else
     {
         xPos = Mathf.Floor(worldPos.x / chunkSize);
     }

     if (yPos < 0)
     {
         yPos = Mathf.Ceil(worldPos.y / chunkSize);

         isYPositive = false;
     }
     else
     {
         yPos = Mathf.Floor(worldPos.y / chunkSize);
     }

     // Set positives
     if (isXPositive && xPos > currentChunkPos.x)
     {
         currentChunkPos.x = xPos;
         currentChunkPosAlt.x = xPos;
     }

     if (isYPositive && yPos > currentChunkPos.y)
     {
         currentChunkPos.y = yPos;
         currentChunkPosAlt.y = yPos;
     }

     // Set negatives
     if (!isXPositive && xPos < currentChunkPos.z)
     {
         currentChunkPos.z = xPos;
         currentChunkPosAlt.x = xPos;
     }

     if (!isYPositive && yPos < currentChunkPos.w)
     {
         currentChunkPos.w = yPos;
         currentChunkPosAlt.y = yPos;
     }

     if (currentChunkPos != lastChunkPos)
     {
         lastChunkPos = currentChunkPos;

         Debug.Log(currentChunkPosAlt);
         Debug.Log(new Vector3((currentChunkPosAlt.x * chunkSize) * halfGridWidth - chunkSize, (currentChunkPosAlt.y * chunkSize) * halfGridHeight - chunkSize, 0));

         GameObject newDummyPrefab = Instantiate(dummyPrefab, Vector3.zero, Quaternion.identity);

         newDummyPrefab.transform.parent = transform;
         newDummyPrefab.transform.position = new Vector3((currentChunkPosAlt.x * chunkSize) * halfGridWidth - chunkSize, (currentChunkPosAlt.y * chunkSize) * halfGridWidth - chunkSize, 0);

         Debug.Log("/////////////////////");
          Debug.Log(currentChunkPos);

         List<Vector2> chunksToLoad = new();

         for (int x = -chunkSize; x <= chunkSize; x++)
          {
              for (int y = -chunkSize; y <= chunkSize; y++)
              {
                  Vector2 chunkPosition = new Vector2(chunkPos.x + x, chunkPos.y + y);

                  if (!chunksToLoad.Contains(chunkPosition))
                  {
                      chunksToLoad.Add(chunkPosition);

                      if (x == 0 && y == 0)
                      {
                          Debug.Log(chunkPosition);
                      }
                  }
              }
          }
     }*/

    // Set Chunk Pos
    /* Vector2 currentChunkPos = new Vector2(Mathf.Floor(worldPos.x / (chunkSize + halfGridWidth)), Mathf.Floor(worldPos.y / (chunkSize + halfGridHeight)));

     if (currentChunkPos != currentChunkPos)
     {
         currentChunkPos = currentChunkPos;

         Vector2 chunkPos = new Vector2(Mathf.Floor(worldPos.x / (chunkSize + halfGridWidth)), Mathf.Floor(worldPos.y / (chunkSize + halfGridHeight)));

         halfGridWidth++;
         halfGridHeight++;

         List<Vector2> chunksToLoad = new();

         for (int x = -Mathf.CeilToInt(loadDistance); x <= Mathf.CeilToInt(loadDistance); x++)
         {
             for (int y = -Mathf.CeilToInt(loadDistance); y <= Mathf.CeilToInt(loadDistance); y++)
             {
                 Vector2 chunkPosition = new Vector2(chunkPos.x + x, chunkPos.y + y);

                 if (!chunksToLoad.Contains(chunkPosition))
                 {
                     chunksToLoad.Add(chunkPosition);

                     if (x == 0 && y == 0)
                     {
                         Debug.Log(chunkPosition);
                     }
                 }
             }
         }
     }*/
    //}

    //// TILE HANDLING ////

    /*TileType[,] GenerateRandomTiles()
    {
        TileType[,] newTiles = new TileType[chunkSize, chunkSize];

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                newTiles[x, y] = (TileType)UnityEngine.Random.Range(0, Enum.GetNames(typeof(TileType)).Length);
            }
        }

        return newTiles;
    }

    void SetTiles(Chunk chunk, Vector2Int offset)
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int newTilePos = new Vector3Int(x + (int)(chunk.position.x) - offset.x, y + (int)(chunk.position.y) - offset.y, 0);

                tilemap.SetTile(newTilePos, GetTileFromType(chunk.tiles[x, y]));
            }
        }
    }

    Tile GetTileFromType(TileType tileType)
    {
        foreach (TileData rule in tileBehaviour.ruleTiles)
        {
            if (rule.tileType == tileType)
            {
                return rule.tile;
            }
        }

        return emptyTile;
    }*/
}
