using System.Collections.Generic;

using UnityEngine;
using static Keybinds;
using static UnitControl;
using static Utils;
using static FloorNormal;
using static GenerateAttack;
using System.Linq;

public class InputHandler : MonoBehaviour, UnitControl
{


    UnitInput currentInput;
    Power power;
    UnitMovement mover;
    Keybinds keys;
    LocalCamera localCam;

    public UnitInput getUnitInuput()
    {
        return currentInput;
    }

    // Start is called before the first frame update
    public void init()
    {
        currentInput = UnitInput.zero();
        currentInput.reset();
    }

    private void Start()
    {
        mover = GetComponentInParent<UnitMovement>();
        power = GetComponentInParent<Power>();
        keys = FindObjectOfType<Keybinds>(true);
        localCam = FindObjectOfType<LocalCamera>();
    }
    // Update is called once per frame
    void Update()
    {


    }
    public void refreshInput()
    {
        setLocalInput();

    }

    void setLocalInput()
    {
        if (!MenuHandler.canInput || !keys)
        {
            currentInput.reset();
            return;
        }
        Vector2 move = Vector2.zero;
        if (Input.GetKey(keys.binding(KeyName.Forward)))
        {
            move += Vector2.up;
        }
        if (Input.GetKey(keys.binding(KeyName.Backward)))
        {
            move += Vector2.down;
        }
        if (Input.GetKey(keys.binding(KeyName.Left)))
        {
            move += Vector2.left;
        }
        if (Input.GetKey(keys.binding(KeyName.Right)))
        {
            move += Vector2.right;
        }
        move.Normalize();
        if (Camera.main)
        {
            move = move.Rotate(-localCam.currentLookAngle);
        }

        currentInput.move = move;

        currentInput.jump = Input.GetKeyDown(keys.binding(KeyName.Jump));
        currentInput.dash = Input.GetKeyDown(keys.binding(KeyName.Dash));
        currentInput.cancel = Input.GetKeyDown(keys.binding(KeyName.Cancel));
        currentInput.interact = Input.GetKeyDown(keys.binding(KeyName.Interact));
        currentInput.recall = Input.GetKeyDown(keys.binding(KeyName.Recall));

        HashSet<ItemSlot> atks = new HashSet<ItemSlot>();
        int attackCount = EnumValues<ItemSlot>().Count();
        for (int i = 0; i < attackCount; i++)
        {
            KeyName key = (KeyName)((int)KeyName.Attack1 + i);
            if (Input.GetKey(keys.binding(key)))
            {
                atks.Add(fromKeyName(key));
            }
        }
        currentInput.attacks = new ItemSlot[atks.Count];
        atks.CopyTo(currentInput.attacks);

        setLocalLook();
    }

    float cameraRayMax
    {
        get
        {
            return power ? 100f * power.scalePhysical() : 100f;
        }
    }
    void setLocalLook()
    {
        if (!Camera.main)
        {
            return;
        }
        Ray r;

        r = Camera.main.ScreenPointToRay(Input.mousePosition);


        RaycastHit[] info = Physics.RaycastAll(r, cameraRayMax, LayerMask.GetMask("Terrain","Stopper"));

        System.Array.Sort(info, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i <= info.Length; i++)
        {
            if (i == info.Length)
            {
                RaycastHit clickHit;
                if (Physics.Raycast(r, out clickHit, cameraRayMax, LayerMask.GetMask("ClickPlane")))
                {
                    Vector3 point = clickHit.point + clickHit.normal * 0.8f * power.scalePhysical();
                    currentInput.lookOffset = point - transform.position;
                    Debug.DrawLine(clickHit.point, clickHit.point + Vector3.up, Color.magenta, 0.01f);
                    break;
                }
                else
                {
                    //throw new System.Exception("no local click target");
                    return;
                }

            }

            RaycastHit hit = info[i];

            
            float angle = Vector3.Angle(transform.position - info[i].point, Camera.main.transform.forward);
            if(angle < 50)
            {
                //too close to the camera
                Debug.DrawLine(hit.point, hit.point + Vector3.up, Color.red, 0.01f);
                continue;
            }

            if (1 << hit.transform.gameObject.layer == LayerMask.GetMask("Terrain"))
            {
                if (
                Vector3.Angle(Vector3.up, info[i].normal) < floorDegrees
                )
                {
                    Vector3 point = info[i].point + info[i].normal * 0.75f * power.scalePhysical();
                    currentInput.lookOffset = point - transform.position;
                    
                    Debug.DrawLine(hit.point, hit.point + Vector3.up, Color.green, 0.01f);
                    break;
                }
                Debug.DrawLine(hit.point, hit.point + Vector3.up, Color.white, 0.01f);
            }
            if (1 << hit.transform.gameObject.layer == LayerMask.GetMask("Stopper"))
            {
  
                Vector3 point = hit.transform.position;
                currentInput.lookOffset = point - transform.position;
                Debug.DrawLine(hit.point, hit.point + Vector3.up, Color.green, 0.01f);
                break;
            }


        }


    }

    public bool isAiActive()
    {
        return false;
    }
}
