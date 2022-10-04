using UnityEngine;
using static RewardManager;

public static class GameColors
{
    public readonly static Color EnemyIndicatorHigh = new Color(1f, 0.9f, 0.8f, 0.6f);
    public readonly static Color EnemyIndicator = new Color(1f, 0.3f, 0, 0.6f);
    public readonly static Color FriendIndicator = new Color(0, 0f, 1, 0.35f);


    public readonly static Color QualityCommon = new Color(0.8f, 0.8f, 0.8f);
    public readonly static Color QualityUncommon = new Color(0, 1f, 0);
    public readonly static Color QualityRare = new Color(0, 0f, 1);
    public readonly static Color QualityEpic = new Color(1, 0f, 1);
    public readonly static Color QualityLegendary = new Color(1, 0.5f, 0);
    public static Color colorQuality(Quality q)
    {
        switch (q)
        {
            case Quality.Common:
                return QualityCommon;
            case Quality.Uncommon:
                return QualityUncommon;
            case Quality.Rare:
                return QualityRare;
            case Quality.Epic:
                return QualityEpic;
            case Quality.Legendary:
                return QualityLegendary;
            default: return Color.white;
        }
    }
}
