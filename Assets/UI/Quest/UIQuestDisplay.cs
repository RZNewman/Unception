using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Atlas;
using System.Linq;
using static GlobalSaveData;
using UnityEngine.UI;
using TMPro;

public class UIQuestDisplay : MonoBehaviour
{
    public GameObject LocationPre;
    public GameObject QuestPre;


    static readonly float tileSize = 50;
    static readonly float padding = 10;

    WorldData worldData;
    WorldProgress worldProgress;
    bool progressSet = false;

    public void displayWorld(WorldData data)
    {
        worldData = data;
        displayWorld();
    }

    public void displayWorld(WorldProgress progress)
    {
        worldProgress = progress;
        progressSet = true;
        displayWorld();
    }

    public void clear()
    {
        worldProgress = new WorldProgress { locations = new Dictionary<string, Dictionary<string, QuestVerticalProgress>>() };
        progressSet = true;
        displayWorld();
    }

    List<GameObject> questMarks = new List<GameObject>();

    void displayWorld()
    {
        if (!worldData || !progressSet)
        {
            return;
        }

        foreach(GameObject o in questMarks)
        {
            Destroy(o);
        }
        questMarks.Clear();

        float offset = 0;
        RectTransform rect;
        int worldMaxTier = 0;
        foreach (Location loc in worldData.locations)
        {
            GameObject l = Instantiate(LocationPre, transform);
            questMarks.Add(l);
            rect = l.GetComponent<RectTransform>();
            int maxTier = loc.verticals.Length == 0 ? loc.overrideTier : loc.verticals.Max(v => v.quests[v.quests.Length - 1].tier);
            int minTier = loc.verticals.Length == 0 ? loc.overrideTier : loc.verticals.Min(v => v.quests[0].tier);
            float width = tileSize * Mathf.Max(loc.verticals.Length, 1) + padding * 2;
            rect.sizeDelta = new Vector2(width, (maxTier - minTier + 1) * tileSize + padding * 2);
            rect.localPosition = new Vector2(offset, minTier * tileSize);

            float offsetVertical = padding;
            foreach (QuestVertical vertical in loc.verticals)
            {

                foreach (Quest quest in vertical.quests)
                {
                    GameObject q = Instantiate(QuestPre, l.transform);
                    if (quest.tier <= worldProgress.questTier(loc.id, vertical.id))
                    {
                        q.GetComponent<Image>().color = Color.green;
                    }
                    q.GetComponentInChildren<TMP_Text>().text = quest.tier.ToString();
                    rect = q.GetComponent<RectTransform>();
                    rect.localPosition = new Vector2(offsetVertical, (quest.tier - minTier) * tileSize + padding);
                }

                offsetVertical += tileSize;
            }

            offset += width + padding;
            worldMaxTier = Mathf.Max(worldMaxTier, maxTier);
        }

        rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(offset, (worldMaxTier + 1) * tileSize + padding * 2);
    }
}
