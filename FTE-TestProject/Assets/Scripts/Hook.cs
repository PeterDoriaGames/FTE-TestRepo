using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HookState
{
    NotThrown,
    StalledReelIn,
    ReelingBack,
    ReelingInPlayer
}

public class Hook : Object2D
{
    public float hookThrowSpeed;

    private HookableBase Hookable = null;
    public HookableBase hookable { get { return Hookable; } }

    private HookState State = HookState.NotThrown;
    public HookState state { get { return State; } }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ThrowHook(Vector2 throwDir, Vector2 startPos)
    {

    }

    // collides but not with hookable --> begins to reel itself back to the player.

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HookableBase h = collision.gameObject.GetComponent<HookableBase>();
        if (h)
        {
            Hookable = h;

        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        HookableBase h = collision.gameObject.GetComponent<HookableBase>();
        if (h == Hookable)
        {
            Hookable = null;
        }
        else
        {
            Debug.LogError("How did this happen? --> Touched another hookable when already hooked on another hookable.");
        }
    }
}
