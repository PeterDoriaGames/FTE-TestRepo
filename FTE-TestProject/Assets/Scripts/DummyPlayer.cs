using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyPlayer : MonoBehaviour
{
    private bool HasLanded = false;
    public bool hasLanded {get {return HasLanded;} private set {HasLanded = value;}}

    private void OnCollisionEnter(Collision collision)
    {
        if (hasLanded == false)
        {
           hasLanded = true;
            print("dummy has landed");
        }
    }
}
