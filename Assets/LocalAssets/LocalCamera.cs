using UnityEngine;

public class LocalCamera : MonoBehaviour
{
    Vector3 localPosition;
    float localClip;
    Vector3 targetPosition;
    float lastMag = 0;
    readonly float transitionTime = 1f;
    float currentTransition = 1f;
    Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }
    // Start is called before the first frame update
    void Start()
    {
        localPosition = transform.localPosition;
        localClip = cam.nearClipPlane;
        lastMag = localPosition.magnitude;
        GetComponentInParent<Power>().subscribePower(scaleCameraSize);

    }

    void scaleCameraSize(Power p)
    {
        targetPosition = localPosition * p.scale();
        cam.nearClipPlane = localClip * p.scale();
        currentTransition = 0;
        lastMag = transform.localPosition.magnitude;
        //cam.nearClipPlane = 1.0f * p.scale();

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
