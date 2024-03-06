using Mirror;
using UnityEngine;
using static UnitControl;
using static TeamOwnership;

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
    UnitUpdateOrder updater;

    public bool isLocalAuthority
    {
        get
        {
            return isClient && isOwned;
        }
    }

    void Start()
    { 
        spawnControl();
        currentInput = new UnitInput();
        currentInput.reset();
        localInput = new UnitInput();
        localInput.reset();
    }
    public void spawnControl()
    {
        if (controller != null)
        {
            return;
        }
        propHolder = GetComponent<UnitPropsHolder>();
        updater = GetComponent<UnitUpdateOrder>();
        isPlayer = propHolder.props.isPlayer;

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

        if (isPlayer)
        {
            updater.setRegistration(true);
        }

        if (controller != null)
        {
            controller.init();
        }

    }

    public UnitInput GetUnitInput()
    {
        serverRead = true;
        return currentInput;
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
            currentInput.merge(controller.getUnitInuput());
            localInput.merge(currentInput);
        }
        else if (isLocalAuthority)
        {
            controller.refreshInput();
            localInput.merge(controller.getUnitInuput());
            if (useLocalLook)
            {
                //Local look for smoother client turning
                currentInput.lookOffset = localInput.lookOffset;
            }
            //Debug.Log(localInput.attacks.Length);

        }
        else if (isServer)
        {
            if (!isPlayer)
            {
                updater.setRegistration(controller.isAiActive());
                controller.refreshInput();
                currentInput.reset();
                currentInput.merge(controller.getUnitInuput());

            }
            localInput.merge(currentInput);
        }

        if (NetworkServer.connections.Count > 1 && checkSendtime(localInput))
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
            if (useLocalLook)
            {
                //Local look for smoother client turning
                input.lookOffset = localInput.lookOffset;
            }
            currentInput = input;
        }

    }

    public uint getTeam()
    {
        return isPlayer ? PLAYER_TEAM : ENEMY_TEAM;
    }
}
