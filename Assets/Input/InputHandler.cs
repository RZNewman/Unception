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

        //Debug.DrawLine(Camera.main.transform.position, Camera.main.transform.position + r.direction * cameraRayMax);

        RaycastHit[] info = Physics.RaycastAll(r, cameraRayMax, MapGenerator.TerrainMask() | LayerMask.GetMask("Stopper"));

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
                    Debug.DrawLine(clickHit.point, point, Color.magenta);
                    break;
                }
                else
                {
                    //throw new System.Exception();
                    //Debug.LogWarning("no local click target");
                    return;
                }

            }

            RaycastHit hit = info[i];

            
            float angle = Vector3.Angle(transform.position - hit.point, Camera.main.transform.forward);
            if(angle < 70)
            {
                //too close to the camera
                Debug.DrawLine(hit.point, hit.point + Vector3.up, Color.red);
                continue;
            }

            if ((1 << hit.transform.gameObject.layer & MapGenerator.TerrainMask()) > 0 )
            {
                if (
                Vector3.Angle(Vector3.up, hit.normal) < floorDegrees
                )
                {
                    Vector3 point = hit.point + hit.normal * 0.75f * power.scalePhysical();
                    currentInput.lookOffset = point - transform.position;

                    //if (currentInput.lookObstructed(transform.position))
                    //{
                    //    continue;
                    //}

                    Debug.DrawLine(hit.point, point, Color.green);
                    break;
                }
                Debug.DrawLine(hit.point, hit.point + Vector3.up, Color.white);
            }
            if (1 << hit.transform.gameObject.layer == LayerMask.GetMask("Stopper"))
            {
  
                Vector3 point = hit.transform.position;
                currentInput.lookOffset = point - transform.position;
                //if (currentInput.lookObstructed(transform.position))
                //{
                //    continue;
                //}

                Debug.DrawLine(hit.point, hit.point + Vector3.up, Color.green);
                break;
            }


        }


    }

}
