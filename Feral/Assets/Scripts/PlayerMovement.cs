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
    private bool isJumping = true;
    private bool isFalling = false;
    public bool isGrounded = false;

    private bool apexOfJump = false;
    public bool alreadyAtApex = false;
    public bool bonked = false;
    private bool attemptedEdgeCorrection = false;
    public float climbMaxTime = 3.0f;
    public float climbTimer = 0f;

    public bool onWall = false;
    private GameObject lastWall;
    private bool shouldClimb = true;
    private float stickMaxTime = 0.25f;
    private float stickTimer = 0f;

    private bool hasDoubleJump = true;

    public string standingOn;

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
            if (lastWall)
            {
                lastWall = null;
            }
            if (alreadyAtApex)
            {
                alreadyAtApex = false;
            }
            if (bonked)
            {
                bonked = false;
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
            stickTimer += Time.deltaTime;
            if (stickTimer >= stickMaxTime)
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
            climbTimer = 0f;
            velocity.y = Mathf.Sqrt(-2.0f * (shouldWalk ? walkJumpHeight : jumpHeight) * gravity * 0.25f);
        }
        if (isJumping && velocity.y < 0f)
        {
            isFalling = true;
        }

        controller.Move(velocity * Time.deltaTime);
        velocity = Vector3.Scale(velocity, new Vector3(0.95f, 1.0f, 0.95f));
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
            if (hit.gameObject != lastWall)
            {
                hasDoubleJump = true;
            }
            bonked = true;
            isJumping = false;
            isFalling = false;
            onWall = true;
            gravity = -10f;

            // Zero out for now, eventually check if they have the jump button held and therefore should climb
            velocity = Vector3.zero;
            lastWall = hit.gameObject;
        }
        else if(hit.gameObject.tag == "Bouncy" && velocity.y < 0.0f)
        {
            float bounceForce = 50.0f + Mathf.Abs(velocity.y);
            velocity = velocity + Vector3.Scale(hit.normal, new Vector3(bounceForce, bounceForce, bounceForce));
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
            standingOn = hit.collider.tag;
        }
        else
        {
            isGrounded = false;
            standingOn = "";
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
        if (bonked && !isGrounded && (transform.position - previousPosition).magnitude < 0.01f && !alreadyAtApex)
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
            climbTimer += 0.01f;
            if (!attemptedEdgeCorrection && (transform.position - previousPosition).magnitude > 0.01f || (attemptedEdgeCorrection && isGrounded))
            {
                climbTimer = 0f;
                apexOfJump = false;
                alreadyAtApex = true;
                StopCoroutine(Unstick());
            }
            else
            {
                if (!attemptedEdgeCorrection)
                {
                    attemptedEdgeCorrection = true;
                }
                if (climbTimer <= climbMaxTime && shouldClimb)
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
