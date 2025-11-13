using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileControl55 : MonoBehaviour
{
    public bool fromPlayer = true;
    public float moveSpeed = 60f;
    public float lifeTime = 3f;
    public float rotateSpeed = 120f;

    private Transform target;
    private Rigidbody rb;
    private EnemyManager55 manager;
    private bool tracking = true;

    public void Init(float direction, Transform target)
    {
        rb = GetComponent<Rigidbody>();

        transform.eulerAngles = Vector3.up * direction;

        this.target = target;

        if (!fromPlayer)
        {
            manager = GameObject.FindGameObjectWithTag("EnemyManager").GetComponent<EnemyManager55>();
            manager.AddMissile(transform);
        }
    }

    void Update()
    {
        // mengarahkan missile untuk bergerak ke player
        if (target != null && tracking)
        {
            Vector3 directionToPlayer = target.position - transform.position;
            Quaternion targetAngle = Quaternion.LookRotation(directionToPlayer);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetAngle, rotateSpeed * Time.deltaTime);
        }

        rb.velocity = transform.forward * moveSpeed;

        if (lifeTime > 0)
        {
            lifeTime -= Time.deltaTime;

            if (lifeTime <= 1f) tracking = false;
        }
        else
        {
            if (!fromPlayer) manager.RemoveMissile(transform);
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (fromPlayer) {
            if (other.gameObject.CompareTag("Enemy")) Destroy(gameObject);
            else if (other.gameObject.CompareTag("ElectricSphere"))
            {
                tracking = false;
                transform.eulerAngles = new(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
            }
        }
        else if (!fromPlayer && other.gameObject.CompareTag("Player"))
        {
            manager.RemoveMissile(transform);
            Destroy(gameObject);
        }
    }
}
