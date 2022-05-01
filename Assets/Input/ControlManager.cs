using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;

public class ControlManager : NetworkBehaviour, TeamOwnership
{
    public GameObject playerControlPre;
    public GameObject aiControlPre;

	UnitControl controller;


	UnitInput currentInput;
	UnitInput lastInput;

	[SyncVar]
    bool isPlayer = true;

	UnitPropsHolder propHolder;
	public bool IsPlayer
    {
        get
        {
			return isPlayer;
        }
    }

	void Start()
	{
		propHolder = GetComponent<UnitPropsHolder>();
		isPlayer = propHolder.props.isPlayer;
		spawnControl();
		currentInput = new UnitInput();
		currentInput.reset();
	}
    public void spawnControl()
	{

		if (isPlayer)
		{
			GameObject o =  Instantiate(playerControlPre, transform);
			controller = o.GetComponent<UnitControl>();
		}
		else
		{
			GameObject o = Instantiate(aiControlPre, transform);
			controller = o.GetComponent<UnitControl>();
		}
		controller.init();
	}

	public UnitInput GetUnitInput()
	{
		return currentInput;
	}

	private void Update()
	{
		if (isClient && isLocalPlayer)
		{
			controller.refreshInput();
			currentInput = controller.getUnitInuput();
			checkServerSend();
		}
		if (isServer && !isPlayer)
		{
			controller.refreshInput();
			currentInput = controller.getUnitInuput();
		}
	}

	static float serverTickRate = 1.0f / 30.0f;
	float currentSendTime = 0;
	void checkServerSend()
	{
		currentSendTime += Time.deltaTime;
		if (currentSendTime > serverTickRate)
		{
			while (currentSendTime > serverTickRate)
			{
				currentSendTime -= serverTickRate;
			}
			CmdSendInput(currentInput);
			controller.reset();
		}
	}

	[Command]
	void CmdSendInput(UnitInput input)
	{
		lastInput = currentInput;
		currentInput = input;
	}

	public uint getTeam()
	{
		return isPlayer ? 1u : 0u;
	}
}
