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

    public float rewardPercent
    {
        get
        {
            return rewardPackPercent;
        }
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
            float itemBasePower = Mathf.Max(other.rewardBasePower, Atlas.playerStartingPower);

            gatheredPower += other.power;
            float packPerItem = 1 / RewardManager.itemsPerPack;
            float powerPerItem = itemBasePower * packPerItem;
            while (gatheredPower > powerPerItem * other.qualityMultiplier)
            {
                gatheredPower -= powerPerItem * other.qualityMultiplier;
                inventory.AddItem(GenerateAttack.generate(itemBasePower, GenerateAttack.AttackGenerationType.Player, other.qualityMultiplier, inventory.pity), other.transform.position);
            }
        }
    }
}
