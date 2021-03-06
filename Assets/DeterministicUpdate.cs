using Mirror;
using System.Collections.Generic;

public class DeterministicUpdate : NetworkBehaviour
{
    List<UnitUpdateOrder> unitList = new List<UnitUpdateOrder>();

    public void register(UnitUpdateOrder unit)
    {
        unitList.Add(unit);

    }
    public void unregister(UnitUpdateOrder unit)
    {
        unitList.Remove(unit);
    }

    void FixedUpdate()
    {
        foreach (UnitUpdateOrder unit in unitList)
        {
            unit.healthTick();
        }
        foreach (UnitUpdateOrder unit in unitList)
        {
            unit.staminaTick();
        }
        foreach (UnitUpdateOrder unit in unitList)
        {
            unit.postureTick();
        }
        foreach (UnitUpdateOrder unit in unitList)
        {
            unit.moveTransition();
        }
        foreach (UnitUpdateOrder unit in unitList)
        {
            unit.moveTick();
        }
    }
}
