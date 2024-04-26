using Cinemachine;
using System;
using UnityEngine;
using static Utils;

public class LocalCamera : MonoBehaviour
{
    float localClip;
    CinemachineVirtualCamera cam;
    Keybinds keys;

    public GameObject focalPoint;
    public GameObject cameraHolder;
    public float cameraBaseDistance = 22f;
    [HideInInspector]
    public float currentLookAngle = 30;
    float currentPitchAngle = 70;
    float currentZoomPercent = 1;
    public float pitchMax = 89;
    public float pitchMin = -60;
    public float zoomMinPercent = 0.4f;
    public float turnXSens = 0.5f;
    public float turnYSens = 0.3f;
    public float zoomSens = 0.02f;
   

    float currentPhysScale;
    public enum CameraMode
    {
        Locked,
        Turn,
    }
    public CameraMode mode;

    private void Awake()
    {
        cam = GetComponentInChildren<CinemachineVirtualCamera>();
    }
    // Start is called before the first frame update
    void Start()
    {
        localClip = cam.m_Lens.NearClipPlane;
        keys = FindObjectOfType<Keybinds>(true);
        Power p = GetComponentInParent<Power>();
        p.subscribePower(scaleCameraSize);
        //pitchMax = Vector3.Angle(Vector3.forward, -cameraOffset());

        float cameraClipScale = 0;
        if (mode == CameraMode.Turn)
        {
            setCursorLocks(true);
        }
        else
        {
            cameraClipScale = p.scalePhysical();
            focalPoint.transform.localRotation = Quaternion.Euler(currentPitchAngle, currentLookAngle, 0);
        }
        
    }
    private void OnDestroy()
    {
        if (mode == CameraMode.Turn)
        {
            setCursorLocks(false);
        }

    }

    void scaleCameraSize(Power p)
    {
        currentPhysScale = p.scalePhysical();
        cam.m_Lens.NearClipPlane = localClip * currentPhysScale;
        //focalPoint.transform.localPosition = Vector3.up * 2f * currentPhysScale;
        FindObjectOfType<MaterialScaling>().setCameraDistance(cameraMagnitude);
        //cam.nearClipPlane = 1.0f * p.scale();

    }
    public float cameraMagnitude
    {
        get
        {
            return cameraBaseDistance* currentPhysScale * currentZoomPercent;
        }
    }


    Vector3 lastMousePosition = Vector3.zero;
    private void Update()
    {
        //float targetMag = cameraMagnitude;

        //RaycastHit hit;
        //if (mode == CameraMode.Turn && Physics.Raycast(transform.parent.position, transform.parent.rotation * targetPosition, out hit, targetMag, LayerMask.GetMask("Terrain")))
        //{
        //    targetMag = hit.distance;
        //}
        float zoomDelt = 0;
        if (Input.GetKey(keys.binding(Keybinds.KeyName.ZoomIn)))
        {
            zoomDelt -= 1;
        }
        if (Input.GetKey(keys.binding(Keybinds.KeyName.ZoomOut)))
        {
            zoomDelt += 1;
        }
        zoomDelt += Input.GetAxis("Mouse ScrollWheel") * -7.5f;
        if (zoomDelt != 0)
        {
            currentZoomPercent += zoomDelt * zoomSens;
            currentZoomPercent = Mathf.Clamp(currentZoomPercent, zoomMinPercent, 1);
            FindObjectOfType<MaterialScaling>().setCameraDistance(cameraMagnitude);
        }

        cameraHolder.transform.localPosition = cameraMagnitude * Vector3.back;


        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        if (mode == CameraMode.Turn)
        {

            //Vector2 mouseDelta = Input.mousePosition - lastMousePosition;


            currentLookAngle += mouseDelta.x * turnXSens;
            currentLookAngle = normalizeAngle(currentLookAngle);
            currentPitchAngle -= mouseDelta.y * turnYSens;
            currentPitchAngle = Mathf.Clamp(currentPitchAngle, pitchMin, pitchMax);

            focalPoint.transform.localRotation = Quaternion.Euler(0, currentLookAngle, 0);
            transform.localRotation = Quaternion.Euler(currentPitchAngle, 0, 0);

            //lastMousePosition = Input.mousePosition;
        }
        else if (mode == CameraMode.Locked)
        {
            if (Input.GetKey(keys.binding(Keybinds.KeyName.CameraRotate)))
            {
                currentLookAngle += mouseDelta.x * turnXSens * 5;
                currentLookAngle = normalizeAngle(currentLookAngle);
                currentPitchAngle += mouseDelta.y * turnYSens * -12;
                currentPitchAngle = Mathf.Clamp(currentPitchAngle, pitchMin, pitchMax);
                focalPoint.transform.localRotation = Quaternion.Euler(currentPitchAngle, currentLookAngle, 0);

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
