using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileBehaviour", menuName = "TileBehaviour")]
public class TileBehaviour : ScriptableObject
{
    public bool logWarnings = false;
    public TileData[] ruleTiles;

    void OnValidate()
    {
        for (int i = 0; i < ruleTiles.Length; i++)
        {
            string name = ruleTiles[i].tileType.ToString();

            ruleTiles[i].name = name;

            int cumulativeChance = 0;

            if (ruleTiles[i].neighboringTiles != null && ruleTiles[i].neighboringTiles.Length > 0)
            {

                for (int j = 0; j < ruleTiles[i].neighboringTiles.Length; j++)
                {
                    cumulativeChance += ruleTiles[i].neighboringTiles[j].chance;
                }
            }

            if (ruleTiles[i].canBeNextToItself)
            {
                int nextSelfChance = 100 - cumulativeChance;

                if (nextSelfChance < 0)
                {
                    if (logWarnings)
                    {
                        Debug.LogWarning(name + "'s next self chance is too low!");
                    }
                }
                else
                {
                    ruleTiles[i].nextSelfChance = nextSelfChance;
                }
            }
            else
            {
                ruleTiles[i].cumulativeChance = cumulativeChance;

                if (cumulativeChance != 100)
                {
                    ruleTiles[i].cumulativeChance = cumulativeChance;

                    if (logWarnings)
                    {
                        Debug.LogWarning(name + "'s cumulative chance isn't 100!");
                    }
                }
            }
        }
    }
}
