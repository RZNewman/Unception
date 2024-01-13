using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Interactor : NetworkBehaviour
{
    public GameObject interactionPrompt;

    LocalPlayer lp;
    UnitMovement mover;
    // Start is called before the first frame update
    void Start()
    {
        interactionPrompt.GetComponentInChildren<UIKeyDisplay>().sync();
        interactionPrompt.SetActive(false);
        lp = GetComponent<LocalPlayer>();
        mover = GetComponent<UnitMovement>();
    }

    List<Interaction> zones = new List<Interaction>();

    public void setInteraction(Interaction ia, bool add)
    {
        if (add)
        {
            zones.Add(ia);
        }
        else
        {
            zones.Remove(ia);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (lp.isLocalUnit)
        {
            if(zones.Count > 0)
            {
                interactionPrompt.SetActive(true);
                TMP_Text txt = interactionPrompt.GetComponentInChildren<TMP_Text>();
                string prompt = zones.First().prompt;
                if(txt.text != prompt)
                {
                    txt.text = prompt;
                }
            }
            else
            {
                interactionPrompt.SetActive(false);
            }
        }

        if (isServer)
        {
            if (zones.Count > 0)
            {
                if (mover.input.interact)
                {
                    zones.First().interact(this);
                }
            }
        }
        
    }

}
