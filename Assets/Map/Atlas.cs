using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static MonsterSpawn;
using static Utils;

public class Atlas : NetworkBehaviour
{
    public static readonly int avgPacksPerMap = 40;
    public static readonly int chestPerFloor = 1;
    public static readonly int potPerFloor = 3;
    public readonly static float avgFloorsPerMap = 2f;
    public static readonly float difficultyRange = 1;

    public RectTransform mapImage;
    public GameObject mapMarkerPre;
    public UiMapDetails mapDeets;
    public Button embarkButton;
    public UiServerMap serverMap;
    public WorldData worldData;

    PlayerGhost owner;
    GlobalPlayer gp;
    SoundManager sound;

    [SyncVar(hook = nameof(hookMaps))]
    MapListing list;

    public struct MapListing
    {
        public Map[] maps;
    }

    public struct Location
    {
        public Vector2 visualLocation;
        public QuestVertical[] verticals;
        public int overrideTier;
    }
    public struct QuestVertical
    {
        public Quest[] quests;
    }

    public struct Quest
    {
        public int tier;
        public Difficulty difficulty;
        public int packs;
        public int floors;
        public int encounters;
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
        public float sparseness;
        public EncounterData[] encounters;
    }
    public struct EncounterData
    {
        public Difficulty difficulty;
        public int packs;
        public EncounterType type;
    }
    public enum EncounterType
    {
        Ambush
    }
    private void Start()
    {
        sound = FindObjectOfType<SoundManager>();
        serverMap.gameObject.SetActive(isServer);
        makeWorld();
        FindObjectOfType<UIQuestDisplay>(true).displayWorld(worldData);
    }
    void makeWorld ()
    {
        if(worldData.locations != null && worldData.locations.Count > 0)
        {
            //return;
        }
        List<Location> locations = new List<Location>();
        locations.Add(new Location
        {
            visualLocation = new Vector2(0.2f, 0.2f),
            verticals = new QuestVertical[] { makeQuestVeritcal(0, 20) }
        });
        locations.Add(new Location
        {
            visualLocation = new Vector2(0.8f, 0.2f),
            verticals = new QuestVertical[] { makeQuestVeritcal(5, 20), makeQuestVeritcal(12, 22) }
        });
        locations.Add(new Location
        {
            visualLocation = new Vector2(0.5f, 0.5f),
            verticals = new QuestVertical[] {  },
            overrideTier = 6
        });
        locations.Add(new Location
         {
             visualLocation = new Vector2(0.8f, 0.8f),
             verticals = new QuestVertical[] { makeQuestVeritcal(10, 25) },
         });

        worldData.locations = locations;
    }
    QuestVertical makeQuestVeritcal(int begin, int end)
    {
        int count = end - begin + 1;
        Quest[] quests = new Quest[count];
        for(int i = 0; i < count; i++)
        {
            quests[i] = makeQuest(begin + i);
        }
        return new QuestVertical
        {
            quests = quests,
        };
    }

    Quest makeQuest(int tier)
    {
        float totalDifficutly = Mathf.Max(0, tier - 2) * 0.2f;
        int encounters = tier switch
        {
            int i when i >4 && i< 10 => 1,
            int i when i >= 10 => 2,
            _ => 0
        };
        return new Quest {
             tier = tier,
            difficulty = Difficulty.fromTotal(totalDifficutly),
             packs = avgPacksPerMap,
             floors = Mathf.RoundToInt(avgFloorsPerMap),
             encounters = encounters,
        };
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
            Difficulty difficulty = Difficulty.fromTotal(currentDifficulty);
            mapsGen[i] = new Map
            {
                index = i,
                power = power,
                difficulty = difficulty,
                floors = mapFloors(difficulty),
                visualLocation = new Vector2(Random.value, Random.value),
                difficultyRangePercent = difficultyRangePercent,
            };
        }

        return mapsGen;
    }

    public static Floor[] mapFloors(Difficulty difficulty)
    {
        int floorCount = Mathf.RoundToInt(avgFloorsPerMap);
        Floor[] floors = new Floor[floorCount];
        for (int j = 0; j < floorCount; j++)
        {
            floors[j] = new Floor
            {
                packs = avgPacksPerMap/floorCount,
                sparseness = 3,
                encounters = floorEncounters(difficulty),
            };
        }
        return floors;
    }
    public static EncounterData[] floorEncounters(Difficulty difficulty)
    {
        int encounterCount = Mathf.RoundToInt(GaussRandomDecline().asRange(1, 2));
        EncounterData[] encounters = new EncounterData[encounterCount];
        float addedDifficulty = (0.25f + 0.3f * difficulty.total) * Random.value.asRange(0.5f, 1);
        for (int j = 0; j < encounterCount; j++)
        {

            encounters[j] = new EncounterData
            {
                difficulty = difficulty.add(addedDifficulty),
                type = EncounterType.Ambush,
                packs = 4,
            };
        }
        return encounters;
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

    #region embark
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

    [Server]
    public void embarkManual()
    {
        embarkButton.interactable = false;
        sound.playSound(SoundManager.SoundClip.Embark);
        gp.player.embark(-1);
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
        Map m;
        if (index > 0)
        {
            m = list.maps[index];
        }
        else
        {
            m = serverMap.getMap(gp.serverPlayer.power);
        }
        //Debug.Log(m.difficulty.pack + " - " + m.difficulty.veteran);
        yield return gen.buildMap(m);
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
    #endregion

}
