using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MonsterSpawn;
using static Utils;
using static WFCTile;
using static WFCTileOption;

public class WFCGeneration : MonoBehaviour
{
    public Vector3 tileScale = new Vector3(8, 4, 8);
    public int collapsePerFrame = 100;
    public List<TileWeight> tiles;

    [System.Serializable]
    public struct TileWeight
    {
        public float weight;
        public WFCTile prefab;
    }
    public enum TileConnection
    {
        Walkable = 1 << 0,
        Air = 1 << 1,
        AirConnect = 1 << 2,
        Ground = 1 << 3,
        GroundConnect = 1 << 4,
        RampRightConnect = 1 << 5,
        RampLeft = 1 << 6,
        RampRight = 1 << 7,
        RampLeftConnect = 1 << 8,
        RampTop = 1 << 9,
        ArchRightConnect = 1 << 10,
        ArchLeft = 1 << 11,
        ArchRight = 1 << 12,
        ArchLeftConnect = 1 << 13,
        Bridge = 1 << 14,
        BridgeWidth = 1 << 15,
    }

    readonly int connectionsRightMask = (int)(
        TileConnection.AirConnect |
        TileConnection.GroundConnect |
        TileConnection.RampRight |
        TileConnection.ArchRight |
        TileConnection.ArchLeftConnect |
        TileConnection.RampLeftConnect
        );
    readonly int connectionsLeftMask = (int)(
        TileConnection.RampLeft |
        TileConnection.ArchLeft |
        TileConnection.ArchRightConnect |
        TileConnection.RampRightConnect
        );

    readonly int enhancementLeftMask = (int)(
        TileConnection.Air |
        TileConnection.Ground |
        TileConnection.ArchRight
        );

    readonly int enhancementRightMask = (int)(
        TileConnection.ArchLeft
        );




    public readonly static int walkableMask = (int)(
        TileConnection.Walkable 
        | TileConnection.RampTop 
        | TileConnection.RampLeft 
        | TileConnection.RampRight
        | TileConnection.RampLeftConnect
        | TileConnection.RampRightConnect
        );

    static readonly int airMask = (int)(
        TileConnection.Air
        | TileConnection.RampTop
        | TileConnection.AirConnect
        );
    static readonly int groundMask = ~airMask;
    public List<TileDirection> walkableDirections(Vector3Int loc)
    {
        List<TileDirection> directions = new List<TileDirection>();
        WFCCell cell = map[loc.x, loc.y, loc.z];
        foreach (TileDirection dir in AllDirections)
        {
            Vector3Int adj = loc + fromDir(dir);
            WFCCell adjCell = map[adj.x, adj.y, adj.z];
            int mask = dir switch
            {
                TileDirection.Forward => cell.forwardMask,
                TileDirection.Right => cell.rightMask,
                TileDirection.Up => cell.upMask,
                TileDirection.Backward => adjCell.forwardMask,
                TileDirection.Left => adjCell.rightMask,
                TileDirection.Down => adjCell.upMask,
                _ => 0,
            };
            if (adjCell.ready && (mask & walkableMask) > 0)
            {
                directions.Add(dir);
            }
        }
        return directions;
    }

    public static List<TileConnection> alignments = new List<TileConnection>() { TileConnection.RampTop };
    public static int alignmentMask()
    {
        return (int)(TileConnection.RampTop);
    }
    public enum TileDirection
    {
        Forward,
        Right,
        Backward,
        Left,
        Up,
        Down,
    }
    static TileDirection opposite(TileDirection direction)
    {
        return direction switch
        {
            TileDirection.Forward => TileDirection.Backward,
            TileDirection.Backward => TileDirection.Forward,
            TileDirection.Left => TileDirection.Right,
            TileDirection.Right => TileDirection.Left,
            TileDirection.Up => TileDirection.Down,
            TileDirection.Down => TileDirection.Up,
            _ => TileDirection.Forward,
        };
    }
    static Vector3Int fromDir(TileDirection dir)
    {
        return dir switch
        {
            TileDirection.Up => new Vector3Int(0, 1, 0),
            TileDirection.Down => new Vector3Int(0, -1, 0),
            TileDirection.Right => new Vector3Int(1, 0, 0),
            TileDirection.Left => new Vector3Int(-1, 0, 0),
            TileDirection.Forward => new Vector3Int(0, 0, 1),
            TileDirection.Backward => new Vector3Int(0, 0, -1),
            _ => Vector3Int.zero
        };
    }

    public static TileDirection rotated(TileDirection direction, Rotation rotation)
    {
        TileDirection newDir = direction switch
        {

            TileDirection.Up => TileDirection.Up,
            TileDirection.Down => TileDirection.Down,
            TileDirection dir => (TileDirection)((((int)dir) + ((int)rotation)) % 4),
        };
        //Debug.Log(rotation + ":" + direction + " - " + newDir);
        return newDir;
    }
    enum MeasureType
    {
        Air, 
        Ground
    }
    enum GapCollapseType
    {
        None,
        Air,
        Ground
    }

    struct AirMeasurements
    {
        public Dictionary<MeasureType, int> typeDists;

        public AirMeasurements(AirMeasurements copy)
        {
            typeDists = new Dictionary<MeasureType, int>(copy.typeDists);
        }
        public bool isAir()
        {
            return typeDists[MeasureType.Air] >0 && typeDists[MeasureType.Ground] > 0
                &&
                typeDists[MeasureType.Air] >= typeDists[MeasureType.Ground]
                ;
        }
    }

    struct AirGap
    {
        public Dictionary<TileDirection, AirMeasurements> measurements;
        GapCollapseType gapCollapseType;

        public AirGap(AirGap copy)
        {
            gapCollapseType = copy.gapCollapseType;
            measurements = new Dictionary<TileDirection, AirMeasurements>();
            foreach(KeyValuePair<TileDirection, AirMeasurements> pair in copy.measurements)
            {
                measurements[pair.Key] = new AirMeasurements(pair.Value);
            }
        }

        bool isGround(int spacing)
        {
            return (measurements[TileDirection.Up].typeDists[MeasureType.Ground] + measurements[TileDirection.Down].typeDists[MeasureType.Ground]) > spacing + 1;
        }
        bool isAir()
        {
            return measurements[TileDirection.Up].isAir() || measurements[TileDirection.Down].isAir();
        }
        //cell in the spacing that arent adjacnet must be entirely one substance (vertical connection type)
        //public bool requiresLikePoles(int spacing)
        //{
        //    int upGround = measurements[TileDirection.Up].typeDists[MeasureType.Ground];
        //    int downGround = measurements[TileDirection.Down].typeDists[MeasureType.Ground];

        //    //up is spacing -1 bc the cell above a ground cell is adj
        //    bool req = (upGround > 0 && upGround < spacing - 1) || (downGround > 1 && downGround < spacing);
        //    if (req) Debug.Log(ToString());
        //    return req;
        //}
        public bool domainFit(int spacing, int upDomain, int downDomain)
        {
            int upGround = measurements[TileDirection.Up].typeDists[MeasureType.Ground];
            int downGround = measurements[TileDirection.Down].typeDists[MeasureType.Ground];
            return !(
                    (
                        (upDomain | groundMask) == groundMask
                        &&
                        (downDomain | airMask) == airMask
                        &&
                        upGroundRestricted(upGround,spacing)
                    )
                    ||
                    (
                        (upDomain | airMask) == airMask
                        &&
                        (downDomain | groundMask) == groundMask
                        &&
                        downGroundRestricted(downGround, spacing)
                    )
                );
        }

        bool upGroundRestricted(int dist, int spacing)
        {
            return (dist > 0 && dist < spacing - 1);
        }
        bool downGroundRestricted(int dist, int spacing)
        {
            return (dist > 1 && dist < spacing);
        }


        public override string ToString()
        {
            return System.String.Join('/',measurements.Select(pair => pair.Key.ToString() + ":" + System.String.Join(',', pair.Value.typeDists.Select(distPair => distPair.Key.ToString() + "-" + distPair.Value)))) +" --Collapse: "+ gapCollapseType;
        }
        public GapCollapseType tryCollapseConnection(int newConn)
        {
            
            if(gapCollapseType == GapCollapseType.None)
            {
                if ((newConn | airMask) == airMask)
                {
                    gapCollapseType = GapCollapseType.Air;
                    return gapCollapseType;
                }
                if ((newConn | groundMask) == groundMask)
                {
                    gapCollapseType = GapCollapseType.Ground;
                    return gapCollapseType;
                }
            }
            return GapCollapseType.None;

        }

        public bool applyDistance(int spacing, TileDirection dir, MeasureType type, int dist, out bool checkDomain)
        {
            checkDomain = false;
            if (measurements[dir].typeDists[type] < dist)
            {
                ////Air measures shouldnt go through ground
                //if (type == MeasureType.Air && gapCollapseType == GapCollapseType.Ground)
                //{
                //    return false;
                //}
                measurements[dir].typeDists[type] = dist;
                if(type == MeasureType.Ground && (
                    (dir == TileDirection.Up  && upGroundRestricted(dist, spacing))
                    ||
                    (dir == TileDirection.Down && downGroundRestricted(dist, spacing))
                    ))
                {
                    checkDomain = true;
                }

                return true;
            }
            return false;
        }

        public GapCollapseType tryCollapseSpacing(int spacing)
        {
            if (gapCollapseType == GapCollapseType.None)
            {
                //Debug.Log(this.ToString());
                if (isAir())
                {
                    //Debug.Log("Air " + ToString());
                    gapCollapseType = GapCollapseType.Air;
                    return gapCollapseType;
                }
                if (isGround(spacing))
                {
                    //Debug.Log("Ground " + ToString());
                    gapCollapseType = GapCollapseType.Ground;
                    return gapCollapseType;
                }
            }
            return GapCollapseType.None;
        }

    }

    void propagateGap(int spacing, HashSet<Vector3Int> queueIn, out HashSet<Vector3Int> queueOut, Vector3Int loc, TileDirection dir, MeasureType type, int dist)
    {
        queueOut = queueIn;
        if(dist <= 0)
        {
            return;
        }
        WFCCell cell = map[loc.x, loc.y, loc.z];
        if (!cell.ready)
        {
            return;
        }
        bool checkDomain;
        if(cell.verticalGap.applyDistance(spacing, dir, type, dist, out checkDomain))
        {
            //Debug.Log("New dist: " + loc + cell.verticalGap.ToString());
            GapCollapseType newCollapse = cell.verticalGap.tryCollapseSpacing(spacing);
            if (newCollapse != GapCollapseType.None)
            {
                //Debug.Log(loc + newCollapse.ToString()+ ": "+ cell.verticalGap.ToString() + ": " + namesFromConnections(cell.upMask));
                Vector3 worldLocation = loc;
                worldLocation = worldLocation.scale(floorScale);
                //Debug.DrawLine(worldLocation, worldLocation + Vector3Int.up*2, Color.yellow, 60f);
                cell.upMask &= newCollapse switch
                {
                    GapCollapseType.Air => airMask,
                    GapCollapseType.Ground => groundMask,
                    _ => throw new System.NotImplementedException(),
                };
                //Debug.Log(loc + " 2 "+cell.upMask);
                if (cell.upMask == 0)
                {
                    throw new System.Exception("Air gap removed mask");
                }
                queueOut.Add(loc);
                queueOut.Add(loc + Vector3Int.up);
            }
            else if (checkDomain)
            {
                queueOut.Add(loc);
            }

            switch (dir)
            {
                case TileDirection.Up:
                    loc.y += 1;
                    propagateGap(spacing, queueOut, out queueOut, loc, dir, type, dist - 1);
                    break;
                case TileDirection.Down:
                    loc.y -= 1;
                    propagateGap(spacing, queueOut, out queueOut, loc, dir, type, dist - 1);
                    break;

            }
        }
        
    }

    void startPropogateGap(int spacing, Vector3Int loc, GapCollapseType gapColl, HashSet<Vector3Int> queueIn, out HashSet<Vector3Int> queueOut)
    {
        queueOut = queueIn;
        //if(gapColl!= GapCollapseType.None)
        //{
        //    Debug.Log("Propagate: " + loc + gapColl.ToString());
        //}
        
        switch (gapColl)
        {
            case GapCollapseType.Air:
                propagateGap(spacing, queueOut, out queueOut, loc, TileDirection.Up, MeasureType.Air, spacing - 1);
                propagateGap(spacing, queueOut, out queueOut, loc, TileDirection.Down, MeasureType.Air, spacing - 1);
                break;
            case GapCollapseType.Ground:
                propagateGap(spacing, queueOut, out queueOut, loc, TileDirection.Up, MeasureType.Ground, spacing);
                propagateGap(spacing, queueOut, out queueOut, loc, TileDirection.Down, MeasureType.Ground, spacing);
                break;
        }
    }
    void tryPropagateGap(int spacing, Vector3Int loc, HashSet<Vector3Int> queueIn, out HashSet<Vector3Int> queueOut)
    {
        queueOut = queueIn;
        WFCCell cell = map[loc.x, loc.y, loc.z];
        WFCCell adjCell = map[loc.x, loc.y-1, loc.z];


        GapCollapseType gapCollTop = cell.verticalGap.tryCollapseConnection(cell.upMask);
        GapCollapseType gapCollBottom = GapCollapseType.None;
        if (adjCell.ready)
        {
            gapCollBottom = adjCell.verticalGap.tryCollapseConnection(adjCell.upMask);
        }
        startPropogateGap(spacing, loc, gapCollTop, queueOut, out queueOut);
        startPropogateGap(spacing, loc+ Vector3Int.down, gapCollBottom, queueOut, out queueOut);


    }


    static string namesFromConnections(int conn)
    {
        string names = "";
        foreach (TileConnection c in EnumValues<TileConnection>())
        {
            int ind = (int)c;
            if ((conn & ind) > 0)
            {
                names += c.ToString() + ",";
            }
        }
        return names;
    }


    class WFCCell
    {
        public bool ready = false;
        public bool collapsed = false;
        public BigMask domainMask;
        public int forwardMask;
        public int rightMask;
        public int upMask;

        public GameObject instancedCell;
        public NavLoad navType;

        public Dictionary<int, HashSet<Rotation>> alignmentRestrictions;
        public AirGap verticalGap;

        public Dictionary<TileType, int> groupWeights;

        public WFCCell()
        {
            ready = false;
            collapsed = false;
            domainMask = new BigMask();
            forwardMask = 0;
            rightMask = 0;
            upMask = 0;
            alignmentRestrictions = new Dictionary<int, HashSet<Rotation>>();
            groupWeights = new Dictionary<TileType, int>();
        }

        public WFCCell(WFCCell copy)
        {
            ready = copy.ready;
            collapsed = copy.collapsed;
            domainMask = new BigMask(copy.domainMask);
            forwardMask = copy.forwardMask;
            rightMask = copy.rightMask;
            upMask = copy.upMask;
            alignmentRestrictions = new Dictionary<int, HashSet<Rotation>>(copy.alignmentRestrictions);
            verticalGap = new AirGap(copy.verticalGap);
            groupWeights = new Dictionary<TileType, int>(copy.groupWeights);
        }

        public void init(BigMask fullDomain, int verticalConnections, int horizontalConnections, Dictionary<int, HashSet<Rotation>> fullRestrictions)
        {
            domainMask = new BigMask(fullDomain);
            forwardMask = horizontalConnections;
            rightMask = horizontalConnections;
            upMask = verticalConnections;
            alignmentRestrictions = new Dictionary<int, HashSet<Rotation>>(fullRestrictions);
            groupWeights = new Dictionary<TileType, int>();
            verticalGap = new AirGap() {
                measurements = new Dictionary<TileDirection, AirMeasurements>()
                {
                    {TileDirection.Up,new AirMeasurements()
                        {
                            typeDists = new Dictionary<MeasureType, int>()
                            {
                                {MeasureType.Air,0},
                                {MeasureType.Ground,0},
                            }
                        }
                    },
                    {TileDirection.Down,new AirMeasurements()
                        {
                            typeDists = new Dictionary<MeasureType, int>()
                            {
                                {MeasureType.Air,0},
                                {MeasureType.Ground,0},
                            }
                        }
                    }
                }
                
            };
        }
        public void makeReady()
        {
            ready = true;
        }

        public void collapse(NavLoad type, GameObject o = null)
        {
            collapsed = true;
            instancedCell = o;
            navType = type;
        }


    }
    WFCCell[,,] mapBackup;
    WFCCell[,,] map;

    void fullConnectionMask(BigMask fullDomainMask, out int verticalMask, out int horizontalMask)
    {
        ConnectionDomainInfo cdi = compositeConnectionDomains(fullDomainMask);
        verticalMask = cdi.validConnections.up;
        horizontalMask = cdi.validConnections.forward;
    }

    

    (BigMask, BigMask) fullDomainMask()
    {
        BigMask mask = new BigMask();
        BigMask EXmask = new BigMask();

        for (int i = 0; i < domainTiles.Count; i++)
        {
            TileOption opt = domainTiles[i];
            if (opt.weight > 0)
            {
                mask.addIndex(i);
            }
            else
            {
                EXmask.addIndex(i);
            }

        }
        return (mask, EXmask);
    }
    static readonly List<Rotation> allRotations = new List<Rotation>() { Rotation.None, Rotation.Quarter, Rotation.Half, Rotation.ThreeQuarters };

    Dictionary<int, HashSet<Rotation>> fullAlignmentMask()
    {
        Dictionary<int, HashSet<Rotation>> restrictions = new Dictionary<int, HashSet<Rotation>>();
        foreach (TileConnection a in alignments)
        {
            restrictions[(int)a] = new HashSet<Rotation>(allRotations);
        }
        return restrictions;
    }
    public static int connectionDomain(IEnumerable<TileConnection> connections)
    {
        int mask = 0;

        foreach (int value in connections)
        {
            mask |= value;
        }
        return mask;
    }
    int invertConnections(int domain)
    {
        int newDomain = domain;

        int connectionsRightOverlap = domain & connectionsRightMask;
        int connectionsLeftOverlap = domain & connectionsLeftMask;
        newDomain ^= connectionsRightOverlap;
        newDomain ^= connectionsLeftOverlap;
        newDomain |= connectionsRightOverlap >> 1;
        newDomain |= connectionsLeftOverlap << 1;

        int enhancementsRightOverlap = domain & enhancementRightMask;
        int enhancementsLeftOverlap = domain & enhancementLeftMask;
        newDomain |= enhancementsRightOverlap >> 1;
        newDomain |= enhancementsLeftOverlap << 1;

        return newDomain;
    }
    int EXEnhanceConnections(int domain)
    {
        int newDomain = domain;

        //Reversed, now we're adding the original key
        int enhancementsRightOverlap = domain & (enhancementRightMask >> 1);
        int enhancementsLeftOverlap = domain & (enhancementLeftMask << 1);
        newDomain |= enhancementsRightOverlap << 1;
        newDomain |= enhancementsLeftOverlap >> 1;


        return newDomain;
    }
    

    struct ConnectionDomainMasks
    {
        public int up;
        public int down;
        public int left;
        public int right;
        public int forward;
        public int backward;

        public static ConnectionDomainMasks operator |(ConnectionDomainMasks a, ConnectionDomainMasks b)
        {
            return new ConnectionDomainMasks
            {
                up = a.up | b.up,
                down = a.down | b.down,
                left = a.left | b.left,
                right = a.right | b.right,
                forward = a.forward | b.forward,
                backward = a.backward | b.backward,
            };
        }
    }

    struct TileOption
    {
        public float weight;
        public WFCTile prefab;
        public Rotation rotation;
        public ConnectionDomainMasks domains;
    }

    List<TileOption> domainTiles = new List<TileOption>();

    void makeDomain()
    {
        foreach (TileWeight tile in tiles)
        {
            if (tile.weight <= -1)
            {
                continue;
            }
            List<Rotation> rots = new List<Rotation>() { Rotation.None };
            Dictionary<TileDirection, int> domains = tile.prefab.getDomains();
            if (domains[TileDirection.Forward] == domains[TileDirection.Backward]
                &&
                domains[TileDirection.Left] == domains[TileDirection.Right])
            {

                if (domains[TileDirection.Forward] != domains[TileDirection.Right])
                {
                    rots.Add(Rotation.Quarter);
                }
            }
            else
            {
                rots.Add(Rotation.Quarter);
                rots.Add(Rotation.Half);
                rots.Add(Rotation.ThreeQuarters);
            }
            foreach (Rotation r in rots)
            {
                Dictionary<TileDirection, int> rotationDict = tile.prefab.getDomains(r);
                domainTiles.Add(new TileOption
                {
                    rotation = r,
                    prefab = tile.prefab,
                    weight = tile.weight / rots.Count,
                    domains = new ConnectionDomainMasks
                    {
                        up = rotationDict[TileDirection.Up],
                        down = rotationDict[TileDirection.Down],
                        backward = rotationDict[TileDirection.Backward],
                        left = rotationDict[TileDirection.Left],
                        right = rotationDict[TileDirection.Right],
                        forward = rotationDict[TileDirection.Forward],
                    },
                });
            }
        }
        //Reverse in the sort function 
        domainTiles.Sort((a, b) => b.weight.CompareTo(a.weight));
    }


    BigMask selectFromTileDomain(BigMask domain, Dictionary<TileType, int> groupWeights)
    {
        if (domain.empty)
        {
            throw new System.Exception("No domain to pick from");
        }
        if (domain.singleDomain())
        {
            return domain;
        }

        List<(int, float)> indexWeights = new List<(int, float)>();
        float weightSum = 0;


        foreach (int index in domain.indicies)
        {
            TileOption opt = domainTiles[index];
            //Debug.Log(opt.prefab.name + " - " + opt.weight);
            float weight = opt.weight;
            if(opt.prefab.typeGroup != TileType.None)
            {
                if (groupWeights.ContainsKey(opt.prefab.typeGroup))
                {
                    float weightBonus = opt.prefab.typeGroup switch
                    {
                        TileType.Surface => 15,
                        TileType.Air => 2,
                        TileType.Ground => 2,
                        _ => 0
                    };
                    weight *= weightBonus * groupWeights[opt.prefab.typeGroup];
                }
            }

            indexWeights.Add((index, weight));
            weightSum += weight;
        }

        if (weightSum == 0)
        {
            //EX only, pick lowest
            indexWeights.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            return new BigMask(indexWeights[0].Item1);
        }

        float pick = Random.value * weightSum;
        int selectedIndex = -1;
        //Debug.Log(pick);

        foreach ((int index, float weight) in indexWeights)
        {
            if (pick <= weight)
            {
                selectedIndex = index;
                break;
            }
            else
            {
                pick -= weight;
            }
        }
        if (selectedIndex == -1)
        {
            throw new System.Exception("Weight not chosen");
        }

        return new BigMask(selectedIndex);

    }

    IEnumerable<(TileOption, int)> optionsFromTileDomain(BigMask domain, bool debug = false)
    {
        if (domain.empty && !debug)
        {
            throw new System.Exception("Empty Domain");
        }
        return domain.indicies.Select(ind => (domainTiles[ind], ind));
    }

    TileOption optionFromSingleDomain(BigMask domain)
    {
        if (!domain.singleDomain())
        {
            throw new System.Exception("mutiple domains at instance");
        }

        foreach (int index in domain.indicies)
        {
            return domainTiles[index];
        }

        throw new System.Exception("tile option not found");
    }

    void addLikeTypeGroups(TileOption opt, Vector3Int loc)
    {
        if(opt.prefab.typeGroup != TileType.None)
        {
            int connMask = opt.prefab.typeGroup switch
            {
                TileType.Air => (int)(TileConnection.Air),
                TileType.Ground => (int)(TileConnection.Ground),
                TileType.Surface => walkableMask,
                _ => 0
            };
            foreach(TileDirection dir in HorizontalDirections)
            {
                Vector3Int adjLoc = loc + fromDir(dir);
                WFCCell cellAdj = map[adjLoc.x, adjLoc.y, adjLoc.z];

                Vector3Int cellConnLoc = dir switch
                {
                    TileDirection.Backward | TileDirection.Left | TileDirection.Down => adjLoc,
                    _ => loc
                };
                TileDirection dirForMask = dir switch
                {
                    TileDirection.Backward | TileDirection.Left | TileDirection.Down => opposite(dir),
                    _ => dir
                };
                WFCCell cellForConn = map[cellConnLoc.x, cellConnLoc.y, cellConnLoc.z];
                int connCell = dirForMask switch
                {
                    TileDirection.Up => cellForConn.upMask,
                    TileDirection.Right => cellForConn.rightMask,
                    TileDirection.Forward => cellForConn.forwardMask,
                    _ => 0
                };

                if((connCell & connMask) != 0)
                {
                    if (cellAdj.groupWeights.ContainsKey(opt.prefab.typeGroup))
                    {
                        cellAdj.groupWeights[opt.prefab.typeGroup] += 1;
                    }
                    else
                    {
                        cellAdj.groupWeights[opt.prefab.typeGroup] = 1;
                    }
                }
            }
        }
    }
    ConnectionDomainInfo compositeConnectionDomains(BigMask domain)
    {
        if (domain.empty)
        {
            throw new System.Exception("Empty Domain");
        }
        ConnectionDomainInfo connections = new ConnectionDomainInfo
        {
            validConnections = new ConnectionDomainMasks(),
            downAlignments = new Dictionary<int, HashSet<Rotation>>(),
            upAlignments = new Dictionary<int, HashSet<Rotation>>(),
        };
        connections.validConnections.up = 0;
        connections.validConnections.down = 0;
        connections.validConnections.left = 0;
        connections.validConnections.right = 0;
        connections.validConnections.forward = 0;
        connections.validConnections.backward = 0;

        foreach (TileConnection conn in alignments)
        {
            int singleAlignmentMask = (int)conn;
            connections.upAlignments[singleAlignmentMask] = new HashSet<Rotation>();
            connections.downAlignments[singleAlignmentMask] = new HashSet<Rotation>();
        }
        foreach ((TileOption opt, _) in optionsFromTileDomain(domain))
        {
            ConnectionDomainMasks connectionsTile = opt.domains;
            connections.validConnections |= connectionsTile;

            foreach (TileConnection conn in alignments)
            {

                int singleAlignmentMask = (int)conn;
                if ((connectionsTile.up & singleAlignmentMask) > 0)
                {
                    if (!connections.upAlignments[singleAlignmentMask].Contains(opt.rotation))
                    {
                        connections.upAlignments[singleAlignmentMask].Add(opt.rotation);
                    }
                }
                if ((connectionsTile.down & singleAlignmentMask) > 0)
                {
                    if (!connections.downAlignments[singleAlignmentMask].Contains(opt.rotation))
                    {
                        connections.downAlignments[singleAlignmentMask].Add(opt.rotation);
                    }
                }
            }

        }

        return connections;
    }


    delegate void UpdateEntropy(Vector3Int loc, float entropy);
    enum RestrictTileResult
    {
        NoChange,
        Reduced,
        ReducedEx,
        Error,
    }
    RestrictTileResult restrictTileDomain(Vector3Int loc, UpdateEntropy update, bool ExCall = false)
    {
        WFCCell cell = map[loc.x, loc.y, loc.z];
        if (!cell.ready)
        {
            return RestrictTileResult.NoChange;
        }
        if (cell.collapsed)
        {
            return RestrictTileResult.NoChange;
        }
        List<TileOption> remaining = new List<TileOption>();
        bool reduced = false;
        //BigMask cachedDomain = new BigMask(cell.domainMask);
        //Debug.Log("Restrict Domain " + loc+ cell.upMask);

        foreach ((TileOption opt, int index) in optionsFromTileDomain(cell.domainMask))
        {

            ConnectionDomainMasks domains = opt.domains;
            bool doesntFit = false;
            foreach (TileDirection dir in AllDirections)
            {
                WFCCell negativeCell;
                int negativeDomain;
                int overlap;
                switch (dir)
                {
                    case TileDirection.Forward:

                        if ((cell.forwardMask & domains.forward) == 0)
                        {
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Right:

                        if ((cell.rightMask & domains.right) == 0)
                        {
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Up:

                        overlap = cell.upMask & domains.up;
                        if (overlap == 0)
                        {
                            doesntFit = true;
                            goto MatchFound;
                        }
                        else if (
                            (overlap | alignmentMask()) == alignmentMask()
                            &&
                            !cell.alignmentRestrictions[overlap].Contains(opt.rotation)
                            )
                        {
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Backward:
                        negativeCell = map[loc.x, loc.y, loc.z - 1];
                        negativeDomain = invertConnections(negativeCell.forwardMask);

                        if ((negativeDomain & domains.backward) == 0)
                        {
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Left:
                        negativeCell = map[loc.x - 1, loc.y, loc.z];
                        negativeDomain = invertConnections(negativeCell.rightMask);

                        if ((negativeDomain & domains.left) == 0)
                        {
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Down:
                        negativeCell = map[loc.x, loc.y - 1, loc.z];
                        negativeDomain = invertConnections(negativeCell.upMask);

                        overlap = negativeDomain & domains.down;
                        if (overlap == 0)
                        {
                            doesntFit = true;
                            goto MatchFound;
                        }
                        else if (
                            (overlap | alignmentMask()) == alignmentMask()
                            &&
                            !negativeCell.alignmentRestrictions[overlap].Contains(opt.rotation)
                            )
                        {

                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                }
            }

            if(!cell.verticalGap.domainFit(gapVerticalSpacingTile, domains.up, domains.down))
            {
                doesntFit = true;
                Vector3 worldLocation = loc;
                worldLocation = worldLocation.scale(floorScale);
                //Debug.DrawLine(worldLocation, worldLocation + Vector3Int.down * 2, Color.cyan, 60f);
            }
        MatchFound:
            if (doesntFit)
            {
                cell.domainMask.removeIndex(index);
                reduced = true;
            }
            else
            {
                remaining.Add(opt);
            }
        }

        if (cell.domainMask.empty)
        {
            if (ExCall)
            {
                Debug.LogError("No tile found for location");
                return RestrictTileResult.Error;
            }
            else
            {
                Debug.LogWarning("Domain reduced to 0:" + loc + ":"
                        //+ " Tiles:" + System.String.Join(',', optionsFromTileDomain(cachedDomain).Select(opt => opt.Item1.prefab.name))
                        + " Up:" + namesFromConnections(cell.upMask)
                        + " Forward:" + namesFromConnections(cell.forwardMask)
                        + " Right:" + namesFromConnections(cell.rightMask)
                        + " Down:" + namesFromConnections(map[loc.x, loc.y - 1, loc.z].upMask)
                        + " Back:" + namesFromConnections(map[loc.x, loc.y, loc.z - 1].forwardMask)
                        + " Left:" + namesFromConnections(map[loc.x - 1, loc.y, loc.z].rightMask)
                        + " Up align:" + string.Join(",", cell.alignmentRestrictions.Select(pair => pair.Key + ":" + string.Join("|", pair.Value)))
                        + " Down align:" + string.Join(",", map[loc.x, loc.y - 1, loc.z].alignmentRestrictions.Select(pair => pair.Key + ":" + string.Join("|", pair.Value)))
                        + " Up gap:" + cell.verticalGap
                        + " Down gap:" + map[loc.x, loc.y - 1, loc.z].verticalGap
                        );
                Vector3 worldLocation = loc;
                worldLocation = worldLocation.scale(floorScale);
                Debug.DrawLine(worldLocation, worldLocation + Vector3.up, Color.red, 60f);
                //start EX call
                cell.domainMask = new BigMask(ExDomain);
                cell.rightMask = EXEnhanceConnections(cell.rightMask);
                cell.upMask = EXEnhanceConnections(cell.upMask);
                cell.forwardMask = EXEnhanceConnections(cell.forwardMask);
                //Alignments
                //when an algnment isnt in the domian, its list becomes 0, preventing EX cells from using it
                //When an alignment is in place, its list should have 1 so dont modify
                List<int> bonds = new List<int>();
                foreach (int bond in cell.alignmentRestrictions.Keys)
                {
                    if (cell.alignmentRestrictions[bond].Count == 0)
                    {
                        bonds.Add(bond);
                    }
                }
                foreach (int bond in bonds)
                {
                    cell.alignmentRestrictions[bond] = new HashSet<Rotation>(allRotations);
                }
                bonds.Clear();
                WFCCell negativeCell = map[loc.x, loc.y - 1, loc.z];
                foreach (int bond in negativeCell.alignmentRestrictions.Keys)
                {
                    if (negativeCell.alignmentRestrictions[bond].Count == 0)
                    {
                        bonds.Add(bond);
                    }
                }
                foreach (int bond in bonds)
                {
                    negativeCell.alignmentRestrictions[bond] = new HashSet<Rotation>(allRotations);
                }

                return restrictTileDomain(loc, update, true);

            }

        }

        if (ExCall)
        {
            //Debug.LogWarning("EX choice: "+ cell.domainMask.);
            return RestrictTileResult.ReducedEx;
        }

        if (reduced)
        {
            update(loc, entropy(remaining, cell.groupWeights));
        }

        return reduced ? RestrictTileResult.Reduced : RestrictTileResult.NoChange;
    }

    float entropy(List<TileOption> options, Dictionary<TileType, int> groupWeights)
    {
        //return options.Count;
        if (options.Count == 0)
        {
            return 0;
        }
        if (options.Count == 1)
        {
            return -10;
        }
        float maxWeight = options[0].weight;
        float entropy = 0;
        int count = 0;
        foreach (TileOption opt in options)
        {
            if (opt.prefab.collapsePriority ==  CollapsePersuasion.Dissuade)
            {
                entropy += 1;
            }
            else if (opt.prefab.collapsePriority == CollapsePersuasion.Encourage)
            {
                entropy -= 1;
            }
            else
            {
                entropy += opt.weight / maxWeight * (1 + 0.05f * count);
            }
            count++;
        }
        entropy -= groupWeights.Select(pair =>pair.Key == TileType.Surface ? pair.Value*3: pair.Value).Sum() * 0.5f;

        //Debug.Log(entropy);
        return entropy;

    }
    delegate void CollapseEnqueue(Vector3Int loc);

    struct ConnectionDomainInfo
    {
        public ConnectionDomainMasks validConnections;
        public Dictionary<int, HashSet<Rotation>> upAlignments;
        public Dictionary<int, HashSet<Rotation>> downAlignments;
    }
    public struct ConnectionTileInfo
    {
        public Dictionary<TileDirection, int> validConnections;
    }

    
    void collapseConnections(Vector3Int loc, CollapseEnqueue queue)
    {
        WFCCell cell = map[loc.x, loc.y, loc.z];
        ConnectionDomainInfo connections = compositeConnectionDomains(cell.domainMask);
        ConnectionDomainMasks domains = connections.validConnections;
        HashSet<Vector3Int> queueLocations = new HashSet<Vector3Int>();
        //Debug.Log("Collapse Connections " + loc);
        foreach (TileDirection dir in AllDirections)
        {
            WFCCell negativeCell;
            int negativeDomain;
            bool altered;
            
            switch (dir)
            {
                case TileDirection.Forward:
                    //Debug.Log(domains[TileDirection.Forward]);
                    if (cell.forwardMask != domains.forward)
                    {
                        cell.forwardMask &= domains.forward;
                        queueLocations.Add(loc + Vector3Int.forward);
                    }
                    break;
                case TileDirection.Right:
                    if (cell.rightMask != domains.right)
                    {
                        cell.rightMask &= domains.right;
                        queueLocations.Add(loc + Vector3Int.right);
                    }
                    break;
                case TileDirection.Up:
                    altered = false;
                    if (cell.upMask != domains.up)
                    {
                        cell.upMask &= domains.up;
                        altered = true;
                        //if(cell.upMask == 0)
                        //{
                        //    Debug.LogError("Mask set to 0");
                        //    Debug.DrawLine(loc, loc + Vector3Int.up * 2, Color.red, 20f);
                        //    Debug.Break();
                        //}
                    }
                    foreach (TileConnection a in alignments)
                    {
                        int singleAlignmentMask = (int)a;
                        if (cell.alignmentRestrictions[singleAlignmentMask] != connections.upAlignments[singleAlignmentMask])
                        {
                            cell.alignmentRestrictions[singleAlignmentMask] = connections.upAlignments[singleAlignmentMask];
                            altered = true;
                        }
                    }
                    if (altered)
                    {
                        queueLocations.Add(loc + Vector3Int.up);
                    }
                    break;
                case TileDirection.Backward:
                    negativeCell = map[loc.x, loc.y, loc.z - 1];
                    negativeDomain = invertConnections(domains.backward);
                    if (negativeCell.forwardMask != negativeDomain)
                    {
                        negativeCell.forwardMask &= negativeDomain;
                        queueLocations.Add(loc + Vector3Int.back);
                    }
                    break;
                case TileDirection.Left:
                    negativeCell = map[loc.x - 1, loc.y, loc.z];
                    negativeDomain = invertConnections(domains.left);
                    if (negativeCell.rightMask != negativeDomain)
                    {
                        negativeCell.rightMask &= negativeDomain;
                        queueLocations.Add(loc + Vector3Int.left);
                    }
                    break;
                case TileDirection.Down:
                    altered = false;
                    negativeCell = map[loc.x, loc.y - 1, loc.z];
                    negativeDomain = invertConnections(domains.down);
                    if (negativeCell.upMask != negativeDomain)
                    {
                        negativeCell.upMask &= negativeDomain;
                        altered = true;
                        //if (negativeCell.upMask == 0)
                        //{
                        //    Debug.LogError("Mask set to 0");
                        //    Debug.DrawLine(loc, loc + Vector3Int.up * 2, Color.red, 20f);
                        //    Debug.Break();

                        //}
                    }
                    foreach (TileConnection a in alignments)
                    {
                        int singleAlignmentMask = (int)a;
                        if (negativeCell.alignmentRestrictions[singleAlignmentMask] != connections.downAlignments[singleAlignmentMask])
                        {
                            negativeCell.alignmentRestrictions[singleAlignmentMask] = connections.downAlignments[singleAlignmentMask];
                            altered = true;
                        }
                    }

                    if (altered)
                    {
                        queueLocations.Add(loc + Vector3Int.down);
                    }
                    break;
            }
        }
        tryPropagateGap(gapVerticalSpacingTile, loc, queueLocations, out queueLocations);

        foreach (Vector3Int q in queueLocations)
        {
            queue(q);
        }
    }

    BigMask ExDomain = new BigMask();
    static List<TileDirection> AllDirections;
    static List<TileDirection> HorizontalDirections;

    private void Start()
    {
        init();
    }
    void init()
    {
        AllDirections = EnumValues<TileDirection>().ToList();
        HorizontalDirections = new List<TileDirection>(AllDirections);
        HorizontalDirections.Remove(TileDirection.Up);
        HorizontalDirections.Remove(TileDirection.Down);
        makeDomain();


    }
    struct PathInfo
    {
        public BoundsInt bounds;
        public List<Vector3Int> path;
        public List<BoundsInt> deltaBounds;
    }
    
    PathInfo fromPath(List<Vector3> pathWorldSpace)
    {
        List<Vector3Int> pathTileSpace = pathWorldSpace.Select(l => { l.Scale(tileScale.oneOver()); return l.asInt(); }).ToList();
        //push bounds into the positive
        Vector3Int negativeMin = new Vector3Int();
        foreach (Vector3Int loc in pathTileSpace)
        {
            negativeMin.x = Mathf.Min(negativeMin.x, loc.x);
            negativeMin.y = Mathf.Min(negativeMin.y, loc.y);
            negativeMin.z = Mathf.Min(negativeMin.z, loc.z);
        }
        negativeMin -= Vector3Int.one * paddingTiles;
        List<Vector3Int> positivePath = new List<Vector3Int>();
        foreach (Vector3Int loc in pathTileSpace)
        {
            positivePath.Add(loc - negativeMin);
        }

        Bounds bigBounds = new Bounds();
        List<BoundsInt> deltaBounds = new List<BoundsInt>();
        bigBounds.center = positivePath[0];
        bigBounds.size = Vector3.zero;
        for (int i = 1; i < positivePath.Count; i++)
        {
            Bounds delta = new Bounds();
            delta.center = positivePath[(i - 1)];
            delta.size = Vector3.zero;
            delta.Encapsulate(positivePath[i]);
            delta.Expand(paddingTiles * 2);
            bigBounds.Encapsulate(delta.min);
            bigBounds.Encapsulate(delta.max);
            deltaBounds.Add(delta.asInt());
        }

        return new PathInfo
        {
            bounds = bigBounds.asInt(),
            path = positivePath,
            deltaBounds = deltaBounds,
        };
    }
    struct TileWalker
    {
        public Vector3Int location;
        Vector3Int remaining;
        public TileDirection lastDirection;
        public float arriveMagnitude;

        HashSet<Vector3Int> prevLocations;

        public TileWalker(Vector3Int loc)
        {
            location = loc;
            remaining = Vector3Int.zero;
            lastDirection = TileDirection.Forward;
            arriveMagnitude = 1.5f;
            prevLocations = new HashSet<Vector3Int>();
            prevLocations.Add(loc);
        }

        public void target(Vector3Int loc)
        {
            remaining = loc - location;
            prevLocations.Clear();
            prevLocations.Add(location);
        }

        public bool arrived
        {
            get
            {
                return remaining.magnitude < arriveMagnitude;
            }
        }

        TileDirection step(TileDirection dir)
        {
            lastDirection = dir;
            Vector3Int diff = fromDir(lastDirection);

            location += diff;
            remaining -= diff;

            prevLocations.Add(location);
            return lastDirection;
        }

        public TileDirection walk(WFCGeneration inst)
        {

            List<TileDirection> preferredDirections = new List<TileDirection>();
            List<TileDirection> walkableDirs = inst.walkableDirections(location);
            List<TileDirection> lastDirList = new List<TileDirection>() { opposite(lastDirection) };
            List<TileDirection> verticalDirList = new List<TileDirection>() { TileDirection.Up, TileDirection.Down };
            List<TileDirection> frontierDirections = new List<TileDirection>();

            if (remaining.x != 0)
            {
                if (remaining.x > 0)
                {
                    preferredDirections.Add(TileDirection.Right);
                }
                else
                {
                    preferredDirections.Add(TileDirection.Left);
                }
            }
            if (remaining.y != 0)
            {
                if (remaining.y > 0)
                {
                    preferredDirections.Add(TileDirection.Up);
                }
                else
                {
                    preferredDirections.Add(TileDirection.Down);
                }
            }
            if (remaining.z != 0)
            {
                if (remaining.z > 0)
                {
                    preferredDirections.Add(TileDirection.Forward);
                }
                else
                {
                    preferredDirections.Add(TileDirection.Backward);
                }
            }

            foreach (TileDirection dir in AllDirections)
            {
                Vector3Int adj = location + fromDir(dir);
                if (!prevLocations.Contains(adj))
                {
                    frontierDirections.Add(dir);
                }
            }

            List<TileDirection> overlap = walkableDirs.Intersect(preferredDirections).Intersect(frontierDirections).ToList();
            if (overlap.Count > 0)
            {
                return step(overlap.RandomItem());
            }
            overlap = walkableDirs.Intersect(frontierDirections).ToList();
            if (overlap.Count > 0)
            {
                return step(overlap.RandomItem());
            }
            overlap = walkableDirs.Except(lastDirList).ToList();
            if (overlap.Count > 0)
            {
                return step(overlap.RandomItem());
            }

            return step(opposite(lastDirection));

        }
    }
    GameObject floorRoot;
    public GenData generationData;
    Vector3 floorScale
    {
        get
        {
            return floorRoot.transform.lossyScale;
        }
    }
    public struct GenData
    {
        public Vector3 start;
        public Vector3 end;
        public List<SpawnTransform> spawns;
        public List<GameObject> navTiles;
    }
    static readonly int paddingWorld = 50;
    static readonly int gapVerticalSpacingWorld = 18;
    int paddingTiles
    {
        get
        {
            return Mathf.CeilToInt(paddingWorld / tileScale.x);
        }
    }
    int gapVerticalSpacingTile
    {
        get
        {
            return Mathf.CeilToInt(gapVerticalSpacingWorld / tileScale.y);
        }
    }
    public struct WFCParameters
    {
        public int segmentCount;
        public int segmentBaseLength;
        public int segmentVariableLength;
        public float straightnessPercent;
        public float verticalityPercent;

        public static WFCParameters basic()
        {
            return new WFCParameters
            {
                segmentCount = Mathf.RoundToInt(Random.value.asRange(4, 6)),
                segmentBaseLength = 50,
                segmentVariableLength = 80,
                straightnessPercent = 0.3f,
                verticalityPercent = 0.25f,
            };
        }
    }

    bool generationBroken = false;
    public IEnumerator generate(GameObject floor, WFCParameters parameters)
    {
        floorRoot = floor;
        floorRoot.transform.localScale = tileScale;
        bool makingFloor = true;
        while (makingFloor)
        {

            generationBroken = false;
            yield return collapseCells(makePath(parameters));

            if (generationBroken)
            {
                Debug.LogWarning("Retrying...");
                Debug.Break();
                yield return null;
                foreach (Transform child in floorRoot.transform)
                {
                    Destroy(child.gameObject);
                }
            }
            else
            {
                makingFloor = false;
            }


        }



    }

    void mapCopy(IEnumerable<Vector3Int> locations, bool toBackup)
    {
        if (toBackup)
        {
            foreach (Vector3Int loc in locations)
            {
                mapBackup[loc.x, loc.y, loc.z] = new WFCCell(map[loc.x, loc.y, loc.z]);
            }

        }
        else
        {
            foreach (Vector3Int loc in locations)
            {
                map[loc.x, loc.y, loc.z] = mapBackup[loc.x, loc.y, loc.z];
            }
        }

    }


    public IEnumerator collapseCells(List<Vector3> randomPath)
    {
        Debug.Log("Start: " + Time.time);
        generationData = new GenData();
        SimplePriorityQueue<Vector3Int> collapseQueue = new SimplePriorityQueue<Vector3Int>();
        int chainCount = 0;
        Queue<Vector3Int> propagation = new Queue<Vector3Int>();

        System.Action<Vector3Int> restrictAndCollapse = (Vector3Int loc) =>
        {
            RestrictTileResult res = restrictTileDomain(loc, collapseQueue.UpdatePriority);
            if (res == RestrictTileResult.Reduced || res == RestrictTileResult.ReducedEx)
            {
                collapseConnections(loc, propagation.Enqueue);
            }
            else if (res == RestrictTileResult.Error)
            {
                generationBroken = true;
                propagation.Clear();
            }
        };

        System.Action propagationLoop = () =>
        {
            while (propagation.Count > 0)
            {
                chainCount++;
                Vector3Int propLocation = propagation.Dequeue();
                restrictAndCollapse(propLocation);
            }
        };


        System.Action<Vector3Int> constrainAfterTileSet = (Vector3Int loc) =>
        {
            collapseConnections(loc, propagation.Enqueue);
            propagationLoop();
        };

        System.Action<Vector3Int> constrainAfterConnectionSet = (Vector3Int loc) =>
        {
            restrictAndCollapse(loc);
            propagationLoop();
        };

        (BigMask fullDomain, BigMask Ex) = fullDomainMask();
        ExDomain = Ex;
        int vertConnections, horizonConnections;
        fullConnectionMask(fullDomain, out vertConnections, out horizonConnections);
        Dictionary<int, HashSet<Rotation>> fullRestrictions = fullAlignmentMask();
        PathInfo infoP = fromPath(randomPath);
        List<Vector3Int> path = infoP.path;
        generationData.start = path[0].asFloat().scale(floorScale);
        Vector3Int startLoc = path[0];
        BoundsInt bounds = infoP.bounds;
        List<BoundsInt> deltas = infoP.deltaBounds;
        HashSet<Vector3Int> uniqueLocations = new HashSet<Vector3Int>();


        map = new WFCCell[bounds.size.x + 1, bounds.size.y + 1, bounds.size.z + 1];
        mapBackup = new WFCCell[bounds.size.x + 1, bounds.size.y + 1, bounds.size.z + 1];
        //Debug.Log(bounds.size);

        Dictionary<TileDirection, HashSet<Vector3Int>> edgeBindings = new Dictionary<TileDirection, HashSet<Vector3Int>>();
        foreach (TileDirection dir in AllDirections)
        {
            edgeBindings[dir] = new HashSet<Vector3Int>();
        }

        foreach (BoundsInt delta in deltas)
        {
            int xMax = delta.max.x;
            int yMax = delta.max.y;
            int zMax = delta.max.z;
            int xMin = delta.min.x;
            int yMin = delta.min.y;
            int zMin = delta.min.z;
            //Debug.Log(delta.min);
            //Debug.Log(delta.max);
            DrawBox(delta.center.scale(floorScale), Quaternion.identity, delta.size.asFloat().scale(floorScale), Color.white, 600);

            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        Vector3Int loc = new Vector3Int(x, y, z);
                        if (map[x, y, z] == null)
                        {
                            map[x, y, z] = new WFCCell();
                            map[x, y, z].init(fullDomain, vertConnections, horizonConnections, fullRestrictions);
                            uniqueLocations.Add(loc);
                        }


                        if (
                            !map[x, y, z].ready
                            &&
                            x > xMin && x < xMax
                            &&
                            y > yMin && y < yMax
                            &&
                            z > zMin && z < zMax
                            )
                        {
                            map[x, y, z].makeReady();
                            collapseQueue.Enqueue(loc, domainTiles.Count);

                            if (y == yMin + 1)
                            {
                                edgeBindings[TileDirection.Down].Add(loc);
                            }
                            else if (y == yMax - 1)
                            {
                                edgeBindings[TileDirection.Up].Add(loc);
                            }

                            if (x == xMin + 1)
                            {
                                edgeBindings[TileDirection.Left].Add(loc);
                            }
                            else if (x == xMax - 1)
                            {
                                edgeBindings[TileDirection.Right].Add(loc);
                            }

                            if (z == zMin + 1)
                            {
                                edgeBindings[TileDirection.Backward].Add(loc);
                            }
                            else if (z == zMax - 1)
                            {
                                edgeBindings[TileDirection.Forward].Add(loc);
                            }



                        }

                    }
                }
            }
        }
        yield return null;
        Debug.Log("Init: " + Time.time);

        //int wallMaskIn = (int)(TileConnection.GroundConnect | TileConnection.AirConnect);
        //int wallMaskOut = (int)(TileConnection.Ground | TileConnection.Air | TileConnection.AirConnect );
        int wallMaskIn = (int)(TileConnection.AirConnect);
        int wallMaskOut = (int)( TileConnection.Air | TileConnection.AirConnect);
        int skyMaskOut = (int)(TileConnection.Air | TileConnection.AirConnect );
        int groundMaskIn = (int)(TileConnection.GroundConnect | TileConnection.AirConnect);

        foreach (TileDirection dir in AllDirections)
        {
            HashSet<Vector3Int> locs = edgeBindings[dir];
            foreach (Vector3Int loc in locs)
            {
                Vector3Int adj = loc + fromDir(dir);
                if (!map[adj.x, adj.y, adj.z].ready)
                {
                    switch (dir)
                    {
                        case TileDirection.Forward:
                            map[loc.x, loc.y, loc.z].forwardMask = wallMaskOut;
                            break;
                        case TileDirection.Right:
                            map[loc.x, loc.y, loc.z].rightMask = wallMaskOut;
                            break;
                        case TileDirection.Up:
                            map[loc.x, loc.y, loc.z].upMask = skyMaskOut;
                            break;
                        case TileDirection.Backward:
                            map[adj.x, adj.y, adj.z].forwardMask = wallMaskIn;
                            break;
                        case TileDirection.Left:
                            map[adj.x, adj.y, adj.z].rightMask = wallMaskIn;
                            break;
                        case TileDirection.Down:
                            map[adj.x, adj.y, adj.z].upMask = groundMaskIn;
                            break;
                    }

                    constrainAfterConnectionSet(loc);

                    if (generationBroken)
                    {
                        yield break;
                    }

                    if (chainCount >= collapsePerFrame)
                    {
                        chainCount = 0;
                        yield return null;
                    }
                }
            }
        }
        Debug.Log("Constrain Walls: " + Time.time);
        //Debug.Break();
        yield return null;

        TileWalker walker = new TileWalker(path[0]);

        path.RemoveAt(0);

        int stepsThisPath = 0;
        Vector3Int lastPos = Vector3Int.zero;
        int retries = 0;

        while (path.Count > 0)
        {
            //Debug.Break();
            if (walker.arrived)
            {
                mapCopy(uniqueLocations, true);
                lastPos = walker.location;
                walker.target(path[0]);
            }

            Vector3Int currentLoc = walker.location;
            TileDirection change = walker.walk(this);
            switch (change)
            {
                case TileDirection.Forward:
                    map[currentLoc.x, currentLoc.y, currentLoc.z].forwardMask &= walkableMask;
                    break;
                case TileDirection.Right:
                    map[currentLoc.x, currentLoc.y, currentLoc.z].rightMask &= walkableMask;
                    break;
                case TileDirection.Up:
                    map[currentLoc.x, currentLoc.y, currentLoc.z].upMask &= walkableMask;
                    break;
                case TileDirection.Backward:
                    map[walker.location.x, walker.location.y, walker.location.z].forwardMask &= walkableMask;
                    break;
                case TileDirection.Left:
                    map[walker.location.x, walker.location.y, walker.location.z].rightMask &= walkableMask;
                    break;
                case TileDirection.Down:
                    map[walker.location.x, walker.location.y, walker.location.z].upMask &= walkableMask;
                    break;
            }

            Vector3 debugOffset = Random.insideUnitSphere * 0.07f;
            Debug.DrawLine((currentLoc.asFloat() + debugOffset).scale(floorScale), (walker.location.asFloat() + debugOffset).scale(floorScale), Color.blue, 600);
            constrainAfterConnectionSet(currentLoc);
            if (generationBroken)
            {
                yield break;
            }
            constrainAfterConnectionSet(walker.location);
            if (generationBroken)
            {
                yield break;
            }


            if (chainCount >= collapsePerFrame)
            {
                chainCount = 0;
                yield return null;
            }



            if (walker.arrived)
            {
                Debug.DrawLine(path[0].asFloat().scale(floorScale), (path[0].asFloat() + Vector3.up).scale(floorScale), Color.green, 600);
                path.RemoveAt(0);
                stepsThisPath = 0;
                walker.arriveMagnitude = 1.5f;
            }
            stepsThisPath++;
            if (stepsThisPath >= 5000 / tileScale.x)
            {
                Debug.DrawLine(path[0].asFloat().scale(floorScale), (path[0].asFloat() + Vector3.up).scale(floorScale), Color.red, 600);
                Debug.LogWarning("Too many steps on this segment");
                retries++;
                if (retries >= 3)
                {
                    Debug.LogError("Too many retries for path");
                    generationBroken = true;
                    yield break;
                }
                else if (retries == 2)
                {
                    walker.arriveMagnitude *= 2;
                }
                walker.location = lastPos;
                walker.target(path[0]);
                mapCopy(uniqueLocations, false);
                stepsThisPath = 0;
            }
        }
        generationData.end = walker.location.asFloat().scale(floorScale);
        Debug.Log("Constrain Walk: " + Time.time);



        while (collapseQueue.Count > 0)
        {
            Vector3Int coords = collapseQueue.Dequeue();
            Vector3 location = new Vector3(coords.x, coords.y, coords.z);
            location.Scale(floorScale);

            WFCCell cell = map[coords.x, coords.y, coords.z];
            cell.domainMask = selectFromTileDomain(cell.domainMask,cell.groupWeights );
            TileOption opt = optionFromSingleDomain(cell.domainMask);
            addLikeTypeGroups(opt, coords);
            GameObject instance = null;
            WFCTileOption alt = null;
            if (!opt.prefab.skipSpawn)
            {
                GameObject prefab = opt.prefab.getPrefabToSpawn();
                alt = prefab.GetComponent<WFCTileOption>();
                float additionalRot = 0;
                if (alt)
                {
                    additionalRot = alt.rotationOptions switch
                    {
                        TileOptionRotations.Halves => Mathf.Round(Random.value) * 180,
                        TileOptionRotations.Quarters => Mathf.Round(Random.value.asRange(0,4)) * 90,
                        _ => 0,
                    };
                }

                instance = Instantiate(
                    prefab,
                    location,
                    Quaternion.AngleAxis(
                        additionalRot + opt.rotation.degrees()
                        , Vector3.up
                    ),
                    floorRoot.transform
                );
                //TODO spawn tile?
            }
            NavLoad navLoadType = (opt.prefab.navType, alt == null ? false : alt.navAddThis) switch
            {
                (NavLoad.None, true) => NavLoad.This,
                (NavLoad.Beneath, true) => NavLoad.ThisAndBeneath,
                _ => opt.prefab.navType,
            };

            cell.collapse(navLoadType, instance);

            constrainAfterTileSet(coords);

            if (generationBroken)
            {
                yield break;
            }
            if (chainCount >= collapsePerFrame)
            {
                chainCount = 0;
                yield return null;
            }


        }
        Debug.Log("Collapse: " + Time.time);

        generationData.spawns = getSpawns(startLoc);
        foreach (SpawnTransform spawn in generationData.spawns)
        {
            Debug.DrawLine(spawn.position, spawn.position + Vector3.forward * spawn.halfExtents.y + Vector3.right * spawn.halfExtents.x, Color.magenta, 600);
        }
        Debug.Log("Spawns: " + Time.time);
        yield return null;
        generationData.navTiles = getNavigation(uniqueLocations);
        Debug.Log("Nav: " + Time.time);
    }



    List<Vector3> makePath(WFCParameters parameters)
    {
        Vector3 point = Vector3.zero;
        List<Vector3> path = new List<Vector3>();
        path.Add(point);

        Vector3 diff = Vector3.zero;

        int points = parameters.segmentCount + 1;
        for (int i = 0; i < points; i++)
        {
            Vector2 dir2d = Random.insideUnitCircle.normalized;
            Vector3 dir = new Vector3(dir2d.x, 0, dir2d.y);

            if (i != 0)
            {
                Vector3 flatDiff = new Vector3(diff.x, 0, diff.z);
                float angle = Vector3.Angle(dir, flatDiff);
                angle *= parameters.straightnessPercent;
                dir = Vector3.RotateTowards(dir, flatDiff, Mathf.PI * angle / 180, 0);
            }
            dir = Vector3.RotateTowards(dir, Random.value > 0.5f ? Vector3.up : Vector3.down, Mathf.PI *0.5f * parameters.verticalityPercent * Random.value, 0);
            dir.Normalize();


            diff = dir * (parameters.segmentBaseLength + Random.value * parameters.segmentVariableLength);

            point += diff;
            path.Add(point);
        }
        return path;
    }

    List<SpawnTransform> getSpawns(Vector3Int start)
    {
        Queue<Vector3Int> search = new Queue<Vector3Int>();
        HashSet<Vector3Int> found = new HashSet<Vector3Int>();
        List<SpawnTransform> spawns = new List<SpawnTransform>();
        search.Enqueue(start);
        found.Add(start);
        int maxSearch = 30_000;
        int searchCount = 0;
        while (search.Count > 0 && searchCount < maxSearch)
        {
            searchCount++;
            Vector3Int loc = search.Dequeue();

            List<TileDirection> walkable = walkableDirections(loc);
            foreach (TileDirection dir in walkable)
            {
                Vector3Int adj = loc + fromDir(dir);
                if (!found.Contains(adj))
                {
                    found.Add(adj);
                    search.Enqueue(adj);
                }
            }

            if (
                !walkable.Contains(TileDirection.Down)
                & (loc - start).magnitude > 2f
                )
            {
                spawns.Add(new SpawnTransform { position = loc.asFloat().scale(floorScale), halfExtents = floorScale * 0.5f });
            }
        }

        return spawns;
    }


    List<GameObject> getNavigation(IEnumerable<Vector3Int> uniqueLocations)
    {
        List<GameObject> nav = new List<GameObject>();
        foreach (Vector3Int loc in uniqueLocations)
        {
            WFCCell cell = map[loc.x, loc.y, loc.z];

            if(cell.navType == NavLoad.This || cell.navType == NavLoad.ThisAndBeneath)
            {
                nav.Add(cell.instancedCell);
            }
            if (cell.navType == NavLoad.Beneath || cell.navType == NavLoad.ThisAndBeneath)
            {
                WFCCell adj = map[loc.x, loc.y - 1, loc.z];
                nav.Add(adj.instancedCell);
            }

        }
        return nav;

    }

}