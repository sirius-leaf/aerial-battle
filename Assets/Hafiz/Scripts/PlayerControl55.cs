using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Linq;

public class PlayerControl55 : MonoBehaviour
{
    public GameObject bullet;
    public GameObject missile;
    public GameObject explosion;
    public Transform[] missilePoints;
    public Transform firePoint;
    public Transform jetModel;
    public Transform thrust;
    public RectTransform indicatorUi;
    public RectTransform enemyDirIndicatorUi;
    public Camera cam;
    public Camera deathCam;
    public AudioSource audioSrc;
    public AudioSource audioSfxSrc;
    public AudioSource shockSfx;
    public AudioClip bulletSfx;
    public AudioClip missileSfx;
    public AudioClip explodeSfx;
    public CameraControl55 camControl;
    public Volume globalVolume;
    public AudioLowPassFilter lowPassFilter;
    public Material glitchFx;
    public int health = 30;
    public float moveSpeed = 30f;
    public float rotateSpeed = 120f;
    public float fireRate = 5f;
    public float maxSpeed = 80f;
    public float minSpeed = 15f;
    public float acceleration = 10f;
    public float rollAmount = 60f;
    public float rotateDamp = 5f;
    public float maxBulletMiss = 5f;
    public float maxAutoAimRange = 30f;
    public float maxAutoAimAngle = 5f;
    public float missileShotDelay = 2f;

    [HideInInspector] public Transform target;
    [HideInInspector] public float currentVelocity = 0f;
    [HideInInspector] public float[] missileDelay = { 0f, 0f };
    [HideInInspector] public float blackOutTimer = 0f;
    [HideInInspector] public float glitchPower = 0f;
    [HideInInspector] public int maxHealth;
    [HideInInspector] public bool isAlive = true;
    [HideInInspector] public bool overG = false;

    private Quaternion targetRot;
    private Rigidbody rb;
    private AudioSource engineSfxPlayer;
    private EnemyManager55 enManager;
    private UiControl55 uiControl;
    private Vignette vignette;
    private ColorAdjustments colorAdjust;
    private TrailRenderer[] trails;
    private float shotDelay = 0f;
    private float regenDelay = 0f;
    private float fallSpeed = 0f;
    private float explodeTimer = 1.5f;
    private bool exploded = false;
    private bool isBlackOut = false;

    void Start()
    {
        currentVelocity = moveSpeed;
        maxHealth = health;
        rb = GetComponent<Rigidbody>();
        engineSfxPlayer = GetComponent<AudioSource>();
        enManager = GameObject.FindGameObjectWithTag("EnemyManager").GetComponent<EnemyManager55>();
        uiControl = GameObject.FindGameObjectWithTag("UiControl").GetComponent<UiControl55>();

        trails = new TrailRenderer[] {missilePoints[0].gameObject.GetComponent<TrailRenderer>(), missilePoints[1].gameObject.GetComponent<TrailRenderer>()};

        // mengaktifkan post processing
        if (globalVolume.profile.TryGet(out vignette))
        {
            vignette.active = true;
            vignette.intensity.overrideState = true;
        }
        if (globalVolume.profile.TryGet(out colorAdjust))
        {
            colorAdjust.active = true;
            colorAdjust.saturation.overrideState = true;
            colorAdjust.postExposure.overrideState = true;
        }

        FindNearestEnemy();
    }

    void Update()
    {
        Vector2 inputAxis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        float jetRoll = Mathf.LerpAngle(jetModel.localEulerAngles.z, -inputAxis.x * rollAmount, 1 - Mathf.Exp(-rotateDamp * Time.deltaTime));

        if (isAlive)
        {
            if (!isBlackOut && glitchPower < 0.7f)
            {
                // memutar body player sesuai arah belok
                transform.Rotate(Vector3.up * inputAxis.x * rotateSpeed * Time.deltaTime);
                jetModel.localEulerAngles = new Vector3(0f, 0f, jetRoll);

                InputControl(inputAxis);
            } else
            {
                // mengembalikan roll body jika lurus
                jetModel.localEulerAngles = new Vector3(0f, 0f, Mathf.LerpAngle(jetModel.localEulerAngles.z, 0f, 1 - Mathf.Exp(-rotateDamp * Time.deltaTime)));

                currentVelocity = Mathf.MoveTowards(currentVelocity, moveSpeed, 3f * acceleration * Time.deltaTime);
            }
        }
        else
        {
            // mengaktifkan animasi kalah
            deathCam.gameObject.SetActive(true);
            jetModel.localEulerAngles += new Vector3(0f, 0f, 400f * Time.deltaTime);

            fallSpeed += 1f * Time.deltaTime;
            deathCam.transform.LookAt(jetModel.position);

            if (explodeTimer > 0)
            {
                jetModel.localPosition -= new Vector3(0f, fallSpeed, 0f);
                explodeTimer -= Time.deltaTime;
            }
        }

        if (explodeTimer > 0) rb.velocity = transform.forward * currentVelocity;
        else
        {
            // spawn animasi ledakan
            if (!exploded)
            {
                rb.velocity = Vector3.zero;

                jetModel.gameObject.SetActive(false);
                audioSfxSrc.PlayOneShot(explodeSfx);
                Instantiate(explosion, jetModel.position, Quaternion.identity);

                exploded = true;
            }

            // restart scene setelah ledakan
            if (explodeTimer > -1.5f) explodeTimer -= Time.deltaTime;
            else
            {
                Scene currentScene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(currentScene.name);
            }
        }

        overG = currentVelocity > 55f && Mathf.Abs(((jetRoll + 180f) % 360f) - 180f) > 40f ? true : false;

        if (overG || isBlackOut)
        {
            blackOutTimer += Time.deltaTime;

            // menampilkan efek over-g
            lowPassFilter.cutoffFrequency = Mathf.Lerp(22000f, 1000f, (blackOutTimer - 5f) / 1f);
            colorAdjust.saturation.value = Mathf.Lerp(0f, -100f, (blackOutTimer - 3f) / 3f);
            vignette.intensity.value = Mathf.Lerp(0.25f, 1f, (blackOutTimer - 5f) / 1f);
            colorAdjust.postExposure.value = Mathf.Lerp(0f, -5f, (blackOutTimer - 5.5f) / 0.5f);

            // g-loc setelah terlalu lama over-g
            if (blackOutTimer > 6f)
            {
                isBlackOut = true;

                if (blackOutTimer > 8f)
                {
                    // mengembalikan kesadaran
                    isBlackOut = false;
                    blackOutTimer = 3f;
                }
            }
            

            EmitTrails(true);
        }
        else
        {
            // menghentikan effect ketika tidak mengalami over-g
            if (!isBlackOut)
            {
                blackOutTimer = Mathf.MoveTowards(blackOutTimer, 0f, Time.deltaTime);
                lowPassFilter.cutoffFrequency = Mathf.MoveTowards(lowPassFilter.cutoffFrequency, 22000f, 42000f * Time.deltaTime);
                colorAdjust.saturation.value = Mathf.MoveTowards(colorAdjust.saturation.value, 0f, 200f * Time.deltaTime);
                vignette.intensity.value = Mathf.MoveTowards(vignette.intensity.value, 0.25f, 1.5f * Time.deltaTime);
                colorAdjust.postExposure.value = Mathf.MoveTowards(colorAdjust.postExposure.value, 0f, 10f * Time.deltaTime);
            }

            EmitTrails(false);
        }

        // cooldown missile
        if (missileDelay[0] > 0) missileDelay[0] -= Time.deltaTime;
        if (missileDelay[1] > 0) missileDelay[1] -= Time.deltaTime;

        // mengatur suara mesin
        engineSfxPlayer.pitch = Mathf.LerpUnclamped(1f, 2f, (currentVelocity - moveSpeed) / (maxSpeed - moveSpeed));

        // mencari target baru bila target sebelumnya sudah hilang
        if (glitchPower > 0)
        {
            if (target != null) target.gameObject.GetComponent<EnemyControl55>().isTarget = false;
            target = null;
        }
        else if (target == null)
        {
            FindNearestEnemy();
        }

        // mengatur efek glitch
        glitchPower = Mathf.Max(0f, glitchPower - Time.deltaTime * 0.25f);
        glitchFx.SetFloat("_Glitch_Power", glitchPower);
        shockSfx.volume = glitchPower * 0.6f;

        // mengaktifkan suara peringatan missile
        if (enManager.enemieMissiles.Count > 0 && !audioSrc.isPlaying) audioSrc.Play();
        else if (enManager.enemieMissiles.Count <= 0 && audioSrc.isPlaying) audioSrc.Stop();

        // regenerasi nyawa player
        if (regenDelay <= 0)
        {
            health = Mathf.Min(health + 1, maxHealth);

            regenDelay = 2f;
        }
        else regenDelay -= Time.deltaTime;

        if (health <= 0)
        {
            isAlive = false;
        }
    }

    void LateUpdate()
    {
        ShowTargetIndicator();
    }

    private void EmitTrails(bool enable)
    {
        trails[0].emitting = enable;
        trails[1].emitting = enable;
    }

    private void InputControl(Vector2 inputAxis)
    {
        // input untuk menembakkan peluru
        if (Input.GetKey(KeyCode.Z))
        {
            if (shotDelay <= 0)
            {
                AimToTarget();
                GameObject b = Instantiate(bullet, firePoint.position, Quaternion.identity);
                b.GetComponent<BulletControl55>().Init(firePoint.eulerAngles.y);
                audioSfxSrc.PlayOneShot(bulletSfx, 2f);

                shotDelay = 1f / fireRate;
            }
            else shotDelay -= Time.deltaTime;
        }
        else if (shotDelay > 0) shotDelay -= Time.deltaTime;

        // input untuk menembakkan missile
        if (Input.GetKeyDown(KeyCode.X) && (missileDelay[0] <= 0 || missileDelay[1] <= 0))
        {
            Transform missilePoint;

            if (missileDelay[0] <= 0)
            {
                missilePoint = missilePoints[0];
                missileDelay[0] = missileShotDelay;
            }
            else
            {
                missilePoint = missilePoints[1];
                missileDelay[1] = missileShotDelay;
            }

            GameObject m = Instantiate(missile, missilePoint.position, Quaternion.identity);
            m.GetComponent<MissileControl55>().Init(transform.eulerAngles.y, target);
            audioSfxSrc.PlayOneShot(missileSfx);
        }

        // input untuk mencari target baru
        if (Input.GetKeyDown(KeyCode.C))
        {
            FindNearestEnemy();
        }

        // input untuk mengatur kecepatan player
        const float thrustSizeDelta = 0.6f;
        if (inputAxis.y > 0.1 && currentVelocity < maxSpeed)
        {
            currentVelocity = Mathf.MoveTowards(currentVelocity, maxSpeed, acceleration * Time.deltaTime);
            ScaleThrust(1f, thrustSizeDelta);
        }
        else if (inputAxis.y < -0.1 && currentVelocity > minSpeed)
        {
            currentVelocity = Mathf.MoveTowards(currentVelocity, minSpeed, 2.5f * acceleration * Time.deltaTime);
            ScaleThrust(0.2f, thrustSizeDelta);
        }
        else if (Mathf.Abs(currentVelocity - moveSpeed) > 1f)
        {
            currentVelocity = Mathf.MoveTowards(currentVelocity, moveSpeed, acceleration * Time.deltaTime);
            ScaleThrust(0.2f, thrustSizeDelta);
        }

    }

    public void ScaleThrust(float targetSize, float thrustSizeDelta)
    {
        thrust.localScale = new Vector3(1f, 1f, Mathf.MoveTowards(thrust.localScale.z, targetSize, thrustSizeDelta * Time.deltaTime));
    }

    private void FindNearestEnemy()
    {
        // mengambil daftar musuh dari enemy manager
        Transform[] enemies = enManager.enemies.Select(item => (Transform)item[0]).ToArray();
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        // menghentikan kode jika musuh habis
        if (enemies.Length <= 0) return;

        // mencari musuh terdekat
        foreach (Transform enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.position);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }

            enemy.gameObject.GetComponent<EnemyControl55>().isTarget = false;
        }

        // set target ke musuh terdekat
        target = nearestEnemy;
        target.gameObject.GetComponent<EnemyControl55>().isTarget = true;
    }

    private void AimToTarget()
    {
        if (target != null && Vector3.Angle(transform.forward, target.position - transform.position) < maxAutoAimAngle)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            targetRot = Quaternion.LookRotation(direction);
        }
        else
        {
            targetRot = transform.rotation;
        }

        firePoint.rotation = targetRot;
        firePoint.eulerAngles += new Vector3(0f, Random.Range(-maxBulletMiss, maxBulletMiss), 0f);
    }

    private void ShowTargetIndicator()
    {
        if (target != null)
        {
            Vector3 targetScreenPos = camControl.cam[camControl.camMode].WorldToScreenPoint(target.position);

            // menampilkan indikator lokasi musuh jika di depan player
            if (targetScreenPos.z > 0)
            {
                enemyDirIndicatorUi.gameObject.SetActive(false);
                indicatorUi.gameObject.SetActive(true);
                indicatorUi.position = targetScreenPos;
            }
            else // menampilkan panah penunjuk musuh jika di belakang player
            {
                Vector3 direction = target.position - transform.position;
                float angleToEnemy = Mathf.Rad2Deg * Mathf.Atan2(Vector3.Cross(transform.forward, direction).y, Vector3.Dot(transform.forward, direction));

                indicatorUi.gameObject.SetActive(false);
                enemyDirIndicatorUi.gameObject.SetActive(true);
                enemyDirIndicatorUi.eulerAngles = Vector3.forward * -angleToEnemy;
            }
        }
        else // hide semua indikator jika target kosong
        {
            enemyDirIndicatorUi.gameObject.SetActive(false);
            indicatorUi.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EnemyBullet")) {
            health--;
        }
        else if (other.gameObject.CompareTag("EnemyMissile"))
        {
            health -= 12;
            uiControl.damageOverlayTimer = 1f;
            audioSfxSrc.PlayOneShot(explodeSfx);
            camControl.explodeShakeMag = 1f;
        } else if (other.gameObject.CompareTag("ElectricSphere"))
        {
            health -= 6;
            glitchPower = 1f;
        }
    }
}
