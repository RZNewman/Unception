using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static MonsterSpawn;

public class Atlas : NetworkBehaviour
{
    public static readonly int avgPacksPerFloor = 20;
    public readonly static float avgFloorsPerMap = 2f;
    public static readonly float difficultyRange = 1;

    public RectTransform mapImage;
    public GameObject mapMarkerPre;
    public UiMapDetails mapDeets;
    public Button embarkButton;

    PlayerGhost owner;
    GlobalPlayer gp;
    SoundManager sound;

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
        public float difficultyRangePercent;
    }
    public struct Floor
    {
        public int packs;
        public int tiles;
    }
    private void Start()
    {
        sound = FindObjectOfType<SoundManager>();
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
        foreach (Map m in list.maps)
        {
            GameObject marker = Instantiate(mapMarkerPre, mapImage.transform);
            Vector2 size = mapImage.sizeDelta - Vector2.one * 25f;
            marker.transform.localPosition = (size * m.visualLocation) - size / 2;
            marker.GetComponent<UiMapMarker>().init(this, m);
        }
    }
    void clearMapMarkers()
    {
        selectedMap = null;
        displayMap = null;
        foreach (Transform child in mapImage.transform)
        {
            Destroy(child.gameObject);
        }
    }

    void hookMaps(MapListing old, MapListing neww)
    {
        createMapMarkers();
    }

    public Map[] generateMapOpitons(float power, float baseDifficulty = 0)
    {
        int mapsToGen = Random.Range(4, 7);
        Map[] mapsGen = new Map[mapsToGen];

        for (int i = 0; i < mapsToGen; i++)
        {
            float difficultyRangePercent = (float)i / (mapsToGen - 1);
            float currentDifficulty = baseDifficulty + Mathf.Lerp(0, difficultyRange, difficultyRangePercent);
            int floorCount = Mathf.RoundToInt(avgFloorsPerMap);
            Floor[] floors = new Floor[floorCount];
            for (int j = 0; j < floorCount; j++)
            {
                floors[j] = new Floor
                {
                    packs = avgPacksPerFloor,
                    tiles = avgPacksPerFloor + 10
                };
            }

            mapsGen[i] = new Map
            {
                index = i,
                power = power,
                difficulty = Difficulty.fromTotal(currentDifficulty),
                floors = floors,
                visualLocation = new Vector2(Random.value, Random.value),
                difficultyRangePercent = difficultyRangePercent,
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
        if (displayMap == m)
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
        checkEmbarkButton();
    }

    [Client]
    public void inventoryChange(Inventory inv)
    {
        checkEmbarkButton();
    }
    void checkEmbarkButton()
    {
        embarkButton.interactable = selectedMap && !isBurdened();
    }
    bool isBurdened()
    {
        foreach (Inventory inv in FindObjectsOfType<Inventory>())
        {
            if (inv.overburdened)
            {
                return true;
            }
        }
        return false;
    }

    [Client]
    public void embark()
    {
        embarkButton.interactable = false;
        sound.playSound(SoundManager.SoundClip.Embark);
        gp.player.embark(selectedMap.getMap().index);
    }
    //Server
    bool onMission = false;
    public bool embarked
    {
        get
        {
            return onMission;
        }
    }
    [Server]
    public IEnumerator embarkServer(int index)
    {
        bool success = !onMission && !isBurdened();
        if (!success)
        {
            yield break;
        }
        onMission = true;
        MapGenerator gen = FindObjectOfType<MapGenerator>();
        yield return gen.buildMap(list.maps[index]);
    }

    [Server]
    public void disembark(bool needsMapDestroyed = false)
    {
        onMission = false;
        foreach (GameObject unit in FindObjectsOfType<PlayerGhost>().Select(g => g.unit))
        {
            Destroy(unit);
        }
        if (needsMapDestroyed)
        {
            FindObjectOfType<MapGenerator>().destroyFloor();
        }
        clearMapMarkers();
        makeMaps();
        foreach (Inventory inv in FindObjectsOfType<Inventory>())
        {
            inv.syncInventoryUpwards();
            inv.GetComponent<PlayerGhost>().TargetMainMenu(inv.connectionToClient);
        }

    }


}
