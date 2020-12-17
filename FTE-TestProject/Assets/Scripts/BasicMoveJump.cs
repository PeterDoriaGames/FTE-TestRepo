using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* PD - 12/10/2020
    Matches physics layers in unity project.
    Can compare physics layers using int casted enums instead of hardcoded values or string comparisons.
*/
public enum PhysicsLayers
{
    Default,        // 0
    TransparentFX,  // 1
    IgnoreRaycast,  // 2
    Layer3,         // 3
    Water,          // 4
    UI,             // 5
    Layer6,         // 6
    Layer7,         // 7
    PostProcessing, // 8
    Ground          // 9
}


public class BasicMoveJump : Object2D
{

    public float groundMoveSpeed;
    public float airMoveSpeed;
    public float startJumpForce;
    public float jumpAscentForce;
    public float jumpAscentTime;


    private Rigidbody2D MyRB;
    private Collider2D MyCollider;
    // Determines how sloped ground can be
    private float MinGroundNormal = 0.65f;
    private float XDir = 0;
    private float XVel = 0;
    private float YVel = 0;
    private float JumpTimer = 0;
    private bool JumpStartInput = false;
    private bool JumpAscentInput = false;
    private bool IsGrounded = false;
    private bool IsJumping = false;

    
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

    // Update is called once per frame
    void Update()
    {
        XDir = Input.GetAxisRaw("Horizontal");

        if (IsGrounded)
        {
            JumpStartInput = Input.GetButtonDown("Jump");
        }
        else
        {
            JumpAscentInput = Input.GetButton("Jump");
        }
        if (JumpStartInput && IsGrounded == false)
        {
            print("can't jump. not grounded");
        }

    }

    void FixedUpdate()
    {
        if (IsGrounded)
        {
            YVel = 0;
            XVel = XDir * groundMoveSpeed * Time.fixedDeltaTime;

            if  (JumpStartInput)
            {
                print("try jump");
                JumpStartInput = false;
                IsJumping = true;
                JumpTimer = jumpAscentTime;

                YVel = startJumpForce;
            }
        }
        else // in air
        {
            XVel = XDir * airMoveSpeed * Time.fixedDeltaTime;

            if (IsJumping)
            {
                if (JumpAscentInput)
                {   
                    JumpTimer -= Time.deltaTime;
                    print("is ascending");
                    YVel = Mathf.Clamp(jumpAscentForce * (JumpTimer / jumpAscentTime), 0, float.MaxValue) * Time.fixedDeltaTime;
                }
                else
                {
                    IsJumping = false;
                }
            }
            
            if (IsJumping == false)
            {
                YVel = 0;
            }

        }

        MyRB.AddForce(new Vector2(0, YVel));
        MyRB.velocity = new Vector2(XVel, MyRB.velocity.y);
    }


    List<ContactPoint2D> enterContacts = new List<ContactPoint2D>();
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 9)
        {
            if (IsGrounded == false)
            {
                enterContacts.Clear();
                collision.GetContacts(enterContacts);
                for (int i = 0; i < enterContacts.Count; i++)
                {
                    if (enterContacts[i].normal.y > MinGroundNormal)
                    {
                        print("grounded");
                        IsGrounded = true;  
                    }

                    if (IsGrounded)
                        break;
                }
            }
        }
    }

    List<ContactPoint2D> colliderContacts = new List<ContactPoint2D>();
    List<ContactPoint2D> exitContacts = new List<ContactPoint2D>();
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 9)
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
    }

    private bool IsColliderTouchingGround()
    {
        IsGrounded = false;

        if (MyCollider.IsTouchingLayers(9))
        {
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
        }

        return false;
    }
}
