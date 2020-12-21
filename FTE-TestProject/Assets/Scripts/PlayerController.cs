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
    public bool JumpInput;
    public bool CancelAimingInput;
    public bool StartAimingInput;
    public bool ContinueAimingInput;
    public bool ThrowHookInput;
    //public bool PauseReelInInput;
    public PlayerInputData(float xDir, bool jump, bool cancelAiming, bool startAiming, bool continueAiming, bool throwHook)
    {
        XDir = xDir;
        JumpInput = jump;
        CancelAimingInput = cancelAiming;
        StartAimingInput = startAiming;
        ContinueAimingInput = continueAiming;
        ThrowHookInput = throwHook;
    }
}

public class PlayerController : Object2D
{
    
    public float groundMoveSpeed;
    public float startJumpForce;
    public Hook myHook;

    private float xSwingSpeed = 10;

    private Rigidbody2D MyRB;
    private Collider2D MyCollider;
    // Determines how sloped ground can be
    private float MinGroundNormal = 0.65f;
    private float XVel = 0;

    public bool isGrounded { get { return isGrounded; } private set { IsGrounded = value; } }
    private bool IsGrounded = false;
    private bool IsSwinging = false;

    public PlayerInputData inputData { get { return InputData; } private set { InputData = value; } }
    private PlayerInputData InputData = new PlayerInputData();
    
    void Awake()
    {
        MyRB = GetComponent<Rigidbody2D>();
        MyCollider = GetComponent<Collider2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        isGrounded = IsColliderTouchingGround();
    }

    private Vector2 ThrowVector;
    private bool IsAiming = false;
    private bool HasThrownHook = false;
    private bool IsHookAttached = false;
    // Update is called once per frame
    void Update()
    {
        float xDir = Input.GetAxisRaw("Horizontal");
        bool jumpInput = Input.GetButtonDown("Jump") && isGrounded;

        bool startAimingInput = Input.GetButtonDown("Fire1");
        bool continueAimingInput = Input.GetButton("Fire1");
        bool throwHookInput = Input.GetButtonUp("Fire1");

        bool cancelAimingInput = Input.GetButtonDown("Fire2") || isGrounded == false;

        //  WHERE TO UPDATE VISUALS IN LOGIC?
        InputData = new PlayerInputData(xDir, jumpInput, startAimingInput, continueAimingInput, cancelAimingInput, throwHookInput);

    }

    private float XSlowDownTimer = 0;
    private float PeakVel = 0;
    void FixedUpdate()
    {
        // NEED TO INTEGRATE PLAYER PHYSICS BASED ON HOOKMODE AND PLAYER STATE. 
        // PHYSICS ARE APPLIED HERE. HOOK TAKES PLAYER INPUT AND TELLS PLAYER WHAT IS HAPPENING. PLAYER TAKES THAT INFO AND APPLIES APPROPRIATE FORCES.


        // check hook mode to determine movement OR let hook tell you what to do???
                     
        if (isGrounded)
        {
            // add check to see if should throw hook.
            
            if (inputData.XDir != 0)
            {
                XVel = inputData.XDir * groundMoveSpeed * Time.fixedDeltaTime;
                MyRB.velocity = new Vector2(XVel, MyRB.velocity.y);
            }
            else
            {
                if (XSlowDownTimer == 0)
                {
                    PeakVel = MyRB.velocity.x;
                }
                XSlowDownTimer += Time.deltaTime;
                float t = XSlowDownTimer / 0.1f;
                MyRB.velocity = new Vector2(Mathf.Lerp(PeakVel, 0, t), MyRB.velocity.y);
                if (t >= 1)
                {
                    XSlowDownTimer = 0;
                    MyRB.velocity = new Vector2(0, MyRB.velocity.y);
                }
            }
    
            if (inputData.JumpInput)
            {
                MyRB.AddForce(new Vector2 (0, startJumpForce), ForceMode2D.Impulse);
            }
        }
        else if (IsHookAttached)
        {
            // check if in air or swinging
            if (IsSwinging)
            {
                XVel = inputData.XDir * xSwingSpeed * Time.fixedDeltaTime;
            }
        }

        // MyRB.AddForce(new Vector2(0, YVel));

    }


    private List<ContactPoint2D> EnterContacts = new List<ContactPoint2D>();
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isGrounded == false)
        {
            EnterContacts.Clear();
            collision.GetContacts(EnterContacts);
            for (int i = 0; i < EnterContacts.Count; i++)
            {
                if (EnterContacts[i].normal.y > MinGroundNormal)
                {
                    isGrounded = true;  
                }

                if (isGrounded)
                    break;
            }
        }
    }

    List<ContactPoint2D> ColliderContacts = new List<ContactPoint2D>();
    List<ContactPoint2D> ExitContacts = new List<ContactPoint2D>();
    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
            
        ColliderContacts.Clear();
        MyCollider.GetContacts(ColliderContacts);
        ExitContacts.Clear();
        collision.GetContacts(ExitContacts);

        // Check if I'm touching any ground. If so, I'm grounded.
        for (int i = 0; i < ColliderContacts.Count; i++)
        {
            if (ExitContacts.Contains(ColliderContacts[i]) == false)
            {
                if (ColliderContacts[i].normal.y > MinGroundNormal)
                {
                    print("still grounded");
                    isGrounded = true;
                }
            }

            if (isGrounded)
                break;
        }
    }

    private bool IsColliderTouchingGround()
    {
        isGrounded = false;

        ColliderContacts.Clear();
        MyCollider.GetContacts(ColliderContacts);   

        // Check if I'm touching any ground. If so, I'm grounded.
        for (int i = 0; i < ColliderContacts.Count; i++)
        {
            if (ColliderContacts[i].normal.y > MinGroundNormal)
            {
                print("still grounded");
                isGrounded = true;
            }

            if (isGrounded)
                return true;
        }

        return false;
    }
}



