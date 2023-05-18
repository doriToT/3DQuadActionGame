using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset; // º¸Á¤°ª


    void Update()
    {
        transform.position = target.position + offset;
    }
}
