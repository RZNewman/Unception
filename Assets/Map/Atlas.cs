using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MonsterSpawn;

public class Atlas : NetworkBehaviour
{
    public static readonly int avgPacksPerFloor = 30;
    public static readonly float difficultyRange = 1;

    public RectTransform mapImage;
    public GameObject mapMarkerPre;
    public UiMapDetails mapDeets;
    public Button embarkButton;

    PlayerGhost owner;
    GlobalPlayer gp;

    [SyncVar(hook = nameof(hookMaps))]
    MapListing list;

    public struct MapListing
    {
        public Map[] maps;
    }
    public struct Map
    {
        public int index;
        public float power;
        public Difficulty difficulty;
        public Floor[] floors;
        public Vector2 visualLocation;
    }
    public struct Floor
    {
        public int packs;
        public int tiles;
    }

    public Map getMap(int index)
    {
        return list.maps[index];
    }

    public void makeMaps()
    {
        if (!gp)
        {
            gp = FindObjectOfType<GlobalPlayer>();
        }
        list = new MapListing
        {
            maps = generateMapOpitons(gp.serverPlayer.power),
        };
        if (isClient)
        {
            createMapMarkers();
        }
    }

    void createMapMarkers()
    {
        foreach(Map m in list.maps)
        {
            GameObject marker = Instantiate(mapMarkerPre, mapImage.transform);
            Vector2 size = mapImage.sizeDelta - Vector2.one * 25f;
            marker.transform.localPosition = (size * m.visualLocation) - size / 2;
            marker.GetComponent<UiMapMarker>().init(this, m);
        }
    }

    void hookMaps(MapListing old, MapListing neww){
        createMapMarkers();
    }

    public Map[] generateMapOpitons(float power, float baseDifficulty = 0)
    {
        int mapsToGen = Random.Range(4, 7);
        Map[] mapsGen = new Map[mapsToGen];

        for (int i = 0; i < mapsToGen; i++)
        {
            float currentDifficulty = baseDifficulty + Mathf.Lerp(0, difficultyRange, (float)i / (mapsToGen - 1));
            mapsGen[i] = new Map
            {
                index = i,
                power = power,
                difficulty = Difficulty.fromTotal(currentDifficulty),
                floors = new Floor[] { new Floor { packs = avgPacksPerFloor, tiles = avgPacksPerFloor + 10 } },
                visualLocation = new Vector2(Random.value, Random.value),
            };
        }

        return mapsGen;
    }

    UiMapMarker displayMap;
    UiMapMarker selectedMap;
    public void setDisplay(UiMapMarker m)
    {
        displayMap = m;
        mapDeets.setMapDetails(m.getMap());
    }

    public void clearDisplay(UiMapMarker m)
    {
        if(displayMap == m)
        {
            if (selectedMap)
            {
                mapDeets.setMapDetails(selectedMap.getMap());
            }
            else
            {
                mapDeets.clearLabels();
            }
        }
        
        
    }
    public void selectMap(UiMapMarker m)
    {
        if (selectedMap)
        {
            selectedMap.deselect();
        }
        selectedMap = m;
        embarkButton.interactable = true;
    }

    [Client]
    public void embark()
    {
        embarkButton.interactable = false;
        gp.player.embark(selectedMap.getMap().index);
    }


}
