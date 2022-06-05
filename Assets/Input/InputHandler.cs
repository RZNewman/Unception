using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;
using static Utils;
public class InputHandler : MonoBehaviour, UnitControl
{


    UnitInput currentInput;



    public UnitInput getUnitInuput()
	{
        return currentInput;
	}

	// Start is called before the first frame update
	public void init()
    {
        currentInput.reset();
    }
    public void reset()
    {
        currentInput.reset();
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
		if (Input.GetKey(KeyCode.W))
		{
            move += Vector2.up;
		}
        if (Input.GetKey(KeyCode.S))
        {
            move += Vector2.down;
        }
        if (Input.GetKey(KeyCode.A))
        {
            move += Vector2.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            move += Vector2.right;
        }
        move.Normalize();
        currentInput.move = move;

        currentInput.jump |= Input.GetKey(KeyCode.Space);
        currentInput.dash |= Input.GetKey(KeyCode.LeftShift);

        HashSet<AttackKey> atks = new HashSet<AttackKey>();
        foreach (AttackKey k in currentInput.attacks)
		{

            atks.Add(k);
			
		}
        if (Input.GetMouseButton(0))
        {
            atks.Add(AttackKey.One);
        }
        if (Input.GetMouseButton(1))
        {
            atks.Add(AttackKey.Two);
        }
        currentInput.attacks = new AttackKey[atks.Count];  
        atks.CopyTo(currentInput.attacks);

        setLocalLook();
    }

    static float cameraRayMax = 100f;
    void setLocalLook()
	{
        if (!Camera.main)
        {
            return;
        }
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit info;
        Vector3 dir;
        if(Physics.Raycast(r, out info, cameraRayMax, LayerMask.GetMask("Terrain"))){
            
            dir = info.point - transform.position;
            
		}
        else
		{
            Physics.Raycast(r, out info, cameraRayMax, LayerMask.GetMask("ClickPlane"));
            dir = info.point - transform.position;

        }
        dir.y = 0;
        dir.Normalize();
        currentInput.look = vec2input(dir);

    }
    

}
