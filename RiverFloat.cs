using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverFloat : MonoBehaviour
{
   // public GameObject Water;
    public float waterLevel = 120;
    public float floatHeight = 120;
    public float bounceDamp = 0.05f;
    public Vector3 UnderwaterGravity;
    public Vector3 BouyancyCenterOffset;
    public GameObject FloatingPoint;
    public float Mass;

    private float forceFactor;
    private Vector3 actionPoint;
    private Vector3 upLift = -Physics.gravity;

    protected Rigidbody Rigidbody;
    protected BoxCollider CenterofMass;

    void Awake()
    {
        // Get Component
        Rigidbody = GetComponent<Rigidbody>();
        // waterLevel = Water.GetComponent<Transform>().position.y;
        Mass = GetComponent<Rigidbody>().mass + 2;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
            bool floating = FloatingPoint.GetComponent<WaterCollider>().floating;
            actionPoint = transform.position + transform.TransformDirection(BouyancyCenterOffset);
            forceFactor = 1f - ((actionPoint.y - waterLevel) / floatHeight);

            if (floating == true)
            {
                //Debug.Log("Weight: " + Mass);

                UnderwaterGravity.y = 2.5f * Mass;

                upLift = UnderwaterGravity * Mass * (forceFactor - Rigidbody.velocity.y * bounceDamp);
                Rigidbody.AddForceAtPosition(upLift, actionPoint);
            }
    }
}
