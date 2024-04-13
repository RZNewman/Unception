using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static AbilityManager;
using static GenerateAttack;
using static UnitControl;
using static Utils;

public class AiHandler : MonoBehaviour, UnitControl
{
    UnitInput currentInput;
    AggroHandler aggro;
    UnitMovement mover;
    UnitEye eye;
    GameObject rotatingBody;
    float scaleSpeed = 1;
    float scalePhys = 1;

    NavMeshAgent agent;

    public struct EffectiveDistance
    {
        public float modDistance;

        public float minDistance;
        public float maxDistance;
        public float width;
        public float widthAngle;
        public float height;
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
        eye = mover.GetComponentInChildren<UnitEye>(true);
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
        scalePhys = p.scalePhysical();
        agent.baseOffset = 0.75f * scalePhys;
        agent.radius = 0.7f * scalePhys;
        agent.height = 1.5f * scalePhys;
        scaleSpeed = p.scaleSpeed();
    }
    float timePathed;
    readonly float pathPeriod = 1f;
    float timeAgentSync;
    readonly float agentSyncPeriod = 0.1f;
    void makePath(Vector3 navTarget)
    {
        agent.SetDestination(navTarget);
        //pathingCorner = 0;
        timePathed = Time.time;

    }

    void syncAgent()
    {
        if (!agent.isOnOffMeshLink  && mover.grounded)
        {
            timeAgentSync = Time.time;
            if((agent.nextPosition -transform.position).magnitude > 2f * scalePhys)
            {
                agent.Warp(transform.position + mover.worldVelocity * Time.fixedDeltaTime);
                transform.localPosition = Vector3.zero;
            }
            
            agent.nextPosition = transform.position + mover.worldVelocity * Time.fixedDeltaTime;
            agent.velocity = mover.worldVelocity;
            agent.speed = mover.props.maxSpeed * scaleSpeed;
            agent.acceleration = agent.speed * 5;
            //agent.angularSpeed = mover.props.lookSpeedDegrees;
        }



    }

    enum DistanceType
    {
        TooClose,
        SlightlyClose,
        JustRight,
        SlightlyFar,
        TooFar,
        WayTooFar,
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
                Vector3 rawDiffAttack = attackTarget - transform.position;

                bool canSee = aggro.canSee(target);
                Optional<Ability> currentAttack = mover.currentAttackingAbility();
                AbilityPair bestNext = GetComponentInParent<AbilityManager>().getBestAbility();
                EffectiveDistance eff = currentAttack.HasValue ? currentAttack.Value.GetEffectiveDistance(mySize.sizeC) : bestNext.ability.GetEffectiveDistance(mySize.sizeC);
                DistanceType dist = DistanceType.WayTooFar;
                bool canAttack = false;
                currentInput.attacks = new ItemSlot[0];

                if (canSee)
                {
                    Quaternion aim = eye.transform.rotation;

                    float startRange = eff.minDistance + eff.modDistance;
                    float fullRange = eff.maxDistance + eff.modDistance + thierSize.scaledRadius;
                    Vector3 aimedDiff = Quaternion.Inverse(aim) * rawDiffAttack;
                    float distanceFromTarget = rawDiffAttack.magnitude;
                    float goodDist = fullRange - startRange;

                    dist = distanceFromTarget switch
                    {
                        float i when i < startRange => DistanceType.TooClose,
                        float i when i < startRange + goodDist * 0.3f => DistanceType.SlightlyClose,
                        float i when i < startRange + goodDist * 0.5f => DistanceType.JustRight,
                        float i when i <= fullRange => DistanceType.SlightlyFar,
                        float i when i <= fullRange * 1.5 => DistanceType.TooFar,
                        _ => DistanceType.WayTooFar,
                    };

                    if (aimedDiff.z > 0)
                    {
                        if(dist != DistanceType.TooClose && dist != DistanceType.TooFar && dist != DistanceType.WayTooFar)
                        {
                            if (
                                (eff.width <= 0 || Mathf.Abs(aimedDiff.x) < eff.width /2)
                                &&
                                (eff.widthAngle <= 0 || Vector3.Angle(Vector3.forward, new Vector3(aimedDiff.x, 0, aimedDiff.z - eff.modDistance)) < eff.widthAngle /2)
                                &&
                                Mathf.Abs(aimedDiff.y) < eff.height
                                )
                            {
                                canAttack = true;

                            }
                        }       
                    }
                }

                if(!currentAttack.HasValue && canAttack)
                {
                    currentInput.attacks = new ItemSlot[] { bestNext.key };
                }
                


                if (Time.time > timePathed + pathPeriod)
                {
                    Vector3 targetNav = thierGround.nav;
                    if(dist == DistanceType.TooClose || dist == DistanceType.SlightlyClose)
                    {
                        targetNav = escapePosition(transform.position, target.transform.position, Mathf.Max(eff.minDistance, rawDiffAttack.magnitude));
                        Debug.DrawLine(targetNav, targetNav + Vector3.up, Color.cyan, 1f);
                    }
                    makePath(targetNav);

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
                currentInput.move = dist switch
                {
                    DistanceType.JustRight => Vector2.zero,
                    _ => inpVec,
                };
                if(mover.grounded && agent.isOnOffMeshLink && agent.nextPosition.y > transform.position.y
                    )
                {
                    currentInput.jump = true;
                }
                else
                {
                    currentInput.jump = false;
                }


                Vector3 moveLookDiff = rawDiff.normalized * 2f;
                if((dist == DistanceType.TooClose || dist == DistanceType.WayTooFar) && !currentAttack.HasValue)
                {
                    currentInput.lookOffset = moveLookDiff;
                }
                else if (canSee)
                {
                    currentInput.lookOffset = rawDiffAttack;
                }
                else
                {
                    currentInput.lookOffset = moveLookDiff;
                }
                

                


            }
            else
            {
                currentInput.reset();
            }
        }

    }

    public Vector3 escapePosition(Vector3 location, Vector3 enemyPosition, float distance)
    {
        Vector3 enemyDiff = enemyPosition - location;
        Optional<Vector3> escape;
        Vector3 awayPos;

        awayPos = transform.position - enemyDiff;
        escape = validEscape(location, awayPos, distance);
        if (escape.HasValue)
        {
            return escape.Value;
        }

        Vector3 cross = Vector3.Cross(Vector3.up, enemyDiff);
        cross *= Random.Range(-1, 1);
        cross.Normalize();
        cross *= enemyDiff.magnitude;

        awayPos = transform.position + cross;
        escape = validEscape(location, awayPos, distance);
        if (escape.HasValue)
        {
            return escape.Value;
        }

        cross *= -1;

        awayPos = transform.position + cross;
        escape = validEscape(location, awayPos, distance);
        if (escape.HasValue)
        {
            return escape.Value;
        }

        awayPos = transform.position + enemyDiff;
        escape = validEscape(location, awayPos, distance);
        if (escape.HasValue)
        {
            return escape.Value;
        }

        return location;

    }

    public Optional<Vector3> validEscape(Vector3 location, Vector3 look, float dist)
    {
        NavMeshHit hit;
        bool didHit;

        didHit = NavMesh.SamplePosition(look, out hit, dist, NavMesh.AllAreas);
        if (didHit)
        {
            Vector3 diff = hit.position - location;
            if(diff.magnitude > dist * 0.2f)
            {
                return hit.position;
            }
        }

        return new Optional<Vector3>();
    }
}
