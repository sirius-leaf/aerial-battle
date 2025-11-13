using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneControl55 : MonoBehaviour
{
    public GameObject electricSphere;
    public GameObject radarIcon;
    public float moveSpeed = 50f;
    public float lifeTime = 10f;
    public int sphereSpawnCount = 5;

    private Rigidbody rb;
    private Transform pl;
    private Vector3 targetSpeed;
    private EnemyManager55 manager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pl = GameObject.FindGameObjectWithTag("Player").transform;
        manager = GameObject.FindGameObjectWithTag("EnemyManager").GetComponent<EnemyManager55>();
    }

    void Update()
    {
        // menentukan arah ke player
        Vector3 directionToPlayer = pl.position - transform.position;
        Quaternion targetAngle = Quaternion.LookRotation(directionToPlayer);

        transform.rotation = targetAngle;

        // mengatur kecepatan berdasarkan jarak ke player
        if (directionToPlayer.magnitude > 5f) targetSpeed = transform.forward * moveSpeed;
        else targetSpeed = Vector3.zero;

        rb.velocity = Vector3.MoveTowards(rb.velocity, targetSpeed, 200f * Time.deltaTime);

        if (lifeTime > 0) lifeTime -= Time.deltaTime;
        else
        {
            for (int i = 0; i < sphereSpawnCount; i++)
            {
                Instantiate(electricSphere, transform.position, Quaternion.identity);
            }

            manager.RemoveOther(transform);
            Destroy(gameObject);
        }
    }
}
