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
    
    public float groundMoveSpeed;
    public float airMoveSpeed;
    public float startJumpForce;
    public float additionalGravityScale = 2f;
    public Hook myHook;

    private float xSwingSpeed = 10;

    private Rigidbody MyRB;
    private CapsuleCollider MyCollider;
    // Determines how sloped ground can be
    private float MinGroundDotProduct = 0.65f;
    private float XVel = 0;
    private float ZVel = 0;
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

        GroundedConstraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
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

    }

    private RigidbodyConstraints GroundedConstraints;
    private float GroundCheckTimer = 0;
    private float JustJumpedCooldown = 0.3f;
    void FixedUpdate()
    {

        if (GroundCheckTimer > 0 )
        {
            GroundCheckTimer -= Time.fixedDeltaTime;
        }
        else
        {
            GroundedCheck();
        }

        // NEED TO INTEGRATE PLAYER PHYSICS BASED ON HOOKMODE AND PLAYER STATE. 
        // PHYSICS ARE APPLIED HERE. HOOK TAKES PLAYER INPUT AND TELLS PLAYER WHAT IS HAPPENING. PLAYER TAKES THAT INFO AND APPLIES APPROPRIATE FORCES.


        // check hook mode to determine movement OR let hook tell you what to do???

        MyRB.AddForce(Physics.gravity * additionalGravityScale);

        if (InputData.JumpInput && isGrounded == false)
        {
            Debug.LogError("try jump but not grounded");
        }

        if (isGrounded)
        {
            MyRB.constraints = GroundedConstraints;

            // add check to see if should throw hook.
            XVel = inputData.XDir * groundMoveSpeed * Time.fixedDeltaTime;
            ZVel = inputData.ZDir * groundMoveSpeed * Time.fixedDeltaTime;
            
            if (inputData.JumpInput)
            {
                MyRB.constraints = RigidbodyConstraints.FreezeRotation;

                MyRB.AddForce(new Vector2 (0, startJumpForce), ForceMode.Impulse);
                GroundCheckTimer = JustJumpedCooldown;
            }

            MyRB.velocity = (new Vector3(XVel, MyRB.velocity.y, ZVel));

        }
        //  IN AIR
        else
        {
            MyRB.constraints = RigidbodyConstraints.FreezeRotation;

            if (myHook.attachedHookable)
            {
                // check if in air or swinging SHOULD NOT BE STRAIGHT BOOL. SHOULD CHECK HOOKMODE
                if (myHook.mode == HookMode.SteadyRope)
                {
                    XVel = inputData.XDir * xSwingSpeed * Time.fixedDeltaTime;
                }
            }
            else
            {
                XVel = inputData.XDir * airMoveSpeed * Time.fixedDeltaTime;
                ZVel = inputData.ZDir * airMoveSpeed * Time.fixedDeltaTime;
                MyRB.velocity = (new Vector3(XVel, MyRB.velocity.y, ZVel));
            }
        }

    }


    public float minGroundDist = 1f;

    /// <summary>
    /// Custom Ground Check to see if player is colliding with ground *this frame*
    /// 
    /// Can't use OnCollisionEnter because that does not happen every frame.
    /// OnCollisionStay can be costly.
    /// CapsuleCast does not capture hits until it moves from its starting point. Makes it hard to figure it out. 
    /// 
    /// </summary>
    private void GroundedCheck()
    {
        int playerlayer = 1 << 10;
        int hooklayer = 1 << 11;
        int finalLayerMask = ~(playerlayer | hooklayer);

        Vector3 capsuleTop = new Vector3(MyCollider.bounds.center.x, MyCollider.bounds.max.y + Physics.defaultContactOffset, MyCollider.bounds.center.z);
        Vector3 capsuleBottom = new Vector3(MyCollider.bounds.center.x, MyCollider.bounds.min.y - Physics.defaultContactOffset - 1, MyCollider.bounds.center.z);
        Debug.DrawLine(capsuleBottom, new Vector3(capsuleBottom.x, capsuleBottom.y - minGroundDist, capsuleBottom.z ));

        // Any colliders touching player?
        Collider[] colliders = new Collider[10];
        Physics.OverlapCapsuleNonAlloc(capsuleTop,
                                        capsuleBottom,
                                        MyCollider.radius + Physics.defaultContactOffset,
                                        colliders,
                                        finalLayerMask,
                                        QueryTriggerInteraction.Ignore);

        // Are any of the colliders touching the player considered ground?
        bool groundValueBeforeCheck = isGrounded;
        isGrounded = false;
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[0] == null)
            {
                //print("NO COLLs");
                break;
            }
            if (colliders[i] == null)
            {
                break;
            }

            Vector3 colliderToPlayer = capsuleBottom - colliders[i].ClosestPoint(capsuleBottom);
            float dist = colliderToPlayer.magnitude;
            if (dist < (minGroundDist))
            {
                float dot = Vector3.Dot(Vector3.up, colliderToPlayer);

                if (dot > MinGroundDotProduct)
                {
                    isGrounded = true;
                    //print("GROUNDED");
                    break;
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



