using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GroveObject;
using System.Linq;
using static GenerateAttack;
using static Utils;
using static UnityEngine.Rendering.DebugUI.Table;
using UnityEngine.UIElements;
using Mirror;

public class Grove : NetworkBehaviour
{
    public Vector2Int gridSize;


    GroveSlot[,] map;
    //List<GroveObject> placedLocal = new List<GroveObject>();
    SyncDictionary<ItemSlot, string> slotAllocations = new SyncDictionary<ItemSlot, string>();
    SyncDictionary<string, GrovePlacedObject> placedItems = new SyncDictionary<string, GrovePlacedObject>();

    GroveWorld groveWorld;

    Inventory inv;
 

    public struct GroveSlot
    {
        Dictionary<string, GroveSlotType> occupants;

        public static GroveSlot empty()
        {
            return new GroveSlot
            {
                occupants = new Dictionary<string, GroveSlotType>()
            };
        }

        public List<string> addOccupant(string id, GroveSlotType type)
        {
            List<string> kicked;
            if (type == GroveSlotType.Hard) {
                kicked = occupants.Keys.ToList();               
            }
            else
            {
                kicked = occupants.Where(pair => pair.Value == GroveSlotType.Hard).Select(pair => pair.Key).ToList();
            }
            occupants.Add(id, type);
            return kicked;
        }

        public void removeOccupant(string id)
        {

            occupants.Remove(id);
        }
    }

    public struct GrovePlacedObject
    {
        public GrovePlacement placement;
        public GroveShape shape;
        public ItemSlot? slot;
        public List<GroveSlotPosition> gridPoints()
        {
            Rotation rot = placement.rotation;
            Vector2Int pos = placement.position;
            return shape.points.Select(point =>
            {
                Vector2Int relativePos = point.position;
                Vector2Int rotatedPos = rot.rotateIntVec(point.position);
                Vector2Int gridPos = pos;
                //Debug.Log("Relative: " + relativePos + ", Rotated: " + rotatedPos + ", Grid: " + gridPos);
                return new GroveSlotPosition
                {
                    type = point.type,
                    position = rotatedPos + gridPos
                };
            }).ToList();
        }
    }

    public struct GrovePlacement
    {
        public Vector2Int position;
        public Rotation rotation;

        

    }
    public struct GroveShape
    {
        public List<GroveSlotPosition> points;

        public static GroveShape shape()
        {

            HashSet<Vector2Int> pointsUsed = new HashSet<Vector2Int>();
            List<GroveSlotPosition> slots = new List<GroveSlotPosition>();
            Dictionary<Vector2Int, int> potentials = new Dictionary<Vector2Int, int>();

            System.Action<Vector2Int> addPotential = (point) =>
            {
                if (!pointsUsed.Contains(point))
                {
                    if (potentials.ContainsKey(point))
                    {
                        potentials[point] += 1;
                    }
                    else
                    {
                        potentials.Add(point, 1);
                    }
                }
            };

            System.Action<Vector2Int, GroveSlotType> confirmPoint = (point, type) =>
            {
                potentials.Remove(point);
                pointsUsed.Add(point);
                slots.Add(new GroveSlotPosition
                {
                    position = point,
                    type = type

                });
                addPotential(point + Vector2Int.up);
                addPotential(point + Vector2Int.down);
                addPotential(point + Vector2Int.left);
                addPotential(point + Vector2Int.right);
            };

            confirmPoint(Vector2Int.zero, GroveSlotType.Hard);

            int additionalPoints = Mathf.RoundToInt(GaussRandomDecline(1.5f).asRange(1, 7));
            for (int i = 0; i < additionalPoints; i++)
            {
                Vector2Int selection = potentials.RandomItemWeighted((pair) => pair.Value).Key;
                confirmPoint(selection, GroveSlotType.Hard);
            }

            //double the weights for direct adjacantcies
            foreach (Vector2Int pos in potentials.Keys.ToList())
            {
                potentials[pos] *= 10;
            }

            int minSoft = 0;
            int maxSoft = 5 + additionalPoints * 2;
            int softCount = Mathf.RoundToInt(GaussRandomCentered(1.5f).asRange(minSoft, maxSoft));
            //Debug.Log("Hard: " + 1 + additionalPoints + " Soft: " + softCount);
            for (int i = 0; i < softCount; i++)
            {
                Vector2Int selection = potentials.RandomItemWeighted((pair) => pair.Value).Key;
                confirmPoint(selection, GroveSlotType.Aura);
            }


            return new GroveShape
            {
                points = slots,
            };
        }

        public static GroveShape basic()
        {
            List<GroveSlotPosition> points = new List<GroveSlotPosition>();

            for(int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    points.Add(new GroveSlotPosition
                    {
                        position = new Vector2Int(x, y),
                        type = GroveSlotType.Hard,
                    });
                }
            }

            return new GroveShape
            {
                points = points,
            };

        }

        public float power(Dictionary<GroveSlotType, float> values)
        {
            return points!= null ? points.Sum(point => values[point.type]) : 0;
        }
    }

    enum SideEffectType
    {
        ReturnToTray,
        AddToCursor,
    }
    struct GroveSideEffect
    {
        public string targetID;
        public SideEffectType type;
    }

    private void Start()
    {
        groveWorld = FindObjectOfType<GroveWorld>(true);
        inv = GetComponent<Inventory>();
        initGrid();
    }
    void initGrid()
    {
        map = new GroveSlot[gridSize.x, gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                map[x, y] = GroveSlot.empty();
            }
        }


    }

    public Vector2Int center
    {
        get
        {
            return gridSize / 2;
        }
    }
    public bool isPlaced(string id)
    {
        return placedItems.ContainsKey(id);
    }

    public IEnumerable<KeyValuePair<ItemSlot, string>> slotted
    {
        get
        {
            return slotAllocations.ToArray();
        }
    }

    public float powerOfSlot(ItemSlot slot)
    {
        CastDataInstance abil = dataOfSlot(slot);
        return abil != null ? abil.actingPower(): 0;
    }

    public CastDataInstance dataOfSlot(ItemSlot slot)
    {
        return slotAllocations.ContainsKey(slot) ? (CastDataInstance)inv.getAbilityInstance(slotAllocations[slot]) : null;
    }

    public Dictionary<string, GrovePlacement> exportPlacements()
    {
        return placedItems.Keys.ToDictionary(key => key, key => placedItems[key].placement);
    }

    [Server]
    public void importPlacements(Dictionary<string, GrovePlacement> placements, Dictionary<string, CastData> items)
    {
        placedItems.Clear();
        foreach (string id in placements.Keys)
        {
            AddPlace(id, new GrovePlacedObject { placement = placements[id], shape = items[id].shape, slot = items[id].slot });
        }
    }

    [Command]
    public void CmdPlaceGrove(string placedID, GrovePlacedObject placedObj)
    {
        GroveSideEffect[] effects = AddPlace(placedID, placedObj);
        inv.syncInventoryUpwards();
        inv.RpcInvChange();
        TargetReplaySideEffects(connectionToClient, effects);
    }

    [Command]
    public void CmdPickGrove(string pickID, GrovePlacedObject pickObj)
    {
        SubtractPlace(pickID, pickObj);
    }

    [TargetRpc]
    void TargetReplaySideEffects(NetworkConnection conn ,GroveSideEffect[] effects)
    {
        foreach(GroveSideEffect effect in effects)
        {
            switch (effect.type)
            {
                case SideEffectType.ReturnToTray:
                    groveWorld.returnToTray(effect.targetID);
                    break;
                case SideEffectType.AddToCursor:
                    groveWorld.addCursor(effect.targetID);
                    break;
            }
        }
    }


    GroveSideEffect[] AddPlace(string placedID, GrovePlacedObject placedObj)
    {
        List<GroveSlotPosition> gridPoints = placedObj.gridPoints();
        foreach (GroveSlotPosition slot in gridPoints)
        {
            if (
                 slot.position.x < 0 ||
                 slot.position.x >= map.GetLength(0) ||
                 slot.position.y < 0 ||
                 slot.position.y >= map.GetLength(1)

                )
            {
                return new GroveSideEffect[] {new GroveSideEffect { targetID = placedID, type = SideEffectType.ReturnToTray} };
            }
        }

        HashSet<string> kickSet = new HashSet<string>();
        foreach (GroveSlotPosition slot in gridPoints)
        {
            //drawMapPos(slot.position);
            List<string> kicked = map[slot.position.x, slot.position.y].addOccupant(placedID, slot.type);
            foreach (string id in kicked)
            {
                kickSet.AddIfNotExists(id);
            }
        }
        if(placedObj.slot.HasValue && slotAllocations.ContainsKey(placedObj.slot.Value))
        {
            kickSet.AddIfNotExists(slotAllocations[placedObj.slot.Value]);
        }
        placedItems.Add(placedID, placedObj);
        if (placedObj.slot.HasValue)
        {
            slotAllocations[placedObj.slot.Value] = placedID;
        }


        if (kickSet.Count == 1)
        {
            string kickID = kickSet.First();
            SubtractPlace(kickID, placedItems[kickID]);           
            //Debug.Log("Rebound: "+ kickSet.First());
            return new GroveSideEffect[] { new GroveSideEffect { targetID = kickID, type = SideEffectType.AddToCursor } };

        }
        else if(kickSet.Count > 1)
        {
            List<GroveSideEffect> effects = new List<GroveSideEffect>();
            foreach (string kickID in kickSet)
            {
                SubtractPlace(kickID, placedItems[kickID]);            
                //Debug.Log("Rebound: "+ kickSet.First());
                effects.Add( new GroveSideEffect { targetID = kickID, type = SideEffectType.ReturnToTray } );
            }
            //Debug.Log("Kick");
            return effects.ToArray();
        }
        


        return new GroveSideEffect[0];
    }

    void SubtractPlace(string placedID, GrovePlacedObject placed)
    {
        foreach (GroveSlotPosition slot in placed.gridPoints())
        {
            map[slot.position.x, slot.position.y].removeOccupant(placedID);
        }
        if (placedItems[placedID].slot.HasValue && slotAllocations[placedItems[placedID].slot.Value] == placedID)
        {
            slotAllocations.Remove(placedItems[placedID].slot.Value);
        }
        placedItems.Remove(placedID);
    }


}
