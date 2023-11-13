using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MonsterSpawn;
using static Utils;
using static WFCTile;

public class WFCGeneration : MonoBehaviour
{
    public Vector3 tileScale = new Vector3(8, 4, 8);
    public int collapsePerFrame = 100;
    public GameObject TileFailurePre;
    public List<TileWeight> tiles;

    [System.Serializable]
    public struct TileWeight
    {
        public float weight;
        public WFCTile prefab;
    }
    public enum TileConnection
    {
        Flat = 1 << 0,
        Air = 1 << 1,
        AirConnect = 1 << 2,
        Ground = 1 << 3,
        GroundConnect = 1 << 4,
        RampLeft = 1 << 5,
        RampRight = 1 << 6,
        RampTop = 1 << 7,
        WallLeft = 1 << 8,
        WallRight = 1 << 9,
        FlatUnwalkable = 1 << 10,
        FlatUnwalkableConnect = 1 << 11,
        GateLeft = 1 << 12,
        GateRight = 1 << 13,
        ArchRightConnect = 1 << 14,
        ArchLeft = 1 << 15,
        ArchRight = 1 << 16,
        ArchLeftConnect = 1 << 17,
        Air2 = 1 << 18,
    }

    int connectionsRightMask = (int)(
        TileConnection.AirConnect |
        TileConnection.GroundConnect |
        TileConnection.RampRight |
        TileConnection.WallRight |
        TileConnection.FlatUnwalkableConnect |
        TileConnection.GateRight |
        TileConnection.ArchRight |
        TileConnection.ArchLeftConnect
        );
    int connectionsLeftMask = (int)(
        TileConnection.RampLeft |
        TileConnection.WallLeft |
        TileConnection.GateLeft |
        TileConnection.ArchLeft |
        TileConnection.ArchRightConnect
        );

    int enhancementLeftMask = (int)(
        TileConnection.Air |
        TileConnection.Ground |
        TileConnection.FlatUnwalkable |
        TileConnection.ArchRight
        );

    int enhancementRightMask = (int)(
        TileConnection.ArchLeft
        );




    public static int walkableMask = (int)(
        TileConnection.Flat 
        | TileConnection.RampTop 
        | TileConnection.RampLeft 
        | TileConnection.RampRight
        | TileConnection.WallLeft
        | TileConnection.WallRight
        );
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
        public WFCCell()
        {
            ready = false;
            collapsed = false;
            domainMask = new BigMask();
            forwardMask = 0;
            rightMask = 0;
            upMask = 0;
            alignmentRestrictions = new Dictionary<int, HashSet<Rotation>>();
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
        }

        public void init(BigMask fullDomain, int fullConnection, Dictionary<int, HashSet<Rotation>> fullRestrictions)
        {
            domainMask = new BigMask(fullDomain);
            forwardMask = fullConnection;
            rightMask = fullConnection;
            upMask = fullConnection;
            alignmentRestrictions = new Dictionary<int, HashSet<Rotation>>(fullRestrictions);
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

    int fullConnectionMask()
    {
        return connectionDomain(EnumValues<TileConnection>());
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

        if ((domain & ((int)TileConnection.Air2)) > 0)
        {
            newDomain |= (int)TileConnection.Air;
        }

        return newDomain;
    }
    public enum Rotation
    {
        None,
        Quarter,
        Half,
        ThreeQuarters,

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
                    rots.Add(Rotation.Half);
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


    BigMask selectFromTileDomain(BigMask domain)
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

            indexWeights.Add((index, opt.weight));
            weightSum += opt.weight;
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
                        + " Up:" + cell.upMask
                        + " Forward:" + cell.forwardMask
                        + " Right:" + cell.rightMask
                        + " Down:" + map[loc.x, loc.y - 1, loc.z].upMask
                        + " Back:" + map[loc.x, loc.y, loc.z - 1].forwardMask
                        + " Left:" + map[loc.x - 1, loc.y, loc.z].rightMask
                        + " Up align:" + string.Join(",", cell.alignmentRestrictions.Select(pair => pair.Key + ":" + string.Join("|", pair.Value)))
                        + " Down align:" + string.Join(",", map[loc.x, loc.y - 1, loc.z].alignmentRestrictions.Select(pair => pair.Key + ":" + string.Join("|", pair.Value)))
                        );

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
            return RestrictTileResult.ReducedEx;
        }

        if (reduced)
        {
            update(loc, entropy(remaining));
        }

        return reduced ? RestrictTileResult.Reduced : RestrictTileResult.NoChange;
    }

    float entropy(List<TileOption> options)
    {
        //return options.Count;
        if (options.Count == 0)
        {
            return 0;
        }
        float maxWeight = options[0].weight;
        float entropy = 0;
        int count = 0;
        foreach (TileOption opt in options)
        {
            if (opt.prefab.dissuadeCollapse)
            {
                entropy += 1;
            }
            else
            {
                entropy += opt.weight / maxWeight * (1 + 0.05f * count);
            }
            count++;
        }

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
                        queue(loc + new Vector3Int(0, 0, 1));
                    }
                    break;
                case TileDirection.Right:
                    if (cell.rightMask != domains.right)
                    {
                        cell.rightMask &= domains.right;
                        queue(loc + new Vector3Int(1, 0, 0));
                    }
                    break;
                case TileDirection.Up:
                    altered = false;
                    if (cell.upMask != domains.up)
                    {
                        cell.upMask &= domains.up;
                        altered = true;
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
                        queue(loc + new Vector3Int(0, 1, 0));
                    }
                    break;
                case TileDirection.Backward:
                    negativeCell = map[loc.x, loc.y, loc.z - 1];
                    negativeDomain = invertConnections(domains.backward);
                    if (negativeCell.forwardMask != negativeDomain)
                    {
                        negativeCell.forwardMask &= negativeDomain;
                        queue(loc + new Vector3Int(0, 0, -1));
                    }
                    break;
                case TileDirection.Left:
                    negativeCell = map[loc.x - 1, loc.y, loc.z];
                    negativeDomain = invertConnections(domains.left);
                    if (negativeCell.rightMask != negativeDomain)
                    {
                        negativeCell.rightMask &= negativeDomain;
                        queue(loc + new Vector3Int(-1, 0, 0));
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
                        queue(loc + new Vector3Int(0, -1, 0));
                    }
                    break;
            }
        }
    }

    BigMask ExDomain = new BigMask();
    static List<TileDirection> AllDirections;

    private void Start()
    {
        init();
    }
    void init()
    {
        AllDirections = EnumValues<TileDirection>().ToList();
        makeDomain();


    }
    struct PathInfo
    {
        public BoundsInt bounds;
        public List<Vector3Int> path;
        public List<BoundsInt> deltaBounds;
    }
    static readonly int padding = 4;
    PathInfo fromPath(List<Vector3Int> path)
    {
        //push bounds into the positive
        Vector3Int negativeMin = new Vector3Int();
        foreach (Vector3Int loc in path)
        {
            negativeMin.x = Mathf.Min(negativeMin.x, loc.x);
            negativeMin.y = Mathf.Min(negativeMin.y, loc.y);
            negativeMin.z = Mathf.Min(negativeMin.z, loc.z);
        }
        negativeMin -= Vector3Int.one * padding;
        List<Vector3Int> positivePath = new List<Vector3Int>();
        foreach (Vector3Int loc in path)
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
            delta.Expand(padding * 2);
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

    bool generationBroken = false;
    public IEnumerator generate(GameObject floor)
    {
        floorRoot = floor;
        floorRoot.transform.localScale = tileScale;
        bool makingFloor = true;
        while (makingFloor)
        {

            generationBroken = false;
            yield return collapseCells(makePath());

            if (generationBroken)
            {
                Debug.LogWarning("Retrying...");
                Debug.Break();
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


    public IEnumerator collapseCells(List<Vector3Int> randomPath)
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
        int fullConnection = fullConnectionMask();
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
                            map[x, y, z].init(fullDomain, fullConnection, fullRestrictions);
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

        int wallMaskIn = (int)(TileConnection.GroundConnect | TileConnection.AirConnect);
        int wallMaskOut = (int)(TileConnection.Ground | TileConnection.Air );
        int skyMaskOut = (int)(TileConnection.Air | TileConnection.AirConnect | TileConnection.Air2);
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


        TileWalker walker = new TileWalker(path[0]);

        path.RemoveAt(0);

        int stepsThisPath = 0;
        Vector3Int lastPos = Vector3Int.zero;
        int retries = 0;

        while (path.Count > 0)
        {
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
            if (stepsThisPath >= 400)
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
            cell.domainMask = selectFromTileDomain(cell.domainMask);
            TileOption opt = optionFromSingleDomain(cell.domainMask);
            GameObject instance = null;
            if (!opt.prefab.skipSpawn)
            {
                instance = Instantiate(
                    opt.prefab.gameObject,
                    location,
                    Quaternion.AngleAxis(
                        opt.rotation switch
                        {
                            Rotation.Quarter => 90,
                            Rotation.Half => 180,
                            Rotation.ThreeQuarters => 270,
                            _ => 0,
                        }
                        , Vector3.up
                    ),
                    floorRoot.transform
                );
                //TODO spawn tile?
            }
            cell.collapse(opt.prefab.navType, instance);

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



    List<Vector3Int> makePath()
    {
        Vector3Int point = Vector3Int.zero;
        List<Vector3Int> path = new List<Vector3Int>();
        path.Add(point);

        Vector3 diff = Vector3.zero;

        int points = 6;
        for (int i = 0; i < points; i++)
        {
            Vector2 dir2d = Random.insideUnitCircle.normalized;
            Vector3 dir = new Vector3(dir2d.x, 0, dir2d.y);

            if (i != 0)
            {
                Vector3 flatDiff = new Vector3(diff.x, 0, diff.z);
                float angle = Vector3.Angle(dir, flatDiff);
                angle *= 0.25f;
                dir = Vector3.RotateTowards(dir, flatDiff, Mathf.PI * angle / 180, 0);
            }
            dir = Vector3.RotateTowards(dir, Random.value > 0.5f ? Vector3.up : Vector3.down, Mathf.PI * 0.3f * Random.value, 0);
            dir.Normalize();

            diff = dir * (4 + Random.value * 6);

            point += diff.asInt();
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
                spawns.Add(new SpawnTransform { position = loc.asFloat().scale(floorScale), rotation = Quaternion.identity, halfExtents = floorScale * 0.5f });
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
            switch (cell.navType)
            {
                case NavLoad.This:
                    nav.Add(cell.instancedCell);
                    break;
                case NavLoad.Beneath:
                    WFCCell adj = map[loc.x, loc.y - 1, loc.z];
                    nav.Add(adj.instancedCell);
                    break;

            }

        }
        return nav;

    }

}