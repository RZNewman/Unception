using UnityEngine;
using UnityEngine.AI;
using static UnitControl;
using static Utils;

public class AiHandler : MonoBehaviour, UnitControl
{
    UnitInput currentInput;
    AggroHandler aggro;
    UnitMovement mover;
    GameObject rotatingBody;
    ModelLoader modelLoader;
    NavMeshPath currentPath;
    int pathingCorner = -1;

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
        modelLoader = GetComponentInParent<ModelLoader>();
        currentPath = new NavMeshPath();
        rotatingBody = mover.GetComponentInChildren<UnitRotation>().gameObject;
    }

    public void refreshInput()
    {
        //Get current target and move to it
        if (aggro)
        {
            GameObject target = aggro.getTopTarget();
            if (target)
            {
                Vector3 moveTarget;
                bool canSee = aggro.canSee(target);
                if (canSee)
                {
                    moveTarget = target.transform.position;
                    pathingCorner = -1;
                }
                else
                {
                    if (pathingCorner < 0)
                    {
                        NavMesh.CalculatePath(transform.position, target.transform.position, NavMesh.AllAreas, currentPath);
                        pathingCorner = 0;
                    }

                    if (currentPath.status == NavMeshPathStatus.PathInvalid)
                    {
                        moveTarget = transform.position;
                        pathingCorner = -1;
                        //Debug.Log("INVALID");
                    }
                    else
                    {
                        Vector3 current = currentPath.corners[pathingCorner];
                        if (modelLoader.size)
                        {
                            Vector3 diff = current - (transform.position + Vector3.down * modelLoader.size.scaledHalfHeight);

                            if (diff.magnitude <= modelLoader.size.scaledRadius)
                            {
                                pathingCorner++;

                            }
                        }

                        if (pathingCorner >= currentPath.corners.Length)
                        {
                            pathingCorner = -1;
                            moveTarget = transform.position;

                        }
                        else
                        {
                            current = currentPath.corners[pathingCorner];

                            moveTarget = current;
                        }


                    }

                }


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
                    float edgeDiffMag = planarDiff.magnitude - rotatingBody.GetComponentInChildren<Size>().scaledRadius - target.GetComponent<Size>().scaledRadius;

                    EffectiveDistance eff = GetComponentInParent<AbiltyList>().getAbility(0).GetEffectiveDistance();
                    Vector3 perpendicularWidth = planarDiff - Vector3.Dot(planarDiff, rotatingBody.transform.forward) * rotatingBody.transform.forward;
                    float dot = Vector3.Dot(planarDiff, rotatingBody.transform.forward);
                    if ((edgeDiffMag <= eff.distance || eff.distance == 0) && perpendicularWidth.magnitude < eff.width && dot > 0)
                    {
                        currentInput.attacks = new AttackKey[] { AttackKey.One };
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
