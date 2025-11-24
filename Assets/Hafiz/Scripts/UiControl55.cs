using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UiControl55 : MonoBehaviour
{
    public RectTransform radarUi;
    public RectTransform spinner;
    public RectTransform hudAngle;
    public Slider[] missileCooldownSlider;
    public Slider speedSlider;
    public Slider healthSlider;
    public GameObject enemyIconPrefab;
    public GameObject missileIconPrefab;
    public GameObject missileWarning;
    public GameObject mainUi;
    public GameObject gameOverUi;
    public GameObject gameMenu;
    public GameObject pauseMenu;
    public GameObject hudUi;
    public GameObject overGWarning;
    public Image damageOverlay;
    public TextMeshProUGUI scoreText;
    public CameraControl55 camControl;
    public Transform plBody;
    public float mapScale = 1f;
    public float minSpinSpeed = 180f;
    public float maxSpinSpeed = 720f;

    [HideInInspector]
    public float damageOverlayTimer = 0f;
    public int score = 0;

    private Transform pl;
    private PlayerControl55 plControl;
    private EnemyManager55 enManager;
    [SerializeField] private Dictionary<Transform, GameObject> enemyIconsUi = new Dictionary<Transform, GameObject>();
    private Dictionary<Transform, GameObject> missileIconsUi = new Dictionary<Transform, GameObject>();
    private Dictionary<Transform, GameObject> otherIconsUi = new Dictionary<Transform, GameObject>();

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        pl = player.transform;
        plControl = player.GetComponent<PlayerControl55>();
        enManager = GameObject.FindGameObjectWithTag("EnemyManager").GetComponent<EnemyManager55>();

        speedSlider.minValue = plControl.minSpeed;
        speedSlider.maxValue = plControl.maxSpeed;
        healthSlider.maxValue = plControl.health;
        missileCooldownSlider[0].maxValue = plControl.missileShotDelay;
        missileCooldownSlider[1].maxValue = plControl.missileShotDelay;
    }

    void Update()
    {
        float spinSpeed = Mathf.Lerp(minSpinSpeed, maxSpinSpeed, (plControl.currentVelocity - plControl.minSpeed) / (plControl.maxSpeed - plControl.minSpeed));

        speedSlider.value = plControl.currentVelocity;
        healthSlider.value = plControl.health;
        missileCooldownSlider[0].value = plControl.missileShotDelay - plControl.missileDelay[0];
        missileCooldownSlider[1].value = plControl.missileShotDelay - plControl.missileDelay[1];
        scoreText.text = "Score: " + score.ToString();

        // mengatur ui first person sesuai mode kamera saat ini
        hudUi.SetActive(camControl.camMode == 1 ? true : false);
        hudAngle.eulerAngles = new Vector3(0f, 0f, -plBody.eulerAngles.z);

        spinner.eulerAngles -= Vector3.forward * spinSpeed * Time.deltaTime;

        SetEnemyIndicator();
        SetEnemyMissileIndicator();
        SetOtherIndicator();

        // memutar radar sesuai rotasi player
        radarUi.eulerAngles = Vector3.forward * pl.eulerAngles.y;

        // mengatur flash damage
        if (damageOverlayTimer > 0)
        {
            damageOverlayTimer -= Time.deltaTime;
            damageOverlay.color = new Color(1f, 0f, 0f, 0.15f * damageOverlayTimer);
        }

        // menampilkan peringatan over-g
        if (plControl.blackOutTimer > 3f && plControl.overG) overGWarning.SetActive(Mathf.Sin(Time.time * 21f) > 0 ? true : false);
        else overGWarning.SetActive(false);

        // menampilkan peringatan missile
        if (enManager.enemieMissiles.Count > 0) missileWarning.SetActive(Mathf.Sin(Time.time * 15f) > 0 ? true : false);
        else missileWarning.SetActive(false);

        // mengaktifkan ui game over jika player kalah
        if (!plControl.isAlive)
        {
            mainUi.SetActive(false);
            gameOverUi.SetActive(true);
        }

        InputControl();
    }

    private void InputControl()
    {
        // input untuk reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }

        // input untuk pause
        if (Input.GetKeyDown(KeyCode.Escape) && plControl.isAlive)
        {
            if (pauseMenu.activeInHierarchy)
            {
                gameMenu.SetActive(true);
                pauseMenu.SetActive(false);
                ResumeGame();
            }
            else
            {
                gameMenu.SetActive(false);
                pauseMenu.SetActive(true);
                PauseGame();
            }
        }
    }

    private bool ContainEnemy(List<object[]> list, Transform enemy) {
        return list.Any(item => (Transform)item[0] == enemy);
    }

    private string DictToStr()
    {
        string result = "";

        foreach (var kvp in enemyIconsUi)
        {
            result += $"[{kvp.Key} : {kvp.Value}], ";
        }

        return result;
    }

    private void SetEnemyIndicator()
    {
        // mengambil daftar musuh dari enemy manager
        var enemiesNow = enManager.enemies;
        var removedEnemy = enemyIconsUi.Keys.Where(e => !ContainEnemy(enemiesNow, e)).ToList();

        // menghapus icon musuh yang sudah kalah dari radar
        foreach (var r in removedEnemy)
        {
            Destroy(enemyIconsUi[r]);
            enemyIconsUi.Remove(r);
        }

        // menambahkan icon radar baru untuk musuh yang baru
        foreach (var enemy in enemiesNow)
        {
            Transform enemyTransform = (Transform)enemy[0];

            if (!enemyIconsUi.ContainsKey(enemyTransform))
            {
                GameObject iconPrefab = (GameObject)enemy[1];
                GameObject icon = Instantiate(iconPrefab, radarUi);
                enemyIconsUi.Add(enemyTransform, icon);
            }
        }

        // menggerakkan icon radar sesuai lokasi relatif musuh ke player
        foreach (var kvp in enemyIconsUi)
        {
            Transform enemy = kvp.Key;
            GameObject icon = kvp.Value;

            if (enemy == null) continue;

            Vector3 offset = enemy.position - pl.position;
            Vector2 radarPos = new Vector2(offset.x, offset.z) * mapScale;

            icon.GetComponent<RectTransform>().anchoredPosition = radarPos;

            if (enemy.gameObject.GetComponent<EnemyControl55>().isTarget) icon.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
            else icon.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.1f);
        }
    }

    private void SetEnemyMissileIndicator()
    {
        // mengambil daftar missile dari enemy manager
        var missilesNow = enManager.enemieMissiles;
        var removedMissile = missileIconsUi.Keys.Where(e => !missilesNow.Contains(e)).ToList();

        // menghapus icon missile yang sudah hilang dari radar
        foreach (var r in removedMissile)
        {
            Destroy(missileIconsUi[r]);
            missileIconsUi.Remove(r);
        }

        if (missilesNow.Count == 0) return;

        // menambahkan icon radar baru untuk missile yang baru
        foreach (var missile in missilesNow)
        {
            if (!missileIconsUi.ContainsKey(missile))
            {
                GameObject icon = Instantiate(missileIconPrefab, radarUi);
                missileIconsUi.Add(missile, icon);
            }
        }

        // menggerakkan icon radar sesuai lokasi relatif musuh ke player
        foreach (var kvp in missileIconsUi)
        {
            Transform missile = kvp.Key;
            GameObject icon = kvp.Value;

            if (missile == null) continue;

            Vector3 offset = missile.position - pl.position;
            Vector2 radarPos = new Vector2(offset.x, offset.z) * mapScale;

            RectTransform iconTransform = icon.GetComponent<RectTransform>();
            iconTransform.anchoredPosition = radarPos;
            iconTransform.localEulerAngles = Vector3.forward * -missile.eulerAngles.y;
        }
    }

    private void SetOtherIndicator()
    {
        // mengambil daftar objek dari enemy manager
        var objNow = enManager.others;
        var removedObj = otherIconsUi.Keys.Where(e => !ContainEnemy(objNow, e)).ToList();

        // menghapus icon objek yang sudah kalah dari radar
        foreach (var r in removedObj)
        {
            Destroy(otherIconsUi[r]);
            otherIconsUi.Remove(r);
        }

        if (objNow.Count == 0) return;

        // menambahkan icon radar baru untuk objek yang baru
        foreach (var obj in objNow)
        {
            Transform enemyTransform = (Transform)obj[0];

            if (!otherIconsUi.ContainsKey(enemyTransform))
            {
                GameObject iconPrefab = (GameObject)obj[1];
                GameObject icon = Instantiate(iconPrefab, radarUi);
                otherIconsUi.Add(enemyTransform, icon);
            }
        }

        // menggerakkan icon radar sesuai lokasi relatif objek ke player
        foreach (var kvp in otherIconsUi)
        {
            Transform obj = kvp.Key;
            GameObject icon = kvp.Value;

            if (obj == null) continue;

            Vector3 offset = obj.position - pl.position;
            Vector2 radarPos = new Vector2(offset.x, offset.z) * mapScale;

            icon.GetComponent<RectTransform>().anchoredPosition = radarPos;
        }
    }

    public void PauseGame()
    {
        AudioListener.pause = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        AudioListener.pause = false;
        Time.timeScale = 1f;
    }

    public void RestartScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        AudioListener.pause = false;
        Time.timeScale = 1f;
    }

    public void BackToMenu(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
        AudioListener.pause = false;
        Time.timeScale = 1f;
    }

    public void ExitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
