using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static AiHandler;
using UnityEngine.UIElements;
using static GenerateHit;
using static UnitControl;
using static UnitSound;
using static Utils;
using static GenerateBuff;
using Unity.Burst.Intrinsics;
using static GenerateAttack;
using System;
using static SpellSource;
using static GenerateHit.HitInstanceData;
using static EventManager;
using static Size;
using static AttackUtils.CapsuleInfo;
using System.Linq;
using System.Text.RegularExpressions;

public static class AttackUtils
{
    public struct KnockBackVectors
    {
        public Vector3 center;
        public Vector3 direction;
    }
    public static bool hit(GameObject other, UnitMovement mover, HitInstanceData hitData, uint team, float power, KnockBackVectors knockbackData)
    {
        if (other.GetComponentInParent<TeamOwnership>().getTeam() != team)
        {
            UnitMovement otherMover = other.GetComponentInParent<UnitMovement>();
            DamageValues damage = hitData.damage(power, otherMover && otherMover.isIncapacitated);
            if (mover)
            {

                other.GetComponentInParent<EventManager>().fireHit(new GetHitEventData
                {
                    other = mover.gameObject,
                    powerByStrength = hitData.powerByStrength,
                    damage = damage.instant,
                });
            }

            Health h = other.GetComponentInParent<Health>();
            Posture p = other.GetComponentInParent<Posture>();
            Mezmerize mez = other.GetComponentInParent<Mezmerize>();
            Knockdown kDown = other.GetComponentInParent<Knockdown>();
            if (h)
            {

                if (damage.dot > 0)
                {
                    SpawnBuff(otherMover.transform, BuffMode.Dot, hitData.scales, damage.dotTime, damage.dot);
                }
                h.takeDamageHit(damage.instant);
                if (damage.expose > 0)
                {
                    SpawnBuff(otherMover.transform, BuffMode.Expose, hitData.scales, 10f / hitData.scales.time, damage.expose);
                }
            }

            if (p)
            {
                float stagger = hitData.stagger;
                if (mez && mez.isMezmerized)
                {
                    stagger *= 1.1f;
                }
                p.takeStagger(stagger);
            }
            if (mez)
            {
                float focusHit = hitData.mezmerize;
                if (kDown && kDown.knockedDown)
                {
                    focusHit *= 1.1f;
                }
                mez.takeFocus(focusHit);
            }


            if (otherMover)
            {
                Vector3 knockBackDir;
                switch (hitData.knockBackType)
                {
                    case KnockBackType.inDirection:
                        knockBackDir = knockbackData.direction;
                        break;
                    case KnockBackType.fromCenter:
                        Vector3 dir = other.transform.position - knockbackData.center;
                        dir.y = 0;
                        dir.Normalize();
                        knockBackDir = dir;
                        break;
                    default:
                        throw new System.Exception("No kb type");
                }
                switch (hitData.knockBackDirection)
                {
                    case KnockBackDirection.Backward:
                        knockBackDir *= -1;
                        break;
                }
                otherMover.knock(knockBackDir, hitData.knockback, hitData.knockup);
            }

            return true;
        }
        return false;
    }


    public static float attackHitboxHalfHeight(HitType type, float halfUnitHeight, float attackDistance)
    {
        switch (type)
        {
            case HitType.Line:
                return Mathf.Max(halfUnitHeight * 1.5f, attackDistance);
            case HitType.Projectile:
                return Mathf.Max(halfUnitHeight, attackDistance);
            case HitType.Ground:
                return attackDistance;
            default:
                return halfUnitHeight * 2;
        }
    }

    public static void SpawnProjectile(SpellSource source, UnitMovement mover, HitInstanceData hitData, BuffInstanceData buffData, AudioDistances dists)
    {
        FloorNormal floor = source.GetComponent<FloorNormal>();
        GameObject prefab = GlobalPrefab.gPre.ProjectilePre;
        Quaternion aim = source.aimRotation(AimType.Normal);
        GameObject instance = GameObject.Instantiate(prefab, source.transform.position, aim);
        Projectile p = instance.GetComponent<Projectile>();
        float hitRadius = hitData.width / 2;
        float terrainRadius = Mathf.Min(hitRadius, source.sizeCapsule.distance * 0.5f);
        p.init(terrainRadius, hitRadius, source.sizeCapsule.distance, mover, hitData, buffData, dists);
        NetworkServer.Spawn(instance);
    }

    public static void SpawnBuff(BuffInstanceData buff, Transform target)
    {
        GameObject prefab = GlobalPrefab.gPre.BuffPre;
        if (buff.slot.HasValue)
        {
            target = target.GetComponent<AbilityManager>().getAbility(buff.slot.Value).transform;
        }
        GameObject instance = GameObject.Instantiate(prefab, target);
        instance.GetComponent<ClientAdoption>().parent = target.gameObject;
        instance.GetComponent<Buff>().setup(buff);
        instance.GetComponent<StatHandler>().setStats(buff.stats);
        NetworkServer.Spawn(instance);
    }

    public static void SpawnBuff(Transform target, BuffMode buffMode, Scales scales, float duration, float value, float regen = 0)
    {
        GameObject prefab = GlobalPrefab.gPre.BuffPre;
        GameObject instance = GameObject.Instantiate(prefab, target);
        instance.GetComponent<ClientAdoption>().parent = target.gameObject;
        instance.GetComponent<Buff>().setup(buffMode, scales, duration, value, regen);
        NetworkServer.Spawn(instance);
    }

    public enum EffectShape
    {
        Centered,
        Slash,
        Overhead
    }
    public enum ShapeColliderType
    {
        Box,
        Sphere,
        Capsule,
    }

    public abstract class ColliderInfo
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool subtract;
        
    }

    public class BoxInfo : ColliderInfo
    {
        public Vector3 size;
    }
    public class SphereInfo : ColliderInfo
    {
        public float radius;
    }
    public class CapsuleInfo : ColliderInfo
    {
        public float radius;
        public float height;
        public CapsuleColliderDirection capsuleDir;

        public struct CapsulePoints
        {
            public Vector3 one;
            public Vector3 two;

            public void rotate(Quaternion q)
            {
                one = q * one;
                two = q * two;
            }
            public void offset(Vector3 o)
            {
                one = one + o;
                two = two + o;
            }

            public float furthestDistance(Vector3 point)
            {
                return Mathf.Max((one - point).magnitude, (two - point).magnitude);
            }
        }

        public CapsulePoints getPoints(Vector3 sourcePosition, Quaternion sourceRotation)
        {
            float pointDiff = Mathf.Max(0, height - radius * 2);
            Vector3 heightDir = capsuleDir switch
            {
                CapsuleColliderDirection.X => Vector3.right,
                CapsuleColliderDirection.Y => Vector3.up,
                CapsuleColliderDirection.Z => Vector3.forward,
                _ => throw new Exception(),
            };
            CapsulePoints points = new CapsulePoints
            {
                one = heightDir * pointDiff,
                two = heightDir * -pointDiff,
            };
            points.rotate(rotation);
            points.offset(position);
            points.rotate(sourceRotation);
            points.offset(sourcePosition);
            return points;

        }

        public static CapsuleInfo fromCollider(CapsuleCollider col)
        {
            return new CapsuleInfo
            {
                position = col.center + col.transform.position,
                height = col.height,
                radius = col.radius,
                capsuleDir = (CapsuleColliderDirection)col.direction,
                rotation = col.transform.rotation,
            };
        }
    }

    public struct IndicatorShaderSettings
    {
        public float angle;
        public float circle;
        public float circleSubtract;
        public float forward;
        public float sidways;
        public float subtractOffset;
    }
    public enum IndicatorShape
    {
        Box,
        BoxFull,
        Circle,
        CircleFull
    }
    public enum IndicatorProgressElement
    {
        None,
        Circle,
        Sideways,
    }
    public struct IndicatorDisplay
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public IndicatorShape shape;
        public IndicatorShaderSettings? settings;
        public IndicatorProgressElement progress;
    }


    public  abstract class VFXInfo
    {
        public float radius;
        public float subtractRadius;
    }

    public class VFXCapsuleInfo : VFXInfo
    {
        public float additionalLength;
    }
    public class VFXArcInfo : VFXInfo
    {

        public float rollDegrees;
        public float arcDegrees;
        public float height;
    }

    public struct ShapeData
    {
        public List<ColliderInfo> colliders;
        public List<IndicatorDisplay> indicators;
        public VFXInfo vfx;
    }

    public static ShapeData getShapeData(EffectShape shape, CapsuleSize sizeC, float range, float length, float width)
    {
        ShapeData data = new ShapeData();
        data.colliders = new List<ColliderInfo>();
        data.indicators = new List<IndicatorDisplay>();
        Vector3 frontPoint = Vector3.forward * sizeC.radius;
        float subtractPercent;
        float barWidth = sizeC.radius;
        switch (shape)
        {
            case EffectShape.Centered:
                float subtractRadius = range + sizeC.radius;
                float fullRadius = width + subtractRadius;
                #region colliders
                data.colliders.Add(new CapsuleInfo
                {
                    position = Vector3.zero + Vector3.forward * length/2,
                    rotation = Quaternion.identity,
                    capsuleDir = CapsuleColliderDirection.Z,
                    radius = fullRadius,
                    height = fullRadius + length,
                });
                data.colliders.Add(new SphereInfo
                {
                    position = Vector3.zero,
                    rotation = Quaternion.identity,
                    subtract = true,
                    radius = subtractRadius,
                });
                #endregion

                #region indicators
                subtractPercent = subtractRadius / fullRadius;
                Quaternion backwards = Quaternion.Euler(0, 180, 0);
                data.indicators.Add(new IndicatorDisplay
                {
                    position = Vector3.zero,
                    rotation = backwards,
                    scale = Vector3.one * fullRadius * 2,
                    shape = IndicatorShape.Circle,
                    settings = new IndicatorShaderSettings
                    {
                        forward = 1,
                    },

                });
                data.indicators.Add(new IndicatorDisplay
                {
                    position = Vector3.forward * length,
                    rotation = Quaternion.identity,
                    scale = Vector3.one * fullRadius * 2,
                    shape = IndicatorShape.Circle,
                    settings = new IndicatorShaderSettings
                    {
                        forward = 1,
                    },
                });
                data.indicators.Add(new IndicatorDisplay
                {
                    position = Vector3.zero,
                    rotation = backwards,
                    scale = Vector3.one * fullRadius * 2,
                    shape = IndicatorShape.CircleFull,
                    settings = new IndicatorShaderSettings
                    {
                        forward = 1,
                        circle = subtractPercent,
                        circleSubtract = subtractPercent,
                    },
                    progress = IndicatorProgressElement.Circle,
                });
                data.indicators.Add(new IndicatorDisplay
                {
                    position = Vector3.forward * length,
                    rotation = Quaternion.identity,
                    scale = Vector3.one * fullRadius * 2,
                    shape = IndicatorShape.CircleFull,
                    settings = new IndicatorShaderSettings
                    {
                        forward = 1,
                        circle = subtractPercent,
                        circleSubtract = subtractPercent,
                        subtractOffset = length / fullRadius,
                    },
                    progress = IndicatorProgressElement.Circle,
                });
                data.indicators.Add(new IndicatorDisplay
                {
                    position = Vector3.forward * length/2 + Vector3.right*fullRadius,
                    rotation = Quaternion.identity,
                    scale = new Vector3(barWidth,0,length),
                    shape = IndicatorShape.BoxFull,
                });
                data.indicators.Add(new IndicatorDisplay
                {
                    position = Vector3.forward * length / 2 + Vector3.left * fullRadius,
                    rotation = Quaternion.identity,
                    scale = new Vector3(barWidth, 0, length),
                    shape = IndicatorShape.BoxFull,
                });
                data.indicators.Add(new IndicatorDisplay
                {
                    position = Vector3.forward * length / 2,
                    rotation = Quaternion.identity,
                    scale = new Vector3(fullRadius * 2, 0, length+fullRadius),
                    shape = IndicatorShape.BoxFull,
                    settings = new IndicatorShaderSettings
                    {
                        sidways = subtractPercent,
                        circleSubtract = subtractPercent,
                        subtractOffset = 1,
                    },
                    progress = IndicatorProgressElement.Sideways,
                });
                #endregion

                data.vfx = new VFXCapsuleInfo()
                {
                    radius = fullRadius,
                    subtractRadius = subtractRadius,
                    additionalLength = length,
                };
                break;
            case EffectShape.Slash:
                #region colliders
                float slashWidth = width;
                float slashLength = length+range;
                float halfCircum = Mathf.PI * slashLength;
                if(slashWidth > halfCircum)
                {
                    //https://www.wolframalpha.com/input?i=w-x+%3D+%28l%2Bx%29+*+pi%2C+solve+for+x
                    float delta = (float)(slashWidth - Math.PI * slashLength) / (1 + Mathf.PI);
                    slashWidth -= delta;
                    slashLength += delta;
                }
                halfCircum = Mathf.PI * slashLength;
                float arcHalfDegrees = slashWidth / halfCircum * 180 / 2;

                
                Quaternion leftRotation = Quaternion.Euler(new Vector3(0, -arcHalfDegrees, 0));
                Quaternion rightRotation = Quaternion.Euler(new Vector3(0, arcHalfDegrees, 0));
                Vector3 leftPoint = frontPoint + leftRotation * Vector3.forward * slashLength;
                Vector3 rightPoint = frontPoint + rightRotation * Vector3.forward * slashLength;
                float boxWidth = (leftPoint - frontPoint).DistanceToLine(rightPoint);
                Vector3 boxSize = new Vector3(boxWidth, sizeC.distance, slashLength);
                Vector3 boxOffset = boxSize / 2;


                data.colliders.Add(new SphereInfo
                {
                    position = frontPoint,
                    rotation = Quaternion.identity,
                    radius = slashLength,
                });
                data.colliders.Add(new SphereInfo
                {
                    position = frontPoint,
                    rotation = Quaternion.identity,
                    radius = range,
                    subtract =true,
                });

                data.colliders.Add(new BoxInfo
                {
                    position = frontPoint + rightRotation*new Vector3(-boxOffset.x,0,boxOffset.z),
                    rotation = rightRotation,
                    size= boxSize,
                });
                data.colliders.Add(new BoxInfo
                {
                    position = frontPoint + leftRotation * new Vector3(boxOffset.x, 0, boxOffset.z),
                    rotation = leftRotation,
                    size = boxSize,
                });
                #endregion

                #region indicators
                subtractPercent = range / slashLength;
                data.indicators.Add(new IndicatorDisplay
                {
                    position= frontPoint,
                    rotation= Quaternion.identity,
                    scale = Vector3.one *slashLength *2,
                    shape = IndicatorShape.Circle,
                    settings = new IndicatorShaderSettings
                    {
                        angle = arcHalfDegrees *2,
                    },

                });
                data.indicators.Add(new IndicatorDisplay
                {
                    position = frontPoint,
                    rotation = Quaternion.identity,
                    scale = Vector3.one * slashLength * 2,
                    shape = IndicatorShape.CircleFull,
                    settings = new IndicatorShaderSettings
                    {
                        angle = arcHalfDegrees * 2,
                        circleSubtract = subtractPercent,
                        circle = subtractPercent,                     
                    },
                    progress = IndicatorProgressElement.Circle,
                });
                data.indicators.Add(new IndicatorDisplay
                {
                    position = frontPoint + rightRotation * new Vector3(0, 0, boxOffset.z),
                    rotation = rightRotation,
                    scale = new Vector3(barWidth, 0,slashLength),
                    shape = IndicatorShape.BoxFull,
                    settings = new IndicatorShaderSettings
                    {
                        circleSubtract = subtractPercent *2,
                        subtractOffset = 1
                    },
                });
                data.indicators.Add(new IndicatorDisplay
                {
                    position = frontPoint + leftRotation * new Vector3(0, 0, boxOffset.z),
                    rotation = leftRotation,
                    scale = new Vector3(barWidth, 0, slashLength),
                    shape = IndicatorShape.BoxFull,
                    settings = new IndicatorShaderSettings
                    {
                        circleSubtract = subtractPercent * 2,
                        subtractOffset = 1
                    },
                });
                #endregion

                data.vfx = new VFXArcInfo()
                {
                    radius = slashLength,
                    subtractRadius = range,
                    arcDegrees = arcHalfDegrees * 2,
                    height = boxSize.y,
                };
                break;
            case EffectShape.Overhead:
                Vector3 outerVec = new Vector3(width / 2, 0, length + range);
                float outerRadius = outerVec.magnitude;
                #region colliders
                data.colliders.Add(new SphereInfo
                {
                    position = frontPoint,
                    rotation = Quaternion.identity,
                    radius = outerRadius,
                });
                data.colliders.Add(new SphereInfo
                {
                    position = frontPoint,
                    rotation = Quaternion.identity,
                    radius =range,
                    subtract=true,
                });
                data.colliders.Add(new BoxInfo
                {
                    position = frontPoint + Vector3.forward * (range + length)/2,
                    rotation = Quaternion.identity,
                    size = new Vector3(width,outerRadius *2, outerRadius),
                });
                #endregion

                #region indicators
                subtractPercent = range / outerRadius;
                float outerArcHalfDegrees = Vector3.Angle(outerVec, Vector3.forward);
                float sidePercent = width / 2 / outerRadius;
                data.indicators.Add(new IndicatorDisplay
                {
                    position = frontPoint,
                    rotation = Quaternion.identity,
                    scale = Vector3.one * outerRadius * 2,
                    shape = IndicatorShape.Circle,
                    settings = new IndicatorShaderSettings
                    {
                        angle = outerArcHalfDegrees * 2,
                    },

                });
                data.indicators.Add(new IndicatorDisplay
                {
                    position = frontPoint,
                    rotation = Quaternion.identity,
                    scale = Vector3.one * outerRadius * 2,
                    shape = IndicatorShape.CircleFull,
                    settings = new IndicatorShaderSettings
                    {
                        sidways = sidePercent,
                        circleSubtract = subtractPercent,
                        circle = subtractPercent,
                    },
                    progress = IndicatorProgressElement.Circle,
                });
                Vector3 boxHalfs = outerVec;
                boxHalfs.z -= range;
                boxHalfs.z /= 2;
                data.indicators.Add(new IndicatorDisplay
                {
                    position = frontPoint +Vector3.forward * boxHalfs.z,
                    rotation = Quaternion.identity,
                    scale = boxHalfs * 2,
                    shape = IndicatorShape.Box,
                }) ;
                #endregion

                data.vfx = new VFXArcInfo()
                {
                    radius = outerRadius,
                    subtractRadius = range,
                    rollDegrees = 90,
                    arcDegrees = 180,
                    height = width,
                };
                break;
        }

        return data;
    }

    public static List<GameObject> ShapeAttack(SpellSource source, ShapeData data)
    {
        List<Collider> hits = null;
        List<Collider> negativeHits = new List<Collider>();

        foreach (ColliderInfo info in data.colliders)
        {
            Collider[] singleHits;
            Quaternion aim = source.aimRotation(AimType.Normal);
            Vector3 totalPosition = source.transform.position+ aim *info.position;
            Quaternion totalRotation = aim * info.rotation;
            switch (info)
            {
                case BoxInfo box:
                    singleHits = Physics.OverlapBox(totalPosition, box.size/2,  totalRotation, LayerMask.GetMask("Players", "Breakables"));
                    if (box.subtract)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        if(hits == null)
                        {
                            hits = singleHits.ToList();
                        }
                        else
                        {
                            hits.innerJoin(singleHits);
                        }
                    }
                    break;
                case SphereInfo sphere:
                    singleHits = Physics.OverlapSphere(totalPosition, sphere.radius, LayerMask.GetMask("Players", "Breakables"));
                    if (sphere.subtract)
                    {
                        foreach(Collider c in singleHits)
                        {
                            if(c is CapsuleCollider)
                            {
                                CapsuleCollider cap = (CapsuleCollider)c;
                                CapsulePoints pointsCheck = fromCollider(cap).getPoints(Vector3.zero, Quaternion.identity);
                                if(pointsCheck.furthestDistance(totalPosition)+cap.radius < sphere.radius)
                                {
                                    negativeHits.Add(c);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (hits == null)
                        {
                            hits = singleHits.ToList();
                        }
                        else
                        {
                            hits.innerJoin(singleHits);
                        }
                    }
                    break;
                case CapsuleInfo capsule:
                    CapsulePoints points = capsule.getPoints(source.transform.position, aim);
                    singleHits = Physics.OverlapCapsule(points.one, points.two, capsule.radius, LayerMask.GetMask("Players", "Breakables"));
                    if (capsule.subtract)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        if (hits == null)
                        {
                            hits = singleHits.ToList();
                        }
                        else
                        {
                            hits.innerJoin(singleHits);
                        }
                    }
                    break;
            }
        }

        foreach(Collider c in negativeHits)
        {
            hits.Remove(c);
        }

        return hits.Select(c =>c.gameObject).ToList();
    }

    public struct LineInfo
    {
        public Vector3 boxCenter;
        public Vector3 boxHalfs;
        public Vector3 capsuleStart;
        public Vector3 capsuleEnd;
        public Vector3 occlusionOrigin;
        public Quaternion aim;
        public float maxDistance;
        public Vector3 bodyForward;
    }
    public static LineInfo LineCalculations(SpellSource source, float range, float length, float width)
    {      
        Vector2 attackVec = new Vector2(length, width / 2);
        float maxDistance = attackVec.magnitude;
        FloorNormal floor = source.GetComponent<FloorNormal>();
        Quaternion aim = source.aimRotation(AimType.Normal);
        Vector3 attackFocus = source.transform.position + aim * Vector3.forward * range;
        Vector3 boxCenter = attackFocus + maxDistance * 0.5f * (aim * Vector3.forward);
        float boxHeight = attackHitboxHalfHeight(HitType.Line, source.sizeCapsule.distance, maxDistance);
        Vector3 boxHalfs = new Vector3(width / 2, boxHeight / 2, maxDistance / 2);

        float capsuleHeightFactor = Mathf.Max(boxHeight / 2 - maxDistance, 0);
        Vector3 capsuleHeightDiff = floor.normal * capsuleHeightFactor;
        Vector3 capsuleStart = attackFocus + capsuleHeightDiff;
        Vector3 capsuleEnd = attackFocus - capsuleHeightDiff;
        return new LineInfo
        {
            boxCenter = boxCenter,
            boxHalfs = boxHalfs,
            capsuleEnd = capsuleEnd,
            capsuleStart = capsuleStart,
            aim = aim,
            maxDistance = maxDistance,
            bodyForward = source.transform.forward,
            occlusionOrigin = source.transform.position,
        };
    }

    public static List<GameObject> LineAttack(LineInfo info)
    {
        List<GameObject> hits = new List<GameObject>();
        List<GameObject> tempHits = new List<GameObject>();

        RaycastHit[] boxHits = Physics.BoxCastAll(info.boxCenter, info.boxHalfs, info.bodyForward, info.aim, 0.0f, LayerMask.GetMask("Players", "Breakables"));
        //RaycastHit[] sphereHits = Physics.SphereCastAll(bodyFocus, maxDistance, body.forward, 0.0f, LayerMask.GetMask("Players"));

        RaycastHit[] capsuleHits = Physics.CapsuleCastAll(info.capsuleStart, info.capsuleEnd, info.maxDistance, info.bodyForward, 0.0f, LayerMask.GetMask("Players", "Breakables"));

        //Debug.DrawLine(bodyFocus, bodyFocus + body.forward * maxDistance, Color.blue, 3.0f); ;
        //Debug.DrawLine(bodyFocus, bodyFocus + (body.forward+body.up).normalized * maxDistance, Color.blue, 3.0f);
        //DrawBox(boxCenter, aim, boxHalfs * 2, Color.blue);
        //Debug.DrawLine(capsuleStart, capsuleEnd, Color.red);
        //Debug.DrawLine(capsuleStart, capsuleStart+ body.forward*maxDistance, Color.red);
        //Debug.DrawLine(capsuleEnd, capsuleEnd + body.forward * maxDistance, Color.red);
        //Debug.Break();

        foreach (RaycastHit hit in boxHits)
        {
            GameObject obj = hit.collider.gameObject;
            tempHits.Add(obj);
        }
        foreach (RaycastHit hit in capsuleHits)
        {
            GameObject obj = hit.collider.gameObject;
            Vector3 lineDiff = hit.collider.bounds.center - info.occlusionOrigin;
            if (tempHits.Contains(obj)
                && !Physics.Raycast(info.occlusionOrigin, lineDiff, lineDiff.magnitude, LayerMask.GetMask("Terrain")))
            {

                hits.Add(obj);
            }
        }

        return hits;

    }

    public static void LineParticle(LineInfo info, HitFlair flair, AudioDistances dists)
    {
        GlobalPrefab gp = GlobalPrefab.gPre;
        GameObject prefab = gp.ParticlePre;
        GameObject i = GameObject.Instantiate(prefab, info.boxCenter, info.aim);
        i.transform.localScale = info.boxHalfs * 2;
        i.GetComponent<Particle>().setVisualsLine(gp.lineAssetsPre[flair.visualIndex], dists);

    }

    public static float GroundRadius(float length, float width)
    {
        return (length + width) / 2;
    }
    public static List<GameObject> GroundAttack(Vector3 origin, float radius)
    {
        List<GameObject> hits = new List<GameObject>();

        RaycastHit[] sphereHits = Physics.SphereCastAll(origin, radius, Vector3.forward, 0.0f, LayerMask.GetMask("Players", "Breakables"));



        foreach (RaycastHit hit in sphereHits)
        {
            GameObject obj = hit.collider.gameObject;
            hits.Add(obj);
        }


        return hits;

    }

    public static void GroundParticle(Vector3 origin, float radius, Quaternion aim, HitFlair flair, AudioDistances dists)
    {
        GlobalPrefab gp = GlobalPrefab.gPre;
        GameObject prefab = gp.ParticlePre;
        GameObject i = GameObject.Instantiate(prefab, origin, aim);
        i.transform.localScale = Vector3.one * radius * 2;
        i.GetComponent<Particle>().setVisualsCircle(gp.groundAssetsPre[flair.visualIndex], dists);


    }
}
