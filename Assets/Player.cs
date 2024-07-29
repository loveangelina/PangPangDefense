using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    
    public List<float> speedLevel = new List<float>();
    public GameObject femalePlayer;
    public GameObject malePlayer;
    public float moveSpeed = 2f; // 이동 속도
    public float positionA = -2f;
    public float positionB = 2f; 
    private int direction = 1; // 이동 방향 (1: 오른쪽, -1: 왼쪽)

    public HPbar slider;
    public int health = 100;
    public List<float> gunRateLevel = new List<float>();
    
    // TODO : 애니메이션 총알 발사 속도 조절 
    
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpGunRate(0);
        ComboManager.Instance.gunRateBonus += UpGunRate;
        ComboManager.Instance.gunRateDown += DownGunRate;
    }

    void Update()
    {
        MoveSideToSide();
    }
    
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    private void MoveSideToSide()
    {
        float newPositionX = transform.position.x + direction * moveSpeed * Time.deltaTime;

        // 새로운 위치가 경계를 벗어났는지 확인
        if (newPositionX > positionB)
        {
            newPositionX = positionB; // 경계를 초과하지 않도록 위치 조정
            direction = -1; // 방향 전환
        }
        else if (newPositionX < positionA)
        {
            newPositionX = positionA; // 경계를 초과하지 않도록 위치 조정
            direction = 1; // 방향 전환
        }

        // 새로운 위치로 이동
        transform.position = new Vector3(newPositionX, transform.position.y, transform.position.z);
    }
    
    // 몬스터가 공격하면 피격효과 
    public void TakeDamage(int damage = 1)
    {
        //Debug.Log("데미지 " + damage);
        health -= damage;
        slider.SetSliderValue(health);

        if (health <= 0)
        {
            // TODO : 게임 시작, 로비, 종료 UI 등 구현
            Debug.Log("게임 종료");
            Time.timeScale = 0f; // 게임 멈춤
            // TODO : 게임 종료 UI 만들기
            //UIManager.Instance.ActivateGameOver();
        }
        
        // TODO : 현재 active된 자식들에게 모두 
        foreach (var player in GetComponentsInChildren<SpawnProjectile>())
        {
            // 피격효과
            player.TakeDamage();
        }
    }

    private void UpGunRate(int level)   // gunRate 바뀌는 건 0~2까지
    {
        if (level == ComboManager.Instance.maxBonusLevel)
        {
            // 플레이어 추가
            femalePlayer.SetActive(true);
            femalePlayer.transform.localPosition = new Vector3(-0.5f, 0, 0);
            malePlayer.transform.localPosition = new Vector3(0.5f, 0, 0);
            
            // 빠른 총알 애니메이션 
            foreach (var player in GetComponentsInChildren<SpawnProjectile>())
            {
                Animator anim = player.GetComponent<Animator>();
               anim.Play("FireAttack");
            }
            return;
        }
        if (level == ComboManager.Instance.maxBonusLevel - 1)
        {
            // 빠른 총알 애니메이션
            foreach (var player in GetComponentsInChildren<SpawnProjectile>())
            {
                Animator anim = player.GetComponent<Animator>();
                anim.speed = 1;
                anim.Play("FireAttack");
            }
            return;
        }
        
        foreach (var player in GetComponentsInChildren<SpawnProjectile>())
        {
            Animator anim = player.GetComponent<Animator>();
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                //anim.SetFloat("gunRate", gunRateLevel[level]);
                anim.speed = gunRateLevel[level];
                // TODO : 나중에 애니메이션 승리 포즈 등으로 바꿀 때 애니메이션 속도 1로 설정하기 
            }
        }
    }

    private void DownGunRate(int level)
    {
        if (level == ComboManager.Instance.maxBonusLevel - 1)
        {
            Debug.Log("플레이어 삭제 " + level);
            // 플레이어 삭제
            femalePlayer.SetActive(false);
            malePlayer.transform.localPosition = Vector3.zero;
            return;
        }
        if (level == ComboManager.Instance.maxBonusLevel - 2)
        {
            // 애니메이션 바꿈
            foreach (var player in GetComponentsInChildren<SpawnProjectile>())
            {
                Animator anim = player.GetComponent<Animator>();
                anim.speed = gunRateLevel[level];
                anim.Play("Attack");
            }
            return;
        }
        
        foreach (var player in GetComponentsInChildren<SpawnProjectile>())
        {
            Animator anim = player.GetComponent<Animator>();
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                //anim.SetFloat("gunRate", gunRateLevel[level]);
                anim.speed = gunRateLevel[level];
                // TODO : 나중에 애니메이션 승리 포즈 등으로 바꿀 때 애니메이션 속도 1로 설정하기 
            }
        }
    }
}
