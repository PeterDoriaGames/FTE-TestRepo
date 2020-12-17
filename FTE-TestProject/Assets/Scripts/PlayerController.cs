using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* controls flow of states for player
 x movement, aiming, throwing, pulling, reel-in, swing

    Aim --
*/

public class PlayerController : Object2D
{
    
    public float groundMoveSpeed;
    public float startJumpForce;

    private float xSwingSpeed = 10;


    private Rigidbody2D MyRB;
    private Collider2D MyCollider;
    // Determines how sloped ground can be
    private float MinGroundNormal = 0.65f;
    private float XDir = 0;
    private float XVel = 0;
    private bool JumpInput = false;
    private bool CancelAimingInput = false;

    private bool IsGrounded = false;
    private bool IsSwinging = false;

    
    void Awake()
    {
        MyRB = GetComponent<Rigidbody2D>();
        MyCollider = GetComponent<Collider2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        IsGrounded = IsColliderTouchingGround();
    }


    private Vector2 ThrowVector;
    private bool StartAimingInput = false;
    private bool ContinueAimingInput = false;
    private bool IsAiming = false;
    private bool ThrowHookInput = false;
    private bool HasThrownHook = false;
    private bool IsHookAttached = false;
    // Update is called once per frame
    void Update()
    {
        XDir = Input.GetAxisRaw("Horizontal");
        JumpInput = Input.GetButtonDown("Jump") && IsGrounded;

        if (HasThrownHook == false)
        {
            StartAimingInput = Input.GetButtonDown("Fire1");
            ContinueAimingInput = Input.GetButton("Fire1");
            ThrowHookInput = Input.GetButtonUp("Fire1");

            CancelAimingInput = Input.GetButtonDown("Fire2") || IsGrounded == false;

            //  WHERE TO UPDATE VISUALS IN LOGIC?

            if (IsAiming)
            {
                if (CancelAimingInput)
                {
                    IsAiming = false;
                }
                else
                {
                    if (ThrowHookInput)
                    {
                        IsAiming = false;
                    }
                    else if (ContinueAimingInput)
                    {
                        ThrowVector = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                    }
                }
            }
            else if (StartAimingInput)
            {
                IsAiming = true;
                ThrowVector = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            }
        }
    }

    private float XSlowDownTimer = 0;
    private float PeakVel = 0;
    void FixedUpdate()
    {
        if (ThrowHookInput)
        { 
            // spawn hook and throw
        }

                     
        if (IsGrounded)
        {
            // add check to see if should throw hook.
            
            if (XDir != 0)
            {
                XVel = XDir * groundMoveSpeed * Time.fixedDeltaTime;
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
    
            if (JumpInput)
            {
                MyRB.AddForce(new Vector2 (0, startJumpForce), ForceMode2D.Impulse);
            }
        }
        else if (IsHookAttached)
        {
            // check if in air or swinging
            if (IsSwinging)
            {
                XVel = XDir * xSwingSpeed * Time.fixedDeltaTime;
            }
        }

        // MyRB.AddForce(new Vector2(0, YVel));

    }


    List<ContactPoint2D> enterContacts = new List<ContactPoint2D>();
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsGrounded == false)
        {
            enterContacts.Clear();
            collision.GetContacts(enterContacts);
            for (int i = 0; i < enterContacts.Count; i++)
            {
                if (enterContacts[i].normal.y > MinGroundNormal)
                {
                    IsGrounded = true;  
                }

                if (IsGrounded)
                    break;
            }
        }
    }

    List<ContactPoint2D> colliderContacts = new List<ContactPoint2D>();
    List<ContactPoint2D> exitContacts = new List<ContactPoint2D>();
    private void OnCollisionExit2D(Collision2D collision)
    {
        IsGrounded = false;
            
        colliderContacts.Clear();
        MyCollider.GetContacts(colliderContacts);
        exitContacts.Clear();
        collision.GetContacts(exitContacts);

        // Check if I'm touching any ground. If so, I'm grounded.
        for (int i = 0; i < colliderContacts.Count; i++)
        {
            if (exitContacts.Contains(colliderContacts[i]) == false)
            {
                if (colliderContacts[i].normal.y > MinGroundNormal)
                {
                    print("still grounded");
                    IsGrounded = true;
                }
            }

            if (IsGrounded)
                break;
        }
    }

    private bool IsColliderTouchingGround()
    {
        IsGrounded = false;

        colliderContacts.Clear();
        MyCollider.GetContacts(colliderContacts);   

        // Check if I'm touching any ground. If so, I'm grounded.
        for (int i = 0; i < colliderContacts.Count; i++)
        {
            if (colliderContacts[i].normal.y > MinGroundNormal)
            {
                print("still grounded");
                IsGrounded = true;
            }

            if (IsGrounded)
                return true;
        }

        return false;
    }
}
