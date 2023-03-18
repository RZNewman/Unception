using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static MonsterSpawn;
using static Utils;
using static RewardManager;
using static Power;
using static Atlas;
using Castle.Core.Internal;

public class Atlas : NetworkBehaviour
{
    static readonly int avgPacksPerfloor = 20;
    public static readonly float packVariance = 0.3f;
    public static readonly int chestPerFloor = 1;
    public static readonly int potPerFloor = 3;
    public readonly static float avgFloorsPerMap = 2f;
    public static readonly int avgPacksPerMap = Mathf.RoundToInt(avgPacksPerfloor * avgFloorsPerMap);

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
        public string id;
        public Vector2 visualLocation;
        public QuestVertical[] verticals;
        public int overrideTier;

        public int unlockTier
        {
            get
            {
                return verticals.Length == 0 ? overrideTier : verticals.Min(v => v.quests[0].tier);
            }
        }
    }
    public struct QuestVertical
    {
        public string id;
        public Quest[] quests;

        public bool nextQuest(int tierComplete, out Quest q)
        {
            q = quests[0];
            if (quests.Last().tier <= tierComplete) { return false; }
            foreach (Quest quest in quests)
            {
                if (quest.tier == tierComplete + 1)
                {
                    q = quest;
                    return true;
                }
            }
            return false;
        }
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
        public int tier;
        public bool quest;
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

    }
    void makeWorld()
    {
        if (worldData.locations != null && worldData.locations.Count > 0)
        {
            FindObjectOfType<UIQuestDisplay>(true).displayWorld(worldData);
            //return;
        }
        List<Location> locations = new List<Location>();
        locations.Add(new Location
        {
            id = "Town",
            visualLocation = new Vector2(0.2f, 0.2f),
            verticals = new QuestVertical[] { makeQuestVeritcal(0, 20, "Intro") }
        });
        locations.Add(new Location
        {
            id = "Ruins",
            visualLocation = new Vector2(0.8f, 0.2f),
            verticals = new QuestVertical[] { makeQuestVeritcal(5, 20, "Explore"), makeQuestVeritcal(12, 22, "Help") }
        });
        locations.Add(new Location
        {
            id = "Wilds",
            visualLocation = new Vector2(0.5f, 0.5f),
            verticals = new QuestVertical[] { },
            overrideTier = 6
        });
        locations.Add(new Location
        {
            id = "Castle",
            visualLocation = new Vector2(0.8f, 0.8f),
            verticals = new QuestVertical[] { makeQuestVeritcal(10, 25, "Finale") },
        });

        worldData.locations = locations;
        FindObjectOfType<UIQuestDisplay>(true).displayWorld(worldData);
    }
    QuestVertical makeQuestVeritcal(int begin, int end, string id)
    {
        int count = end - begin + 1;
        Quest[] quests = new Quest[count];
        for (int i = 0; i < count; i++)
        {
            quests[i] = makeQuest(begin + i);
        }
        return new QuestVertical
        {
            id = id,
            quests = quests,
        };
    }

    Quest makeQuest(int tier)
    {
        float totalDifficutly = Mathf.Max(0, tier - 2) * 0.2f;
        int encounters = tier switch
        {
            int i when i > 4 && i < 10 => 1,
            int i when i >= 10 => 2,
            _ => 0
        };
        int floors = Mathf.Max(Mathf.CeilToInt(tier / 7f), 1);
        int packs = tier switch
        {
            int i when i == 0 => 10,
            int i when i == 1 => 20,
            int i => Mathf.RoundToInt(avgPacksPerfloor * (1 + i * 0.07f) * floors),
        };
        return new Quest
        {
            tier = tier,
            difficulty = Difficulty.fromTotal(totalDifficutly),
            packs = packs,
            floors = floors,
            encounters = encounters,
        };
    }

    int playerTier
    {
        get
        {
            return gp.serverPlayer.progress.highestTier();
        }
    }

    float difficultyRange
    {
        get
        {
            return playerTier * 0.07f;
        }
    }
    float baseDifficulty
    {
        get
        {
            return playerTier * 0.03f;
        }
    }

    float powerAtTier(int tier)
    {
        return tier switch
        {
            int i when i == 0 => playerStartingPower * 0.8f,
            int i when i == 1 => playerStartingPower,
            int i => playerStartingPower * (1 + Mathf.Pow(powerMapPercent, mapClearsToTier(i))),
        };
    }
    float mapClearsToTier(int tier)
    {
        return Enumerable.Range(1, tier - 1).Select(t => mapClearsAtTier(t)).Sum();
    }
    float mapClearsAtTier(int tier)
    {

        return 0.6f + 0.2f * tier;
    }


    public void makeMaps()
    {
        if (!gp)
        {
            gp = FindObjectOfType<GlobalPlayer>();
        }
        makeWorld();
        list = new MapListing
        {
            maps = generateMapOpitons(),
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

    public Map[] generateMapOpitons()
    {
        List<Map> mapsGen = new List<Map>();


        List<Location> locations = new List<Location>(worldData.locations);
        locations.Shuffle();
        bool guaranteedQuest = true;

        int mapIndex = 0;
        foreach (Location location in locations)
        {
            if (location.unlockTier > playerTier + 1)
            {
                continue;
            }

            List<Quest> questsAtLocation = new List<Quest>();
            Quest q;
            foreach (QuestVertical vertical in location.verticals)
            {

                if (vertical.nextQuest(gp.player.progress.questTier(location.id, vertical.id), out q))
                {
                    questsAtLocation.Add(q);
                }
            }



            if (questsAtLocation.Count > 0 && (guaranteedQuest || Random.value > 0.5f))
            {
                q = questsAtLocation.RandomItem();


                mapsGen.Add(new Map
                {
                    index = mapIndex,
                    tier = q.tier,
                    quest = true,
                    power = powerAtTier(q.tier),
                    difficulty = q.difficulty,
                    floors = Enumerable.Repeat(
                        new Floor
                        {
                            encounters = floorEncounters(q.difficulty, q.encounters),
                            sparseness = 3,
                            packs = q.packs / q.floors

                        }
                    , q.floors).ToArray(),
                    visualLocation = location.visualLocation,
                });
            }
            else
            {
                float difficultyRangePercent = Random.value;
                float currentDifficulty = baseDifficulty + Mathf.Lerp(0, difficultyRange, difficultyRangePercent);
                Difficulty difficulty = Difficulty.fromTotal(currentDifficulty);

                List<int> floorPacks = new List<int>();
                int floors = Mathf.RoundToInt(avgFloorsPerMap);
                for (int i = 0; i < floors; i++)
                {
                    float packs = avgPacksPerfloor;

                    floorPacks.Add(Mathf.RoundToInt(GaussRandomCentered().asRange(packs * (1 - packVariance), packs * (1 + packVariance))));
                }
                mapsGen.Add(new Map
                {
                    index = mapIndex,
                    power = gp.serverPlayer.power,
                    difficulty = difficulty,
                    floors = mapRandomFloors(difficulty, floorPacks.ToArray()),
                    visualLocation = location.visualLocation,
                    difficultyRangePercent = difficultyRangePercent,
                });
            }



            mapIndex++;
        }

        return mapsGen.ToArray();
    }

    public static Floor[] mapFloors(Difficulty difficulty)
    {
        List<int> packs = new List<int>();
        for (int i = 0; i < Mathf.RoundToInt(avgFloorsPerMap); i++)
        {
            packs.Add(avgPacksPerfloor);
        }
        return mapRandomFloors(difficulty, packs.ToArray());
    }
    public static Floor[] mapRandomFloors(Difficulty difficulty, int[] packs)
    {
        int floorCount = packs.Length;
        Floor[] floors = new Floor[floorCount];
        for (int j = 0; j < floorCount; j++)
        {
            floors[j] = new Floor
            {
                packs = packs[j],
                sparseness = 3,
                encounters = floorEncounters(difficulty),
            };
        }
        return floors;
    }
    public static EncounterData[] floorEncounters(Difficulty difficulty)
    {
        int encounterCount = Mathf.RoundToInt(GaussRandomDecline().asRange(1, 2));
        return floorEncounters(difficulty, encounterCount);
    }
    public static EncounterData[] floorEncounters(Difficulty difficulty, int encounterCount)
    {
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
        if (index >= 0)
        {
            m = list.maps.Find(m => m.index == index);
        }
        else
        {
            m = serverMap.getMap(gp.serverPlayer.power);
        }
        //Debug.Log(m.quest + ": " + m.tier + " - " + m.power);
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
