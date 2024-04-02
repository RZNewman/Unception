using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorLock : MonoBehaviour
{
    public float xOffset = 0;
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
        Vector2 tempPos = transform.localPosition;
        float signedXOffset = xOffset;
        
        

        if (flip)
        {
            Vector2 tempPivot = myRect.pivot;
            if(Input.mousePosition.x > Screen.width / 2)
            {
                tempPivot.x = 1;
                signedXOffset *= -1;
            }
            else
            {
                tempPivot.x = 0;
            }
            myRect.pivot = tempPivot;
        }

        if (syncX) { tempPos.x = Input.mousePosition.x + signedXOffset; }
        if (syncY) { tempPos.y = Input.mousePosition.y; }
        transform.localPosition = tempPos;
    }
}
