using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HookMode
{
    Holstered,
    Aiming,
    GrapplingHook,
    FishingPole,
    SteadyRope
}
/// <summary>
/// HookMode is dependent on player input
/// </summary>
public class Hook : Object2D
{
    public float hookThrowSpeed;

    private HookableBase AttachedHookable = null;
    public HookableBase attachedHookable { get { return AttachedHookable; } }

    private HookMode Mode = HookMode.Holstered;
    public HookMode mode { get { return Mode; } }
    public PlayerController hookPlayer;

    private PlayerInputData InputData;
    private Vector2 ThrowVector;
    private void Update()
    {
        // put in check for if hook has reached plater --> Holster hook
        
        // put in check for unhooking hook from attachable

        InputData = hookPlayer.inputData;
        if (mode == HookMode.Holstered)
        {
            if (InputData.StartAimingInput)
            {
                Mode = HookMode.Aiming;
                ThrowVector = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            }
        }
        else if (mode == HookMode.Aiming)
        {
            if (InputData.CancelAimingInput)
            {
                Mode = HookMode.Holstered;
            }
            else
            {
                if (InputData.ThrowHookInput)
                {
                    Mode = HookMode.GrapplingHook;
                }
                else if (InputData.ContinueAimingInput)
                {
                    ThrowVector = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                }
            }
        }
        else if (mode == HookMode.FishingPole)
        {
            if (hookPlayer.isGrounded)
            {
                if (InputData.ContinueAimingInput)
                {
                    Mode = HookMode.SteadyRope;
                }
            }
            else
            {
                Mode = HookMode.GrapplingHook;
            }
        }
        else if (mode == HookMode.GrapplingHook)
        {
            if (hookPlayer.isGrounded)
            {
                Mode = HookMode.FishingPole;
            }
            else if (InputData.ContinueAimingInput)
            {
                Mode = HookMode.SteadyRope;
            }
        }
        else if (mode == HookMode.SteadyRope)
        {
            if (InputData.ContinueAimingInput == false)
            {
                if (hookPlayer.isGrounded)
                {
                    Mode = HookMode.FishingPole;
                }
                else
                {
                    Mode = HookMode.GrapplingHook;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        /* handle hook physics? 
            - dragged on ground
            - falling 
                - grappling hook
                - fishing pole
                - steady
                - attached vs unattached
            - thrown
        */
    }

    public void OnHookFullyRetracted()
    {
        // deactivate hook visuals
        Mode = HookMode.Holstered;
    }

    public void AimHook(Vector2 throwDir, Vector2 startPos)
    {
        // hook aiming visuals
    }

    public void ThrowHook(Vector2 throwDir, Vector2 startPos)
    {
        // throw hook functionality. 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HookableBase h = collision.gameObject.GetComponent<HookableBase>();
        if (h)
        {
            AttachedHookable = h;
            AttachedHookable.AddHook(this);
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        HookableBase h = collision.gameObject.GetComponent<HookableBase>();
        if (h == AttachedHookable)
        {
            AttachedHookable.RemoveHook(this);
            AttachedHookable = null;
        }
        else
        {
            Debug.LogError("How did this happen? --> Touched another hookable when already hooked on another hookable.");
        }
    }
}
