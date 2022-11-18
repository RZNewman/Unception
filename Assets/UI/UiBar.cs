using RengeGames.HealthBars;
using UnityEngine;
using UnityEngine.UI;
using static BarValue;

public class UiBar : MonoBehaviour
{
    public RadialSegmentedHealthBar bar;
    public BarValue source;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (source != null)
        {
            fill(source.getBarFill());
        }
    }

    void fill(BarData data)
    {

        bar.gameObject.SetActive(data.active);
        bar.InnerColor.Value = data.color;
        bar.SetPercent(data.fillPercent);
    }
}
