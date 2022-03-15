using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Represents an object that can be triggered on button activation
public class Triggerable : MonoBehaviour
{
    public GameObject gObj;
    
    protected bool active = false;

    public void Activate()
    {
        active = true;
    }

    public void Deactivate()
    {
        active = false;
    }
}
