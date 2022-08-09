using UnityEngine;
using static UnitControl;
using static Utils;

public class AiHandler : MonoBehaviour, UnitControl
{
    UnitInput currentInput;
    AggroHandler aggro;
    UnitMovement mover;
    GameObject rotatingBody;

    public enum EffectiveDistanceType
    {
        None,
        Hit,
        Modifier
    }
    public struct EffectiveDistance
    {
        public float distance;
        public float width;
        public EffectiveDistanceType type;
        public EffectiveDistance(float distance, float width, EffectiveDistanceType t = EffectiveDistanceType.Hit)
        {
            this.distance = distance;
            this.width = width;
            this.type = t;
        }

        public EffectiveDistance sum(EffectiveDistance other)
        {
            return new EffectiveDistance
            {
                distance = other.distance + distance,
                width = other.width + width,
                type = other.type,
            };
        }
    }
    public UnitInput getUnitInuput()
    {
        return currentInput;
    }
    public void init()
    {
        currentInput = new UnitInput();
        currentInput.reset();
        aggro = GetComponent<AggroHandler>();
        mover = GetComponentInParent<UnitMovement>();
        rotatingBody = mover.GetComponentInChildren<UnitRotation>().gameObject;
    }
    public void reset()
    {
        currentInput.reset();
    }
    public void refreshInput()
    {
        //Get current target and move to it
        if (aggro)
        {
            GameObject target = aggro.getTopTarget();
            if (target)
            {
                Vector3 rawDiff = target.transform.position - transform.position;
                Vector3 planarDiff = rawDiff;
                planarDiff.y = 0;
                Vector3 inpDiff = planarDiff;
                inpDiff.Normalize();
                Vector2 inpVec = vec2input(inpDiff);
                currentInput.move = inpVec;
                currentInput.look = inpVec;

                float edgeDiffMag = planarDiff.magnitude - rotatingBody.GetComponentInChildren<Size>().scaledRadius - target.GetComponent<Size>().scaledRadius;

                EffectiveDistance eff = GetComponentInParent<AbiltyList>().getAbility(0).GetEffectiveDistance();
                Vector3 perpendicularWidth = planarDiff - Vector3.Dot(planarDiff, rotatingBody.transform.forward) * rotatingBody.transform.forward;
                if ((edgeDiffMag <= eff.distance || eff.distance == 0) && perpendicularWidth.magnitude < eff.width)
                {
                    currentInput.attacks = new AttackKey[] { AttackKey.One };
                }
                else
                {
                    currentInput.attacks = new AttackKey[0];
                }

            }
            else
            {
                currentInput.reset();
            }
        }

    }

}
