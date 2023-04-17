using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static Keybinds;
using static UnitControl;
using static Utils;
using static FloorNormal;
using static GenerateAttack;

public class InputHandler : MonoBehaviour, UnitControl
{


    UnitInput currentInput;
    Power power;
    Keybinds keys;


    public UnitInput getUnitInuput()
    {
        return currentInput;
    }

    // Start is called before the first frame update
    public void init()
    {
        currentInput.reset();
    }

    private void Start()
    {
        power = GetComponentInParent<Power>();
        keys = FindObjectOfType<Keybinds>(true);
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
            move = move.Rotate(-Camera.main.GetComponent<LocalCamera>().currentLookAngle);
        }

        currentInput.move = move;

        currentInput.jump = Input.GetKey(keys.binding(KeyName.Jump));
        currentInput.dash = Input.GetKeyDown(keys.binding(KeyName.Dash));
        currentInput.cancel = Input.GetKeyDown(keys.binding(KeyName.Cancel));

        HashSet<ItemSlot> atks = new HashSet<ItemSlot>();
        int attackCount = 6;
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


        RaycastHit[] info = Physics.RaycastAll(r, cameraRayMax, LayerMask.GetMask("Terrain"));

        for (int i = 0; i <= info.Length; i++)
        {
            if (i == info.Length)
            {
                RaycastHit clickHit;
                if (Physics.Raycast(r, out clickHit, cameraRayMax, LayerMask.GetMask("ClickPlane")))
                {
                    currentInput.lookOffset = clickHit.point - transform.position;
                    break;
                }
                else
                {
                    //throw new System.Exception("no local click target");
                    return;
                }

            }
            if (Vector3.Angle(Vector3.up, info[i].normal) < floorDegrees)
            {
                currentInput.lookOffset = info[i].point - transform.position;
                break;
            }

        }


    }


}
