using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SharedMaterials;

public class ModelLoader : MonoBehaviour
{
    bool loaded = false;
    Size s;
    public bool modelLoaded
    {
        get
        {
            return loaded;
        }
    }
    public Size size
    {
        get { return s; }
    }
    private void Start()
    {
        SharedMaterials mats = FindObjectOfType<SharedMaterials>();
        mats.getVisuals(GetComponentInParent<UnitPropsHolder>().props.visualsId, loadModel);
    }

    static float max_lank = 0.3f;
    private void loadModel(visualsData data)
    {

        GameObject rotator = GetComponentInChildren<UnitRotation>().gameObject;
        GameObject body = Instantiate(data.built.modelPrefab, rotator.transform);
        body.GetComponent<UnitColorTarget>().colorTargets(data.built.materials);
        float height = (data.source.lank - 1f) * max_lank + 1;
        float width = -(data.source.lank - 1f) * max_lank + 1;
        s = body.GetComponent<Size>();
        s.setBaseSize(new Vector3(width, height, width));

        loaded = true;
    }
}
