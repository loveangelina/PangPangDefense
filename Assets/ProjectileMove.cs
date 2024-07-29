using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMove : MonoBehaviour
{
    private Vector3 direction = new Vector3(0, 0, 1);
    public float speed;
    public float fireRate;
    
    void Start()
    {
        
    }

    void Update()
    {
        if (speed != 0)
        {
            transform.position += direction * (speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("총알이 충돌");
        Destroy(gameObject);
    }
}
