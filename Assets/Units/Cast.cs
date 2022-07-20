using UnityEngine;

public class Cast : MonoBehaviour, BarValue
{
    WindState target;
    public BarValue.BarData getBarFill()
    {
        if (target == null)
        {
            return new BarValue.BarData
            {
                color = Color.cyan,
                fillPercent = 0,
                active = false,
            };
        }
        //TODO cast bar client
        return target.getProgress();
    }

    public void setTarget(WindState s)
    {
        target = s;
    }
    public void removeTarget()
    {
        target = null;
    }
}
