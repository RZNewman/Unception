using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EventManager;

public class Wetstone : NetworkBehaviour
{
    [SyncVar]
    GameObject target;
    public GameObject bindingVis;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Interaction>().setInteraction(Interact);
    }

    void Interact(Interactor i)
    {
        GetComponent<Interaction>().setInteractable(false);
        transform.GetChild(0).GetComponent<Collider>().enabled = false;
        transform.parent = null;
        target = i.gameObject;
        target.GetComponent<UnitPropsHolder>().waterCarried = gameObject;
        bindingVis.SetActive(false);
        PlayerInfo.FireTutorialEventAll(PlayerInfo.TutorialEvent.WaterPickup);
    }

    [Server]
    public void consume()
    {
        StartCoroutine(consumeRoutine());
    }

    IEnumerator consumeRoutine()
    {
        GameObject unit = target;
        unit.GetComponent<UnitPropsHolder>().waterCarried = null;
        target = FindObjectOfType<Flower>().gameObject;
        MenuHandler.controlCharacterCutscene = false;
        LocalCamera cam = FindObjectOfType<LocalCamera>();
        cam.gameObject.SetActive(false);  
        canTeleport = false;
        yield return FindObjectOfType<Atlas>().missionSucceed();
        cam.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        unit.GetComponent<Reward>().recieveReward(GetComponent<Reward>());
        MenuHandler.controlCharacterCutscene = true;
        Destroy(gameObject);
        PlayerInfo.FireTutorialEventAll(PlayerInfo.TutorialEvent.WaterFed);
    }

    private void OnDestroy()
    {
        if (target)
        {
            
        }
    }

    readonly float _targetDist = 2.5f;
    float targetDist
    {
        get
        {
            return _targetDist * transform.lossyScale.x;
        }
    }
    bool canTeleport = true;
    // Update is called once per frame
    void FixedUpdate()
    {
        if (target)
        {
            Vector3 diff = target.transform.position - transform.position;
            if(canTeleport && diff.magnitude > targetDist * 20)
            {
                transform.position = target.transform.position - diff.normalized * targetDist * 5;
            }
            else if(diff.magnitude > targetDist)
            {
                transform.position += diff * 1.0f * Time.fixedDeltaTime;
            }
            else
            {
                diff.x = 0;
                diff.z = 0;
                transform.position += diff * 1.0f * Time.fixedDeltaTime;
            }
        }
        
    }
}
