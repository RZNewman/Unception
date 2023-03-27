
using Firebase.Database;
using Priority_Queue;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using static MapGenerator;
using static RewardManager;

public static class Utils
{
    public static IEnumerable<T> EnumValues<T>()
    {
        return System.Enum.GetValues(typeof(T)).Cast<T>();
    }

    public static Dictionary<T, float> asEnum<T>(this Dictionary<string, float> export)
    {
        if (export == null) { return new Dictionary<T, float>(); }
        return export.ToDictionary(p => (T)System.Enum.Parse(typeof(T), p.Key), p => p.Value);
    }
    public static Vector3 input2vec(Vector2 inp)
    {
        return new Vector3(inp.x, 0, inp.y);
    }
    public static Vector2 vec2input(Vector3 world)
    {
        return new Vector2(world.x, world.z);
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
    public static string asPercent(this float value)
    {
        return Mathf.Round(value * 1000) / 10 + "%";
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

    public static T RandomItemWeighted<T>(this SimplePriorityQueue<T> list, float weight = 2f)
    {
        int index = Mathf.FloorToInt(Mathf.Pow(Random.value, 1 / weight) * list.Count);
        if (index == list.Count)
        {
            index--;
        }
        return list.Skip(index).First();
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
    public static void DrawBox(Vector3 pos, Quaternion rot, Vector3 scale, Color c)
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

        Debug.DrawLine(point1, point2, c);
        Debug.DrawLine(point2, point3, c);
        Debug.DrawLine(point3, point4, c);
        Debug.DrawLine(point4, point1, c);

        Debug.DrawLine(point5, point6, c);
        Debug.DrawLine(point6, point7, c);
        Debug.DrawLine(point7, point8, c);
        Debug.DrawLine(point8, point5, c);

        Debug.DrawLine(point1, point5, c);
        Debug.DrawLine(point2, point6, c);
        Debug.DrawLine(point3, point7, c);
        Debug.DrawLine(point4, point8, c);

        //// optional axis display
        //Debug.DrawRay(m.GetPosition(), m.GetForward(), Color.magenta);
        //Debug.DrawRay(m.GetPosition(), m.GetUp(), Color.yellow);
        //Debug.DrawRay(m.GetPosition(), m.GetRight(), Color.red);
    }
}

