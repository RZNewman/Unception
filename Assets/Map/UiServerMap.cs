using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static Atlas;
using static MonsterSpawn;

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

        Difficulty d = new Difficulty
        {
            pack = p,
            veteran = v,
        };

        return new Map
        {
            visualLocation = Vector2.zero,
            difficultyRangePercent = 0,
            floors = mapFloors(d),
            index = -1,
            power = power,
            difficulty = d
        };
    }
}
