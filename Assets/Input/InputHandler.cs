using System.Collections.Generic;
using UnityEngine;
using static Keybinds;
using static UnitControl;
using static Utils;
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

        HashSet<AttackKey> atks = new HashSet<AttackKey>();
        if (Input.GetKey(keys.binding(KeyName.Attack1)))
        {
            atks.Add(AttackKey.One);
        }
        if (Input.GetKey(keys.binding(KeyName.Attack2)))
        {
            atks.Add(AttackKey.Two);
        }
        if (Input.GetKey(keys.binding(KeyName.Attack3)))
        {
            atks.Add(AttackKey.Three);
        }
        if (Input.GetKey(keys.binding(KeyName.Attack4)))
        {
            atks.Add(AttackKey.Four);
        }
        currentInput.attacks = new AttackKey[atks.Count];
        atks.CopyTo(currentInput.attacks);

        setLocalLook();
    }

    float cameraRayMax
    {
        get
        {
            return power ? 100f * power.scale() : 100f;
        }
    }
    void setLocalLook()
    {
        if (!Camera.main)
        {
            return;
        }
        Ray r;
        if (Camera.main.GetComponent<LocalCamera>().mode == LocalCamera.CameraMode.Turn)
        {
            r = Camera.main.ScreenPointToRay(new Vector3(Camera.main.scaledPixelWidth/2, Camera.main.scaledPixelHeight/2));
        }
        else
        {
            r = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        RaycastHit info;
        Vector3 dir;
        if (Physics.Raycast(r, out info, cameraRayMax, LayerMask.GetMask("Terrain")))
        {

            dir = info.point - transform.position;

        }
        else if (Physics.Raycast(r, out info, cameraRayMax, LayerMask.GetMask("ClickPlane")))
        {
            dir = info.point - transform.position;

        }
        else
        {
            //throw new System.Exception("no local click target");
            return;
        }
        currentInput.lookOffset = dir;

    }


}
