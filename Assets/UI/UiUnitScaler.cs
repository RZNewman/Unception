using UnityEngine;

public class UiUnitScaler : MonoBehaviour
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
        transform.localPosition = localPosition * p.scale();
        transform.localScale = Vector3.one * p.scale();
    }
}
