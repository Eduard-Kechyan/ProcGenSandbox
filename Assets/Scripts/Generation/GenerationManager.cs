using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.U2D;

public class GenerationManager : MonoBehaviour
{
    // Variables
    public TileBehaviour tileBehaviour;
    public UIDocument uiDoc;
    public PixelPerfectCamera pixelPerfectCamera;

    [Header("Debug")]
    public bool useUpdateForMovementChecking = false;
    public bool logElapsedTime = false;
    public bool generateAtStart = false;
    public bool generateInUpdate = false;

    [Header("Stats")]
    [ReadOnly]
    public int chunkCount = 0;
    [ReadOnly]
    public int tileCount = 0;

    [HideInInspector]
    public bool initialChunksGenerated = false;

    //[HideInInspector]
    public List<Chunk> chunks = new();

    // References
    private GenerationChunks generationChunks;
    private GenerationTiles generationTiles;

    // UI
    private VisualElement root;
    private Button generateButton;

    void Start()
    {
        // Cache
        generationChunks = GetComponent<GenerationChunks>();
        generationTiles = GetComponent<GenerationTiles>();

        // UI
        root = uiDoc.rootVisualElement;
        generateButton = root.Q<Button>("GenerateButton");

        // UI Taps
        generateButton.clicked += () => GenerateChunks();

        if (generateAtStart)
        {
            StartCoroutine(GenerateChunksAtStart());
        }
    }

    void Update()
    {
        if (useUpdateForMovementChecking && initialChunksGenerated && !generateInUpdate)
        {
            OnWorldMoved(transform.parent.position);
        }

        if (generateInUpdate)
        {
            GenerateChunks();
        }
    }

    // Generate chunks when the world moves
    public void OnWorldMoved(Vector2 worldPos)
    {
        // Invert the world position, since we are moving the world and not the camera
        Vector2 invertedPos = new Vector2(-worldPos.x, -worldPos.y);

        Vector2Int worldPosChunk = generationChunks.GetChunkPosFromWorldPos(invertedPos);

        // Check if we are on a different chunk
        if (worldPosChunk != generationChunks.currentChunkPos)
        {
            generationChunks.currentChunkPos = worldPosChunk;

            generationChunks.UpdateChunks();
        }
    }

    // Initial chunk generation
    void GenerateChunks()
    {
        var stopwatch = Glob.StartStopWatch();

        generationChunks.CalcSizes();

        generationTiles.ClearTilemap();

        generationChunks.currentChunkPos = generationChunks.GetChunkPosFromWorldPos(Vector3.zero);

        generationChunks.UpdateChunks();

        initialChunksGenerated = true;

        // Log out elapsed time
        if (logElapsedTime)
        {
            Debug.Log(Glob.FormatMilliseconds(Glob.StopStopWatch(stopwatch)));
        }
    }

    IEnumerator GenerateChunksAtStart()
    {
        yield return new WaitForSeconds(0.03f);

        GenerateChunks();
    }
}