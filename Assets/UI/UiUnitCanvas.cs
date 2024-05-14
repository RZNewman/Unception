using Newtonsoft.Json.Bson;
using UnityEngine;

public class UiUnitCanvas : MonoBehaviour
{
    Vector3 localPosition;
    public GameObject barObject;
    // Start is called before the first frame update
    void Start()
    {
        localPosition = transform.localPosition;
        GetComponentInParent<Power>().subscribePower(scaleUi);
        barObject.SetActive(!GetComponentInParent<LocalPlayer>().isLocalUnit);
    }


    void scaleUi(Power p)
    {
        float scalePhys = p.scalePhysical();
        transform.localPosition = localPosition * scalePhys;
        transform.localScale = Vector3.one * scalePhys;
    }
}
