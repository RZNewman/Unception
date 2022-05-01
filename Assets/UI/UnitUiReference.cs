using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitUiReference : MonoBehaviour
{
    public UiBar healthbar;
    public UiBar staggerbar;

    private void Start()
    {
        healthbar.source = GetComponentInParent<Health>();
        staggerbar.source = GetComponentInParent<Posture>();
    }
}
