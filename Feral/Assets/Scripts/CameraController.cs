using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float yawSensitivity = 2.0f;
    public float pitchSensitivity = 2.0f;
    public float zoomSensitivity = 10.0f;
    public float collisionLerpSmooth = 10.0f;
    public float collisionYOffset = 3.0f;

    public Transform playerTransform;
    public float cameraHeightOffset;
    public float cameraDistance;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    private float desiredZoom = 15.0f;
    private float zoom = 15.0f;
    private float currentZoomSpeed = 0.0f;
    private const float minZoom = 0.5f;
    private const float maxZoom = 25.0f;
    private const float maxRayDistance = 5000.0f;

    private const int layerMask = ~(1 << 6); // Ignore all gameobjects that have layer mask 6 

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Don't show cursor
    }
     
    // Update is called once per frame
    void LateUpdate()
    {
        if (PauseMenu.isPaused) return;

        Rotate();
        Zoom();

        TryResolveInsideMesh();
    }

    private void Rotate()
    {
        yaw += yawSensitivity * Input.GetAxis("Mouse X");
        pitch += yawSensitivity * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -89.0f, 89.0f);

        Quaternion newRotation = Quaternion.Euler(-pitch, yaw, 0.0f);

        Vector3 cameraOffset = -(newRotation * Vector3.forward * cameraDistance);
        cameraOffset.y += cameraHeightOffset;

        transform.localRotation = newRotation;
    }

    private void Zoom()
    {
        currentZoomSpeed -= Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity;

        currentZoomSpeed *= Mathf.Pow(0.01f, Time.deltaTime); // Apply damping to zoom speed
        if(Mathf.Abs(currentZoomSpeed) < 0.005f) // Stop zooming
        {
            currentZoomSpeed = 0.0f;
        }

        if(currentZoomSpeed != 0.0f)
        {
            desiredZoom += currentZoomSpeed * Time.deltaTime;
            desiredZoom = Mathf.Clamp(desiredZoom, minZoom, maxZoom);
        }

        zoom = desiredZoom;
    }

    private void TryResolveInsideMesh()
    {
        Vector3 direction = transform.localRotation * Vector3.forward;
        Vector3 desiredCameraPos = playerTransform.position + (-direction * desiredZoom) + new Vector3(0.0f, cameraHeightOffset, 0.0f);

        // Cast a ray from our camera's position to our player's position to see if we've collided with any objects
        RaycastHit hit;

        Vector3 rayOrigin = playerTransform.position;
        rayOrigin.y += collisionYOffset;
        Vector3 rayDirection = desiredCameraPos - rayOrigin;
        float rayDistance = rayDirection.magnitude;
        rayDirection.Normalize();

        Ray ray = new Ray(rayOrigin, rayDirection);

        if (Physics.Raycast(ray, out hit, rayDistance, layerMask))  // We hit something on the way back, there is something between us and the character
        {
            float collisionDistance = hit.distance; // Cache the distance between the character and the initial collision

            // Since raycasts don't detect back faces, we now need to do another cast from the camera to the character. If we collide with nothing, then we are inside of a mesh.
            rayDirection = rayOrigin - desiredCameraPos; // Reverse the ray direction
            rayDirection.Normalize();

            ray = new Ray(desiredCameraPos, rayDirection); // Re-assign our ray to reflect the new cast

            if (!Physics.Raycast(ray, out hit, rayDistance, layerMask)) // We collided with nothing, we are inside a mesh!
            {
                transform.localPosition = rayOrigin + (-direction * (collisionDistance * 0.9f));
                return;
            }     
        }

        transform.localPosition = desiredCameraPos;
    }
}
