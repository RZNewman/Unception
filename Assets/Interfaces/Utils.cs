
using Firebase.Database;
using Mirror;
using Priority_Queue;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static MapGenerator;
using static RewardManager;

public static class Utils
{
    //So slow, please cache
    public static IEnumerable<T> EnumValues<T>()
    {
        return System.Enum.GetValues(typeof(T)).Cast<T>();
    }

    public static Dictionary<T, float> asEnum<T>(this Dictionary<string, float> export)
    {
        if (export == null) { return new Dictionary<T, float>(); }
        return export.ToDictionary(p => (T)System.Enum.Parse(typeof(T), p.Key), p => p.Value);
    }


    public enum Rotation
    {
        None,
        Quarter,
        Half,
        ThreeQuarters,

    }
    public static float degrees(this Rotation rot)
    {
        return rot switch
        {
            Rotation.Quarter => 90,
            Rotation.Half => 180,
            Rotation.ThreeQuarters => 270,
            _ => 0,
        };
    }
    public static Vector2Int rotateIntVec(this Rotation rot, Vector2Int vec)
    {
        return rot switch
        {
            Rotation.Quarter => new Vector2Int(vec.y, -vec.x),
            Rotation.Half => new Vector2Int(-vec.x, -vec.y),
            Rotation.ThreeQuarters => new Vector2Int(-vec.y, vec.x),
            _ => vec,
        };
    }
    public static Rotation rotate(this Rotation rot, int delta)
    {
        int rotations = 4;
        int r = (int)(rot) + delta;
        while (r < 0)
        {
            r += rotations;
        }
        return (Rotation)(r % rotations);
    }

    public static Vector3 input2vec(Vector2 inp)
    {
        return new Vector3(inp.x, 0, inp.y);
    }
    public static Vector2 vec2input(Vector3 world)
    {
        return new Vector2(world.x, world.z);
    }

    public static Vector2Int roundToInt(this Vector2 vec)
    {
        return new Vector2Int(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.y));
    }
    public static Vector2Int Abs(this Vector2Int vec)
    {
        return new Vector2Int(Mathf.Abs(vec.x), Mathf.Abs(vec.y));
    }
    public static Vector3 Abs(this Vector3 vec)
    {
        return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
    }

    public static void AddIfNotExists<T>(this HashSet<T> set, T item)
    {
        if (!set.Contains(item))
        {
            set.Add(item);
        }
    }

    public static Vector3 roundToInterval(this Vector3 vec, float interval)
    {
        return new Vector3(
            vec.x.roundToInterval(interval),
            vec.y.roundToInterval(interval),
            vec.z.roundToInterval(interval)
            );
    }
    public static float roundToInterval(this float number, float interval)
    {
        int count = Mathf.FloorToInt(number / interval);
        if(number - (count* interval) >= interval * 0.5f)
        {
            count++;
        }
        return count * interval;
    }

    public static float normalizeAngle(float angle)
    {
        if (angle > 180) angle -= 360;
        if (angle < -180) angle += 360;
        return angle;
    }
    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        float tx = v.x;
        float ty = v.y;

        return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
    }
    public static Vector3 positiveVector(Vector3 v)
    {
        return new Vector3(Mathf.Max(0, v.x), Mathf.Max(0, v.y), Mathf.Max(0, v.z));
    }

    public static float GaussRandomDecline(float balance = 2)
    {
        float value = Random.value;
        return Mathf.Pow(value, balance);
    }

    public static float GaussRandomCentered(float balance = 2)
    {
        float mult = Random.value < 0.5f ? -1 : 1;
        float value = GaussRandomDecline(balance) * mult;
        return (value + 1) / 2;
    }
    public static float asRange(this float value, float min, float max)
    {
        value = Mathf.Clamp01(value);
        return min + (max - min) * value;
    }
    public static double asRange(this double value, double min, double max)
    {
        value = Mathd.Clamp01(value);
        return min + (max - min) * value;
    }
    public static string asPercent(this float value)
    {
        return Mathf.Round(value * 1000) / 10 + "%";
    }

    public static void scaleToFit(this Image image)
    {
        Sprite s = image.sprite;
        float ratio = s.bounds.size.x / s.bounds.size.y;
        image.rectTransform.sizeDelta = new Vector2(image.rectTransform.sizeDelta.y * ratio, image.rectTransform.sizeDelta.y);
    }
    public static Dictionary<T, float> invert<T>(this IDictionary<T, float> dict)
    {
        Dictionary<T, float> newDict = new Dictionary<T, float>();
        foreach (T key in dict.Keys)
        {
            newDict[key] = -dict[key];
        }
        return newDict;
    }
    public static Dictionary<T, float> sum<T>(this IDictionary<T, float> dict1, IDictionary<T, float> dict2)
    {
        Dictionary<T, float> newDict = new Dictionary<T, float>();
        foreach (T key in dict1.Keys)
        {
            newDict[key] = dict1[key];
        }
        foreach (T key in dict2.Keys)
        {
            if (newDict.ContainsKey(key))
            {
                newDict[key] += dict2[key];
            }
            else
            {
                newDict[key] = dict2[key];
            }

        }
        return newDict;
    }

    public static Dictionary<T, int> invert<T>(this IDictionary<T, int> dict)
    {
        Dictionary<T, int> newDict = new Dictionary<T, int>();
        foreach (T key in dict.Keys)
        {
            newDict[key] = -dict[key];
        }
        return newDict;
    }
    public static Dictionary<T, int> sum<T>(this IDictionary<T, int> dict1, IDictionary<T, int> dict2)
    {
        Dictionary<T, int> newDict = new Dictionary<T, int>();
        foreach (T key in dict1.Keys)
        {
            newDict[key] = dict1[key];
        }
        foreach (T key in dict2.Keys)
        {
            if (newDict.ContainsKey(key))
            {
                newDict[key] += dict2[key];
            }
            else
            {
                newDict[key] = dict2[key];
            }

        }
        return newDict;
    }

    public static Dictionary<T, float> scale<T>(this IDictionary<T, float> dict, float scale, params T[] exclusions)
    {
        Dictionary<T, float> newDict = new Dictionary<T, float>();
        foreach (T key in dict.Keys)
        {
            newDict[key] = dict[key];
            if (!exclusions.Contains(key))
            {
                newDict[key] *= scale;
            }
        }
        return newDict;
    }
    public static float distance(this NavMeshPath path)
    {
        float distance = 0;
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            distance += (path.corners[i] - path.corners[i + 1]).magnitude;
        }
        return distance;
    }
    public static List<GameObject> ChildrenWithTag(this GameObject o, string tag)
    {
        List<GameObject> targets = new List<GameObject>();
        if (o.tag == tag)
        {
            targets.Add(o);
        }
        foreach (Transform t in o.transform)
        {
            targets.AddRange(t.gameObject.ChildrenWithTag(tag));
        }
        return targets;
    }
    public static void SortChildren(this Transform t, System.Func<Transform, System.IComparable> comp, bool descending = false)
    {


        List<(Transform, System.IComparable)> childrenValues = new List<(Transform, System.IComparable)>();
        foreach (Transform child in t)
        {
            childrenValues.Add((child, comp(child)));
        }
        if (descending)
        {
            childrenValues.Sort((a, b) => b.Item2.CompareTo(a.Item2));
        }
        else
        {
            childrenValues.Sort((a, b) => a.Item2.CompareTo(b.Item2));
        }
        for (int i = 0; i < childrenValues.Count; i++)
        {
            childrenValues[i].Item1.SetSiblingIndex(i);
        }

    }


    public struct FloatComponents
    {
        public bool negative;
        public int exponent;
        public float digits;
    }
    public static FloatComponents asComponents(this float value)
    {

        byte[] valueBytes = System.BitConverter.GetBytes(value);
        int bits = System.BitConverter.ToInt32(valueBytes, 0);
        int mantissa = bits & (0x7fffff | 1 << 30);
        float normalized = System.BitConverter.ToSingle(System.BitConverter.GetBytes(mantissa), 0) / 2;

        return new FloatComponents
        {
            negative = (bits & (1 << 31)) == 1,
            exponent = (bits >> 23) & 0xff - 128,
            digits = normalized,
        };
    }



    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public static T RandomItem<T>(this IList<T> list)
    {
        int index = Mathf.FloorToInt(Random.value * list.Count);
        if (index == list.Count)
        {
            index--;
        }
        return list[index];
    }
    public static int RandomIndex<T>(this IList<T> list)
    {
        int index = Mathf.FloorToInt(Random.value * list.Count);
        if (index == list.Count)
        {
            index--;
        }
        return index;
    }

    public struct Weighted<T>
    {
        public T item;
        public float weight;
    }
    public static List<T> RandomItemsWeighted<T>(this IList<T> list, float totalCost, System.Func<T, float> weightSelector, System.Func<T, float> costSelector)
    {
        List<Weighted<T>> weights = new List<Weighted<T>>();
        float totalWeight = 0;
        foreach (T item in list)
        {
            float weight = weightSelector(item);
            weights.Add(new Weighted<T>
            {
                item = item,
                weight = weight,
            });
            totalWeight += weight;
        }
        weights.Sort((w1, w2) => w1.weight.CompareTo(w2.weight));

        List<T> selections = new List<T>();
        float cost = 0;
        while (cost < totalCost)
        {
            float random = Random.value * totalWeight;
            T selection = weights.Last().item;
            int i = 0;
            while (random > 0 && i < weights.Count)
            {
                random -= weights[i].weight;
                if (random <= 0)
                {
                    selection = weights[i].item;
                    break;
                }
                i++;
            }
            selections.Add(selection);
            cost += costSelector(selection);
        }

        return selections;

    }


    public static T RandomItemWeighted<T>(this IEnumerable<T> list, System.Func<T, float> weightSelector)
    {
        float sum = 0;
        foreach(T item in list)
        {
            sum += weightSelector(item);
        }
        float value = Random.value * sum;
        foreach (T item in list)
        {
            value -= weightSelector(item);
            if(value <= 0)
            {
                return item;
            }
        }
        return list.First();
    }

    public struct Optional<T>
    {
        public bool HasValue { get; private set; }
        private T value;
        public T Value
        {
            get
            {
                if (HasValue)
                    return value;
                else
                    throw new System.InvalidOperationException();
            }
        }

        public Optional(T value)
        {
            this.value = value;
            HasValue = true;
        }

        public static explicit operator T(Optional<T> optional)
        {
            return optional.Value;
        }
        public static implicit operator Optional<T>(T value)
        {
            return new Optional<T>(value);
        }
        public bool Equals(Optional<T> other)
        {
            if (HasValue && other.HasValue)
                return object.Equals(value, other.value);
            else
                return HasValue == other.HasValue;
        }
    }
    public static void DrawBox(Vector3 pos, Quaternion rot, Vector3 scale, Color c, float time = 5f)
    {
        // create matrix
        Matrix4x4 m = new Matrix4x4();
        m.SetTRS(pos, rot, scale);

        var point1 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f));
        var point2 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f));
        var point3 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f));
        var point4 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f));

        var point5 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f));
        var point6 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f));
        var point7 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f));
        var point8 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f));

        Debug.DrawLine(point1, point2, c, time);
        Debug.DrawLine(point2, point3, c, time);
        Debug.DrawLine(point3, point4, c, time);
        Debug.DrawLine(point4, point1, c, time);

        Debug.DrawLine(point5, point6, c, time);
        Debug.DrawLine(point6, point7, c, time);
        Debug.DrawLine(point7, point8, c, time);
        Debug.DrawLine(point8, point5, c, time);

        Debug.DrawLine(point1, point5, c, time);
        Debug.DrawLine(point2, point6, c, time);
        Debug.DrawLine(point3, point7, c, time);
        Debug.DrawLine(point4, point8, c, time);

        //// optional axis display
        //Debug.DrawRay(m.GetPosition(), m.GetForward(), Color.magenta);
        //Debug.DrawRay(m.GetPosition(), m.GetUp(), Color.yellow);
        //Debug.DrawRay(m.GetPosition(), m.GetRight(), Color.red);
    }

    public static bool oneFlagSet(this ulong i)
    {
        //http://aggregate.org/MAGIC/#Is%20Power%20of%202
        return i > 0 && (i & (i - 1)) == 0;
    }

    public static Vector3Int asInt(this Vector3 v)
    {
        return new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
    }

    public static Vector3 asFloat(this Vector3Int v)
    {
        return v;
    }

    public static Vector3 scale(this Vector3 v, Vector3 scale)
    {
        v.Scale(scale);
        return v;
    }

    public static BoundsInt asInt(this Bounds b)
    {

        BoundsInt bb = new BoundsInt(b.min.asInt(), b.size.asInt());

        return bb;
    }
}

