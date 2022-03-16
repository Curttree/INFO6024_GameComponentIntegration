using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBoundaries : MonoBehaviour
{
    public CharacterController playerController;
    public Vector2 X_Boundaries;
    public Vector2 Y_Boundaries;
    public Vector2 Z_Boundaries;
    public Vector3 spawnPosition;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.transform.position.x < X_Boundaries.x || playerController.transform.position.x > X_Boundaries.y
            || playerController.transform.position.y < Y_Boundaries.x || playerController.transform.position.y > Y_Boundaries.y
            || playerController.transform.position.z < Z_Boundaries.x || playerController.transform.position.z > Z_Boundaries.y)
        {
            playerController.enabled = false;
            Debug.Log("Out of bounds. Returning to spawn.");
            playerController.transform.position = spawnPosition;
            playerController.enabled = true;
        }
    }
}
