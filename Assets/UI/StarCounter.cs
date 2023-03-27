using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarCounter : MonoBehaviour
{
    public GameObject starPre;

    public void setStars(int stars)
    {
        Color color = Color.Lerp(Color.white, Color.red, stars / 5f);
        for (int i = 0; i < stars; i++)
        {
            GameObject o = Instantiate(starPre, transform);
            o.GetComponent<Image>().color = color;
        }
    }
}
