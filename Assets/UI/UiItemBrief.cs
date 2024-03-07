using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GenerateAttack;
using static RewardManager;

public class UiItemBrief : MonoBehaviour
{
    public Image quality;
    public Image symbol;
    public TMP_Text title;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void init(Quality q, AttackFlair flair)
    {
        quality.sprite = GlobalPrefab.gPre.bgFromQuality(q);
        symbol.sprite = FindObjectOfType<Symbol>().symbols[flair.symbol];
        symbol.color = flair.color;
        title.text = flair.name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
