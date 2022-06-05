using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SharedMaterials;

public class ModelLoader : MonoBehaviour
{
    bool loaded = false;
    CapsuleCollider _col;
    public bool modelLoaded
    {
        get
        {
            return loaded;
        }
    }
    public CapsuleCollider col
    {
        get { return _col; }
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
        body.transform.localScale = new Vector3(width, height, width);
        _col = body.GetComponent<CapsuleCollider>();
        loaded = true;
    }
}
