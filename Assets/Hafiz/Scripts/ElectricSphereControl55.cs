using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricSphereControl55 : MonoBehaviour
{
    public GameObject radarIcon;
    public float minMoveSpeed = 10f;
    public float maxMoveSpeed = 50f;
    public float decelerationSpeedRatio = 0.2f;

    private Rigidbody rb;
    private EnemyManager55 manager;
    private float currentSpeed;
    private float startSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        manager = GameObject.FindGameObjectWithTag("EnemyManager").GetComponent<EnemyManager55>();
        currentSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        startSpeed = currentSpeed;

        transform.eulerAngles = new(0f, Random.Range(0f, 360f), 0f);

        manager.AddOther(transform, radarIcon);
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = transform.forward * currentSpeed;
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, startSpeed * decelerationSpeedRatio * Time.deltaTime);
    }
    
    public void DestroySelf()
    {
        Destroy(gameObject);
        manager.RemoveOther(transform);
    }
}
