using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarryObject : MonoBehaviour
{
    public GameObject held;
    public Vector3 positionCheckOffset;
    public float pickupDistance;
    public Vector3 heldObjectOffset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) && !held)
        {
           Pickup();
        }
        else if (Input.GetMouseButtonUp(0) && held)
        {
            Drop();
        }
    }

    private void LateUpdate()
    {
        if (held)
        {
            held.transform.position = gameObject.transform.position +  (transform.right * heldObjectOffset.x) + (transform.up * heldObjectOffset.y) + (transform.forward * heldObjectOffset.z);
        }
    }

    void Pickup()
    {
        // Attempt to pickup a game object.
        Ray ray = new Ray(transform.position + positionCheckOffset, transform.forward);
        Debug.DrawRay(transform.position + positionCheckOffset, transform.forward * pickupDistance, Color.red);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupDistance))
        {
            if (hit.collider.gameObject.CompareTag("Object"))
            {
                held = hit.collider.gameObject;
                if (held.GetComponent<Collider>())
                {
                    held.GetComponent<Collider>().enabled = false;
                }
                if (held.GetComponent<Rigidbody>())
                {
                    held.GetComponent<Rigidbody>().isKinematic = true;
                }
            }
        }
    }

    // To be eventually replaced by throw.
    void Drop()
    {
        if (!held)
        {
            // We can't drop anything if we aren't holding it.
            return;
        }

        if (held.GetComponent<Collider>())
        {
            held.GetComponent<Collider>().enabled = true;
        }

        if (held.GetComponent<Rigidbody>())
        {
            held.GetComponent<Rigidbody>().isKinematic = false;
        }

        held = null;
    }
}
