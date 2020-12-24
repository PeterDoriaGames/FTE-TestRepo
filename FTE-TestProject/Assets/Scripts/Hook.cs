using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// HookMode is dependent on player input
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Hook : Object2D
{
    public float hookThrowForce;
    public GameObject hookSpriteGO;
    public GameObject ropePrefab;
    public float minRopeLength;
    public float maxRopeLength;

    private HookableBase AttachedHookable = null;
    public HookableBase attachedHookable { get { return AttachedHookable; } }
    public PlayerController hookPlayer;

    private HookMode Mode = HookMode.Holstered;
    public HookMode mode { get { return Mode; } }
    private HookMode PreviousMode;

    private Rigidbody2D HookRB2D;
    private Collider2D HookColl;
    private PlayerInputData InputData;
    private Vector2 ThrowVector;
    private Vector2 HookToPlayer;
    private void Awake()
    {
        HookRB2D = GetComponent<Rigidbody2D>();
        HookColl = GetComponent<Collider2D>();
        ChangeMode(HookMode.Holstered);
    }

    private void Update()
    {
        // put in check for if hook has reached plater --> Holster hook

        // put in check for unhooking hook from attachable

        UpdateHookModeByInput();

        if (mode != HookMode.Holstered)
        {
            RopeController();
        }
    }

    private void UpdateHookModeByInput()
    {
        InputData = hookPlayer.inputData;
        if (mode == HookMode.Holstered)
        {
            if (InputData.FireInputDown)
            {
                ChangeMode(HookMode.Aiming);
                ThrowVector = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            }
        }
        else if (mode == HookMode.Aiming)
        {
            if (InputData.CancelFireInput)
            {
                ChangeMode(HookMode.Holstered);
            }
            else
            {
                ThrowVector = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;

                if (InputData.ThrowHookInput)
                {
                    ChangeMode(HookMode.InitialThrow);
                }
            }
        }
        else if (mode == HookMode.InitialThrow)
        {
            if (InputData.FireInputDown)
            {
                ChangeMode(HookMode.SteadyRope);
            }
        }
        else if (mode == HookMode.FishingPole)
        {
            if (AttachedHookable)
            {
                if (InputData.DisAttachHookInput)
                {

                }
            }
            if (hookPlayer.isGrounded)
            {
                if (InputData.FireInputDown)
                {
                    ChangeMode(HookMode.SteadyRope);
                }
            }
            else
            {
                ChangeMode(HookMode.GrapplingHook);
            }
        }
        else if (mode == HookMode.GrapplingHook)
        {
            if (hookPlayer.isGrounded)
            {
                ChangeMode(HookMode.FishingPole);
            }
            else if (InputData.FireInputDown)
            {
                ChangeMode(HookMode.SteadyRope);
            }
        }
        else if (mode == HookMode.SteadyRope)
        {
            if (InputData.ContinueAimingInput == false)
            {
                ChangeMode(PreviousMode);
            }
        }
    }

    private void ChangeMode(HookMode newMode)
    {
        PreviousMode = Mode;

        if (newMode == HookMode.Holstered)
        {
            Mode = HookMode.Holstered;
            print("holstered");

            HookRB2D.bodyType = RigidbodyType2D.Static;
            HookColl.enabled = false;
            hookSpriteGO.SetActive(false);
            if (AttachedHookable)
            {
                DisAttachHook();
            }
        }
        else if (newMode == HookMode.InitialThrow)
        {
            Mode = HookMode.InitialThrow;
            print("initial throw");

            HookRB2D.bodyType = RigidbodyType2D.Dynamic;
            transform.position = hookPlayer.transform.position;
            HookColl.enabled = true;
            hookSpriteGO.SetActive(true);
        }
        else if (newMode == HookMode.Aiming)
        {
            print("aiming");

            Mode = HookMode.Aiming;
        }
        else if (newMode == HookMode.GrapplingHook)
        {
            print("grappling hook");

            Mode = HookMode.GrapplingHook;
        }
        else if (newMode == HookMode.FishingPole)
        {
            print("fishing pole");

            Mode = HookMode.FishingPole;
        }
        else if (newMode == HookMode.SteadyRope)
        {
            print("steady rope");

            Mode = HookMode.SteadyRope;
        }
    }

    private void AttachHook(HookableBase hookable)
    {
        AttachedHookable = hookable;
        AttachedHookable.AddHook(this);
    }
    private void DisAttachHook()
    {
        AttachedHookable.RemoveHook(this);
        AttachedHookable = null;
    }
    private void OnHookFullyRetracted()
    {
        // deactivate hook visuals
        ChangeMode(HookMode.Holstered);
    }

    private bool StartedThrow = false;
    private void FixedUpdate()
    {
        if (mode != HookMode.Holstered)
        {
            HookToPlayer = (hookPlayer.transform.position - transform.position);
            if (HookToPlayer.sqrMagnitude > maxRopeLength && mode != HookMode.Aiming)
            {
                ChangeMode(HookMode.FishingPole);
            }

            // ADD CHECK TO SEE IF HOOK IS STUCK. Use debug key to respawn for now
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ChangeMode(HookMode.Holstered);
            }
            if (AttachedHookable == null )
            {
                float dist = (transform.position - hookPlayer.transform.position).sqrMagnitude;
                if (mode != HookMode.InitialThrow || mode != HookMode.Aiming || mode != HookMode.Holstered)
                {
                    if (dist < minRopeLength)
                    {
                        ChangeMode(HookMode.Holstered);
                    }
                }
                else
                {
                    if (dist > maxRopeLength)
                    {
                        ChangeMode(HookMode.FishingPole);
                    }
                }
            }

            // functionality to move hook when needed
            if (Mode == HookMode.FishingPole)
            {
                if (AttachedHookable)
                {
                    // reel in thing hook is attached to
                    AttachedHookable.PullHookable(HookToPlayer.normalized);
                }
                else
                {
                    Vector2 vec = HookToPlayer.normalized * hookThrowForce * Time.fixedDeltaTime;
                    HookRB2D.AddForce(vec);
                }
            }
            else if (Mode == HookMode.InitialThrow)
            {
                if (StartedThrow == false)
                {
                    Vector2 normalized = ThrowVector.normalized;
                    transform.position = new Vector2(hookPlayer.transform.position.x, hookPlayer.transform.position.y)
                                            + normalized;
                    HookRB2D.velocity = ThrowVector.normalized * hookThrowForce * Time.fixedDeltaTime;
                }
            }
        }
    }

    private void RopeController()
    {
        // rope anchor
        // hook position
        // change rope length accordingly
        if (mode == HookMode.InitialThrow)
        {
            // rope visuals
        }

    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (AttachedHookable == false)
        {
            HookableBase h = collision.gameObject.GetComponent<HookableBase>();
            if (h)
            {
                AttachHook(h);
            }
            else if (collision.gameObject.layer == 10)
            {
                // collided with player
                if (Mode != HookMode.InitialThrow)
                {
                    OnHookFullyRetracted();
                }
            }
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        HookableBase h = collision.gameObject.GetComponent<HookableBase>();
        if (h == AttachedHookable)
        {
            DisAttachHook();

            ChangeMode(HookMode.FishingPole);
        }
        else
        {
            Debug.LogError("How did this happen? --> Touched another hookable when already hooked on another hookable.");
        }
    }
}

/// <summary>
/// Holstered: Hook is not being used
/// Aiming: Player is aiming trajectory for hook's initial throw
/// Initial Throw: After initial throw is ended (Hook comes to rest), then hook transitions into next thing. 
/// Grappling Hook: Hook is attached to Hookable. Player should move towards hook position.
/// Fishing Pole:
///     - Attached: Reels in thing hook is attached to
///     - Dis-attached: Reels in hook to player
/// SteadyRope: Nothing is pulled. Hook does not apply forces in either direction. Acts like rope. Can still catch on things. 
/// </summary>
public enum HookMode
{
    Holstered,
    Aiming,
    InitialThrow,
    GrapplingHook,
    FishingPole,
    SteadyRope
}