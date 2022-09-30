using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reward : MonoBehaviour
{
    Power p;
    Inventory inventory;
    float rewardPackPercent = 0;
    float rewardMultiplier = 1f;
    float rewardBasePower = 0;
    private void Start()
    {
        p = GetComponent<Power>();
    }
    public void setReward(float basePower, float multiplier, float packPercent)
    {
        rewardBasePower = basePower; rewardPackPercent = packPercent; rewardMultiplier = multiplier;
    }
    public void setInventory(Inventory i)
    {
        inventory = i;
    }
    float power
    {
        get { return rewardTotalPercent * rewardBasePower; }
    }

    float rewardTotalPercent
    {
        get
        {
            return ((rewardMultiplier * RewardManager.rewardPerDifficulty) - rewardMultiplier + 1) * rewardPackPercent;
        }
    }

    public float basePower
    {
        get { return rewardBasePower; }
    }
    public void recieveReward(Reward other)
    {
        recievePower(other);
        recieveItems(other);
    }
    void recievePower(Reward other)
    {
        float gathered = other.power;

        gathered *= RewardManager.powerPackPercent;
        p.addPower(gathered);
    }

    float gatheredPower = 0;
    void recieveItems(Reward other)
    {
        if (inventory)
        {
            gatheredPower += other.power;
            float packPerItem = 1 / RewardManager.itemsPerPack;
            float powerPerItem = other.rewardBasePower * packPerItem;
            while (gatheredPower > powerPerItem)
            {
                gatheredPower -= powerPerItem;
                inventory.AddItem(GenerateAttack.generate(other.rewardBasePower, false));
            }
        }
    }
}
