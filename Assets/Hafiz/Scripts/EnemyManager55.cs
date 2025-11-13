using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager55 : MonoBehaviour
{
    public GameObject enemy;
    public GameObject drone;
    public int maxEnemyCount = 6;
    public int minEnemyCount = 3;
    public float[] enemySpawnDist = new float[] { 100f, 250f };
    public float[] droneSpawnDelay = new float[] { 15f, 40f };
    // public float minEnemySpawnDist = 100f;
    // public float maxEnemySpawnDist = 250f;
    public bool spawnOneEnemy = false;

    [HideInInspector]
    public List<object[]> enemies = new();
    [HideInInspector]
    public List<object[]> others = new();
    [HideInInspector]
    public List<Transform> enemieMissiles = new();

    private Transform plTransform;
    private float droneSpawnTimer = 30f;
    private int targetMinEnemyCount = 2;
    private int targetEnemyCount;
    
    void Start()
    {
        plTransform = GameObject.FindGameObjectWithTag("Player").transform;
        targetEnemyCount = Random.Range(minEnemyCount, maxEnemyCount);
        droneSpawnTimer = Random.Range(droneSpawnDelay[0], droneSpawnDelay[1]);

        if (!spawnOneEnemy) for (int i = 0; i < minEnemyCount; i++)
            {
                AddEnemy();
            }
        else AddEnemy();
    }

    void Update()
    {
        if (droneSpawnTimer > 0) droneSpawnTimer -= Time.deltaTime;
        else
        {
            AddDrone();

            droneSpawnTimer = Random.Range(droneSpawnDelay[0], droneSpawnDelay[1]);
        }
    }

    private void AddEnemy()
    {
        float spawnAngle = Random.Range(0f, Mathf.PI * 2f);
        float spawnDistance = Random.Range(enemySpawnDist[0], enemySpawnDist[1]);
        Vector3 spawnPosOffset = new Vector3(Mathf.Sin(spawnAngle) * spawnDistance, 0f, Mathf.Cos(spawnAngle) * spawnDistance);
        GameObject e = Instantiate(enemy, plTransform.position + spawnPosOffset, Quaternion.identity);
        
        enemies.Add(new object[] {e.transform, e.GetComponent<EnemyControl55>().radarIcon});
    }

    public void RemoveEnemy(Transform enemy)
    {
        enemies.RemoveAll(item => (Transform)item[0] == enemy);

        RespawnEnemy();
    }

    private void RespawnEnemy()
    {
        if (!spawnOneEnemy && enemies.Count <= targetMinEnemyCount)
        {
            for (int i = 0; i < targetEnemyCount - enemies.Count; i++)
            {
                AddEnemy();
            }

            targetMinEnemyCount = Random.Range(2, maxEnemyCount - 1);
            targetEnemyCount = Random.Range(targetMinEnemyCount + 1, maxEnemyCount);
        }
    }

    public void AddMissile(Transform missile)
    {
        if (!enemieMissiles.Contains(missile)) enemieMissiles.Add(missile);
    }

    public void RemoveMissile(Transform missile)
    {
        if (enemieMissiles.Contains(missile)) enemieMissiles.Remove(missile);
    }

    private void AddDrone()
    {
        float spawnAngle = Random.Range(0f, Mathf.PI * 2f);
        float spawnDistance = Random.Range(enemySpawnDist[0], enemySpawnDist[1]);
        Vector3 spawnPosOffset = new Vector3(Mathf.Sin(spawnAngle) * spawnDistance, 0f, Mathf.Cos(spawnAngle) * spawnDistance);
        GameObject e = Instantiate(drone, plTransform.position + spawnPosOffset, Quaternion.identity);

        others.Add(new object[] { e.transform, e.GetComponent<DroneControl55>().radarIcon });
    }

    public void AddOther(Transform objTransform, GameObject icon)
    {
        others.Add(new object[] { objTransform, icon });
    }

    public void RemoveOther(Transform enemy)
    {
        others.RemoveAll(item => (Transform)item[0] == enemy);
    }
}
