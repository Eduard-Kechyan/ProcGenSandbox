using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerationSeed : MonoBehaviour
{
    // Variables
    public bool useSeed = false;

    [Condition("useSeed", true)]
    public bool useCustomSeed = false;

    [Condition(new string[] { "useSeed", "useCustomSeed" }, true, false)]
    public int customSeed = 0;

    [Condition(new string[] { "useSeed", "useCustomSeed" }, new bool[] { false, true }, true, false)]
    [ReadOnly]
    public int seed = 0;

    [Condition(new string[] { "useSeed", "useCustomSeed" }, new bool[] { false, true }, true, false)]
    public bool copySeed = false;

    // References
    private GenerationManager generationManager;

    void Start()
    {
        // Cache
        generationManager = GetComponent<GenerationManager>();

        HandleSeed();
    }

    void OnValidate()
    {
        if (copySeed)
        {
            copySeed = false;

            CopySeed();
        }
    }

    void HandleSeed()
    {
        if (useSeed)
        {
            if (useCustomSeed)
            {
                seed = customSeed;
            }
            else
            {
                seed = Random.Range(int.MinValue, int.MaxValue);
            }

            Random.InitState(seed);
        }
    }

    void CopySeed()
    {
        GUIUtility.systemCopyBuffer = seed.ToString();
    }
}
