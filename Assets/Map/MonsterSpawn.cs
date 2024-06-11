using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GenerateValues;
using static GenerateAttack;
using static Atlas;
using static Utils;
using static Breakable;
using Unity.Burst.CompilerServices;
using UnityEngine.AI;
using Pathfinding;
using UnityEngine.TerrainUtils;

public class MonsterSpawn : NetworkBehaviour
{
    public GameObject UnitPre;
    public GameObject PackPre;
    public GameObject UrnPre;
    public GameObject EncounterPre;



    List<UnitTemplate> monsterTemplates = new List<UnitTemplate>();
    List<SpawnUnit> monsterUnits;

    Transform floor;


    public static readonly float lengthPerPack = 8f;

    float spawnPower = 100;
    Scales mapScales;

    public static float Ai2PlayerPowerFactor = 1.2f;
    static float maxSingleUnitFactor = 0.8f;
    static float lowerUnitPowerExponent = 2f;

    static int maxPackSize = 8;

    //float difficultyMultiplier = 1f;

    GlobalPlayer gp;

    private void Start()
    {
        gp = FindObjectOfType<GlobalPlayer>();
        if (isServer)
        {


        }
    }



    public struct Difficulty
    {
        public float pack;
        public float veteran;
        public float champion;
        public float total
        {
            get
            {
                return veteran + pack + champion;
            }
        }
        public static Difficulty fromTotal(float difficulty)
        {
            float[] split = generateRandomValues(3, 1).Select(v => v.val).ToArray();
            float sum = split.Sum();
            return new Difficulty
            {
                pack = split[0] * difficulty / sum,
                veteran = split[1] * difficulty / sum,
                champion = split[2] * difficulty / sum,
            };
        }
        public Difficulty add(float difficulty)
        {
            Difficulty added = fromTotal(difficulty);
            return add(added);
        }

        public Difficulty add(Difficulty difficulty)
        {
            return new Difficulty
            {
                pack = pack + difficulty.pack,
                veteran = veteran + difficulty.veteran,
                champion = champion + difficulty.champion,
            };
        }
    }
    struct UnitTemplate
    {
        public float power;
        public UnitProperties props;
        public List<CastData> abilitites;
    }
    public struct SpawnTransform
    {
        public Vector3 position;
        public float radius;

        public Vector3 randomNavLocaion
        {
            get
            {
                Vector2 circlePoint = Random.insideUnitCircle;
                Vector3 planePoint = position + new Vector3(circlePoint.x, 0, circlePoint.y) * radius;

                Vector3 pos = position;
                //RaycastHit hit;
                //float distance = WFCGeneration.tileScale.y * 3.1f;
                NNInfo nodeInfo;
                for (int i =0; i<4; i++)
                {
                    nodeInfo = AstarPath.active.GetNearest(planePoint);
                    if(nodeInfo.node != null & (nodeInfo.position - position).magnitude < radius)
                    //if (Physics.Raycast(planePoint + Vector3.up * distance * 0.5f, Vector3.down, out hit, distance, LayerMask.GetMask("Terrain")))
                    {
                        //pos = hit.point;
                        pos = nodeInfo.position;
                        break;
                    }
                }
                
                

                //if (NavMesh.SamplePosition(transform.position, out hit, sizeC.distance * 3, NavMesh.AllAreas))
                return pos;
            }
        }
    }
    struct SpawnPack
    {
        public SpawnTransform spawnTransform;
        public float packMult;
        public bool ignoreWakeup;
    }

    struct SpawnUnit
    {
        public UnitProperties props;
        public List<CastData> abilitites;
        public float championHealthMult;
        public List<Color> indicatorColors;
        public float power;
        public float halfHeight;
        public float killerHealMult;
        public float rewardMult;
        public float rewardPercent;
        public float packPercent;
    }
    public void setFloor(Transform f)
    {
        floor = f;
    }

    List<SpawnUnit> createUnits(Difficulty difficulty)
    {
        float championHealthPercent = 0.25f;
        float championAbilityPercent = 0.5f;
        float championTotalPercent = championHealthPercent + championAbilityPercent;

        List<SpawnUnit> units = new List<SpawnUnit>();
        foreach (UnitTemplate template in monsterTemplates)
        {
            float poolPower = weightedPower(template.power);
            float packPercent = poolPower / weightedPool();
            float veteranMult = (1 + difficulty.veteran) / (1 + difficulty.pack);
            poolPower *= veteranMult;
            float unitPower = inverseWeightedPower(poolPower);
            float championMult = (1 + difficulty.champion) / (1 + difficulty.pack + difficulty.veteran);
            poolPower *= championMult;
            int championTiers = Mathf.FloorToInt(championMult / championTotalPercent);
            int extraAbilityCount = championTiers;
            float remainingMult = championMult - (championTiers * championTotalPercent);
            if (remainingMult > championAbilityPercent)
            {
                extraAbilityCount += 1;
                remainingMult -= championAbilityPercent;
            }
            List<CastData> abilities = template.abilitites.Take(extraAbilityCount + 1).ToList();
            float healthMult = (championTiers * championHealthPercent + remainingMult) * 2.0f;

            float rewardPercent = poolPower / weightedPool();
            float scalePhys = Power.getScales(unitPower).world;

            SpawnUnit unit = new SpawnUnit()
            {
                props = template.props,
                power = unitPower,
                halfHeight = 0.75f * scalePhys,
                abilitites = abilities,
                championHealthMult = healthMult,
                indicatorColors = abilities.Count > 1 ? abilities.Skip(1).Select(a => a.flair.color).ToList() : new List<Color>(),
                killerHealMult = 1 / (1 + difficulty.pack),
                rewardMult = difficulty.total,
                rewardPercent = rewardPercent,
                packPercent = packPercent,
            };
            units.Add(unit);
        }
        return units;
    }

    public IEnumerator spawnLevel(List<GraphNode> nodes, Map map, GameObject endWater, Vector3 startPos)
    {
        Debug.Log("Start spawn: " + Time.time);
        monsterUnits = createUnits(map.difficulty);
        endWater.GetComponent<Reward>().setReward(spawnPower, map.difficulty.total, 3);
        endWater.GetComponent<Power>().setPower(spawnPower);
        NetworkServer.Spawn(endWater);

        if (map.hideMonsters)
        {
            yield break;
        }

        float packRadius = 7;
        List<SpawnTransform> locations = nodes.RandomLocations(packRadius,map.floor.sparseness).Select(v => 
            new SpawnTransform
            {
                position = v,
                radius = packRadius,
            }
        ).Where(st => (st.position - startPos).magnitude > packRadius *2).ToList();

        EncounterData[] encounters = map.floor.encounters;
        for (int i = 0; i < encounters.Length; i++)
        {
            EncounterData e = encounters[i];

            SpawnTransform t;
            GameObject reveal = null;
            if(i == 0)
            {
                t = new SpawnTransform
                {
                    position = endWater.transform.position,
                    radius = mapScales.world * 0.5f ,
                };
                endWater.GetComponent<Interaction>().setInteractable(false);
                reveal = endWater;
            }
            else
            {
                int z = locations.RandomIndex();
                t = locations[z];
                locations.RemoveAt(z);
            }


            yield return spawnEncounter(e, t, reveal);
        }
        Debug.Log("Encounters: " + Time.time);

        int zoneCount = locations.Count;
        for (int i = 0; i < zoneCount && i < (breakablesPerFloor); i++)
        {
            int z = locations.RandomIndex();
            SpawnTransform t = locations[z];
            locations.RemoveAt(z);
            BreakableType bType = gp.serverPlayer.pityTimers.rollBreakable(1);
            spawnBreakables(t, bType, map.difficulty.total);
            yield return null;
        }
        Debug.Log("Breakables: " + Time.time);

        while (locations.Count>0)
        {
            int z = locations.RandomIndex();
            SpawnTransform t = locations[z];
            locations.RemoveAt(z);

            SpawnPack packData = new SpawnPack
            {
                spawnTransform = t,
                packMult = 1 + map.difficulty.pack,
                ignoreWakeup = false,
            };
            Pack pack = Instantiate(PackPre, floor).GetComponent<Pack>();
            yield return spawnCreatures(packData, monsterUnits, pack);
            pack.init();
                     
        }
        Debug.Log("Units: " + Time.time);
    }

    IEnumerator spawnEncounter(EncounterData encounterData, SpawnTransform spawn, GameObject reveal)
    {
        Vector3 encounterPos = spawn.position;
        RaycastHit hit;
        if (Physics.Raycast(encounterPos, Vector3.down, out hit, 10f * mapScales.world, MapGenerator.TerrainMask()))
        {
            encounterPos = hit.point + Vector3.up * mapScales.world;
        }
        GameObject o = Instantiate(EncounterPre, encounterPos, Quaternion.identity, floor);
        o.transform.localScale = Vector3.one * mapScales.world;
        o.GetComponent<ClientAdoption>().parent = floor.gameObject;
        Encounter encounter = o.GetComponent<Encounter>();
        encounter.setScale(mapScales.world);
        encounter.revealOnEnd = reveal;

        List<SpawnUnit> encounterUnits = createUnits(encounterData.difficulty);


        NetworkServer.Spawn(o);
        Pack pack;
        for (int i = 0; i < encounterData.packs; i++)
        {
            SpawnPack packData = new SpawnPack
            {
                spawnTransform = spawn,
                packMult = 1 + encounterData.difficulty.pack,
                ignoreWakeup = true,
            };
            pack = Instantiate(PackPre, floor).GetComponent<Pack>();
            yield return spawnCreatures(packData, encounterUnits, pack);
            pack.init();
            encounter.addPack(pack);

        }
        o.GetComponent<Reward>().setReward(spawnPower, encounterData.difficulty.total, encounter.rewardPercent);
        encounter.init();
    }

    void spawnBreakables(SpawnTransform spawn, BreakableType type, float totalDifficulty)
    {
        float diffMult = totalDifficulty + 1;
        int numBreakables = numberBreakables(type);
        float packPercentTotal = packpercent(type) * diffMult;
        float packPercent = packPercentTotal / numBreakables;
        for (int j = 0; j < numBreakables; j++)
        {

            GameObject o = Instantiate(UrnPre, numBreakables <= 1 ? spawn.position : spawn.randomNavLocaion, Quaternion.identity, floor);
            o.transform.localScale = Vector3.one * mapScales.world;
            o.GetComponent<ClientAdoption>().parent = floor.gameObject;
            o.GetComponent<Gravity>().gravity *= mapScales.speed;
            o.GetComponent<Reward>().setReward(spawnPower, 1.0f, packPercent);
            o.GetComponent<Breakable>().type = type;

            NetworkServer.Spawn(o);
        }
    }


    IEnumerator spawnCreatures(SpawnPack spawnData, List<SpawnUnit> unitsToSpawn, Pack p)
    {
        p.scale = mapScales.world;
        p.transform.position = spawnData.spawnTransform.position;
        if (spawnData.ignoreWakeup)
        {
            p.GetComponent<Collider>().enabled = false;
        }

        List<SpawnUnit> toCreate = Utils.RandomItemsWeighted(unitsToSpawn, spawnData.packMult, u => 1 / u.packPercent, u => u.packPercent);
        foreach (SpawnUnit unit in toCreate)
        {
            InstanceCreature(spawnData, unit, p);
            yield return null;
        }


        NetworkServer.Spawn(p.gameObject);
    }
    float weightedPool()
    {
        return Mathf.Pow(spawnPower, lowerUnitPowerExponent) * Ai2PlayerPowerFactor;
    }
    static float weightedPower(float power)
    {
        return Mathf.Pow(power, lowerUnitPowerExponent);
    }

    static float inverseWeightedPower(float pool)
    {
        return Mathf.Pow(pool, 1 / lowerUnitPowerExponent);
    }

    struct InstanceInfo
    {
        public int instanceCount;
        public bool filledPool;
        public float remainingPool;
    }


    void InstanceCreature(SpawnPack spawnData, SpawnUnit spawnUnit, Pack p)
    {

        Vector3 unitPos = spawnData.spawnTransform.randomNavLocaion+ spawnUnit.halfHeight * Vector3.up;
        //RaycastHit hit;
        //if (Physics.Raycast(unitPos, Vector3.down, out hit, spawnData.spawnTransform.halfExtents.y * 6, LayerMask.GetMask("Terrain")))
        //{
        //    unitPos = hit.point + Vector3.up * mapScales.world;
        //}
        GameObject o = Instantiate(UnitPre, unitPos, Quaternion.identity, floor);
        o.GetComponent<UnitMovement>().currentLookAngle = Random.Range(-180f, 180f);
        o.GetComponent<ClientAdoption>().parent = floor.gameObject;
        o.GetComponent<Power>().setPower(spawnUnit.power);
        //UnitChampInd ind = o.GetComponent<UnitChampInd>();
        //ind.colors.Clear();
        //foreach (Color color in spawnUnit.indicatorColors)
        //{
        //    ind.colors.Add(color);
        //}

        o.GetComponent<Reward>().setReward(spawnPower, spawnUnit.rewardMult, spawnUnit.rewardPercent);
        o.GetComponent<PackHeal>().percentHealKiller = spawnUnit.packPercent * spawnUnit.killerHealMult;

        //Debug.Log(spawnUnit.power + " - " + spawnUnit.poolCost + " - " + weightedPool() + " - " + spawnPower + " - " + reward);
        p.addToPack(o);
        UnitPropsHolder holder = o.GetComponent<UnitPropsHolder>();
        holder.props = spawnUnit.props;
        holder.championHealthMultiplier = spawnUnit.championHealthMult;
        AbilityManager al = o.GetComponent<AbilityManager>();
        //al.clear();
        NetworkServer.Spawn(o);
        al.addAbility(spawnUnit.abilitites);

    }

    public void setSpawnPower(float power, Scales scales)
    {
        spawnPower = power;
        mapScales = scales;
        //clear data
        monsterTemplates.Clear();

        float pool = weightedPool();
        float minPower = inverseWeightedPower(pool / maxPackSize);
        float maxPower = inverseWeightedPower(pool * maxSingleUnitFactor);

        //int types = Mathf.RoundToInt(Random.value.asRange(2, 3));
        int types = 3;
        for(int i = 0; i < types; i++)
        {
            float templatePower = Random.value.asRange(minPower, maxPower);
            monsterTemplates.Add(createType(minPower));
        }
        monsterTemplates.Sort((t1,t2) => t1.power.CompareTo(t2.power));

    }


    UnitTemplate createType(float power)
    {
        UnitTemplate u = new UnitTemplate();
        u.power = power;
        u.props = GenerateUnit.generate(power, UnitPre.GetComponentInChildren<PartAssignment>().getVisuals());
        u.abilitites = new List<CastData>();
        for (int i = 0; i < 6; i++)
        {
            u.abilitites.Add(createAbility(power, i > 0));
        }
        return u;
    }

    CastData createAbility(float power, bool isStrong)
    {
        CastData a = GenerateAttack.generate(power, isStrong ? GenerateAttack.AttackGenerationType.MonsterStrong : GenerateAttack.AttackGenerationType.Monster);
        a.scales = true;
        return a;
    }

}
