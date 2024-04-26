using Mirror;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    // Start is called before the first frame
    public float _gravity = -9.81f;
    public float gravity
    {
        get { return _gravity; }
        set { _gravity = value;
            setForce();
        }
    }
    UnitMovement movement;
    UnitUpdateOrder updater;
    Rigidbody rb;
    LifeManager lifeManager;
    Power power;

    ConstantForce force;
    float gravMult = 1;

    private void Awake()
    {
        
        force = gameObject.AddComponent<ConstantForce>();
        setForce();
    }

    void scaleGrav(Power p)
    {
        setForce();
    }

    public void turnOffGrav()
    {
        gravMult = 0;
    }

    void setForce()
    {
        float f = _gravity * gravMult;
        if (power) { f *= power.scaleSpeed(); }
        force.force = Vector3.up * f;
    }
    void Start()
    {
        movement = GetComponent<UnitMovement>();
        rb = GetComponent<Rigidbody>();
        lifeManager = GetComponent<LifeManager>();
        power = GetComponent<Power>();
        if (power)
        {
            power.subscribePower(scaleGrav);
        }
        updater = GetComponent<UnitUpdateOrder>();
        
    }

    // Update is called once per frame
    //void FixedUpdate()
    //{
    //    if (!updater)
    //    {
    //        rb.velocity += new Vector3(0, gravity, 0) * Time.fixedDeltaTime;
    //    }

    //}

    public void OrderedUpdate()
    {
        if (!lifeManager.IsDead && movement)
        {
            if(movement.grounded && gravMult > 0)
            {
                gravMult = 0;
                setForce();
            }
            else if(!movement.grounded && gravMult == 0)
            {
                gravMult = 1;
                setForce();
            }
        }

    }
}
