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
public class Atlas : NetworkBehaviour
{
    public static readonly int breakablesPerFloor = 4;
    public readonly static float avgFloorsPerMap = 1f;
    public readonly static float softcap = 12_000f;
    public static readonly float sparsness = 5f;

    public RectTransform mapImage;
    public GameObject mapMarkerPre;
    public UiMapDetails mapDeets;
    public Button embarkButton;
    public UiServerMap serverMap;
    public WorldData worldData;

    PlayerGhost owner;
    GlobalPlayer gp;
    SoundManager sound;
    MapGenerator gen;

    [SyncVar(hook = nameof(hookMaps))]
    MapListing list;

    Map embarkedMap;

    public Map currentMap { get { return embarkedMap; } }

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
                return verticals.Length == 0 ? overrideTier : verticals.Min(v => v.unlockTier);
            }
        }
    }
    public struct QuestVertical
    {
        public string id;
        public Quest[] quests;

        public int unlockTier
        {
            get
            {
                return quests[0].tier;
            }
        }

        public bool nextQuest(int tierComplete, out Quest q)
        {
            q = quests[0];
            if (tierComplete == -1) { return true; }
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
        public int floors;
        public int encounters;
    }

    public struct QuestIds
    {
        public string locationId;
        public string verticalId;
        public int tier;
    }
    public struct Map
    {
        public int index;
        public int tier;
        public bool quest;
        public QuestIds ids;
        public float power;
        public Difficulty difficulty;
        public Floor[] floors;
        public Vector2 visualLocation;
        public float difficultyRangePercent;
    }
    public struct Floor
    {
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
        gen = FindObjectOfType<MapGenerator>();
        serverMap.gameObject.SetActive(false);
    }
    void makeWorld()
    {
        if (worldData.locations != null && worldData.locations.Count > 0)
        {
            RpcDisplayQuests(worldData.locations);
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
        RpcDisplayQuests(worldData.locations);
    }
    [ClientRpc]
    void RpcDisplayQuests(List<Location> locations)
    {
        WorldData worldData = ScriptableObject.CreateInstance<WorldData>();
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
        return new Quest
        {
            tier = tier,
            difficulty = Difficulty.fromTotal(totalDifficutly),
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
        float power = tier switch
        {
            int i when i == 0 => playerStartingPower * 0.8f,
            int i when i == 1 => playerStartingPower,
            int i => playerStartingPower * (Mathf.Pow(1 + powerMapPercent, mapClearsToTier(i))),
        };
        return Mathf.Min(power, softcap);
    }
    float mapClearsToTier(int tier)
    {
        return Enumerable.Range(1, tier - 1).Select(t => mapClearsAtTier(t)).Sum();
    }
    float mapClearsAtTier(int tier)
    {
        return tier switch
        {
            int i when i < 2 => 0,
            int i when i < 6 => 1,
            int i => 0.65f * Mathf.Pow(1.1f, tier),
        };
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

            List<(Quest, string)> questsAtLocation = new List<(Quest, string)>();
            Quest q;
            foreach (QuestVertical vertical in location.verticals)
            {
                if (vertical.unlockTier > playerTier + 1)
                {
                    continue;
                }
                if (vertical.nextQuest(gp.player.progress.questTier(location.id, vertical.id), out q))
                {
                    questsAtLocation.Add((q, vertical.id));
                }
            }



            if (questsAtLocation.Count > 0 && (guaranteedQuest || Random.value > 0.5f))
            {
                (Quest, string) questData = questsAtLocation.RandomItem();
                string verticalId = questData.Item2;
                q = questData.Item1;


                mapsGen.Add(new Map
                {
                    index = mapIndex,
                    tier = q.tier,
                    quest = true,
                    ids = new QuestIds
                    {
                        locationId = location.id,
                        verticalId = verticalId,
                        tier = q.tier,
                    },
                    power = powerAtTier(q.tier),
                    difficulty = q.difficulty,
                    floors = Enumerable.Repeat(
                        new Floor
                        {
                            encounters = floorEncounters(q.difficulty, q.encounters),
                            sparseness = sparsness,

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

                mapsGen.Add(new Map
                {
                    index = mapIndex,
                    power = gp.serverPlayer.power,
                    difficulty = difficulty,
                    floors = mapRandomFloors(difficulty, Mathf.RoundToInt(avgFloorsPerMap)),
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
        return mapRandomFloors(difficulty, Mathf.RoundToInt(avgFloorsPerMap));
    }
    public static Floor[] mapRandomFloors(Difficulty difficulty, int floorCount)
    {
        Floor[] floors = new Floor[floorCount];
        for (int j = 0; j < floorCount; j++)
        {
            floors[j] = new Floor
            {
                sparseness = sparsness,
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
                packs = 3,
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
            selectedMap.highlighted(false);
        }
        selectedMap = m;
        selectedMap.highlighted(true);
        checkEmbarkButton();
    }

    public void openServerMap()
    {
        serverMap.gameObject.SetActive(true);
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
        serverMap.gameObject.SetActive(false);
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

        Map m;
        if (index >= 0)
        {
            m = list.maps.Where(m => m.index == index).First();
        }
        else
        {
            m = serverMap.getMap(gp.serverPlayer.power);
        }
        embarkedMap = m;
        //Debug.Log(m.quest + ": " + m.tier + " - " + m.power);
        setScaleServer(Power.scaleNumerical(m.power), Power.scaleNumerical(gp.serverPlayer.power));
        yield return gen.buildMap();
    }
    public Vector3 playerSpawn
    {
        get
        {
            return gen.playerSpawn;
        }
    }

    [Server]
    public void disembark(bool mapSuccess = true)
    {
        onMission = false;
        foreach (GameObject unit in FindObjectsOfType<PlayerGhost>().Select(g => g.unit))
        {
            Destroy(unit);
        }
        if (Pause.isPaused)
        {
            FindObjectOfType<Pause>().togglePause();
        }
        if (mapSuccess)
        {
            if (embarkedMap.quest)
            {
                foreach (SaveData save in FindObjectsOfType<SaveData>())
                {
                    save.saveQuestProgress(embarkedMap.ids);
                }
            }
        }
        else
        {
            //floor wasnt cleaned up by next floor routine
            FindObjectOfType<MapGenerator>().destroyFloor();
        }
        setScaleServer(1, 1);
        clearMapMarkers();
        makeMaps();
        foreach (Inventory inv in FindObjectsOfType<Inventory>())
        {
            if (mapSuccess)
            {
                inv.addBlessing(embarkedMap.power, embarkedMap.difficulty.total);
            }
            inv.syncInventoryUpwards();
            inv.GetComponent<PlayerGhost>().TargetMenuFinish(inv.connectionToClient, mapSuccess);
        }

    }
    [Server]
    void setScaleServer(float scalePhys, float scaleTime)
    {

        Power.setPhysicalScale(scalePhys);
        Power.setTimeScale(scaleTime);
        RpcSetScale(scalePhys, scaleTime);
    }
    [ClientRpc]
    void RpcSetScale(float scalePhys, float scaleTime)
    {
        Power.setTimeScale(scaleTime);
        Power.setPhysicalScale(scalePhys);
    }
    #endregion

}
