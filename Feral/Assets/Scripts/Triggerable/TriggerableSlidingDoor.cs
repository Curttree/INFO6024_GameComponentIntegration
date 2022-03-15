using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerableSlidingDoor : Triggerable
{
    private float minY;
    private float maxY;

    void Start()
    {
        minY = gObj.transform.position.y;
        maxY = minY + 15;
    }

    // Update is called once per frame
    void Update()
    {
        if(active)
        {
            if (gObj.transform.position.y < maxY)
            {
                gObj.transform.position = gObj.transform.position + new Vector3(0, 3, 0) * Time.deltaTime;
            }
        }
        else
        {
            if(gObj.transform.position.y > minY)
            {
                gObj.transform.position = gObj.transform.position + new Vector3(0, -3, 0) * Time.deltaTime;
            }
        }
    }
}
