using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    AttackBlock attackFormat;
    GameObject rotatingBody;
    // Start is called before the first frame update
    void Start()
    {
        rotatingBody = transform.parent.GetComponentInChildren<UnitRotation>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public List<AttackState> cast()
	{
        return attackFormat.buildStates(this);
    }
    public void setFormat(AttackBlock b)
    {
        attackFormat = b;
    }
    public GameObject getSpawnBody()
    {
        return rotatingBody;
    }

}
