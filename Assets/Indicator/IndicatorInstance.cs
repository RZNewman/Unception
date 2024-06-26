using Mirror;
using System.Collections;
using UnityEngine;
using static Cast;
using static GenerateAttack;
using static GenerateHit;
using static IndicatorHolder;
using static SpellSource;

public abstract class IndicatorInstance : MonoBehaviour
{
    public float maxTime = 0;
    protected float selfPercent = 0;

    public IndicatorOffsets currentOffsets;


    uint teamOwner;


    protected abstract void setSize();
    protected abstract void setCurrentProgress(float percent);

    public abstract void setColor(Color color, Color stunning);

    public void setLocalOffsets(IndicatorOffsets offsets, float activePercent)
    {
        currentOffsets = offsets;
        selfPercent = activePercent;
        if (offsets.time > maxTime)
        {
            maxTime = offsets.time;
        }
    }
    private void Start()
    {
        updateColor();
        setCurrentProgress(0);
        setLocalPosition();
        StartCoroutine(fixRotation());
    }

    public void OrderedUpdate()
    {
        float progress;
        if (maxTime == 0)
        {
            progress = 0;
        }
        else
        {
            progress = Mathf.Max(maxTime - currentOffsets.time, 0) / maxTime;
        }

        setCurrentProgress(progress);
        setLocalPosition();
    }

    void setLocalPosition()
    {
        if (transform.parent)
        {
            GameObject trackingBody = transform.parent.gameObject;
            IndicatorHolder ih = trackingBody.GetComponentInChildren<IndicatorHolder>();
            transform.localPosition = ih.indicatorPosition() + currentOffsets.distance * ih.offsetMultiplier();

        }
    }

    IEnumerator fixRotation()
    {
        while (true)
        {
            setLocalRotation();
            yield return new WaitForSeconds(0.1f);
        }

    }

    void setLocalRotation()
    {
        if (transform.parent)
        {
            SpellSource source = GetComponentInParent<SpellSource>();
            if (source)
            {
                transform.rotation = source.aimRotation();
            }
            
        }
    }

    public void setTeam(uint team)
    {
        teamOwner = team;
        updateColor();
    }

    void updateColor()
    {
        float threat = getThreat();
        bool stun = willStagger();
        IndicatorColors cols = getIndicatorColor(teamOwner, threat, stun);
        setColor(cols.color, cols.stunning);
    }

    public struct IndicatorColors
    {
        public Color color;
        public Color stunning;
    }
    public static IndicatorColors getIndicatorColor(uint team, float threat, bool stunning)
    {
        if (team == 1u)
        {
            return new IndicatorColors
            {
                color = GameColors.FriendIndicator,
                stunning = GameColors.FriendIndicator,
            };

        }
        else
        {
            Color threatColor = getEnemyThreatColor(threat);
            return new IndicatorColors
            {
                color = threatColor,
                stunning = stunning ? GameColors.EnemyIndicatorStun : threatColor,
            };
        }
    }

    public static Color getEnemyThreatColor(float threat)
    {
        Color middle = GameColors.EnemyIndicator;
        Color low = middle;
        low.a = 0.1f;
        Color high = GameColors.EnemyIndicatorHigh;

        Color output;
        if (threat <= 1)
        {
            output = Color.Lerp(low, middle, threat);
        }
        else
        {
            output = Color.Lerp(middle, high, (threat - 1) / 3.0f);
        }
        return output;

    }

    protected virtual float getThreat()
    {
        return 1.0f;
    }

    protected virtual bool willStagger()
    {
        return false;
    }
}
