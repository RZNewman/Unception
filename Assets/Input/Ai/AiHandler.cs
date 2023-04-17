using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static AbiltyList;
using static GenerateAttack;
using static UnitControl;
using static Utils;

public class AiHandler : MonoBehaviour, UnitControl
{
    UnitInput currentInput;
    AggroHandler aggro;
    UnitMovement mover;
    FloorNormal ground;
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
        public Vector3 maximums;
        public EffectiveDistanceType type;
        public EffectiveDistance(float distance, float width, float height, EffectiveDistanceType t = EffectiveDistanceType.Hit)
        {
            this.maximums = new Vector3(width, height, distance);
            this.type = t;
        }

        public EffectiveDistance sum(EffectiveDistance other)
        {
            return new EffectiveDistance
            {
                maximums = other.maximums + maximums,
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
        ground = GetComponentInParent<FloorNormal>(true);
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
        scale = p.scalePhysical();
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

                    AbilityPair pair = GetComponentInParent<AbiltyList>().getBestAbility();
                    EffectiveDistance eff = pair.ability.GetEffectiveDistance(mySize.scaledHalfHeight);

                    Quaternion aim = ground.getAimRotation(rotatingBody.transform.forward);
                    Vector3 aimedDiff = Quaternion.Inverse(aim) * rawDiffAttack;
                    float fullRange = eff.maximums.z + mySize.scaledRadius + thierSize.scaledRadius;
                    if (aimedDiff.z > 0 && aimedDiff.z <= fullRange && Mathf.Abs(aimedDiff.x) < eff.maximums.x && Mathf.Abs(aimedDiff.y) < eff.maximums.y)
                    {
                        currentInput.attacks = new ItemSlot[] { pair.key };
                        if (aimedDiff.z <= fullRange * 0.8f)
                        {
                            currentInput.move = Vector2.zero;
                        }
                    }
                    else
                    {
                        currentInput.attacks = new ItemSlot[0];
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
