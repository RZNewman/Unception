using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reward : MonoBehaviour
{
    Power p;
    Inventory inventory;
    float rewardPower = 0;
    float rewardMultiplier = 1f;
    float rewardBasePower = 0;
    private void Start()
    {
        p = GetComponent<Power>();
    }
    public void setReward(float basePower, float multiplier, float rewardPow)
    {
        rewardBasePower = basePower; rewardPower = rewardPow; rewardMultiplier = multiplier;
    }
    public void setInventory(Inventory i)
    {
        inventory = i;
    }
    public float power
    {
        get { return rewardPower; }
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
            while (gatheredPower > other.rewardBasePower)
            {
                gatheredPower -= other.power;
                inventory.AddItem(GenerateAttack.generate(other.rewardBasePower, false));
            }
        }
    }
}
