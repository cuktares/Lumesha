using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public GameObject[] enemyPrefabs;
        public int enemyCount;
        public float spawnInterval;
        public float waveDuration;
    }

    [Header("Dalga Ayarlari")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float timeBetweenWaves = 5f;

    [Header("Referanslar")]
    [SerializeField] private CardManager cardManager;

    private int currentWaveIndex = 0;
    private bool isSpawning = false;

    public int CurrentWave => currentWaveIndex + 1;

    private void Start()
    {
        StartCoroutine(StartWaveSystem());
    }

    private IEnumerator StartWaveSystem()
    {
        while (currentWaveIndex < waves.Length)
        {
            yield return StartCoroutine(SpawnWave(waves[currentWaveIndex]));

            yield return new WaitForSeconds(timeBetweenWaves);

            currentWaveIndex++;

            // 2. dalgadan sonra kart düşürme sistemini aktifleştir
            if (currentWaveIndex >= 2 && cardManager != null)
            {
                cardManager.ActivateCardDrops();
            }
        }
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        isSpawning = true;

        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemy(wave);
            yield return new WaitForSeconds(wave.spawnInterval);
        }

        yield return new WaitForSeconds(wave.waveDuration);
        isSpawning = false;
    }

    private void SpawnEnemy(Wave wave)
    {
        if (spawnPoints.Length == 0) return;

        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject randomEnemyPrefab = wave.enemyPrefabs[Random.Range(0, wave.enemyPrefabs.Length)];

        Instantiate(randomEnemyPrefab, randomSpawnPoint.position, Quaternion.identity);
    }

    public int GetCurrentWave()
    {
        return currentWaveIndex;
    }
}