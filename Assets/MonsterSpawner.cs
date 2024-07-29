using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject[] monsterPrefab; // 몬스터 프리팹
    public Vector3 spawnAreaCenter; // 스폰 영역의 중심
    public Vector3 spawnAreaSize; // 스폰 영역의 크기
    //public float initialSpawnInterval = 2f; // 초기 몬스터 생성 간격
    //public float minSpawnInterval = 0.5f; // 최소 몬스터 생성 간격
    public float spawnAcceleration = 0.1f; // 스폰 간격 감소 속도
    public int poolSize = 30; // 오브젝트 풀 크기

    private ObjectPool objectPool;
    private float currentSpawnInterval;
    private PhaseData currentPhase;
    private Coroutine spawnCoroutine;

    void Start()
    {
        spawnCoroutine = StartCoroutine(SpawnMonsters());
    }
    
    public void SetPhase(PhaseData phase)
    {
        // 이전 페이즈의 풀 초기화
        //ClearObjectPool();
        
        currentPhase = phase;
        InitializeSpawner();
        
        // 기존 코루틴이 실행 중인 경우 중지
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    
        // 새로운 코루틴 시작
        spawnCoroutine = StartCoroutine(SpawnMonsters());
    }

    private void InitializeSpawner()
    {
        objectPool = new ObjectPool(currentPhase.monsterPrefabs[0], poolSize, transform);
        currentSpawnInterval = currentPhase.spawnInterval;
    }

    IEnumerator SpawnMonsters()
    {
        while (true)
        {
            if (currentPhase == null)
            { 
                yield return null; // PhaseData가 설정되지 않은 경우 대기
                continue;
            }
            
            // 오브젝트 풀에서 몬스터 가져오기
            GameObject monster = objectPool.GetPooledObject();
            if (monster != null)
            {
                monster.transform.position = GetRandomSpawnPoint();
                monster.SetActive(true);
            }

            yield return new WaitForSeconds(currentSpawnInterval);

            // 시간이 지남에 따라 스폰 간격을 줄임
            if (currentSpawnInterval > currentPhase.spawnInterval)
            {
                currentSpawnInterval -= spawnAcceleration;
            }
        }
    }
    
    Vector3 GetRandomSpawnPoint()
    {
        float randomX = Random.Range(spawnAreaCenter.x - spawnAreaSize.x / 2, spawnAreaCenter.x + spawnAreaSize.x / 2);
        float randomY = spawnAreaCenter.y; // Y축은 고정 (2D 게임인 경우)
        float randomZ = Random.Range(spawnAreaCenter.z - spawnAreaSize.z / 2, spawnAreaCenter.z + spawnAreaSize.z / 2);

        return new Vector3(randomX, randomY, randomZ);
    }
}
