using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object2D : MonoBehaviour
{
    void Awake()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }
}
