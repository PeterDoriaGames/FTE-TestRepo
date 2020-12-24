using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hookable :  HookableBase
{
    
    
    public override void PullHookable(Vector2 pullDir)
    {
        if (hookableHousing.bodyType != RigidbodyType2D.Dynamic)
        {
            hookableHousing.bodyType = RigidbodyType2D.Dynamic;
        }
        hookableHousing.AddForce(pullDir * pullForce);
    }
}
