using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiTutorial : MonoBehaviour
{
    public GameObject tutorialSectionPre;

    public void setSections(List<PlayerInfo.TutorialSection> sections)
    {
        foreach(PlayerInfo.TutorialSection section in sections)
        {
            GameObject o = Instantiate(tutorialSectionPre, transform);
            o.GetComponent<UiTutorialSection>().setSection(section);
        }
    }
}
