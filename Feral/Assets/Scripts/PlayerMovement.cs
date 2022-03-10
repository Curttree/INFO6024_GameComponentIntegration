using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Animator anim;
    public Transform cam;
    public Rigidbody rb;
    public float walkSpeed = 16.0f;
    public float runSpeed = 26.0f;
    private float terminalVelocity = 50f;

    public float gravity = -25f;
    public Vector3 velocity;
    public Vector3 previousPosition;

    public float jumpHeight = 1.5f;
    public float walkJumpHeight = 3f;
    public float maxTurnSpeed;

    private bool shouldWalk = true;
    private bool isJumping = false;
    private bool isFalling = false;
    public bool isGrounded = true;

    private bool apexOfJump = false;
    private bool attemptedEdgeCorrection = false;
    private float edgeCorrectMaxTime = 0.5f;
    private float edgeTimer = 0f;

    private bool onWall = false;
    private bool shouldClimb = true;
    private float climbMaxTime = 0.1f;
    private float climbTimer = 0f;

    private bool hasDoubleJump = true;

    private void Start()
    {
        controller.enableOverlapRecovery = true;
    }

    private void LateUpdate()
    {
        CheckGround();
        EdgeCleanup();
    }
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
        anim.SetBool("IsJumping", isJumping);
        anim.SetBool("IsFalling", isFalling);
        anim.SetBool("IsGrounded", isGrounded);

        if (direction.magnitude > 0)
        {
            MoveInDirection(direction);
        }
        if (controller.isGrounded && isGrounded && !onWall)
        {
            velocity.y = 0.0f;
            if (isJumping)
            {
                isJumping = false;
            }
            if (isFalling)
            {
                isFalling = false;
            }
            if (shouldClimb)
            {
                shouldClimb = false;
            }
            if (!hasDoubleJump)
            {
                hasDoubleJump = true;
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
            if (velocity.y < -terminalVelocity)
            {
                velocity.y = -terminalVelocity;
            }
        }

        if (onWall)
        {
            //TODO: Check to see if still touching the wall (if walljump timer is large enough)

            climbTimer += Time.deltaTime;
            if (climbTimer >= climbMaxTime)
            {
                onWall = false;
                gravity = -25f;
            }
        }

        if (!isJumping && isGrounded && Input.GetButtonDown("Jump"))
        {
            isJumping = true;
            shouldClimb = true;
            velocity.y = Mathf.Sqrt(-2.0f * (shouldWalk ? walkJumpHeight : jumpHeight) * gravity);
        }
        else if (hasDoubleJump && Input.GetButtonDown("Jump"))
        {
            isJumping = true;
            shouldClimb = true;
            hasDoubleJump = false;
            edgeTimer = 0f;
            velocity.y = Mathf.Sqrt(-2.0f * (shouldWalk ? walkJumpHeight : jumpHeight) * gravity * 0.25f);
        }
        if (isJumping && velocity.y < 0f)
        {
            isFalling = true;
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

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.tag == "Wall")
        {
            hasDoubleJump = true;
            isJumping = false;
            isFalling = false;
            onWall = true;
            gravity = -10f;
            // Zero out for now, eventually check if they have the jump button held and therefore should climb
            velocity = Vector3.zero;
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

    private void CheckGround()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        Debug.DrawRay(transform.position, Vector3.down * 1.0f, Color.green);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1.0f))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void EdgeCleanup()
    {
        previousPosition = transform.position;
        if (apexOfJump)
        {
            //Coroutine has already been triggered. Let it handle the cleanup.
            return;
        }
        if (!isGrounded && (transform.position - previousPosition).magnitude < 0.01f)
        {
            apexOfJump = true;
            StartCoroutine(Unstick());
        }
    }
    private IEnumerator Unstick()
    {
        yield return new WaitForSeconds(1.1f);
        while (apexOfJump)
        {
            CheckGround();
            edgeTimer += 0.01f;
            if (!attemptedEdgeCorrection && (transform.position - previousPosition).magnitude > 0.01f || (attemptedEdgeCorrection && isGrounded))
            {
                edgeTimer = 0f;
                apexOfJump = false;
                StopCoroutine(Unstick());
            }
            else
            {
                if (!attemptedEdgeCorrection)
                {
                    attemptedEdgeCorrection = true;
                }
                if (edgeTimer <= edgeCorrectMaxTime && shouldClimb)
                {
                    //They could just be trying to climb up the edge of a platform. Help them up.
                    transform.position += transform.forward * 0.1f;
                    transform.position += transform.up * 0.1f;
                }
                else
                {
                    //They are trying to climb up a surface. Eventually check to see if climbing requirements are met, but for now just push them off.
                    transform.position -= transform.forward * 0.15f;
                    transform.position -= transform.up * 0.15f;
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
    }
}
