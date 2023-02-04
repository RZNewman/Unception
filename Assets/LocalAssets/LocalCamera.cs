using System;
using UnityEngine;
using static Utils;

public class LocalCamera : MonoBehaviour
{
    Vector3 localPosition;
    float localClip;
    Vector3 targetPosition;
    float lastMag = 0;
    readonly float transitionTime = 1f;
    float currentTransition = 1f;
    Camera cam;

    public float currentLookAngle = 0;
    public float currentPitchAngle = 70;
    float pitchMax = 70;
    float pitchMin = 45;
    public GameObject rootRotation;

    public enum CameraMode
    {
        Locked,
        Turn,
    }
    public CameraMode mode;

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
        if(mode == CameraMode.Turn)
        {

            Cursor.visible = false;
        }

    }
    private void OnDestroy()
    {
        if (mode == CameraMode.Turn)
        {
            Cursor.visible = true;
        }

    }
    bool initial = true;
    void scaleCameraSize(Power p)
    {
        targetPosition = localPosition * p.scale();
        cam.nearClipPlane = localClip * p.scale();
        currentTransition = 0;
        lastMag = transform.localPosition.magnitude;
        if (initial)
        {
            initial = false;
            currentTransition = transitionTime;
            transform.localPosition = targetPosition;
        }

        //cam.nearClipPlane = 1.0f * p.scale();

    }
    Vector3 lastMousePosition =Vector3.zero;
    private void Update()
    {
        if (currentTransition < transitionTime)
        {
            float targetMag = targetPosition.magnitude;
            float frameMag = Mathf.SmoothStep(lastMag, targetMag, currentTransition / transitionTime);
            transform.localPosition = localPosition.normalized * frameMag;

            currentTransition += Time.deltaTime;
        }


        if(mode == CameraMode.Turn)
        {
            
            Vector2 mouseDelta = Input.mousePosition - lastMousePosition;

            currentLookAngle += mouseDelta.x * 0.2f;
            currentLookAngle = normalizeAngle(currentLookAngle);
            currentPitchAngle -= mouseDelta.y * 0.2f;
            currentPitchAngle = Mathf.Clamp(currentPitchAngle, pitchMin, pitchMax);

            rootRotation.transform.localRotation = Quaternion.Euler(0, currentLookAngle, 0);
            transform.localRotation = Quaternion.Euler(currentPitchAngle, 0, 0);

            lastMousePosition = Input.mousePosition;
        }
        

    }


}
