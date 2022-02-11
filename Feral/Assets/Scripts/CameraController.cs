using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float yawSensitivity = 2.0f;
    public float pitchSensitivity = 2.0f;

    public Transform playerTransform;
    public float cameraHeightOffset;
    public float cameraDistance;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Don't show cursor
    }

    // Update is called once per frame
    void LateUpdate()
    {
        yaw += yawSensitivity * Input.GetAxis("Mouse X");
        pitch += yawSensitivity * Input.GetAxis("Mouse Y");

        Quaternion newRotation = Quaternion.Euler(-pitch, yaw, 0.0f);
 
        Vector3 cameraOffset = -(newRotation * Vector3.forward * cameraDistance);
        cameraOffset.y += cameraHeightOffset;

        transform.localRotation = newRotation;
        transform.position = playerTransform.position + cameraOffset;
    }
}
