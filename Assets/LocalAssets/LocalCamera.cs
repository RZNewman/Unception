using UnityEngine;

public class LocalCamera : MonoBehaviour
{
    Vector3 localPosition;
    Vector3 targetPosition;
    float lastMag = 0;
    readonly float transitionTime = 1f;
    float currentTransition = 1f;

    // Start is called before the first frame update
    void Start()
    {
        localPosition = transform.localPosition;
        lastMag = localPosition.magnitude;
        GetComponentInParent<Power>().subscribePower(scaleCameraSize);
    }

    void scaleCameraSize(Power p)
    {
        targetPosition = localPosition * p.scale();
        currentTransition = 0;
        lastMag = transform.localPosition.magnitude;

    }
    private void Update()
    {
        if (currentTransition < transitionTime)
        {
            float targetMag = targetPosition.magnitude;
            float frameMag = Mathf.SmoothStep(lastMag, targetMag, currentTransition / transitionTime);
            transform.localPosition = localPosition.normalized * frameMag;

            currentTransition += Time.deltaTime;
        }

    }


}
