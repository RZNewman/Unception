using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiPopups : MonoBehaviour
{
    public GameObject tutorialPre;

    GameObject instancedPopup;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void createTutorial(List<PlayerInfo.TutorialSection> sections)
    {
        instancedPopup = Instantiate(tutorialPre, transform);
        instancedPopup.GetComponent<UiTutorial>().setSections(sections);
    }

    public void closePopup()
    {
        if (instancedPopup)
        {
            Destroy(instancedPopup);
        }
        
    }
}
