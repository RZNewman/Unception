using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GroveObject;

public class UIGroveTray : MonoBehaviour
{
    public GameObject GroveIconPre;

    //List<GroveShape> shapes = new List<GroveShape>();

    private void Start()
    {
        for(int i = 0; i < 10; i++)
        {
            UIGroveObject icon =Instantiate(GroveIconPre, transform).GetComponent<UIGroveObject>();

            GroveShape shape = GroveShape.shape();
            //shapes.Add(shape);

            icon.assignShape(shape);
        }
    }

    public void returnShape(GroveShape shape)
    {
        UIGroveObject icon = Instantiate(GroveIconPre, transform).GetComponent<UIGroveObject>();
        icon.assignShape(shape);
    }
}
