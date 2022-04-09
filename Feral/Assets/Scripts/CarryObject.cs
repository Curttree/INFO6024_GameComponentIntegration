using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarryObject : MonoBehaviour
{
    public GameObject held;
    public Vector3 positionCheckOffset;
    public float pickupDistance;
    public Vector3 heldObjectOffset;
    public Transform cameraTransform;
    bool throwing = false;
    float throwingForce = INITAL_THROW_FORCE;

    const float INITAL_THROW_FORCE = 200.0f;
    const float MAX_THROW_FORCE = 2000.0f;

    private GameObject outlinedObject = null;

    public Text interactText;

    private const int layerMask = ~(1 << 2); // Ignore all gameobjects that have layer mask 2

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TryHighlightMesh();

        if (!PauseMenu.isPaused)
        {
            if (Input.GetMouseButton(0) && !held)
            {
                Pickup();
            }
            else if (Input.GetMouseButtonDown(0) && held)
            {
                Drop();
            }
            else if (Input.GetMouseButton(1) && held)
            {
                throwing = true;
            }
            else if (throwing && Input.GetMouseButtonUp(1) && held)
            {
                Throw();
            }

            if (throwing)
            {
                throwingForce *= Mathf.Pow(3.0f, Time.deltaTime);
                throwingForce = Mathf.Clamp(throwingForce, INITAL_THROW_FORCE, MAX_THROW_FORCE);
            }
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
        throwing = false;
    }

    void Throw()
    {
        if(!held)
        {
            return;
        }

        held.GetComponent<Collider>().enabled = true;
        held.GetComponent<Rigidbody>().isKinematic = false;

        Rigidbody rb = held.GetComponent<Rigidbody>();
        Vector3 throwForce = cameraTransform.rotation * Vector3.forward * throwingForce;
        rb.AddForce(throwForce, ForceMode.Force); // Apply force to the rigidbody (ForceMode.Force means to "Add a continuous force to the rigidbody, using its mass." )

        held = null;
        throwingForce = INITAL_THROW_FORCE;
        throwing = false;
    }

    private void TryHighlightMesh()
    {
        Ray ray = new Ray(transform.position + positionCheckOffset, transform.forward);
        Debug.DrawRay(transform.position + positionCheckOffset, transform.forward * pickupDistance, Color.red);
        RaycastHit hit;

        GameObject hitObject = null;

        if (Physics.Raycast(ray, out hit, pickupDistance, layerMask)) // We hit something
        {
            if (hit.collider.gameObject.CompareTag("Object"))
            {
                hitObject = hit.collider.gameObject;
                OnLookAtInteractable(hitObject);
            }
        }

        bool hitNothing = hitObject == null && outlinedObject != null;
        if (hitNothing) // No longer looking at anything
        {
            OnUnlookInteractable(outlinedObject);
        }

        if(outlinedObject != null && hitObject != outlinedObject) // We are now looking at a different interactable, set the old interactable to not be glowing
        {
            outlinedObject.GetComponent<Outline>().enabled = false;
        }

        outlinedObject = hitObject;
    }

    private void OnLookAtInteractable(GameObject obj)
    {
        var outline = obj.GetComponent<Outline>(); // Add outline component to the object
        if (outline == null)
        {
            outline = obj.AddComponent<Outline>();
        }

        if (!outline.enabled)
        {
            outline.enabled = true;
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.OutlineColor = Color.green;
            outline.OutlineWidth = 4.0f;
        }

        interactText.text = "Press LMB to pickup";
    }

    private void OnUnlookInteractable(GameObject obj)
    {
        obj.GetComponent<Outline>().enabled = false;
        interactText.text = "";
    }
}
