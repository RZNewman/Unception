using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils 
{
    public static Vector3 input2vec(Vector2 move)
	{
		return new Vector3(move.x, 0, move.y);
	}
}
