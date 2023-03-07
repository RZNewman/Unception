using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnitControl;
using static Utils;

public class AiHandler : MonoBehaviour, UnitControl
{
    UnitInput currentInput;
    AggroHandler aggro;
    UnitMovement mover;
    GameObject rotatingBody;
    float scale = 1;

    NavMeshAgent agent;
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
        mover = GetComponentInParent<UnitMovement>(true);
        //pathCalculator = FindObjectOfType<NavPathCalc>();
        //obstacle = GetComponent<NavMeshObstacle>();
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
        rotatingBody = mover.GetComponentInChildren<UnitRotation>(true).gameObject;
        GetComponentInParent<Power>(true).subscribePower(setRadius);
    }

    void setRadius(Power p)
    {
        scale = p.scale();
        agent.baseOffset = 0.75f * scale;
        agent.radius = 0.7f * scale;
        agent.height = 1.5f * scale;
    }
    float timePathed;
    readonly float pathPeriod = 1f;
    float timeAgentSync;
    readonly float agentSyncPeriod = 0.2f;
    void makePath(Vector3 navTarget)
    {
        agent.SetDestination(navTarget);
        //pathingCorner = 0;
        timePathed = Time.time;

    }

    void syncAgent()
    {
        timeAgentSync = Time.time;
        agent.nextPosition = transform.position + mover.worldVelocity * Time.fixedDeltaTime;
        agent.velocity = mover.worldVelocity;
        agent.speed = mover.props.maxSpeed * scale;
        agent.acceleration = agent.speed * 5;
        //agent.angularSpeed = mover.props.lookSpeedDegrees;
    }

    public void refreshInput()
    {

        //Get current target and move to it
        if (aggro)
        {
            GameObject target = aggro.getTopTarget();
            if (target)
            {
                Size mySize = rotatingBody.GetComponentInChildren<Size>();
                Size thierSize = target.GetComponent<Size>();
                FloorNormal thierGround = target.GetComponentInParent<FloorNormal>();

                if (!thierGround)
                {
                    return;
                }

                Vector3 moveTarget;
                Vector3 attackTarget = target.transform.position;
                bool canSee = aggro.canSee(target);

                if (Time.time > timePathed + pathPeriod)
                {
                    makePath(thierGround.nav);

                }
                moveTarget = agent.nextPosition;
                //syncAgent();
                if (Time.time > timeAgentSync + agentSyncPeriod)
                {
                    syncAgent();

                }


                Debug.DrawLine(transform.position, moveTarget, Color.red);
                Vector3 rawDiff = moveTarget - transform.position;
                Vector3 planarDiff = rawDiff;
                planarDiff.y = 0;
                Vector3 inpDiff = planarDiff;
                inpDiff.Normalize();
                Vector2 inpVec = vec2input(inpDiff);
                currentInput.move = inpVec;
                currentInput.lookOffset = rawDiff;

                if (canSee)
                {
                    Vector3 rawDiffAttack = attackTarget - transform.position;
                    currentInput.lookOffset = rawDiffAttack;

                    //TODO check height
                    Vector3 planarDiffAttack = rawDiffAttack;
                    planarDiffAttack.y = 0;
                    float edgeDiffMag = planarDiffAttack.magnitude - mySize.scaledRadius - thierSize.scaledRadius;

                    EffectiveDistance eff = GetComponentInParent<AbiltyList>().getAbility(0).GetEffectiveDistance();
                    Vector3 perpendicularWidth = planarDiffAttack - Vector3.Dot(planarDiffAttack, rotatingBody.transform.forward) * rotatingBody.transform.forward;
                    float dot = Vector3.Dot(planarDiffAttack, rotatingBody.transform.forward);
                    if ((edgeDiffMag <= eff.distance || eff.distance == 0) && perpendicularWidth.magnitude < eff.width && dot > 0)
                    {
                        currentInput.attacks = new AttackKey[] { AttackKey.One };
                        if (edgeDiffMag <= eff.distance * 0.8f)
                        {
                            currentInput.move = Vector2.zero;
                        }
                    }
                    else
                    {
                        currentInput.attacks = new AttackKey[0];
                    }
                }


            }
            else
            {
                currentInput.reset();
            }
        }

    }

}
