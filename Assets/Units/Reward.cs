using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RewardManager;

public class Reward : MonoBehaviour
{
    Power p;
    Inventory inventory;
    float rewardPackPercent = 0;
    float rewardMultiplier = 1f;
    float qualityMultiplier = 1f;
    float rewardBasePower = 0;

    public bool givesPower = true;
    public bool givesItems = true;


    private void Start()
    {
        p = GetComponent<Power>();

    }
    public void setReward(float basePower, float multiplier, float packPercent, float quality = 1f)
    {
        rewardBasePower = basePower; rewardPackPercent = packPercent; rewardMultiplier = multiplier; qualityMultiplier = quality;
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
            return ((rewardMultiplier * RewardManager.bonusRewardPerDifficulty) + 1) * rewardPackPercent;
        }
    }

    public float basePower
    {
        get { return rewardBasePower; }
    }
    public void recieveReward(Reward other)
    {
        if (other.givesPower)
        {
            recievePower(other);
        }
        if (other.givesItems)
        {
            recieveItems(other);
        }


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
            while (gatheredPower > powerPerItem * other.qualityMultiplier)
            {
                gatheredPower -= powerPerItem * other.qualityMultiplier;
                Quality q = inventory.rollQuality(other.qualityMultiplier);
                inventory.AddItem(GenerateAttack.generate(other.rewardBasePower, false, q), other.transform.position);
            }
        }
    }
}
