using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Atlas;
using System.Linq;

public class UIQuestDisplay : MonoBehaviour
{
    public GameObject LocationPre;
    public GameObject QuestPre;


    static readonly float tileSize = 100;
    static readonly float padding = 20;
    public void displayWorld(WorldData world)
    {
        float offset = 0;
        RectTransform rect;
        int worldMaxTier = 0;
        foreach (Location loc in world.locations)
        {
            GameObject l = Instantiate(LocationPre, transform);
            rect = l.GetComponent<RectTransform>();
            int maxTier = loc.verticals.Length == 0 ? loc.overrideTier : loc.verticals.Max(v => v.quests[v.quests.Length - 1].tier);
            int minTier = loc.verticals.Length == 0 ? loc.overrideTier : loc.verticals.Min(v => v.quests[0].tier);
            float width = tileSize * Mathf.Max(loc.verticals.Length, 1) + padding * 2 ;
            rect.sizeDelta = new Vector2(width, (maxTier - minTier + 1) * tileSize + padding *2);
            rect.localPosition = new Vector2(offset, minTier * tileSize);

            float offsetVertical = padding;
            foreach (QuestVertical vertical in loc.verticals)
            {
                
                foreach(Quest quest in vertical.quests)
                {
                    GameObject q = Instantiate(QuestPre, l.transform);
                    rect = q.GetComponent<RectTransform>();
                    rect.localPosition = new Vector2(offsetVertical, (quest.tier - minTier) * tileSize + padding);
                }

                offsetVertical += tileSize;
            }

            offset += width + padding;
            worldMaxTier = Mathf.Max(worldMaxTier, maxTier);
        }

        rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(offset, (worldMaxTier+1) * tileSize + padding * 2);
    }
}
