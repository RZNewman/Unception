using UnityEngine;

public class LocalClick : MonoBehaviour
{
    Vector3 localPosition;
    // Start is called before the first frame update
    void Start()
    {
        localPosition = transform.localPosition;
        GetComponentInParent<Power>().subscribePower(scaleUi);
    }

    void scaleUi(Power p)
    {
        float scalePhys = p.scalePhysical();
        transform.localPosition = localPosition * scalePhys;
        transform.localScale = Vector3.one * scalePhys;
    }
}
