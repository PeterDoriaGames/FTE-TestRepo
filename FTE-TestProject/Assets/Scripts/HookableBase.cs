using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Get hook state to determine if should be pulled on or not.
/// </summary>
public abstract class HookableBase : Object2D
{
    public Rigidbody2D hookableHousing;
    public float pullForce = 30f;


    [SerializeField]
    private bool IsFirmlyAttached = false;
    public bool isFirmlyAttached { get { return IsFirmlyAttached; } protected set { IsFirmlyAttached = value; } }
    [SerializeField]
    private bool IsLooselyAttached = false;
    public bool isLooselyAttached { get { return IsLooselyAttached; } protected set { IsLooselyAttached = value; } }
    
    private Hook HookedHook = null;
    public Hook hookedHook { get { return HookedHook; } }

    private void Awake()
    {
        if (hookableHousing == null)
            Debug.LogError("No housing for hookable");

        // hacky solution. add editor script and or enum wrapper later. Maybe SO if I want to get fancy
        if (IsFirmlyAttached && IsLooselyAttached)
        {
            IsLooselyAttached = false;
        }
    }

    public virtual void PullHookable(Vector2 pullDir)
    {
        // #TODO - add constraints for pulling at different angles.
    }

    public virtual void AddHook(Hook hook)
    {
        if (HookedHook == null)
        {
            HookedHook = hook;
        }
        else
        {
            Debug.LogError("Got 2 hooks?!");
        }
    }
    public virtual void RemoveHook(Hook hook)
    {
        if (HookedHook == hook)
        {
            HookedHook = null;
        }
        else
        {
            Debug.LogError("Got 2 hooks?!");
        }
    }

}
