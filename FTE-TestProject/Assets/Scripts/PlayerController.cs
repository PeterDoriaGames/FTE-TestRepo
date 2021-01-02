using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* controls flow of states for player
 x movement, aiming, throwing, pulling, reel-in, swing

    Aim --
*/


public struct PlayerInputData
{
    public float XDir;
    public float ZDir;
    public bool JumpInput;
    public bool CancelFireInput;
    public bool FireInputDown;
    public bool ContinueAimingInput;
    public bool ThrowHookInput;
    public bool DisAttachHookInput;
    //public bool PauseReelInInput;
    public PlayerInputData(float xDir, float zDir, bool jump, bool cancelAiming, bool startAiming, bool continueAiming, bool throwHook, bool disAttachHook)
    {
        XDir = xDir;
        ZDir = zDir;
        JumpInput = jump;
        CancelFireInput = cancelAiming;
        FireInputDown = startAiming;
        ContinueAimingInput = continueAiming;
        ThrowHookInput = throwHook;
        DisAttachHookInput = disAttachHook;
    }
}
[SelectionBase]
public class PlayerController : MonoBehaviour
{

    public float maxWalkSpeed;
    public float walkAcceleration;
    public float walkDeaccelerationTime;
    public float maxAirSpeed;
    public float airAcceleration;
    public float airControlTime;
    public float startJumpForce;
    [Tooltip("Time player needs to be in air to be stunned when landing.")]
    public float stunningInAirTime;
    [Tooltip("Time until player can move again after they have landed on ground.")]
    public float stunnedLandingTime;
    public float additionalGravityScale = 2f;
    public float minGroundDist = 0.1f;
    public Hook myHook;

    [SerializeField]
    private bool NoControl = false;

    private RigidbodyConstraints GroundedConstraints;
    private float xSwingSpeed = 10;

    private Rigidbody MyRB;
    private CapsuleCollider MyCollider;
    // Determines how sloped ground can be
    private float MinGroundDotProduct = 0.65f;
    private float XMove = 0;
    private float ZMove = 0;
    private float YVel = 0;

    public bool isGrounded { get { return IsGrounded; } private set { IsGrounded = value; } }
    private bool IsGrounded = false;
    private bool IsSwinging = false;

    public PlayerInputData inputData { get { return InputData; } private set { InputData = value; } }
    private PlayerInputData InputData;
    
    void Awake()
    {
        MyRB = GetComponent<Rigidbody>();
        MyCollider = GetComponent<CapsuleCollider>();

        GroundedConstraints = RigidbodyConstraints.FreezeRotation ;
    }


    // Update is called once per frame
    void Update()
    {
        float xDir = Input.GetAxisRaw("Horizontal");
        float zDir = Input.GetAxisRaw("Vertical");
        bool jumpInput = Input.GetButtonDown("Jump");

        bool startAimingInput = Input.GetButtonDown("Fire1");
        bool continueAimingInput = Input.GetButton("Fire1");
        bool throwHookInput = Input.GetButtonUp("Fire1");

        bool cancelAimingInput = Input.GetButtonDown("Fire2") || isGrounded;
        bool disAttachHookInput = Input.GetButtonDown("Fire2");

        //  WHERE TO UPDATE VISUALS IN LOGIC?
        InputData = new PlayerInputData(xDir, zDir, jumpInput, startAimingInput, continueAimingInput, cancelAimingInput, throwHookInput, disAttachHookInput);
        if (inputData.JumpInput)
        {
            ShouldJump = true;
        }
    }

    private bool ShouldJump = false;
    private float XMoveWhenLeftGround = 0;
    private float ZMoveWhenLeftGround = 0;
    private float AirControlTimer = 0;
    private float InAirTimer = 0;
    private float StunnedLandingTimer;
    private float XVel = 0;
    private float ZVel = 0;
    void FixedUpdate()
    {
        CollisionChecks();

        if (AirControlTimer > 0)
        {
            AirControlTimer -= Time.fixedDeltaTime;
        }

        if (StunnedLandingTimer > 0)
        {
            StunnedLandingTimer -= Time.fixedDeltaTime;
            if (NoControl == false)
            {
                NoControl = true;
            }
        }
        else if (NoControl)
        {
            NoControl = false;
        }

        // NEED TO INTEGRATE PLAYER PHYSICS BASED ON HOOKMODE AND PLAYER STATE. 
        // PHYSICS ARE APPLIED HERE. HOOK TAKES PLAYER INPUT AND TELLS PLAYER WHAT IS HAPPENING. PLAYER TAKES THAT INFO AND APPLIES APPROPRIATE FORCES.


        // check hook mode to determine movement OR let hook tell you what to do???

        MyRB.AddForce(Physics.gravity * additionalGravityScale);

        if (NoControl == false)
        {
            if (InputData.JumpInput && isGrounded == false)
            {
                Debug.Log("try jump but not grounded");
            }

            // force-based movement. max vel's. Accleration value. Deacceleration value. 
            if (isGrounded)
            {
                // ground movement
                XMove = inputData.XDir * walkAcceleration * Time.fixedDeltaTime;
                ZMove = inputData.ZDir * walkAcceleration * Time.fixedDeltaTime;
                MyRB.AddForce(XMove, 0, ZMove);

                // start jump
                if (ShouldJump)
                {
                    MyRB.constraints = RigidbodyConstraints.FreezeRotation;
                    MyRB.AddForce(new Vector2(0, startJumpForce), ForceMode.Impulse);
                    ShouldJump = false;
                }
                // ground movement limiters
                else if (XMove != 0 || ZMove != 0)
                {
                    // clamping velocity to max walk speed
                    Vector2 WalkVelocity = new Vector2(MyRB.velocity.x, MyRB.velocity.z);
                    if (WalkVelocity.sqrMagnitude > maxWalkSpeed * maxWalkSpeed)
                    {
                        float XDot = Vector2.Dot(WalkVelocity, new Vector2(inputData.XDir, 0));
                        float ZDot = Vector2.Dot(WalkVelocity, new Vector2(0, inputData.ZDir));

                        MyRB.velocity = new Vector3(XDot * MyRB.velocity.x, 0, ZDot * MyRB.velocity.z);
                    }
                
                    // #TODO change this to add force to relative to camera angle if make sections that are not strictly 2D like the train car. 
                    float xVelAfterFriction = Mathf.SmoothDamp(MyRB.velocity.x, 0, ref XVel, walkDeaccelerationTime * (MyRB.velocity.x / maxWalkSpeed), float.MaxValue, Time.fixedDeltaTime);
                    float zVelAfterFriction = Mathf.SmoothDamp(MyRB.velocity.z, 0, ref ZVel, walkDeaccelerationTime * (MyRB.velocity.z / maxWalkSpeed), float.MaxValue, Time.fixedDeltaTime);
                    MyRB.velocity = new Vector3(xVelAfterFriction, MyRB.velocity.y, zVelAfterFriction);
                }
            }
            //  IN AIR
            else
            {
                InAirTimer += Time.fixedDeltaTime;


                if (myHook.attachedHookable)
                {
                    // check if in air or swinging SHOULD NOT BE STRAIGHT BOOL. SHOULD CHECK HOOKMODE
                    if (myHook.mode == HookMode.SteadyRope)
                    {
                        XMove = inputData.XDir * xSwingSpeed * Time.fixedDeltaTime;
                    }
                }
                else
                {
                    XMove = inputData.XDir * airAcceleration * (AirControlTimer / airControlTime) * Time.fixedDeltaTime;
                    ZMove = inputData.ZDir * airAcceleration * (AirControlTimer / airControlTime) * Time.fixedDeltaTime;
                    MyRB.AddForce(new Vector3(XMove, 0, ZMove));

                    // clamping velocity to max walk speed
                    Vector2 AirVelocity = new Vector2(MyRB.velocity.x, MyRB.velocity.z);
                    if (AirVelocity.sqrMagnitude > maxAirSpeed * maxAirSpeed)
                    {
                        float XDot = Vector2.Dot(AirVelocity, new Vector2(inputData.XDir, 0));
                        float ZDot = Vector2.Dot(AirVelocity, new Vector2(0, inputData.ZDir));

                        MyRB.velocity = new Vector3(XDot * MyRB.velocity.x, 0, ZDot * MyRB.velocity.z);
                    }

                    //// #TODO change this to add force to relative to camera angle if make sections that are not strictly 2D like the train car. 
                    //float xVelAfterFriction = Mathf.SmoothDamp(MyRB.velocity.x, 0, ref XVel, walkDeaccelerationTime * (MyRB.velocity.x / maxAirSpeed), float.MaxValue, Time.fixedDeltaTime);
                    //float zVelAfterFriction = Mathf.SmoothDamp(MyRB.velocity.z, 0, ref ZVel, walkDeaccelerationTime * (MyRB.velocity.z / maxAirSpeed), float.MaxValue, Time.fixedDeltaTime);
                    //MyRB.velocity = new Vector3(xVelAfterFriction, MyRB.velocity.y, zVelAfterFriction);
                }
            }
        }


    }

    private void LeftGround()
    {
        MyRB.constraints = RigidbodyConstraints.FreezeRotation;
        XMoveWhenLeftGround = MyRB.velocity.x;
        ZMoveWhenLeftGround = MyRB.velocity.z;
        InAirTimer = 0;
        AirControlTimer = airControlTime;
    }
    private void LandedOnGround()
    {
        MyRB.constraints = GroundedConstraints;
        if (InAirTimer > stunningInAirTime)
        {
            StunnedLandingTimer = stunnedLandingTime;
        }
    }

    /// <summary>
    /// Custom Ground Check to see if player is colliding with ground *this frame*
    /// 
    /// Using Physics.OverlapCapsuleNonAlloc()
    /// Can't use OnCollisionEnter because that does not happen every frame.
    /// OnCollisionStay can be costly.
    /// Capsule cast checks for collisions in a direction, which is finnicky to use for collisions happening to the player right now. 
    /// </summary>
    private void CollisionChecks()
    {
        int playerlayer = 1 << 10;
        int hooklayer = 1 << 11;
        int finalLayerMask = ~(playerlayer | hooklayer);

        Vector3 cCenter = MyCollider.bounds.center;
        float cRadius = MyCollider.radius;
        float cTop = MyCollider.bounds.max.y;
        float cBot = MyCollider.bounds.min.y;

        // Any colliders touching player. Using OverlapCapsule as opposed to CapsuleCast. CapsuleCast checks for colliders in a direction. OverlapCapsule casts a capsule into one location and checks
        Vector3 capsuleTop = new Vector3(cCenter.x, cTop, cCenter.z);
        Vector3 capsuleBottom = new Vector3(cCenter.x, cBot, cCenter.z);
        Collider[] colliders = new Collider[10];
        Physics.OverlapCapsuleNonAlloc(capsuleTop,
                                        capsuleBottom,
                                        cRadius,
                                        colliders,
                                        finalLayerMask,
                                        QueryTriggerInteraction.Ignore);
 
        bool groundedValueLastFrame = isGrounded;
        isGrounded = false;
        int i = 0;
        // is touching another collider
        if (colliders[0] != null)
        {
            for (; i < colliders.Length; i++)
            {
                // ADD COLLISION CHECKS FOR OTHER THINGS (I.E. collided into wall in direction player is moving in the air.)

                if (colliders[i] == null)
                {
                    break;
                }

                // Grounded Check
                if (isGrounded == false)
                {
                    Vector3 drawPoint = new Vector3(capsuleBottom.x, capsuleBottom.y + 0.5f, capsuleBottom.z);
                    Debug.DrawLine(drawPoint, colliders[i].ClosestPoint(capsuleBottom), Color.red, 0.3f, true);

                    // Ground check
                    Vector3 colliderToPlayer = capsuleBottom - colliders[i].ClosestPoint(capsuleBottom);
                    float dist = colliderToPlayer.sqrMagnitude;
                    if (dist < (minGroundDist * minGroundDist))
                    {
                        Vector3 playerDotVec = drawPoint - colliders[i].ClosestPoint(capsuleBottom);
                        float dot = Vector3.Dot(Vector3.up, playerDotVec.normalized);

                        if (dot > MinGroundDotProduct)
                        {
                            isGrounded = true;
                            break;
                        }
                    }
                }
            }

            if (isGrounded)
            {
                Debug.DrawLine(capsuleBottom + Vector3.up, colliders[i].ClosestPoint(capsuleBottom), Color.green, 0.3f, true);
                if (groundedValueLastFrame == false)
                {
                    LandedOnGround();
                }
            }
            else if (groundedValueLastFrame)
            {
                LeftGround();
            }

            i = 0;
            // IN AIR COLLISION CHECKS
            if (isGrounded == false)
            {
                for (; i < colliders.Length; i++)
                {
                    if (colliders[i] == null)
                    {
                        break;
                    }

                    // compare against XZMove when left ground

                    Debug.DrawLine(cCenter, colliders[i].ClosestPoint(cCenter), Color.red, 0.3f, true);

                    // moving towards wall check. Get dot product of XZMove and direction towards collider player is colliding with.
                    Vector3 colliderToPlayer = cCenter - colliders[i].ClosestPoint(cCenter);
                    float dist = colliderToPlayer.sqrMagnitude - cRadius * cRadius;
                    if (dist < (minGroundDist * minGroundDist))
                    {
                        Vector3 XZMoveWhenLeftGround = new Vector3(XMoveWhenLeftGround, 0, ZMoveWhenLeftGround);
                        float dot = Vector3.Dot(XZMoveWhenLeftGround.normalized, colliderToPlayer.normalized);

                        if (dot > MinGroundDotProduct)
                        {
                            isGrounded = true;
                        }
                    }
                }
            }
        }
    }

    //private List<ContactPoint2D> CurrentCollisionContacts = new List<ContactPoint2D>();
    //private List<ContactPoint2D> EnterContacts = new List<ContactPoint2D>();
    //private List<ContactPoint2D> ExitContacts = new List<ContactPoint2D>();
    //private void OnCollisionEnter(Collision collision)
    //{
    //    EnterContacts.Clear();
    //    collision.GetContacts(EnterContacts);

    //    for (int i = 0; i < EnterContacts.Count; i++)
    //    {
    //        if (EnterC)
    //    }

    //    if (isGrounded == false)
    //    {
            

    //        EnterContacts.Clear();
    //        collision.GetContacts(EnterContacts);
    //        for (int i = 0; i < EnterContacts.Count; i++)
    //        {
    //            if (EnterContacts[i].normal.y > MinGroundDotProduct)
    //            {
    //                isGrounded = true;  
    //            }

    //            if (isGrounded)
    //                break;
    //        }
    //    }
    //}
    //private void OnCollisionExit(Collision collision)
    //{
    //    isGrounded = false;
            
    //    CurrentCollisionContacts.Clear();
        
    //    MyCollider.GetContacts(CurrentCollisionContacts);
    //    ExitContacts.Clear();
    //    collision.GetContacts(ExitContacts);

    //    // Check if I'm touching any ground. If so, I'm grounded.
    //    for (int i = 0; i < CurrentCollisionContacts.Count; i++)
    //    {
    //        if (ExitContacts.Contains(CurrentCollisionContacts[i]) == false)
    //        {
    //            if (CurrentCollisionContacts[i].normal.y > MinGroundDotProduct)
    //            {
    //                print("still grounded");
    //                isGrounded = true;
    //            }
    //        }

    //        if (isGrounded)
    //            break;
    //    }
    //}

    //private bool IsColliderTouchingGround()
    //{
    //    isGrounded = false;

    //    CurrentCollisionContacts.Clear();
    //    MyCollider.GetContacts(CurrentCollisionContacts);   

    //    // Check if I'm touching any ground. If so, I'm grounded.
    //    for (int i = 0; i < CurrentCollisionContacts.Count; i++)
    //    {
    //        if (CurrentCollisionContacts[i].normal.y > MinGroundDotProduct)
    //        {
    //            print("still grounded");
    //            isGrounded = true;
    //        }

    //        if (isGrounded)
    //            return true;
    //    }

    //    return false;
    //}
}



