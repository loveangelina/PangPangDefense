using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO : Phase 클래스 정의 / 페이즈별로 나타나는 몬스터 종류, 스폰 속도, 스폰할 총 몬스터 수 등
[System.Serializable]
public class PhaseData
{
    public int num; // 페이즈 단계   // TODO : 삭제하기
    public List<GameObject> monsterPrefabs; // 페이즈에 등장할 몬스터 프리팹 목록
    public float spawnInterval; // 몬스터 스폰 간격 (MonsterSpawner의 minSpawnInterval)

    public float duration; // 각 페이즈의 지속 시간
    //public int limitedNum; // 제한하는 아이템 종류 개수

    //public int firingRate; // 플레이어 발사 속도 (딱히 단계별로 정해진 게 아니라 기존 값에서 증감하는 형태)
    //public float playerMoveSpeed;   // 플레이어가 양옆으로 이동하는 속도 <- 이 두 변수는 콤보와 연관있게 
}

public class PhaseManager : MonoBehaviour
{
    public List<PhaseData> phases; // 모든 페이즈 리스트
    private int currentPhaseIndex = 0; // 현재 페이즈 인덱스
    //private int monstersSpawned = 0; // 현재 페이즈에서 스폰된 몬스터 수
    //private bool isPhaseActive = false; // 현재 페이즈가 활성화 상태인지
    public Vector3[] spawnPosMiddle;
    public Vector3 spawnPosBoss;

    private Coroutine spawnCoroutine;
    private MonsterSpawner monsterSpawner;

    private void Start()
    {
        monsterSpawner = FindObjectOfType<MonsterSpawner>();
        StartPhase(currentPhaseIndex);
    }

    private void StartPhase(int phaseIndex)
    {
        if (phaseIndex >= phases.Count)
        {
            Debug.Log("모든 페이즈 완료");
            // TODO : clear ui 만들기
            return;
        }

        PhaseData currentPhase = phases[phaseIndex];
        //Debug.Log("Starting Phase: " + currentPhase.num);
        
        if (monsterSpawner != null)
        {
            monsterSpawner.SetPhase(currentPhase);
        }

        //monstersSpawned = 0;
        //isPhaseActive = true;

        spawnCoroutine = StartCoroutine(SpawnMonsters(currentPhase));
    }

    private IEnumerator SpawnMonsters(PhaseData phase)
    {
        float phaseDuration = phase.duration; 
        float elapsedTime = 0f;
        while (elapsedTime  < phaseDuration)
        {
            //monstersSpawned++;
            yield return new WaitForSeconds(phase.spawnInterval);
            elapsedTime += phase.spawnInterval;
        }
        
        // 마지막에 중간급 or 보스 몬스터 생성
        if (phase.monsterPrefabs[1] != null)
        {
            GameObject middleMonster = Instantiate(phase.monsterPrefabs[1], monsterSpawner.transform);
            middleMonster.transform.position = spawnPosMiddle[0];
            middleMonster = Instantiate(phase.monsterPrefabs[1], monsterSpawner.transform);
            middleMonster.transform.position = spawnPosMiddle[1];
        }

        if (phase.monsterPrefabs[2] != null)
        {
            GameObject bossMonster = Instantiate(phase.monsterPrefabs[2], monsterSpawner.transform);
            bossMonster.transform.position = spawnPosBoss;
        }

        //isPhaseActive = false;
        NextPhase();
    }

    private void NextPhase()
    {
        /*if (!isPhaseActive && spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }*/

        currentPhaseIndex++;

        if (currentPhaseIndex < phases.Count)
        {
            StartPhase(currentPhaseIndex);
        }
        else
        {
            Debug.Log("모든 페이즈 완료");
        }
    }
}
