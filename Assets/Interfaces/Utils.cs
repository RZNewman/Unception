using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils 
{
    public static Vector3 input2vec(Vector2 move)
	{
		return new Vector3(move.x, 0, move.y);
	}
	public static Vector2 vec2input(Vector3 world)
	{
		return new Vector2(world.x, world.z);
	}
}

