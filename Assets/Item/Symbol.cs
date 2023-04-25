using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;

public class Symbol : MonoBehaviour
{
    public Sprite[] symbols;
    public Sprite[] itemSprites;

    public Sprite fromSlot(ItemSlot slot)
    {
        return itemSprites[(int)slot];
    }

}
