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
    Dictionary<TileConnection, TileConnection> EXInversions = new Dictionary<TileConnection, TileConnection>()
    {
        {TileConnection.Air2, TileConnection.Air },
    };

    public static int walkableMask = (int)(TileConnection.Flat | TileConnection.RampTop | TileConnection.RampLeft | TileConnection.RampRight);
    public List<TileDirection> walkableDirections(Vector3Int loc)
    {
        List<TileDirection> directions = new List<TileDirection>();
        WFCCell cell = map[loc.x, loc.y, loc.z];
        foreach (TileDirection dir in EnumValues<TileDirection>())
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
            //REversed, now we're adding the original key
            int keyMask = (int)key;
            TileConnection value = inversionsReverse[key];
            int valueMask = (int)value;

            if ((domain & valueMask) > 0)
            {
                domain |= keyMask;
            }
        }
        foreach (TileConnection key in EXInversions.Keys)
        {
            int keyMask = (int)key;
            if ((domain & keyMask) > 0)
            {
                domain |= (int)EXInversions[key];
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

                        if ((cell.forwardMask & domains[TileDirection.Forward]) == 0)
                        {
                            debugConnentions.Add(" Forward:" + cell.forwardMask);
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Right:

                        if ((cell.rightMask & domains[TileDirection.Right]) == 0)
                        {
                            debugConnentions.Add(" Right:" + cell.rightMask);
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Up:

                        overlap = cell.upMask & domains[TileDirection.Up];
                        if (overlap == 0)
                        {
                            debugConnentions.Add(" Up:" + cell.upMask);
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

                        if ((negativeDomain & domains[TileDirection.Backward]) == 0)
                        {
                            debugConnentions.Add(" Backward:" + negativeDomain);
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Left:
                        negativeCell = map[loc.x - 1, loc.y, loc.z];
                        negativeDomain = invertConnections(negativeCell.rightMask);

                        if ((negativeDomain & domains[TileDirection.Left]) == 0)
                        {
                            debugConnentions.Add(" Left:" + negativeDomain);
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Down:
                        negativeCell = map[loc.x, loc.y - 1, loc.z];
                        negativeDomain = invertConnections(negativeCell.upMask);

                        overlap = negativeDomain & domains[TileDirection.Down];
                        if (overlap == 0)
                        {
                            debugConnentions.Add(" Down:" + negativeDomain);
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

        HashSet<Vector3Int> prevLocations;

        public TileWalker(Vector3Int loc)
        {
            location = loc;
            remaining = Vector3Int.zero;
            lastDirection = TileDirection.Forward;
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
                return remaining.magnitude < 1.5f;
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

            foreach (TileDirection dir in EnumValues<TileDirection>())
            {
                Vector3Int adj = location + fromDir(dir);
                if (!prevLocations.Contains(adj))
                {
                    frontierDirections.Add(dir);
                }
            }

            List<TileDirection> overlap = walkableDirs.Intersect(preferredDirections).Except(lastDirList).ToList();
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
    public IEnumerator collapseCells(List<Vector3Int> randomPath)
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
        PathInfo infoP = fromPath(randomPath);
        List<Vector3Int> path = infoP.path;
        BoundsInt bounds = infoP.bounds;
        List<BoundsInt> deltas = infoP.deltaBounds;


        map = new WFCCell[bounds.size.x + 1, bounds.size.y + 1, bounds.size.z + 1];
        //Debug.Log(bounds.size);

        Dictionary<TileDirection, HashSet<Vector3Int>> edgeBindings = new Dictionary<TileDirection, HashSet<Vector3Int>>();
        foreach (TileDirection dir in EnumValues<TileDirection>())
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
            DrawBox(delta.center.scale(transform.lossyScale), Quaternion.identity, delta.size.asFloat().scale(transform.lossyScale), Color.white, 600);

            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        if (map[x, y, z] == null)
                        {
                            map[x, y, z] = new WFCCell();
                            map[x, y, z].init(fullDomain, fullConnection, fullRestrictions);
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
                            collapseQueue.Enqueue(new Vector3Int(x, y, z), domainTiles.Count);

                            if (y == yMin + 1)
                            {
                                edgeBindings[TileDirection.Down].Add(new Vector3Int(x, y, z));
                                //ground tiles have no other restrictions
                                continue;
                            }
                            else if (y == yMax - 1)
                            {
                                edgeBindings[TileDirection.Up].Add(new Vector3Int(x, y, z));
                            }

                            if (x == xMin + 1)
                            {
                                edgeBindings[TileDirection.Left].Add(new Vector3Int(x, y, z));
                            }
                            else if (x == xMax - 1)
                            {
                                edgeBindings[TileDirection.Right].Add(new Vector3Int(x, y, z));
                            }

                            if (z == zMin + 1)
                            {
                                edgeBindings[TileDirection.Backward].Add(new Vector3Int(x, y, z));
                            }
                            else if (z == zMax - 1)
                            {
                                edgeBindings[TileDirection.Forward].Add(new Vector3Int(x, y, z));
                            }



                        }

                    }
                }
            }
        }

        int wallMaskIn = (int)(TileConnection.GroundConnect | TileConnection.AirConnect | TileConnection.Flat | TileConnection.FlatUnwalkable);
        int wallMaskOut = (int)(TileConnection.Ground | TileConnection.Air | TileConnection.Flat | TileConnection.FlatUnwalkable);
        int skyMaskOut = (int)(TileConnection.Air | TileConnection.AirConnect | TileConnection.Air2);
        int groundMaskIn = (int)(TileConnection.GroundConnect | TileConnection.AirConnect);

        foreach (TileDirection dir in EnumValues<TileDirection>())
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
                            map[loc.x, loc.y, loc.z].forwardMask = skyMaskOut;
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

                    createTileRestrictions(loc);

                    chainCount++;
                    if (chainCount >= collapsePerFrame)
                    {
                        chainCount = 0;
                        yield return null;
                    }
                }
            }
        }


        TileWalker walker = new TileWalker(path[0]);
        path.RemoveAt(0);

        int stepsThisPath = 0;

        while (path.Count > 0)
        {
            if (walker.arrived)
            {
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
            Debug.DrawLine((currentLoc.asFloat() + debugOffset).scale(transform.lossyScale), (walker.location.asFloat() + debugOffset).scale(transform.lossyScale), Color.blue, 600);
            createTileRestrictions(currentLoc);
            createTileRestrictions(walker.location);


            if (chainCount >= collapsePerFrame)
            {
                chainCount = 0;
                yield return null;
            }



            if (walker.arrived)
            {
                Debug.DrawLine(path[0].asFloat().scale(transform.lossyScale), (path[0].asFloat() + Vector3.up).scale(transform.lossyScale), Color.green, 600);
                path.RemoveAt(0);
                stepsThisPath = 0;
            }
            stepsThisPath++;
            if (stepsThisPath >= 400)
            {
                Debug.DrawLine(path[0].asFloat().scale(transform.lossyScale), (path[0].asFloat() + Vector3.up).scale(transform.lossyScale), Color.red, 600);
                throw new System.Exception("Too many steps on this segment");
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
