using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EnemyControl55 : MonoBehaviour
{
    public GameObject bullet;
    public GameObject missile;
    public GameObject tempAudio;
    public GameObject explosion;
    public GameObject radarIcon;
    public Transform firePoint;
    public AudioClip explodeSfx;
    public int health = 10;
    public float rotateSpeed = 120f;
    public float moveSpeed = 20f;
    public float maxShotRange = 40f;
    public float fireRate = 5f;
    public float maxShotAngle = 60f;
    public float missAngle = 6f;


    [HideInInspector]
    public bool isTarget = false;

    private enum EnemyState { ATTACK, AVOID }

    private Transform player;
    private Rigidbody rb;
    private EnemyState state = EnemyState.ATTACK;
    private EnemyManager55 manager;
    private UiControl55 uiControl;
    private float shotDelay = 0f;
    private bool stopShooting = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        manager = GameObject.FindGameObjectWithTag("EnemyManager").GetComponent<EnemyManager55>();
        uiControl = GameObject.FindGameObjectWithTag("UiControl").GetComponent<UiControl55>();
    }

    private int bulletFired = 0;
    private int maxShotBeforeAvoid = 13;
    private float avoidTimer = 0f;
    private float attackTimer = 0f;
    private float missileTimer = 0f;
    private float maxAvoidTimer = 0f;
    private float maxAttackTimer = 15f;
    void Update()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        Quaternion targetAngle = Quaternion.LookRotation(directionToPlayer);
        Quaternion flipedTargetAngle = Quaternion.LookRotation(-directionToPlayer);
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // mengatur arah gerakan enemy berdasarkan state
        if (state == EnemyState.ATTACK) transform.rotation = Quaternion.RotateTowards(transform.rotation, targetAngle, rotateSpeed * Time.deltaTime);
        else transform.rotation = Quaternion.RotateTowards(transform.rotation, flipedTargetAngle, rotateSpeed * Time.deltaTime);

        rb.velocity = transform.forward * moveSpeed;

        switch (state)
        {
            case EnemyState.ATTACK:
                if (directionToPlayer.magnitude < maxShotRange)
                {
                    stopShooting = true;

                    // menembak ke arah player jika ada di depan musuh
                    if (shotDelay <= 0 && angleToPlayer < maxShotAngle)
                    {
                        firePoint.LookAt(player);
                        GameObject b = Instantiate(bullet, firePoint.position, Quaternion.identity);
                        b.GetComponent<BulletControl55>().Init(firePoint.eulerAngles.y + Random.Range(-missAngle, missAngle));

                        shotDelay = 1 / fireRate;
                        bulletFired++;
                    }
                    else shotDelay -= Time.deltaTime;
                }
                else
                {
                    // reset counter tembakan peluru
                    if (stopShooting)
                    {
                        maxShotBeforeAvoid = Random.Range(10, 20);
                        bulletFired = 0;

                        stopShooting = false;
                    }

                    // menembakkan missile jika player di depan
                    if (missileTimer <= 0 && angleToPlayer < maxShotAngle / 2f && directionToPlayer.magnitude < maxShotRange * 3f && Time.timeSinceLevelLoad > 5f)
                    {
                        if (Random.Range(0, 10) < 1)
                        {
                            GameObject m = Instantiate(missile, firePoint.position, Quaternion.identity);
                            m.GetComponent<MissileControl55>().Init(firePoint.eulerAngles.y, player);
                        }

                        missileTimer = 1f;
                    }
                    else missileTimer -= Time.deltaTime;
                }

                // pindah state jika sudah banyak menembak atau terlalu lama mengejar
                if (bulletFired > maxShotBeforeAvoid || attackTimer > maxAttackTimer)
                {
                    maxAvoidTimer = Random.Range(2f, 5f);
                    avoidTimer = 0f;
                    state = EnemyState.AVOID;
                }
                else attackTimer += Time.deltaTime;

                break;
            case EnemyState.AVOID:
                // pindah state jika sudah terlalu jauh dengan player atau terlalu lama menghindar
                if (avoidTimer >= maxAvoidTimer || directionToPlayer.magnitude > 300f)
                {
                    maxAttackTimer = Random.Range(10f, 20f);
                    attackTimer = 0f;
                    state = EnemyState.ATTACK;
                }
                else avoidTimer += Time.deltaTime;

                break;
        }

        if (health <= 0)
        {
            manager.RemoveEnemy(transform);
            Instantiate(explosion, transform.position, Quaternion.identity);
            GameObject a = Instantiate(tempAudio, transform.position, Quaternion.identity);
            a.GetComponent<TempAudioSource55>().Init(explodeSfx);
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerBullet"))
        {
            health--;
            uiControl.score += 10;
        }
        else if (other.gameObject.CompareTag("PlayerMissile")) {
            health -= 9;
            uiControl.score += 50;
        }
    }
}
