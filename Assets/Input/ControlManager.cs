using Mirror;
using UnityEngine;
using static UnitControl;

public class ControlManager : NetworkBehaviour, TeamOwnership
{
    public GameObject playerControlPre;
    public GameObject aiControlPre;

    public bool useLocalLook = true;

    UnitControl controller;

    UnitInput currentInput;
    UnitInput localInput;


    [SyncVar]
    bool isPlayer = true;

    UnitPropsHolder propHolder;

    public bool isLocalAuthority
    {
        get
        {
            return isClient && hasAuthority;
        }
    }

    void Start()
    {
        propHolder = GetComponent<UnitPropsHolder>();
        isPlayer = propHolder.props.isPlayer;
        spawnControl();
        currentInput = new UnitInput();
        currentInput.reset();
        localInput = new UnitInput();
        localInput.reset();
    }
    public void spawnControl()
    {

        if (isLocalAuthority)
        {
            GameObject o = Instantiate(playerControlPre, transform);
            controller = o.GetComponent<UnitControl>();
        }
        if (isServer && !isPlayer)
        {
            GameObject o = Instantiate(aiControlPre, transform);
            controller = o.GetComponent<UnitControl>();
        }
        if (controller != null)
        {
            controller.init();
        }

    }

    public UnitInput GetUnitInput()
    {
        UnitInput current = currentInput;
        if (useLocalLook && isLocalAuthority)
        {
            //Local look for smoother client turning
            current.lookOffset = localInput.lookOffset;
        }
        serverRead = true;
        return current;
    }
    //Used to sync keydown events between update and Fixedupdate
    //set to true on read, so that the buttons can be cleared the next write
    bool serverRead = false;

    private void Update()
    {
        if (isLocalAuthority && isServer)
        {
            if (serverRead)
            {
                currentInput.cleanButtons();
                serverRead = false;
            }
            controller.refreshInput();
            currentInput = currentInput.merge(controller.getUnitInuput());
            localInput = localInput.merge(currentInput);
        }
        else if (isLocalAuthority)
        {
            controller.refreshInput();
            localInput = localInput.merge(controller.getUnitInuput());
            //Debug.Log(localInput.attacks.Length);

        }
        else if (isServer)
        {
            if (!isPlayer)
            {
                controller.refreshInput();
                currentInput = controller.getUnitInuput();

            }
            localInput = localInput.merge(currentInput);
        }

        if (checkSendtime(localInput))
        {
            localInput.cleanButtons();
        }
    }

    static float serverTickRate = 1.0f / 30.0f;
    float currentSendTime = 0;
    bool checkSendtime(UnitInput local)
    {
        currentSendTime += Time.deltaTime;
        if (currentSendTime > serverTickRate)
        {
            while (currentSendTime > serverTickRate)
            {
                currentSendTime -= serverTickRate;
            }
            if (isLocalAuthority && isClientOnly)
            {
                CmdSendInput(local);
            }
            else if (isServer)
            {
                RpcSendInput(local);
            }

            return true;
        }
        return false;
    }

    [Command]
    void CmdSendInput(UnitInput input)
    {
        //maybe merge?
        currentInput = input;
    }

    [ClientRpc]
    void RpcSendInput(UnitInput input)
    {
        if (!isServer)
        {
            currentInput = input;
        }

    }

    public uint getTeam()
    {
        return isPlayer ? 1u : 0u;
    }
}
