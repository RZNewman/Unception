using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using static Utils;

public class WFCGeneration : MonoBehaviour
{
    public int collapsePerFrame = 4;
    public Vector3Int mapSize = new Vector3Int(100, 30, 100);
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
    };
    Dictionary<TileConnection, TileConnection> inversionsReverse = new Dictionary<TileConnection, TileConnection>();
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
        public long domainMask;
        public int forwardMask;
        public int rightMask;
        public int upMask;

        public Dictionary<int, List<Rotation>> alignmentRestrictions;

        public void init(long fullDomain, int fullConnection, Dictionary<int, List<Rotation>> fullRestrictions)
        {
            domainMask = fullDomain;
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

    (long, long) fullDomainMask()
    {
        long mask = 0;
        long EXmask = 0;

        for (int i = 0; i < domainTiles.Count; i++)
        {
            TileOption opt = domainTiles[i];
            if (opt.weight > 0)
            {
                mask |= 1L << i;
            }
            else
            {
                EXmask |= 1L << i;
            }

        }
        return (mask, EXmask);
    }

    Dictionary<int, List<Rotation>> fullAlignmentMask()
    {
        Dictionary<int, List<Rotation>> restrictions = new Dictionary<int, List<Rotation>>();
        foreach (TileConnection a in alignments)
        {
            restrictions[(int)a] = new List<Rotation>() { Rotation.None, Rotation.Quarter, Rotation.Half, Rotation.ThreeQuarters };
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
        int subtractionMask = 0;
        int additionMask = 0;
        foreach (TileConnection key in inversions.Keys)
        {
            int keyMask = (int)key;
            if ((domain & (int)keyMask) > 0)
            {
                subtractionMask |= keyMask;
                additionMask |= (int)inversions[key];
            }
        }
        foreach (TileConnection key in inversionsReverse.Keys)
        {
            int keyMask = (int)key;
            if ((domain & (int)keyMask) > 0)
            {
                additionMask |= (int)inversionsReverse[key];
            }
        }
        return domain ^ subtractionMask | additionMask;
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

    void makeInversions()
    {
        foreach (TileConnection key in inversions.Keys)
        {
            TileConnection value = inversions[key];
            if (!inversions.ContainsKey(value))
            {
                // the oppisite direction
                inversionsReverse.Add(value, key);
            }
        }
    }
    static bool oneFlagSet(long i)
    {
        //http://aggregate.org/MAGIC/#Is%20Power%20of%202
        return i > 0 && (i & (i - 1)) == 0;
    }
    long selectFromTileDomain(long domain)
    {
        if (domain == 0)
        {
            throw new System.Exception("No domain to pick from");
        }
        if (oneFlagSet(domain))
        {
            return domain;
        }

        long processDomain = domain;
        List<(int, float)> indexWeights = new List<(int, float)>();
        float weightSum = 0;

        for (int i = 0; i < domainTiles.Count; i++)
        {
            if ((processDomain & 1) > 0)
            {
                TileOption opt = domainTiles[i];
                //Debug.Log(opt.prefab.name + " - " + opt.weight);

                indexWeights.Add((i, opt.weight));
                weightSum += opt.weight;


            }
            processDomain = processDomain >> 1;
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

        //Debug.Log(selectedIndex);
        //TODO maybe just spawn the tile here??
        return 1L << selectedIndex;

    }

    List<(TileOption, int)> optionsFromTileDomain(long domain)
    {
        if (domain == 0)
        {
            throw new System.Exception("Empty Domain");
        }
        List<(TileOption, int)> options = new List<(TileOption, int)>();
        int index = 0;
        //Debug.Log(domain);
        while (domain > 0 && index < domainTiles.Count)
        {
            if ((domain & 1) > 0)
            {
                options.Add((domainTiles[index], index));
                //Debug.Log(domainTiles[index].prefab.name);
            }

            domain = domain >> 1;
            index++;
        }
        return options;
    }

    TileOption optionFromSingleDomain(long domain)
    {
        if (!oneFlagSet(domain))
        {
            throw new System.Exception("mutiple domains at instance");
        }

        int index = 0;
        while (index < domainTiles.Count)
        {
            if ((domain & 1) > 0)
            {
                return domainTiles[index];
            }

            domain = domain >> 1;
            index++;
        }

        throw new System.Exception("tile option not found");
    }


    ConnectionDomainInfo compositeConnectionDomains(long domain)
    {
        if (domain == 0)
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
                            debugConnentions.Add(" Overlap:" + overlap + " - " + opt.rotation);
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Backward:
                        negativeCell = map[loc.x, loc.y, loc.z - 1];
                        negativeDomain = invertConnections(negativeCell.forwardMask);
                        debugConnentions.Add(" Backward:" + negativeCell.forwardMask);
                        if ((negativeDomain & domains[TileDirection.Backward]) == 0)
                        {
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Left:
                        negativeCell = map[loc.x - 1, loc.y, loc.z];
                        negativeDomain = invertConnections(negativeCell.rightMask);
                        debugConnentions.Add(" Left:" + negativeCell.rightMask);
                        if ((negativeDomain & domains[TileDirection.Left]) == 0)
                        {
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Down:
                        negativeCell = map[loc.x, loc.y - 1, loc.z];
                        negativeDomain = invertConnections(negativeCell.upMask);
                        debugConnentions.Add(" Down:" + negativeCell.upMask);
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
                            debugConnentions.Add(" Overlap:" + overlap + " - " + opt.rotation);
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
                cell.domainMask ^= 1L << index;
                reduced = true;
            }
            else
            {
                remaining.Add(opt);
            }
        }

        if (cell.domainMask == 0)
        {
            if (ExCall)
            {
                Debug.LogWarning("EXDomain:" + string.Join(',', debugConnentions));
                return false;
            }
            else
            {
                cell.domainMask = ExDomain;
                if (restrictTileDomain(loc, update, true))
                {
                    //Ex tile found
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

    long ExDomain = 0;
    public void init()
    {
        makeDomain();
        makeInversions();
        (long fullDomain, long Ex) = fullDomainMask();
        ExDomain = Ex;
        int fullConnection = fullConnectionMask();
        Dictionary<int, List<Rotation>> fullRestrictions = fullAlignmentMask();
        map = new WFCCell[mapSize.x, mapSize.y, mapSize.z];
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int z = 0; z < mapSize.z; z++)
                {
                    map[x, y, z] = new WFCCell();
                    map[x, y, z].init(fullDomain, fullConnection, fullRestrictions);
                }
            }
        }

    }

    public IEnumerator collapseCells(int xx, int yy, int zz, int width, int height, int length)
    {
        SimplePriorityQueue<Vector3Int> collapseQueue = new SimplePriorityQueue<Vector3Int>();


        for (int x = xx; x < xx + width; x++)
        {
            for (int y = yy; y < yy + height; y++)
            {
                for (int z = zz; z < zz + length; z++)
                {
                    map[x, y, z].makeReady();
                    collapseQueue.Enqueue(new Vector3Int(x, y, z), domainTiles.Count);
                }
            }
        }

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


        //TODO real constraints
        for (int x = xx; x < xx + width; x++)
        {
            for (int z = zz; z < zz + length; z++)
            {
                map[x, yy, z].upMask = (int)TileConnection.Ground;
                createTileRestrictions(new Vector3Int(x, yy, z));
                createTileRestrictions(new Vector3Int(x, yy + 1, z));
            }
        }
        //Vector3Int middle = new Vector3Int(xx + width / 2, yy + height / 2, zz + length / 2);
        //map[middle.x, middle.y, middle.z].domainMask = 1;
        //collapseQueue.UpdatePriority(middle, 0);
        //propagateTileRestrictions(middle);



        int collapseCount = 0;
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

            collapseCount++;
            if (collapseCount >= collapsePerFrame)
            {
                collapseCount = 0;
                yield return null;
            }


        }
    }

}
