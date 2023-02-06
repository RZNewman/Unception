using System;
using UnityEngine;
using static Utils;

public class LocalCamera : MonoBehaviour
{
    float localClip;
    Vector3 targetPosition;
    float lastMag = 0;
    readonly float transitionTime = 1f;
    float currentTransition = 1f;
    Camera cam;

    public Vector3 lockedOffset = new Vector3(0, 20, -7);
    public Vector3 turnOffset = new Vector3(0, 12, -7);
    [HideInInspector]
    public float currentLookAngle = 0;
    [HideInInspector]
    public float currentPitchAngle = 70;
    float pitchMax = 60;
    float pitchMin = 40;
    public GameObject rootRotation;
    public float turnXSens = 0.5f;
    public float turnYSens = 0.3f;

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
        localClip = cam.nearClipPlane;
        lastMag = transform.localPosition.magnitude;
        GetComponentInParent<Power>().subscribePower(scaleCameraSize);
        if (mode == CameraMode.Turn)
        {
            setCursorLocks(true);
        }

    }
    private void OnDestroy()
    {
        if (mode == CameraMode.Turn)
        {
            setCursorLocks(false);
        }

    }
    Vector3 cameraOffset()
    {
        switch (mode)
        {

            case CameraMode.Turn:
                return turnOffset;
            case CameraMode.Locked:
            default:
                return lockedOffset;
        }
    }
    bool initial = true;
    void scaleCameraSize(Power p)
    {
        targetPosition = cameraOffset() * p.scale();
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
    Vector3 lastMousePosition = Vector3.zero;
    private void Update()
    {
        if (currentTransition < transitionTime)
        {
            float targetMag = targetPosition.magnitude;
            float frameMag = Mathf.SmoothStep(lastMag, targetMag, currentTransition / transitionTime);
            transform.localPosition = cameraOffset().normalized * frameMag;

            currentTransition += Time.deltaTime;
        }


        if (mode == CameraMode.Turn)
        {

            //Vector2 mouseDelta = Input.mousePosition - lastMousePosition;
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            currentLookAngle += mouseDelta.x * turnXSens;
            currentLookAngle = normalizeAngle(currentLookAngle);
            currentPitchAngle -= mouseDelta.y * turnYSens;
            currentPitchAngle = Mathf.Clamp(currentPitchAngle, pitchMin, pitchMax);

            rootRotation.transform.localRotation = Quaternion.Euler(0, currentLookAngle, 0);
            transform.localRotation = Quaternion.Euler(currentPitchAngle, 0, 0);

            //lastMousePosition = Input.mousePosition;
        }


    }
    public void pause(bool paused)
    {
        if (mode == CameraMode.Turn)
        {
            setCursorLocks(!paused);
        }
    }

    void setCursorLocks(bool locked)
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


}
