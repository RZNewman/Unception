using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorLock : MonoBehaviour
{

    public bool syncX;
    public bool syncY;
    public bool flip;

    RectTransform myRect;

    private void Start()
    {
        myRect = GetComponent<RectTransform>();
    }
    // Update is called once per frame
    void Update()
    {
        Vector2 tempPos = transform.position;
        if (syncX) { tempPos.x = Input.mousePosition.x; }
        if (syncY) { tempPos.y = Input.mousePosition.y; }
        transform.position = tempPos;

        if (flip)
        {
            Vector2 tempPivot = myRect.pivot;
            if(Input.mousePosition.x > Screen.width / 2)
            {
                tempPivot.x = 1;
            }
            else
            {
                tempPivot.x = 0;
            }
            myRect.pivot = tempPivot;
        }
    }
}
