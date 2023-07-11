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
        //RampLeft,
        //RampRight,
        //RampTop,
        //WallLeft,
        //WallRight,
        //FlatUnwalkable,
    }

    public static bool walkable(TileConnection conn)
    {
        return (conn & (TileConnection.Flat)) != 0;
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
        public int domainMask;
        public int forwardMask;
        public int rightMask;
        public int upMask;

        public void init(int fullDomain, int fullConnection)
        {
            domainMask = fullDomain;
            forwardMask = fullConnection;
            rightMask = fullConnection;
            upMask = fullConnection;
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

    int fullDomainMask()
    {
        int mask = 0;

        for (int i = 0; i < domainTiles.Count; i++)
        {
            mask |= 1 << i;
        }
        return mask;
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
                    weight = tile.weight,
                });
            }
        }
        //Reverse in the sort function 
        domainTiles.Sort((a, b) => b.weight.CompareTo(a.weight));
    }
    static bool oneFlagSet(int i)
    {
        //http://aggregate.org/MAGIC/#Is%20Power%20of%202
        return i > 0 && (i & (i - 1)) == 0;
    }
    int selectFromTileDomain(int domain)
    {
        if (domain == 0)
        {
            throw new System.Exception("No domain to pick from");
        }
        if (oneFlagSet(domain))
        {
            return domain;
        }

        int processDomain = domain;
        List<(int, float)> indexWeights = new List<(int, float)>();
        float weightSum = 0;

        for (int i = 0; i < domainTiles.Count; i++)
        {
            if ((processDomain & 1) > 0)
            {
                TileOption opt = domainTiles[i];
                indexWeights.Add((i, opt.weight));
                weightSum += opt.weight;
            }
            processDomain = processDomain >> 1;
        }

        float pick = Random.value * weightSum;
        int selectedIndex = -1;

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

        //TODO maybe just spawn the tile here??
        return 1 << selectedIndex;

    }

    List<(TileOption, int)> optionsFromTileDomain(int domain)
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

    TileOption optionFromSingleDomain(int domain)
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
    Dictionary<TileDirection, int> compositeConnectionDomains(int domain)
    {
        if (domain == 0)
        {
            throw new System.Exception("Empty Domain");
        }
        Dictionary<TileDirection, int> connections = new Dictionary<TileDirection, int>();
        foreach (TileDirection dir in EnumValues<TileDirection>())
        {
            connections[dir] = 0;
        }
        foreach ((TileOption opt, _) in optionsFromTileDomain(domain))
        {
            Dictionary<TileDirection, int> connectionsTile = opt.getDomains();
            foreach (TileDirection dir in EnumValues<TileDirection>())
            {
                connections[dir] |= connectionsTile[dir];
                //Debug.Log(connections[dir]);
            }
        }

        return connections;
    }


    delegate void UpdateEntropy(Vector3Int loc, float entropy);
    bool restrictTileDomain(Vector3Int loc, UpdateEntropy update)
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
            Dictionary<TileDirection, int> domains = opt.getDomains();
            bool doesntFit = false;
            foreach (TileDirection dir in EnumValues<TileDirection>())
            {
                WFCCell negativeCell;
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
                        if ((cell.upMask & domains[TileDirection.Up]) == 0)
                        {

                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Backward:
                        negativeCell = map[loc.x, loc.y, loc.z - 1];
                        debugConnentions.Add(" Backward:" + negativeCell.forwardMask);
                        if ((negativeCell.forwardMask & domains[TileDirection.Backward]) == 0)
                        {
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Left:
                        negativeCell = map[loc.x - 1, loc.y, loc.z];
                        debugConnentions.Add(" Left:" + negativeCell.rightMask);
                        if ((negativeCell.rightMask & domains[TileDirection.Left]) == 0)
                        {
                            doesntFit = true;
                            goto MatchFound;
                        }
                        break;
                    case TileDirection.Down:
                        negativeCell = map[loc.x, loc.y - 1, loc.z];
                        debugConnentions.Add(" Down:" + negativeCell.upMask);
                        if ((negativeCell.upMask & domains[TileDirection.Down]) == 0)
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
                debugConnentions.Add("///");
                cell.domainMask ^= 1 << index;
                reduced = true;
            }
            else
            {
                remaining.Add(opt);
            }
        }

        if (cell.domainMask == 0)
        {
            throw new System.Exception("Domain reduced to 0" + string.Join(',', debugConnentions));
        }
        if (reduced)
        {
            update(loc, entropy(remaining));
        }

        return reduced;
    }

    float entropy(List<TileOption> options)
    {
        if (options.Count == 0)
        {
            return 0;
        }
        float maxWeight = options[0].weight;
        float entropy = 0;
        int count = 0;
        foreach (TileOption opt in options)
        {
            entropy += opt.weight / maxWeight * (1 + 0.05f * count);
            count++;
        }

        //Debug.Log(entropy);
        return entropy;

    }
    delegate void CollapseEnqueue(Vector3Int loc);
    void collapseConnections(Vector3Int loc, CollapseEnqueue queue)
    {
        WFCCell cell = map[loc.x, loc.y, loc.z];
        Dictionary<TileDirection, int> domains = compositeConnectionDomains(cell.domainMask);
        foreach (TileDirection dir in EnumValues<TileDirection>())
        {
            WFCCell negativeCell;
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
                    if (cell.upMask != domains[TileDirection.Up])
                    {
                        cell.upMask &= domains[TileDirection.Up];
                        queue(loc + new Vector3Int(0, 1, 0));
                    }
                    break;
                case TileDirection.Backward:
                    negativeCell = map[loc.x, loc.y, loc.z - 1];
                    if (negativeCell.forwardMask != domains[TileDirection.Backward])
                    {
                        negativeCell.forwardMask &= domains[TileDirection.Backward];
                        queue(loc + new Vector3Int(0, 0, -1));
                    }
                    break;
                case TileDirection.Left:
                    negativeCell = map[loc.x - 1, loc.y, loc.z];
                    if (negativeCell.rightMask != domains[TileDirection.Left])
                    {
                        negativeCell.rightMask &= domains[TileDirection.Left];
                        queue(loc + new Vector3Int(-1, 0, 0));
                    }
                    break;
                case TileDirection.Down:
                    negativeCell = map[loc.x, loc.y - 1, loc.z];
                    if (negativeCell.upMask != domains[TileDirection.Down])
                    {
                        negativeCell.upMask &= domains[TileDirection.Down];
                        queue(loc + new Vector3Int(0, -1, 0));
                    }
                    break;
            }
        }
    }
    public void init()
    {
        makeDomain();
        int fullDomain = fullDomainMask();
        int fullConnection = fullConnectionMask();
        map = new WFCCell[mapSize.x, mapSize.y, mapSize.z];
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int z = 0; z < mapSize.z; z++)
                {
                    map[x, y, z] = new WFCCell();
                    map[x, y, z].init(fullDomain, fullConnection);
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
        Vector3Int middle = new Vector3Int(xx + width / 2, yy + height / 2, zz + length / 2);
        map[middle.x, middle.y, middle.z].domainMask = 1;
        propagateTileRestrictions(middle);



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
