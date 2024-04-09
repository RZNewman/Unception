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
using static UnityEngine.UI.Image;
using static AttackSegment;
using static Persistent;

public static class AttackUtils
{
    public struct KnockBackVectors
    {
        public Vector3 center;
        public Vector3 direction;
    }
    public static void hit(GameObject other, UnitMovement mover, HarmPortions harm, uint team, HitList hitList, BuffInstanceData buffData)
    {
        if (other.GetComponentInParent<TeamOwnership>().getTeam() != team && hitList.tryAddHit(other))
        {
            EventManager otherEvents = other.GetComponentInParent<EventManager>();

            otherEvents.fireHit(new GetHitEventData
            {
                other = mover?.gameObject,
                harm = harm.instant,
            });
            if (harm.OverTime.HasValue)
            {
                otherEvents.fireApply(new ApplyDotEventData
                {
                    other = mover?.gameObject,
                    harm = harm.OverTime.Value,
                    time = harm.time,
                });
            }
            
            if(buffData != null && buffData.type == BuffType.Debuff)
            {
                BuffManager bm = other.GetComponentInParent<BuffManager>();
                if (bm)
                {
                    SpawnBuff(buffData, bm.transform);
                }
            }
        }

    }
    public static GameObject SpawnPersistent(SpellSource source, UnitMovement mover, HitInstanceData hitData, BuffInstanceData buffData, HitList hitList, AudioDistances dists, bool isAura)
    {
        Quaternion aim = source.aimRotation();
        CapsuleSize sizeC = source.sizeCapsule;
        return SpawnPersistent(aim, source.gameObject,sizeC, mover, hitData, buffData, hitList, dists, isAura);
    }

    public static GameObject SpawnPersistent(Quaternion aim, GameObject root, CapsuleSize sizeC, UnitMovement mover, HitInstanceData hitData, BuffInstanceData buffData, HitList hitList, AudioDistances dists, bool isAura)
    {
        GameObject prefab = GlobalPrefab.gPre.ProjectilePre;
        GameObject instance = GameObject.Instantiate(prefab, root.transform.position, aim);
        MoveMode moveType = MoveMode.World;
        if (hitData.type == HitType.DamageDash)
        {
            instance.transform.parent = root.transform;
            instance.GetComponent<ClientAdoption>().parent = root;
            moveType = MoveMode.Parent;
        }
        Persistent p = instance.GetComponent<Persistent>();
        NetworkServer.Spawn(instance);
        PersistMode modeP = isAura ? PersistMode.AuraPlaced : hitData.type switch
        {
            HitType.ProjectileExploding  => PersistMode.Explode,
            HitType.ProjectileWave => PersistMode.Wave,
            HitType.DamageDash => PersistMode.Dash,
            _=> throw new NotImplementedException(),
        };
        p.init(sizeC, mover, hitData, buffData, hitList, moveType, dists, modeP);      
        return instance;
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

    public static void SpawnShield(Transform target,  Scales scales, float duration, float value, float regen = 0)
    {
        GameObject prefab = GlobalPrefab.gPre.BuffPre;
        GameObject instance = GameObject.Instantiate(prefab, target);
        instance.GetComponent<ClientAdoption>().parent = target.gameObject;
        instance.GetComponent<Buff>().setupShield(scales, duration, value, regen);
        NetworkServer.Spawn(instance);
    }
    public static void SpawnDot(Transform target, Scales scales, float duration, HarmValues harm)
    {
        GameObject prefab = GlobalPrefab.gPre.BuffPre;
        GameObject instance = GameObject.Instantiate(prefab, target);
        instance.GetComponent<ClientAdoption>().parent = target.gameObject;
        instance.GetComponent<Buff>().setupDot(scales, duration, harm);
        NetworkServer.Spawn(instance);
    }

    public static Buff SpawnAuraBuff(Transform target, Scales scales, Persistent persist, HarmValues harm)
    {
        GameObject prefab = GlobalPrefab.gPre.BuffPre;
        GameObject instance = GameObject.Instantiate(prefab, target);
        instance.GetComponent<ClientAdoption>().parent = target.gameObject;
        Buff buff = instance.GetComponent<Buff>();
        buff.setupAura(scales, persist, harm);
        NetworkServer.Spawn(instance);
        return buff;
    }

    public enum EffectShape : byte
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
        public float? angle;
        public float? circle;
        public float? circleSubtract;
        public float? forward;
        public float? sideways;
        public float? subtractOffset;
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


    public abstract class VFXInfo
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
        public float effectCenterDist
        {
            get
            {
                return (radius - subtractRadius) / 2 + subtractRadius;
            }
        }
        public float arcWidth
        {
            get
            {
                return (Quaternion.Euler(0, arcDegrees / 2, 0) * Vector3.forward * radius).x * 2;
            }
        }
    }

    public struct ShapeData
    {
        public List<ColliderInfo> colliders;
        public List<IndicatorDisplay> indicators;
        public VFXInfo vfx;
        public EffectiveDistance effective;
    }

    public static RangeForShape usesRangeForHitbox(HitType typeOfHit)
    {

        if(typeOfHit == HitType.Attached)
        {
            return RangeForShape.RangeAndRadius;
        }
        else if (typeOfHit == HitType.DamageDash)
        {
            return RangeForShape.RadiusOnly;
        }
        else
        {
            return RangeForShape.None;
        }
    }

    public enum RangeForShape
    {
        None,
        RadiusOnly,
        RangeAndRadius
    }

    public static ShapeData getShapeData(EffectShape shape, CapsuleSize sizeC, float range, float length, float width, RangeForShape rangeUse)
    {
        ShapeData data = new ShapeData();
        data.colliders = new List<ColliderInfo>();
        data.indicators = new List<IndicatorDisplay>();
        float rangeMult = rangeUse == RangeForShape.RangeAndRadius ? 1 : 0;
        float radiusMult = rangeUse >= RangeForShape.RadiusOnly ? 1 : 0;
        Vector3 frontPoint = Vector3.forward * sizeC.radius * radiusMult;
        float subtractRadius;
        float subtractPercent;
        float barWidth;
        switch (shape)
        {
            case EffectShape.Centered:
                subtractRadius = range* rangeMult + sizeC.radius* radiusMult;
                float fullRadius = width / 2 + subtractRadius;
                barWidth = fullRadius * 0.08f;
                #region colliders
                data.colliders.Add(new CapsuleInfo
                {
                    position = Vector3.zero + Vector3.forward * length / 2,
                    rotation = Quaternion.identity,
                    capsuleDir = CapsuleColliderDirection.Z,
                    radius = fullRadius,
                    height = fullRadius + length,
                });
                if (subtractRadius > 0)
                {
                    data.colliders.Add(new SphereInfo
                    {
                        position = Vector3.zero,
                        rotation = Quaternion.identity,
                        subtract = true,
                        radius = subtractRadius,
                    });
                }              
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
                    position = Vector3.forward * length / 2 + Vector3.right * (fullRadius - barWidth / 2),
                    rotation = Quaternion.identity,
                    scale = new Vector3(barWidth, length),
                    shape = IndicatorShape.BoxFull,
                    settings = new IndicatorShaderSettings
                    {
                        circle = 2,
                    },
                });
                data.indicators.Add(new IndicatorDisplay
                {
                    position = Vector3.forward * length / 2 + Vector3.left * (fullRadius - barWidth / 2),
                    rotation = Quaternion.identity,
                    scale = new Vector3(barWidth, length),
                    shape = IndicatorShape.BoxFull,
                    settings = new IndicatorShaderSettings
                    {
                        circle = 2,
                    },
                });
                float lengthPercent = length / fullRadius;
                data.indicators.Add(new IndicatorDisplay
                {
                    position = Vector3.back * fullRadius + Vector3.forward * length,
                    rotation = Quaternion.identity,
                    scale = new Vector3(fullRadius * 2, fullRadius * 2),
                    shape = IndicatorShape.BoxFull,
                    settings = new IndicatorShaderSettings
                    {
                        sideways = subtractPercent,
                        circleSubtract = subtractPercent,
                        forward = 2 - lengthPercent,
                        subtractOffset = -1 + lengthPercent,
                        circle = 2,
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
                data.effective = new EffectiveDistance()
                {
                    height = fullRadius * 2,
                    minDistance = subtractRadius,
                    maxDistance = fullRadius,
                    width = width,
                };
                break;
            case EffectShape.Slash:
                #region colliders
                float slashWidth = width;
                subtractRadius = range * rangeMult;
                float slashLength = length + subtractRadius;
                float halfCircum = Mathf.PI * slashLength;
                if (slashWidth > halfCircum)
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
                Vector3 boxSize = new Vector3(boxWidth, sizeC.distance * 1.2f, slashLength);
                Vector3 boxOffset = boxSize / 2;

                barWidth = slashLength * 0.05f;


                data.colliders.Add(new SphereInfo
                {
                    position = frontPoint,
                    rotation = Quaternion.identity,
                    radius = slashLength,
                });
                if (subtractRadius > 0)
                {
                    data.colliders.Add(new SphereInfo
                    {
                        position = frontPoint,
                        rotation = Quaternion.identity,
                        radius = subtractRadius,
                        subtract = true,
                    });
                }
                

                data.colliders.Add(new BoxInfo
                {
                    position = frontPoint + rightRotation * new Vector3(-boxOffset.x, 0, boxOffset.z),
                    rotation = rightRotation,
                    size = boxSize,
                });
                data.colliders.Add(new BoxInfo
                {
                    position = frontPoint + leftRotation * new Vector3(boxOffset.x, 0, boxOffset.z),
                    rotation = leftRotation,
                    size = boxSize,
                });
                #endregion

                #region indicators
                subtractPercent = subtractRadius / slashLength;
                data.indicators.Add(new IndicatorDisplay
                {
                    position = frontPoint,
                    rotation = Quaternion.identity,
                    scale = Vector3.one * slashLength * 2,
                    shape = IndicatorShape.Circle,
                    settings = new IndicatorShaderSettings
                    {
                        angle = arcHalfDegrees * 2,
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
                    scale = new Vector3(barWidth, slashLength),
                    shape = IndicatorShape.BoxFull,
                    settings = new IndicatorShaderSettings
                    {
                        circleSubtract = subtractPercent * 2,
                        subtractOffset = 1,
                        circle = 2,

                    },
                });
                data.indicators.Add(new IndicatorDisplay
                {
                    position = frontPoint + leftRotation * new Vector3(0, 0, boxOffset.z),
                    rotation = leftRotation,
                    scale = new Vector3(barWidth, slashLength),
                    shape = IndicatorShape.BoxFull,
                    settings = new IndicatorShaderSettings
                    {
                        circleSubtract = subtractPercent * 2,
                        subtractOffset = 1,
                        circle = 2,
                    },
                });
                #endregion

                data.vfx = new VFXArcInfo()
                {
                    radius = slashLength,
                    subtractRadius = subtractRadius,
                    arcDegrees = arcHalfDegrees * 2,
                    height = boxSize.y,
                };
                data.effective = new EffectiveDistance()
                {
                    height = boxSize.y,
                    minDistance = subtractRadius,
                    maxDistance = slashLength,
                    widthAngle = arcHalfDegrees *2,
                };
                break;
            case EffectShape.Overhead:
                subtractRadius = range * rangeMult;
                Vector3 outerVec = new Vector3(width / 2, 0, length + subtractRadius);
                float outerRadius = outerVec.magnitude;
                barWidth = outerRadius * 0.1f;
                #region colliders
                data.colliders.Add(new SphereInfo
                {
                    position = frontPoint,
                    rotation = Quaternion.identity,
                    radius = outerRadius,
                });
                if (subtractRadius > 0)
                {
                    data.colliders.Add(new SphereInfo
                    {
                        position = frontPoint,
                        rotation = Quaternion.identity,
                        radius = subtractRadius,
                        subtract = true,
                    });
                }
                
                data.colliders.Add(new BoxInfo
                {
                    position = frontPoint + Vector3.forward * (subtractRadius + length) / 2,
                    rotation = Quaternion.identity,
                    size = new Vector3(width, outerRadius * 2, outerRadius),
                });
                #endregion

                #region indicators
                subtractPercent = subtractRadius / outerRadius;
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
                        sideways = sidePercent,
                        circleSubtract = subtractPercent,
                        circle = subtractPercent,
                        forward = 1 + subtractPercent,
                    },
                    progress = IndicatorProgressElement.Circle,
                });
                Vector3 boxHalfs = outerVec;
                boxHalfs.z -= subtractRadius;
                boxHalfs.z /= 2;
                data.indicators.Add(new IndicatorDisplay
                {
                    position = frontPoint + Vector3.forward * outerVec.z + Vector3.back * boxHalfs.z,
                    rotation = Quaternion.identity,
                    scale = new Vector3(boxHalfs.x, boxHalfs.z) * 2,
                    shape = IndicatorShape.Box,
                    settings = new IndicatorShaderSettings
                    {
                        circle = 2,
                    }
                });
                #endregion

                data.vfx = new VFXArcInfo()
                {
                    radius = outerRadius,
                    subtractRadius = subtractRadius,
                    rollDegrees = 90,
                    arcDegrees = 180,
                    height = width,
                };
                data.effective = new EffectiveDistance()
                {
                    height = outerRadius * 2,
                    minDistance = subtractRadius,
                    maxDistance = outerRadius,
                    width = width,
                };
                break;
        }

        return data;
    }

    public static List<GameObject> ShapeAttack(SpellSource source, ShapeData data)
    {
        return ShapeAttack(source.aimRotation(), source.transform.position, data);
    }

    public static List<GameObject> ShapeAttack(Quaternion aim, Vector3 origin, ShapeData data)
    {
        List<Collider> hits = null;
        List<Collider> negativeHits = new List<Collider>();

        foreach (ColliderInfo info in data.colliders)
        {
            Collider[] singleHits;
            Vector3 totalPosition = origin + aim * info.position;
            Quaternion totalRotation = aim * info.rotation;
            switch (info)
            {
                case BoxInfo box:
                    singleHits = Physics.OverlapBox(totalPosition, box.size / 2, totalRotation, LayerMask.GetMask("Players", "Breakables"));
                    if (box.subtract)
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
                case SphereInfo sphere:
                    singleHits = Physics.OverlapSphere(totalPosition, sphere.radius, LayerMask.GetMask("Players", "Breakables"));
                    if (sphere.subtract)
                    {
                        foreach (Collider c in singleHits)
                        {
                            if (c is CapsuleCollider)
                            {
                                CapsuleCollider cap = (CapsuleCollider)c;
                                CapsulePoints pointsCheck = fromCollider(cap).getPoints(Vector3.zero, Quaternion.identity);
                                if (pointsCheck.furthestDistance(totalPosition) + cap.radius < sphere.radius)
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
                    CapsulePoints points = capsule.getPoints(origin, aim);
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

        foreach (Collider c in negativeHits)
        {
            hits.Remove(c);
        }

        return hits.Select(c => c.gameObject).ToList();
    }


    public static float GroundRadius(float length, float width)
    {
        return (length + width) / 2;
    }


    public static void ShapeParticle(SpellSource source, ShapeData data, EffectShape shape, HitFlair flair, AudioDistances dists, Transform persistParent = null)
    {
        ShapeParticle(source.aimRotation(), source.transform.position, source.transform.forward, data, shape, flair, dists, persistParent);
    }

    public static void ShapeParticle(Quaternion aim, Vector3 position, Vector3 forward, ShapeData data, EffectShape shape, HitFlair flair, AudioDistances dists, Transform persistParent = null)
    {
        GlobalPrefab gp = GlobalPrefab.gPre;
        GameObject prefab = gp.ParticlePre;
        GameObject i = null;
        switch (data.vfx)
        {
            case VFXArcInfo arc:
                i = GameObject.Instantiate(prefab, position + forward * arc.effectCenterDist, aim);
                Vector3 scale = shape switch
                {
                    EffectShape.Overhead => new Vector3(arc.height,arc.radius*2,arc.radius),
                    _ => new Vector3(arc.arcWidth,arc.height,arc.radius)
                };
                i.transform.localScale = scale;
                i.GetComponent<Particle>().setVisuals(persistParent ? Particle.VisualType.Projectile : Particle.VisualType.Line, flair.visualIndex, dists);
                break;
            case VFXCapsuleInfo cap:
                i = GameObject.Instantiate(prefab, position + forward * cap.additionalLength/2, aim);
                i.transform.localScale = new Vector3(cap.radius*2,cap.radius * 2, cap.radius * 2 + cap.additionalLength);
                i.GetComponent<Particle>().setVisuals(persistParent ? Particle.VisualType.FullCircle : Particle.VisualType.Circle, flair.visualIndex, dists);
                break;
        }
        if (persistParent)
        {
            i.transform.parent = persistParent;
            i.GetComponent<ClientAdoption>().parent = persistParent.gameObject;
        }
        NetworkServer.Spawn(i);

    }

    public class HitList
    {
        public List<GameObject> alreadyHit = new List<GameObject>();

        public bool tryAddHit(GameObject o)
        {
            if (alreadyHit.Contains(o))
            {
                return false;
            }
            alreadyHit.Add(o);
            return true;
        }
    }
}
