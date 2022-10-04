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
    float rewardBasePower = 0;

    public bool givesPower = true;
    public bool givesItems = true;

    PityTimer<Quality> pityQuality;
    private void Start()
    {
        p = GetComponent<Power>();
        pityQuality = new PityTimer<Quality>(Quality.Common, 0.25f);
        //float uncChance = RewardManager.uncommonChance; TODO
        float uncChance = 0.25f;
        pityQuality.addCategory(Quality.Uncommon, uncChance);
        pityQuality.addCategory(Quality.Rare, uncChance * Mathf.Pow(RewardManager.qualityRarityFactor, 1));
        pityQuality.addCategory(Quality.Epic, uncChance * Mathf.Pow(RewardManager.qualityRarityFactor, 2));
        pityQuality.addCategory(Quality.Legendary, uncChance * Mathf.Pow(RewardManager.qualityRarityFactor, 3));
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
            while (gatheredPower > powerPerItem)
            {
                gatheredPower -= powerPerItem;
                Quality q = pityQuality.roll();
                inventory.AddItem(GenerateAttack.generate(other.rewardBasePower, false, q), other.transform.position);
            }
        }
    }
}
