using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;
using static Utils;
public class InputHandler : NetworkBehaviour, UnitControl
{


    UnitInput currentInput;

    UnitInput lastInput;

    public UnitInput getUnitInuput()
	{
        return currentInput;
	}

	// Start is called before the first frame update
	void Start()
    {
        currentInput.reset();
    }

    // Update is called once per frame
    void Update()
    {
		if (isClient && isLocalPlayer)
		{
            UpdateClient();
		}
        if (isServer)
        {
            UpdateServer();
        }
    }
    void UpdateClient()
	{
        setLocalInput();
        checkServerSend();
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
    static float serverTickRate = 1.0f / 30.0f;
    float currentSendTime=0;
    void checkServerSend()
	{
        currentSendTime += Time.deltaTime;
		if (currentSendTime > serverTickRate)
		{
            while(currentSendTime > serverTickRate)
			{
                currentSendTime -= serverTickRate;
			}
            CmdSendInput(currentInput);
            currentInput.reset();
		}
	}

    [Command]
    void CmdSendInput(UnitInput input)
	{
        lastInput = currentInput;
        currentInput = input;
	}
    void UpdateServer()
    {
        

    }

}
