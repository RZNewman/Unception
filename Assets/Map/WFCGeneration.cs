using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using static Utils;

public class WFCGeneration : MonoBehaviour
{
    public int collapsePerFrame = 4;
    public Vector3Int mapSize = new Vector3Int(100, 30, 100);
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
        Ground = 1 << 2,
        AirConnect = 1 << 3,
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
        ArchLeft = 1 << 14,
        ArchRight = 1 << 15,
        ArchRightConnect = 1 << 16,
        ArchLeftConnect = 1 << 17,
        Air2 = 1 << 18,
    }

    Dictionary<TileConnection, TileConnection> inversions = new Dictionary<TileConnection, TileConnection>()
    {
        {TileConnection.AirConnect, TileConnection.Air },
        {TileConnection.GroundConnect, TileConnection.Ground },
        {TileConnection.RampLeft, TileConnection.RampRight },
        {TileConnection.RampRight, TileConnection.RampLeft },
        {TileConnection.WallLeft, TileConnection.WallRight },
        {TileConnection.WallRight, TileConnection.WallLeft },
        {TileConnection.FlatUnwalkableConnect, TileConnection.FlatUnwalkable },
        {TileConnection.GateLeft, TileConnection.GateRight },
        {TileConnection.GateRight, TileConnection.GateLeft },
        {TileConnection.ArchLeft, TileConnection.ArchRight },
        {TileConnection.ArchRight, TileConnection.ArchLeft },
        {TileConnection.ArchLeftConnect, TileConnection.ArchRight },
        {TileConnection.ArchRightConnect, TileConnection.ArchLeft },
    };
    Dictionary<TileConnection, TileConnection> inversionsReverse = new Dictionary<TileConnection, TileConnection>()
    {
        {TileConnection.Air, TileConnection.AirConnect },
        {TileConnection.Ground, TileConnection.GroundConnect },
        {TileConnection.FlatUnwalkable, TileConnection.FlatUnwalkableConnect },
        {TileConnection.ArchLeft, TileConnection.ArchRightConnect },
        {TileConnection.ArchRight, TileConnection.ArchLeftConnect },
    };
    public static int walkableMask()
    {
        return (int)(TileConnection.Flat | TileConnection.RampTop);
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

        public Dictionary<int, List<Rotation>> alignmentRestrictions;

        public void init(BigMask fullDomain, int fullConnection, Dictionary<int, List<Rotation>> fullRestrictions)
        {
            domainMask = new BigMask(fullDomain);
            forwardMask = fullConnection;
            rightMask = fullConnection;
            upMask = fullConnection;
            alignmentRestrictions = new Dictionary<int, List<Rotation>>(fullRestrictions);
        }
        public void makeReady()
        {
            ready = true;
        }


    }

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

    Dictionary<int, List<Rotation>> fullAlignmentMask()
    {
        Dictionary<int, List<Rotation>> restrictions = new Dictionary<int, List<Rotation>>();
        foreach (TileConnection a in alignments)
        {
            restrictions[(int)a] = new List<Rotation>(allRotations);
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
    const int impossibleInvertMask = (int)(TileConnection.FlatUnwalkableConnect | TileConnection.AirConnect | TileConnection.GroundConnect);
    int invertConnections(int domain)
    {
        int subtractionMask = 0;
        int additionMask = 0;
        foreach (TileConnection key in inversions.Keys)
        {
            int keyMask = (int)key;
            if ((domain & keyMask) > 0)
            {
                subtractionMask |= keyMask;
                additionMask |= (int)inversions[key];
            }
        }
        foreach (TileConnection key in inversionsReverse.Keys)
        {
            int keyMask = (int)key;
            if ((domain & keyMask) > 0)
            {
                additionMask |= (int)inversionsReverse[key];
            }
        }
        int newMask = (domain ^ subtractionMask) | additionMask;
        if ((newMask & impossibleInvertMask) > 0
            && ((newMask | impossibleInvertMask) == impossibleInvertMask)
            )
        {
            throw new System.Exception("impossible invert: " + domain + " resulted in " + newMask);
        }
        return newMask;
    }
    int EXEnhanceConnections(int domain)
    {
        foreach (TileConnection key in inversionsReverse.Keys)
        {
            int keyMask = (int)key;
            TileConnection value = inversionsReverse[key];
            int valueMask = (int)value;

            if ((domain & valueMask) > 0)
            {
                domain |= keyMask;
            }
        }
        return domain;
    }
    public enum Rotation
    {
        None,
        Quarter,
        Half,
        ThreeQuarters,

    }

    struct TileOption
    {
        public float weight;
        public WFCTile prefab;
        public Rotation rotation;

        public Dictionary<TileDirection, int> getDomains()
        {
            return prefab.getDomains(rotation);
        }
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
                domainTiles.Add(new TileOption
                {
                    rotation = r,
                    prefab = tile.prefab,
                    weight = tile.weight / rots.Count,
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

    List<(TileOption, int)> optionsFromTileDomain(BigMask domain)
    {
        if (domain.empty)
        {
            throw new System.Exception("Empty Domain");
        }
        return domain.indicies.Select(ind => (domainTiles[ind], ind)).ToList();
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
            validConnections = new Dictionary<TileDirection, int>(),
            downAlignments = new Dictionary<int, List<Rotation>>(),
            upAlignments = new Dictionary<int, List<Rotation>>(),
        };
        foreach (TileDirection dir in EnumValues<TileDirection>())
        {
            connections.validConnections[dir] = 0;
        }
        foreach (TileConnection conn in alignments)
        {
            int singleAlignmentMask = (int)conn;
            connections.upAlignments[singleAlignmentMask] = new List<Rotation>();
            connections.downAlignments[singleAlignmentMask] = new List<Rotation>();
        }
        foreach ((TileOption opt, _) in optionsFromTileDomain(domain))
        {
            Dictionary<TileDirection, int> connectionsTile = opt.getDomains();
            foreach (TileDirection dir in EnumValues<TileDirection>())
            {
                connections.validConnections[dir] |= connectionsTile[dir];
                //Debug.Log(connections[dir]);
            }
            foreach (TileConnection conn in alignments)
            {

                int singleAlignmentMask = (int)conn;
                if ((connectionsTile[TileDirection.Up] & singleAlignmentMask) > 0)
                {
                    if (!connections.upAlignments[singleAlignmentMask].Contains(opt.rotation))
                    {
                        connections.upAlignments[singleAlignmentMask].Add(opt.rotation);
                    }
                }
                if ((connectionsTile[TileDirection.Down] & singleAlignmentMask) > 0)
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
    bool restrictTileDomain(Vector3Int loc, UpdateEntropy update, bool ExCall = false)
    {
        WFCCell cell = map[loc.x, loc.y, loc.z];
        if (!cell.ready)
        {
            return false;
        }
        if (cell.collapsed)
        {
            return false;
        }
        List<(TileOption, int)> options = optionsFromTileDomain(cell.domainMask);
        List<TileOption> remaining = new List<TileOption>();
        bool reduced = false;
        List<string> debugConnentions = new List<string>();
        foreach ((TileOption opt, int index) in options)
        {
            debugConnentions.Add(" Tile:" + opt.prefab.name + "*" + opt.rotation + " ");
            Dictionary<TileDirection, int> domains = opt.getDomains();
            bool doesntFit = false;
            foreach (TileDirection dir in EnumValues<TileDirection>())
            {
                WFCCell negativeCell;
                int negativeDomain;
                int overlap;
                switch (dir)
                {
                    case TileDirection.Forward:
                        debugConnentions.Add(" Forward:" + cell.forwardMask);
                        if ((cell.forwardMask & domains[TileDirection.Forward]) == 0)
                        {

                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Right:
                        debugConnentions.Add(" Right:" + cell.rightMask);
                        if ((cell.rightMask & domains[TileDirection.Right]) == 0)
                        {

                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Up:
                        debugConnentions.Add(" Up:" + cell.upMask);
                        overlap = cell.upMask & domains[TileDirection.Up];
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
                            debugConnentions.Add(" Overlap:" + overlap + " - " + opt.rotation + " out of " + string.Join(',', cell.alignmentRestrictions[overlap]));
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Backward:
                        negativeCell = map[loc.x, loc.y, loc.z - 1];
                        negativeDomain = invertConnections(negativeCell.forwardMask);
                        debugConnentions.Add(" Backward:" + negativeDomain);
                        if ((negativeDomain & domains[TileDirection.Backward]) == 0)
                        {
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Left:
                        negativeCell = map[loc.x - 1, loc.y, loc.z];
                        negativeDomain = invertConnections(negativeCell.rightMask);
                        debugConnentions.Add(" Left:" + negativeDomain);
                        if ((negativeDomain & domains[TileDirection.Left]) == 0)
                        {
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Down:
                        negativeCell = map[loc.x, loc.y - 1, loc.z];
                        negativeDomain = invertConnections(negativeCell.upMask);
                        debugConnentions.Add(" Down:" + negativeDomain);
                        overlap = negativeDomain & domains[TileDirection.Down];
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
                            debugConnentions.Add(" Overlap:" + overlap + " - " + opt.rotation + " out of " + string.Join(',', negativeCell.alignmentRestrictions[overlap]));
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                }
            }
        MatchFound:
            if (doesntFit)
            {
                debugConnentions.Add("///");
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
                Debug.LogWarning("EXDomain:" + string.Join(',', debugConnentions));
                return false;
            }
            else
            {
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
                    cell.alignmentRestrictions[bond] = new List<Rotation>(allRotations);
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
                    negativeCell.alignmentRestrictions[bond] = new List<Rotation>(allRotations);
                }
                if (restrictTileDomain(loc, update, true))
                {
                    //Ex tile found
                    Debug.LogWarning("Domain reduced to 0:" + loc + ":" + string.Join(',', debugConnentions));
                }
                else
                {
                    throw new System.Exception("Domain reduced to 0:" + loc + ":" + string.Join(',', debugConnentions));
                }
            }

        }

        if (ExCall)
        {
            return true;
        }

        if (reduced)
        {
            update(loc, entropy(remaining));
        }

        return reduced;
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
        public Dictionary<TileDirection, int> validConnections;
        public Dictionary<int, List<Rotation>> upAlignments;
        public Dictionary<int, List<Rotation>> downAlignments;
    }
    public struct ConnectionTileInfo
    {
        public Dictionary<TileDirection, int> validConnections;
    }
    void collapseConnections(Vector3Int loc, CollapseEnqueue queue)
    {
        WFCCell cell = map[loc.x, loc.y, loc.z];
        ConnectionDomainInfo connections = compositeConnectionDomains(cell.domainMask);
        Dictionary<TileDirection, int> domains = connections.validConnections;
        foreach (TileDirection dir in EnumValues<TileDirection>())
        {
            WFCCell negativeCell;
            int negativeDomain;
            bool altered;
            switch (dir)
            {
                case TileDirection.Forward:
                    //Debug.Log(domains[TileDirection.Forward]);
                    if (cell.forwardMask != domains[TileDirection.Forward])
                    {
                        cell.forwardMask &= domains[TileDirection.Forward];
                        queue(loc + new Vector3Int(0, 0, 1));
                    }
                    break;
                case TileDirection.Right:
                    if (cell.rightMask != domains[TileDirection.Right])
                    {
                        cell.rightMask &= domains[TileDirection.Right];
                        queue(loc + new Vector3Int(1, 0, 0));
                    }
                    break;
                case TileDirection.Up:
                    altered = false;
                    if (cell.upMask != domains[TileDirection.Up])
                    {
                        cell.upMask &= domains[TileDirection.Up];
                        altered = true;
                    }
                    foreach (TileConnection a in alignments)
                    {
                        int singleAlignmentMask = (int)a;
                        if (cell.alignmentRestrictions[singleAlignmentMask].Count > connections.upAlignments[singleAlignmentMask].Count)
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
                    negativeDomain = invertConnections(domains[TileDirection.Backward]);
                    if (negativeCell.forwardMask != negativeDomain)
                    {
                        negativeCell.forwardMask &= negativeDomain;
                        queue(loc + new Vector3Int(0, 0, -1));
                    }
                    break;
                case TileDirection.Left:
                    negativeCell = map[loc.x - 1, loc.y, loc.z];
                    negativeDomain = invertConnections(domains[TileDirection.Left]);
                    if (negativeCell.rightMask != negativeDomain)
                    {
                        negativeCell.rightMask &= negativeDomain;
                        queue(loc + new Vector3Int(-1, 0, 0));
                    }
                    break;
                case TileDirection.Down:
                    altered = false;
                    negativeCell = map[loc.x, loc.y - 1, loc.z];
                    negativeDomain = invertConnections(domains[TileDirection.Down]);
                    if (negativeCell.upMask != negativeDomain)
                    {
                        negativeCell.upMask &= negativeDomain;
                        altered = true;
                    }
                    foreach (TileConnection a in alignments)
                    {
                        int singleAlignmentMask = (int)a;
                        if (negativeCell.alignmentRestrictions[singleAlignmentMask].Count > connections.downAlignments[singleAlignmentMask].Count)
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
    public void init()
    {
        makeDomain();
        map = new WFCCell[mapSize.x, mapSize.y, mapSize.z];

    }

    BoundsInt fromPath(List<Vector3Int> path)
    {
        Bounds bounds = new Bounds(path[0], Vector3.zero);
        foreach (Vector3Int loc in path)
        {
            bounds.Encapsulate(loc);
        }
        bounds.Expand(8);
        return bounds.asInt();
    }
    struct TileWalker
    {
        public Vector3Int location;
        public Vector3Int remaining;
        public TileDirection lastDirection;
        public TileDirection facing;

        TileDirection step(TileDirection dir)
        {
            lastDirection = dir;
            Vector3Int diff = fromDir(lastDirection);

            location += diff;
            remaining -= diff;
            return lastDirection;
        }

        public TileDirection walk()
        {
            if (lastDirection == TileDirection.Up || lastDirection == TileDirection.Down)
            {
                return step(facing);
            }

            List<TileDirection> directions = new List<TileDirection>();
            if (remaining.x != 0)
            {
                if (remaining.x > 0)
                {
                    directions.Add(TileDirection.Right);
                }
                else
                {
                    directions.Add(TileDirection.Left);
                }
            }
            if (remaining.y != 0)
            {
                if (remaining.y > 0)
                {
                    directions.Add(TileDirection.Up);
                }
                else
                {
                    directions.Add(TileDirection.Down);
                }
            }
            if (remaining.z != 0)
            {
                if (remaining.z > 0)
                {
                    directions.Add(TileDirection.Forward);
                }
                else
                {
                    directions.Add(TileDirection.Backward);
                }
            }

            TileDirection picked = directions.RandomItem();
            if (picked != TileDirection.Up && picked != TileDirection.Down)
            {
                facing = picked;
            }
            return step(picked);
        }
    }
    public IEnumerator collapseCells(List<Vector3Int> path)
    {
        SimplePriorityQueue<Vector3Int> collapseQueue = new SimplePriorityQueue<Vector3Int>();
        int chainCount = 0;
        Queue<Vector3Int> propagation = new Queue<Vector3Int>();
        System.Action<Vector3Int> propagateTileRestrictions = (Vector3Int loc) =>
        {
            collapseConnections(loc, propagation.Enqueue);
            while (propagation.Count > 0)
            {
                Vector3Int propLocation = propagation.Dequeue();
                if (restrictTileDomain(propLocation, collapseQueue.UpdatePriority))
                {
                    collapseConnections(propLocation, propagation.Enqueue);
                }

            }
        };

        System.Action<Vector3Int> createTileRestrictions = (Vector3Int loc) =>
        {

            if (restrictTileDomain(loc, collapseQueue.UpdatePriority))
            {
                propagateTileRestrictions(loc);
            }


        };

        (BigMask fullDomain, BigMask Ex) = fullDomainMask();
        ExDomain = Ex;
        int fullConnection = fullConnectionMask();
        Dictionary<int, List<Rotation>> fullRestrictions = fullAlignmentMask();
        BoundsInt bounds = fromPath(path);

        int xMax = bounds.max.x;
        int yMax = bounds.max.y;
        int zMax = bounds.max.z;
        int xMin = bounds.min.x;
        int yMin = bounds.min.y;
        int zMin = bounds.min.z;
        Debug.Log(bounds.min);
        Debug.Log(bounds.max);


        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                for (int z = zMin; z <= zMax; z++)
                {
                    map[x, y, z] = new WFCCell();
                    map[x, y, z].init(fullDomain, fullConnection, fullRestrictions);

                    if (
                        x > xMin && x < xMax
                        &&
                        y > yMin && y < yMax
                        &&
                        z > zMin && z < zMax
                        )
                    {
                        map[x, y, z].makeReady();
                        collapseQueue.Enqueue(new Vector3Int(x, y, z), domainTiles.Count);
                    }

                }
            }
        }

        //floor
        for (int x = xMin + 1; x < xMax; x++)
        {
            for (int z = zMin + 1; z < zMax; z++)
            {
                map[x, yMin, z].upMask = (int)(TileConnection.GroundConnect | TileConnection.AirConnect);
                map[x, yMax - 1, z].upMask = (int)(TileConnection.Air | TileConnection.AirConnect | TileConnection.Air2);

                createTileRestrictions(new Vector3Int(x, yMin + 1, z));
                createTileRestrictions(new Vector3Int(x, yMax - 1, z));
                chainCount++;
                if (chainCount >= collapsePerFrame)
                {
                    chainCount = 0;
                    yield return null;
                }
            }
        }
        //walls
        int wallMaskIn = (int)(TileConnection.GroundConnect | TileConnection.AirConnect | TileConnection.Flat | TileConnection.FlatUnwalkable);
        int wallMaskOut = (int)(TileConnection.Ground | TileConnection.Air | TileConnection.Flat | TileConnection.FlatUnwalkable);
        for (int x = xMin + 1; x < xMax; x++)
        {
            for (int y = yMin + 2; y < yMax; y++)
            {
                map[x, y, zMin].forwardMask = wallMaskIn;
                createTileRestrictions(new Vector3Int(x, y, zMin + 1));
                map[x, y, zMax - 1].forwardMask = wallMaskOut;
                createTileRestrictions(new Vector3Int(x, y, zMax - 1));
                chainCount++;
                if (chainCount >= collapsePerFrame)
                {
                    chainCount = 0;
                    yield return null;
                }
            }
        }
        for (int z = zMin + 1; z < zMax; z++)
        {
            for (int y = yMin + 2; y < yMax; y++)
            {
                map[xMin, y, z].rightMask = wallMaskIn;
                createTileRestrictions(new Vector3Int(xMin + 1, y, z));
                map[xMax - 1, y, z].rightMask = wallMaskOut;
                createTileRestrictions(new Vector3Int(xMax - 1, y, z));
                chainCount++;
                if (chainCount >= collapsePerFrame)
                {
                    chainCount = 0;
                    yield return null;
                }
            }
        }
        //TODO real constraints

        //Vector3Int middle = new Vector3Int(xMin + width / 2, yMin + height / 2, zMin + length / 2);
        //map[middle.x, middle.y, middle.z].domainMask = new BigMask(0);
        //collapseQueue.UpdatePriority(middle, 0);
        //propagateTileRestrictions(middle);
        TileWalker walker = new TileWalker
        {
            location = path[0],
            facing = TileDirection.Forward,
            lastDirection = TileDirection.Forward,
            remaining = new Vector3Int()
        };
        path.RemoveAt(0);

        while (path.Count > 0)
        {
            if (walker.remaining == Vector3Int.zero)
            {
                walker.remaining = path[0] - walker.location;
            }

            Vector3Int currentLoc = walker.location;
            TileDirection change = walker.walk();
            switch (change)
            {
                case TileDirection.Forward:
                    map[currentLoc.x, currentLoc.y, currentLoc.z].forwardMask = walkableMask();
                    break;
                case TileDirection.Right:
                    map[currentLoc.x, currentLoc.y, currentLoc.z].rightMask = walkableMask();
                    break;
                case TileDirection.Up:
                    map[currentLoc.x, currentLoc.y, currentLoc.z].upMask = walkableMask();
                    break;
                case TileDirection.Backward:
                    map[walker.location.x, walker.location.y, walker.location.z].forwardMask = walkableMask();
                    break;
                case TileDirection.Left:
                    map[walker.location.x, walker.location.y, walker.location.z].rightMask = walkableMask();
                    break;
                case TileDirection.Down:
                    map[walker.location.x, walker.location.y, walker.location.z].upMask = walkableMask();
                    break;
            }

            Debug.DrawLine(currentLoc.asFloat().scale(transform.lossyScale), walker.location.asFloat().scale(transform.lossyScale), Color.blue, 600);
            createTileRestrictions(currentLoc);
            createTileRestrictions(walker.location);


            if (chainCount >= collapsePerFrame)
            {
                chainCount = 0;
                yield return null;
            }



            if (walker.remaining.magnitude < 2f)
            {
                path.RemoveAt(0);
                walker.remaining = Vector3Int.zero;
            }
        }




        while (collapseQueue.Count > 0)
        {
            Vector3Int coords = collapseQueue.Dequeue();
            Vector3 location = new Vector3(coords.x, coords.y, coords.z);
            location.Scale(transform.lossyScale);

            WFCCell cell = map[coords.x, coords.y, coords.z];
            cell.domainMask = selectFromTileDomain(cell.domainMask);
            cell.collapsed = true;
            TileOption opt = optionFromSingleDomain(cell.domainMask);
            if (!opt.prefab.skipSpawn)
            {
                Instantiate(
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
                transform
            );
            }


            propagateTileRestrictions(coords);

            chainCount++;
            if (chainCount >= collapsePerFrame)
            {
                chainCount = 0;
                yield return null;
            }


        }
    }

}
