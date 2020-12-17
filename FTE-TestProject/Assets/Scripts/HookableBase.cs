using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Get hook state to determine if should be pulled on or not.
/// </summary>
public abstract class HookableBase : Object2D
{

    [SerializeField]
    private bool IsFirmlyAttached = false;
    public bool isFirmlyAttached { get { return IsFirmlyAttached; } }
    [SerializeField]
    private bool IsLooselyAttached = false;
    public bool isLooselyAttached { get { return IsLooselyAttached; } }
    
    private Hook HookedHook = null;
    public Hook hookedHook { get { return HookedHook; } }

    private void Awake()
    {
        // hacky solution. add editor script and or enum wrapper later. Maybe SO if I want to get fancy
        if (IsFirmlyAttached && IsLooselyAttached)
        {
            IsLooselyAttached = false;
        }
    }

    protected virtual void FixedUpdate()
    {
        if (HookedHook)
        {
            if (HookedHook.state == HookState.ReelingBack)
            {
                PullHookable();
            }
        }
    }

    protected virtual void PullHookable()
    {

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
