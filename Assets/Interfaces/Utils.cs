using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utils 
{
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
	public static Vector3 positiveVector(Vector3 v)
	{
		return new Vector3(Mathf.Max(0, v.x), Mathf.Max(0, v.y), Mathf.Max(0, v.z));
	}

    public static float GaussRandom(float min, float max, int balance=2)
    {
        float total = 0;
        for(int i = 0; i < balance; i++)
        {
            total += Random.value;
        }
        float value = total / balance;
        return min + (max- min) * value;

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

