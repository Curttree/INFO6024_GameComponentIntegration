using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Animator anim;
    public Transform cam;
    public Rigidbody rb;
    public float walkSpeed = 4.0f;
    public float runSpeed = 12.0f;

    public float gravity = -9.81f;
    public Vector3 velocity;

    public float jumpHeight = 3.0f;
    public float maxTurnSpeed;

    public bool shouldWalk = true;

    // Update is called once per frame
    void Update()
    {
        CheckRun();

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(x, 0, z).normalized;

        //Set animation values
        anim.SetFloat("Velocity", direction.magnitude);
        anim.SetBool("ShouldWalk", shouldWalk);

        if (direction.magnitude > 0)
        {
            MoveInDirection(direction);
        }
        if (controller.isGrounded)
        {
            velocity.y = 0.0f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(-2.0f * jumpHeight * gravity);
        }
        controller.Move(velocity * Time.deltaTime);
    }

    public void MoveInDirection(Vector3 direction)
    {
        direction.Normalize();
        /*If we have a non-zero direction then look towards that direction, otherwise do noting*/
        if (direction.sqrMagnitude > 0.001f)
        {
            /*Determine how the agent should rotate to face the direction on the Y axis*/
            /*Atan2 returns the angle in radians whose tan is direction.z/direction.x */
            float rotY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            /*Determines the amount to turn from it's current rotation to it's desired rotation based on max turn speed */
            float rotationAmount = Mathf.LerpAngle(transform.rotation.eulerAngles.y, rotY, Time.deltaTime * maxTurnSpeed);
            /*Converts angle to Quaterion and applies rotation to agent*/
            transform.rotation = Quaternion.Euler(0, rotationAmount, 0);

            Vector3 moveDir = Quaternion.Euler(0f, rotY, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * (shouldWalk ? walkSpeed : runSpeed) * Time.deltaTime);
        }
    }

    private void CheckRun()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            shouldWalk = false;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            shouldWalk = true;
        }
    }
}
