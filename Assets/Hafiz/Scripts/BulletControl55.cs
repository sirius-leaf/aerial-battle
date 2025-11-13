using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class BulletControl55 : MonoBehaviour
{
    public bool fromPlayer = true;
    public float moveSpeed = 60f;
    public float lifeTime = 3f;

    public void Init(float direction)
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        transform.eulerAngles = Vector3.up * direction;
        rb.velocity = transform.forward * moveSpeed;
    }

    void Update()
    {
        if (lifeTime > 0)
        {
            lifeTime -= Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (fromPlayer && other.gameObject.CompareTag("Enemy")) Destroy(gameObject);
        else if (!fromPlayer && other.gameObject.CompareTag("Player")) Destroy(gameObject);
    }
}
