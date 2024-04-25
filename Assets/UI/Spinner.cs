using System;
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
        transform.localRotation = Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, rotationAxis()) * transform.localRotation;
    }
    Vector3 rotationAxis()
    {
        switch (axis)
        {
            case SpinnerAxis.Forward:
                return Vector3.forward;
            case SpinnerAxis.Right:
                return Vector3.right;
            case SpinnerAxis.Up:
            default:
                return Vector3.up;
        }
    }

    internal void reset()
    {
        transform.localRotation = Quaternion.identity;
    }
}
