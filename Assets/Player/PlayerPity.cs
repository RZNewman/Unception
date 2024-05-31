using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RewardManager;
using static Breakable;
using static GlobalSaveData;
using System.Linq;

public class PlayerPity : NetworkBehaviour
{
    PityTimer<Quality> pityQuality;
    PityTimer<BreakableType> pityBreakable;
    PityTimer<ModCount> pityStarCount;
    PityTimer<ModBonus> pityModBonus;



    [Server]
    public void create()
    {
        pityQuality = new PityTimer<Quality>(0.25f, uncommonChance, qualityRarityFactor);

        pityBreakable = new PityTimer<BreakableType>(BreakableType.Urn, 0.25f);
        pityBreakable.addCategory(BreakableType.Chest, chestChance);

        pityStarCount = new PityTimer<ModCount>(0.1f, oneModChance, modCountRarityFactor);
        pityModBonus = new PityTimer<ModBonus>(0.02f, modBonusChance, modBonusRarityFactor);
    }

    [Server]
    public void load(PitySaveData data)
    {
        pityQuality = new PityTimer<Quality>(0.25f, uncommonChance, qualityRarityFactor, data.quality.asEnum<Quality>());
        //pityQuality.debug();

        pityBreakable = new PityTimer<BreakableType>(BreakableType.Urn, 0.25f);
        pityBreakable.addCategory(BreakableType.Chest, chestChance, data.breakables[BreakableType.Chest.ToString()]);

        pityStarCount = new PityTimer<ModCount>(0.1f, oneModChance, modCountRarityFactor, data.modCount.asEnum<ModCount>());
        pityModBonus = new PityTimer<ModBonus>(0.02f, modBonusChance, modBonusRarityFactor, data.modBonus.asEnum<ModBonus>());
    }

    public PitySaveData save()
    {
        return new PitySaveData
        {
            quality = pityQuality.export(),
            breakables = pityBreakable.export(),
            modCount = pityStarCount.export(),
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
    public int rollStarCount(float qualityMultiplier)
    {
        return (int)pityStarCount.roll(qualityMultiplier);
    }
    [Server]
    public ModBonus rollModBonus(float qualityMultiplier)
    {
        return pityModBonus.roll(qualityMultiplier);
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
