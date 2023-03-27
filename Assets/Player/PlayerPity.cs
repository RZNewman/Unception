using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RewardManager;
using static Breakable;
using static GlobalSaveData;

public class PlayerPity : NetworkBehaviour
{
    PityTimer<Quality> pityQuality;
    PityTimer<BreakableType> pityBreakable;
    PityTimer<ModCount> pityModCount;



    [Server]
    public void create()
    {
        pityQuality = new PityTimer<Quality>(Quality.Common, 0.25f);
        pityQuality.addCategory(Quality.Uncommon, uncommonChance);
        pityQuality.addCategory(Quality.Rare, uncommonChance * Mathf.Pow(qualityRarityFactor, 1));
        pityQuality.addCategory(Quality.Epic, uncommonChance * Mathf.Pow(qualityRarityFactor, 2));
        pityQuality.addCategory(Quality.Legendary, uncommonChance * Mathf.Pow(qualityRarityFactor, 3));

        pityBreakable = new PityTimer<BreakableType>(BreakableType.Urn, 0.25f);
        pityBreakable.addCategory(BreakableType.Chest, chestChance);

        pityModCount = new PityTimer<ModCount>(ModCount.Zero, 0.1f);
        pityModCount.addCategory(ModCount.One, oneModChance);
        pityModCount.addCategory(ModCount.Two, oneModChance * Mathf.Pow(modCountRarityFactor, 1));
        pityModCount.addCategory(ModCount.Three, oneModChance * Mathf.Pow(modCountRarityFactor, 2));
        pityModCount.addCategory(ModCount.Four, oneModChance * Mathf.Pow(modCountRarityFactor, 3));
        pityModCount.addCategory(ModCount.Five, oneModChance * Mathf.Pow(modCountRarityFactor, 4));
    }

    [Server]
    public void load(PitySaveData data)
    {
        pityQuality = new PityTimer<Quality>(Quality.Common, 0.25f);
        pityQuality.addCategory(Quality.Uncommon, uncommonChance, data.quality[Quality.Uncommon.ToString()]);
        pityQuality.addCategory(Quality.Rare, uncommonChance * Mathf.Pow(qualityRarityFactor, 1), data.quality[Quality.Rare.ToString()]);
        pityQuality.addCategory(Quality.Epic, uncommonChance * Mathf.Pow(qualityRarityFactor, 2), data.quality[Quality.Epic.ToString()]);
        pityQuality.addCategory(Quality.Legendary, uncommonChance * Mathf.Pow(qualityRarityFactor, 3), data.quality[Quality.Legendary.ToString()]);

        pityBreakable = new PityTimer<BreakableType>(BreakableType.Urn, 0.25f);
        pityBreakable.addCategory(BreakableType.Chest, chestChance, data.breakables[BreakableType.Chest.ToString()]);

        pityModCount = new PityTimer<ModCount>(ModCount.Zero, 0.1f);
        pityModCount.addCategory(ModCount.One, oneModChance, data.modCount[ModCount.One.ToString()]);
        pityModCount.addCategory(ModCount.Two, oneModChance * Mathf.Pow(modCountRarityFactor, 1), data.modCount[ModCount.Two.ToString()]);
        pityModCount.addCategory(ModCount.Three, oneModChance * Mathf.Pow(modCountRarityFactor, 2), data.modCount[ModCount.Three.ToString()]);
        pityModCount.addCategory(ModCount.Four, oneModChance * Mathf.Pow(modCountRarityFactor, 3), data.modCount[ModCount.Four.ToString()]);
        pityModCount.addCategory(ModCount.Five, oneModChance * Mathf.Pow(modCountRarityFactor, 4), data.modCount[ModCount.Five.ToString()]);
    }

    public PitySaveData save()
    {
        return new PitySaveData
        {
            quality = pityQuality.export(),
            breakables = pityBreakable.export(),
            modCount = pityModCount.export(),
        };


    }

    [Server]
    public Quality rollQuality(float qualityMultiplier)
    {
        return pityQuality.roll(qualityMultiplier);
    }

    [Server]
    public BreakableType rollBreakable(float qualityMultiplier)
    {
        return pityBreakable.roll(qualityMultiplier);
    }
    [Server]
    public ModCount rollModCount(float qualityMultiplier)
    {
        return pityModCount.roll(qualityMultiplier);
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
