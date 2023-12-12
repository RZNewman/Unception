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

    string textCache = "";
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
            if (data.active)
            {
                if(data.segments != null)
                {
                    basicBar.set(data.segments);
                }
                else
                {
                    basicBar.set(new UiBarBasic.BarSegment[]
                    {
                        new UiBarBasic.BarSegment
                        {
                            color = data.color,
                            percent = data.fillPercent
                        }
                    });
                }
                
            }
            
        }
        else
        {
            bar.gameObject.SetActive(data.active);
            bar.InnerColor.Value = data.color;
            bar.SetPercent(data.fillPercent);
        }

        if(data.text != textCache)
        {
            textCache = data.text;
            label.text = data.text;
        }
        
    }
}
