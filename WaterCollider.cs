using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCollider : MonoBehaviour
{

    public bool floating = false;

    //Sets floating value on Enter
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "RiverSegmentA")
        {
            floating = true;
        }
    }

    //Sets floating value on Exit
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "RiverSegmentA")
        {
            floating = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
