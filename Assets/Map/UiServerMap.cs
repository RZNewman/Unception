using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static Atlas;

public class UiServerMap : MonoBehaviour
{
    public TMP_InputField pack;
    public TMP_InputField veteran;

    public Map getMap(float power)
    {
        float p;
        float v;

        if (!float.TryParse(pack.text, out p))
        {
            p = 0;
        }
        if (!float.TryParse(veteran.text, out v))
        {
            v = 0;
        }

        return new Map
        {
            visualLocation = Vector2.zero,
            difficultyRangePercent = 0,
            floors = mapFloors(),
            index = -1,
            power = power,
            difficulty = new MonsterSpawn.Difficulty
            {
                pack = p,
                veteran = v,
            }
        };
    }
}
