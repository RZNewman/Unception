using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static RewardManager;

public class UiItemPopups : MonoBehaviour
{
    public GameObject itemPopupPre;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void createPop(Quality quality, AttackFlair flair)
    {
        GameObject pop = Instantiate(itemPopupPre, transform);
        pop.GetComponent<UiItemBrief>().init(quality, flair);
        pop.transform.SetAsFirstSibling();
        data.Add(new PopupData
        {
            pop = pop,
            birth = Time.time
        });
    }

    struct PopupData
    {
        public GameObject pop;
        public float birth;
    }

    List<PopupData> data = new List<PopupData>();

    // Update is called once per frame
    void Update()
    {
        foreach(PopupData d in data.ToArray())
        {
            if(d.birth + 5f < Time.time)
            {
                Destroy(d.pop);
                data.Remove(d);
            }
        }
    }
}
