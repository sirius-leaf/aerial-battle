using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl55 : MonoBehaviour
{
    public Camera[] cam;
    public Material zoomBlur;
    public ParticleSystem debri;
    public Transform plBody;
    public float rotateDamping = 5;
    public float minFov = 60f;
    public float maxFov = 85f;
    public float camShakeMag = 0.2f;
    public float camShakeFreq = 5f;
    public float zoomBlurMaxIntensity = 0.04f;
    public int camMode = 0;

    [HideInInspector]
    public float explodeShakeMag = 0f;

    private GameObject player;
    private PlayerControl55 plControl;
    private Vector3[] offset;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        plControl = player.GetComponent<PlayerControl55>();
        offset = new Vector3[] { cam[0].gameObject.transform.localPosition, cam[1].gameObject.transform.localPosition };
        camMode = PlayerPrefs.GetInt("camMode", 0);
    }

    void Update()
    {
        float currentAndMaxSpeedRatio = (plControl.currentVelocity - plControl.moveSpeed) / (plControl.maxSpeed - plControl.moveSpeed);
        float camShakeMagMultiplier = Mathf.Lerp(0f, camShakeMag, currentAndMaxSpeedRatio);
        float zoomBlurIntensity = Mathf.Lerp(0f, 1f, currentAndMaxSpeedRatio);
        Vector2 camShake = new Vector2((Mathf.PerlinNoise1D(Time.realtimeSinceStartup * camShakeFreq + 1000f) - 0.5f) * camShakeMag, (Mathf.PerlinNoise1D(Time.realtimeSinceStartup * camShakeFreq) * camShakeMag) - 0.5f) * camShakeMagMultiplier;
        Vector2 explodeCamShake = new Vector2((Mathf.PerlinNoise1D(Time.realtimeSinceStartup * camShakeFreq * 10f + 1000f) * 2f - 1f) * camShakeMag, (Mathf.PerlinNoise1D(Time.realtimeSinceStartup * camShakeFreq * 10f) * camShakeMag) * 2f - 1f) * explodeShakeMag;

        // input untuk pindah mode kamera
        if (Input.GetKeyDown(KeyCode.LeftAlt)) {
            camMode = ++camMode % 2;
            PlayerPrefs.SetInt("camMode", camMode);
            PlayerPrefs.Save();
        }

        // mengaktifkan kamera berdasarkan mode kamera
        switch (camMode)
        {
            case 0:
                cam[0].gameObject.SetActive(true);
                cam[1].gameObject.SetActive(false);

                break;
            case 1:
                cam[1].gameObject.SetActive(true);
                cam[0].gameObject.SetActive(false);

                cam[1].gameObject.transform.localEulerAngles = new Vector3(0f, 0f, plBody.localEulerAngles.z);

                break;
        }

        if (explodeShakeMag > 0) explodeShakeMag = Mathf.MoveTowards(explodeShakeMag, 0f, Time.deltaTime * 1f);

        // mengatur efek kamera berdasarkan kecepatan player
        cam[camMode].fieldOfView = Mathf.LerpUnclamped(minFov, maxFov, currentAndMaxSpeedRatio);
        cam[camMode].gameObject.transform.localPosition = new Vector3(offset[camMode].x + camShake.x + explodeCamShake.x, offset[camMode].y + camShake.y + explodeCamShake.y, offset[camMode].z);
        zoomBlur.SetFloat("_Intensity", zoomBlurIntensity * zoomBlurMaxIntensity);

        if (plControl.currentVelocity > 55f) debri.Play();
        else debri.Pause();
    }

    void LateUpdate()
    {
        if (plControl.isAlive) transform.position = player.transform.position;

        // input untuk menghadapkan kamera ke musuh
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && plControl.target != null && plControl.isAlive)
        {
            Vector3 directionToTarget = plControl.target.position - transform.position;
            Quaternion targetAngle = Quaternion.LookRotation(directionToTarget);

            transform.eulerAngles = Vector3.up * Mathf.LerpAngle(transform.eulerAngles.y, targetAngle.eulerAngles.y, 1 - Mathf.Exp(-rotateDamping * Time.deltaTime));
        }
        else transform.eulerAngles = Vector3.up * Mathf.LerpAngle(transform.eulerAngles.y, player.transform.eulerAngles.y, 1 - Mathf.Exp(-rotateDamping * Time.deltaTime));
    }
}
