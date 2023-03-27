using System;
using UnityEngine;
using static Utils;

public class LocalCamera : MonoBehaviour
{
    float localClip;
    Vector3 targetPosition;
    float oldPowerMag = 0;
    readonly float transitionTime = 1f;
    float currentTransition = 1f;
    Camera cam;
    Keybinds keys;

    public Vector3 lockedOffset = new Vector3(0, 20, -7);
    public Vector3 turnOffset = new Vector3(0, 12, -7);
    [HideInInspector]
    public float currentLookAngle = 0;
    [HideInInspector]
    public float currentPitchAngle = 70;
    public float pitchMax = 60;
    public float pitchMin;
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
        oldPowerMag = transform.localPosition.magnitude;
        keys = FindObjectOfType<Keybinds>(true);
        GetComponentInParent<Power>().subscribePower(scaleCameraSize);
        pitchMax = Vector3.Angle(Vector3.forward, -cameraOffset());
        if (mode == CameraMode.Turn)
        {
            setCursorLocks(true);
        }
        else
        {
            transform.localRotation = Quaternion.Euler(pitchMax, 0, 0);
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
        float scalePhys = p.scalePhysical();
        targetPosition = cameraOffset() * scalePhys;
        cam.nearClipPlane = localClip * scalePhys;
        currentTransition = 0;
        oldPowerMag = transform.localPosition.magnitude;
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
        float targetMag = targetPosition.magnitude;

        RaycastHit hit;
        if (mode == CameraMode.Turn && Physics.Raycast(transform.parent.position, transform.parent.rotation * targetPosition, out hit, targetMag, LayerMask.GetMask("Terrain")))
        {
            targetMag = hit.distance;
        }

        if (currentTransition < transitionTime)
        {
            float frameMag = Mathf.SmoothStep(oldPowerMag, targetMag, currentTransition / transitionTime);
            transform.localPosition = cameraOffset().normalized * frameMag;

            currentTransition += Time.deltaTime;
        }
        else
        {
            transform.localPosition = cameraOffset().normalized * targetMag;
        }

        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        if (mode == CameraMode.Turn)
        {

            //Vector2 mouseDelta = Input.mousePosition - lastMousePosition;


            currentLookAngle += mouseDelta.x * turnXSens;
            currentLookAngle = normalizeAngle(currentLookAngle);
            currentPitchAngle -= mouseDelta.y * turnYSens;
            currentPitchAngle = Mathf.Clamp(currentPitchAngle, pitchMin, pitchMax);

            rootRotation.transform.localRotation = Quaternion.Euler(0, currentLookAngle, 0);
            transform.localRotation = Quaternion.Euler(currentPitchAngle, 0, 0);

            //lastMousePosition = Input.mousePosition;
        }
        else if (mode == CameraMode.Locked)
        {
            if (Input.GetKey(keys.binding(Keybinds.KeyName.CameraRotate)))
            {
                currentLookAngle += mouseDelta.x * turnXSens * 5;
                currentLookAngle = normalizeAngle(currentLookAngle);
                rootRotation.transform.localRotation = Quaternion.Euler(0, currentLookAngle, 0);

            }

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
