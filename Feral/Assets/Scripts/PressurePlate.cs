using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public string keyString;
    public Triggerable obj;

    void Update()
    {

    }

    void OnTriggerEnter(Collider c)
    {
        if(c.tag == keyString)
        {
            obj.Activate();
        } 
    }

    void OnTriggerExit(Collider c)
    {
        if (c.tag == keyString)
        {
            obj.Deactivate();
        }
    }
}
