using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    public float rotationSpeed = 45;
    public SpinnerAxis axis;

    public enum SpinnerAxis
    {
        Forward,
        Up,
        Right,
    }
    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, rotationAxis()) * transform.rotation;
    }
    Vector3 rotationAxis()
    {
        switch (axis)
        {
            case SpinnerAxis.Forward:
                return transform.forward;
            case SpinnerAxis.Right:
                return transform.right;
            case SpinnerAxis.Up:
            default:
                return transform.up;
        }
    }
}
