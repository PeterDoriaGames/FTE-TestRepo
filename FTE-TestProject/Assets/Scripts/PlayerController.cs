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

    private bool IsGrounded = false;
    private bool FinishedAiming = false;
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

    private float YInputDir = 0;
    private Vector2 InitialClickPos;
    private Vector2 ThrowVector;
    private bool ShouldAimInput = false;
    private bool IsAiming = false;
    private bool ShouldThrow = false;
    private bool IsThrowing = false;
    // Update is called once per frame
    void Update()
    {
        XDir = Input.GetAxisRaw("Horizontal");
        YInputDir = Input.GetAxisRaw("Vertical");
        JumpInput = Input.GetButtonDown("Jump");

        ShouldAimInput = Input.GetButton("Fire1") && YInputDir == -1;
        
        if (IsGrounded)
        {
            print("grounded");
            if (IsThrowing == false)
            {
                if (IsAiming)
                {
                    if (ShouldAimInput)
                    {
                        Vector2 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        ThrowVector = InitialClickPos - clickPos;
                    }
                    else
                    {
                        if(YInputDir == -1 && ThrowVector.sqrMagnitude > 0)
                        {
                            ShouldThrow = true;
                        }
                        IsAiming = false;
                    }
                }
                else
                {
                    if (ShouldAimInput)
                    {
                        InitialClickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    }
                }
            }
        }
    }

    private float Timer = 0;
    private float PeakVel = 0;
    void FixedUpdate()
    {
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
                if (Timer == 0)
                {
                    PeakVel = MyRB.velocity.x;
                }
                Timer += Time.deltaTime;
                float t = Timer / 0.1f;
                MyRB.velocity = new Vector2(Mathf.Lerp(PeakVel, 0, t), MyRB.velocity.y);
                if (t >= 1)
                {
                    Timer = 0;
                    MyRB.velocity = new Vector2(0, MyRB.velocity.y);
                }
            }

            
            if (JumpInput && ShouldAimInput == false)
            {
                MyRB.AddForce(new Vector2 (0, startJumpForce), ForceMode2D.Impulse);
            }
        }
        else 
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
