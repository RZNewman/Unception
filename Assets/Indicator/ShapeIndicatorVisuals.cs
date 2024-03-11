using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackUtils;

public class ShapeIndicatorVisuals : HitIndicatorInstance
{
    public Sprite box;
    public Sprite boxFull;
    public Sprite circle;
    public Sprite circleFull;

    public GameObject indPiecePre;

    // Start is called before the first frame update
    List<GameObject> staticElements = new List<GameObject>();

    struct ProgressData{
        public GameObject obj;
        public float start;
        public IndicatorProgressElement element;
    }
    List<ProgressData> progressElements = new List<ProgressData>();
    protected override void setSize()
    {
        foreach(IndicatorDisplay display in shapeData.indicators)
        {
            GameObject o = Instantiate(indPiecePre, transform);
            o.transform.localRotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            o.transform.localRotation = display.rotation * o.transform.localRotation;
            o.transform.localPosition = display.position;
            o.transform.localScale = display.scale;
            o.GetComponent<SpriteRenderer>().sprite = display.shape switch
            {
                IndicatorShape.Box => box,
                IndicatorShape.BoxFull => boxFull,
                IndicatorShape.Circle => circle,
                IndicatorShape.CircleFull => circleFull,
                _ => null
            };

            if (display.settings.HasValue)
            {
                settingsSet(display.settings.Value, o);
            }

            if(display.progress != IndicatorProgressElement.None)
            {
                progressElements.Add(new ProgressData
                {
                    obj = o,
                    element = display.progress,
                    start = display.progress switch
                    {
                        IndicatorProgressElement.Circle => display.settings.Value.circle.Value,
                        IndicatorProgressElement.Sideways => display.settings.Value.sideways.Value,
                        _ => 0,
                    }
                });
            }
            else
            {
                staticElements.Add(o);
            }
        }

    }

    static void settingsSet(IndicatorShaderSettings settings, GameObject obj)
    {
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        Renderer renderer = obj.GetComponent<Renderer>();

        // Get the current value of the material properties in the renderer.
        renderer.GetPropertyBlock(propBlock);

        if(settings.angle.HasValue) propBlock.SetFloat("_Angle", settings.angle.Value);
        if(settings.circle.HasValue) propBlock.SetFloat("_Circle", settings.circle.Value);
        if(settings.circleSubtract.HasValue) propBlock.SetFloat("_Subtract_Circle", settings.circleSubtract.Value);
        if(settings.forward.HasValue) propBlock.SetFloat("_Forward", settings.forward.Value);
        if(settings.sideways.HasValue) propBlock.SetFloat("_Sideways", settings.sideways.Value);
        if(settings.subtractOffset.HasValue) propBlock.SetFloat("_Subtract_Offset", settings.subtractOffset.Value);
        // Apply the edited values to the renderer.
        renderer.SetPropertyBlock(propBlock);
    }

    void changeProgress(float progress, IndicatorProgressElement ele, GameObject obj )
    {
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        Renderer renderer = obj.GetComponent<Renderer>();

        // Get the current value of the material properties in the renderer.
        renderer.GetPropertyBlock(propBlock);
        switch (ele)
        {
            case IndicatorProgressElement.Circle:
                propBlock.SetFloat("_Circle", progress);
                break;
            case IndicatorProgressElement.Sideways:
                propBlock.SetFloat("_Sideways", progress);
                break;
        }
        renderer.SetPropertyBlock(propBlock);
    }

    public override void setColor(Color color, Color stunning)
    {
        foreach(GameObject o in staticElements)
        {
            o.GetComponent<SpriteRenderer>().color = stunning;
        }
        foreach(ProgressData data in progressElements)
        {
            data.obj.GetComponent<SpriteRenderer>().color = color;
        }
        
    }

    protected override void setCurrentProgress(float percent)
    {
        foreach (ProgressData data in progressElements)
        {
            changeProgress(Mathf.Lerp(data.start, 1, percent), data.element, data.obj);
        }
    }


}
