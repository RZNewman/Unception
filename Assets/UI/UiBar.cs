using RengeGames.HealthBars;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static BarValue;

public class UiBar : MonoBehaviour
{
    public RadialSegmentedHealthBar bar;
    public UiBarBasic basicBar;
    public bool useBasic = false;
    public BarValue source;

    public TMP_Text label;
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
        if (useBasic)
        {
            basicBar.gameObject.SetActive(data.active);
            basicBar.set(new UiBarBasic.BarSegment
            {
                color = data.color,
                percent = data.fillPercent
            }, new UiBarBasic.BarSegment
            {
                color = data.color2,
                percent = data.fillPercent2
            });
        }
        else
        {
            bar.gameObject.SetActive(data.active);
            bar.InnerColor.Value = data.color;
            bar.SetPercent(data.fillPercent);
        }

        label.text = data.text;
    }
}
