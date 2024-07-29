using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMonster : MonoBehaviour, IDamageable
{
    // TODO :  Monster 스크립트랑 합치기 (일반 몬스터는 체력 1, 인터페이스로 만들어서 특정함수만 서로 다르게 오버라이드하기)
    public int health = 2;
    public GameObject slashEffect;
    public float attackDistance = 7f;    // 플레이어 공격하는 거리 
    
    private Animator animator;
    public float moveSpeed = 2f;
    private bool isAttacking = false;   // isDying의 역할을 같이 함
    
    private AudioSource audioSource;
    public List<AudioClip> audioClips = new List<AudioClip>();
    
    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isAttacking)
            return;
        
        Move();
    }
    
    void Move()
    {
        Vector3 playerPos = Player.Instance.GetPosition();  // 몬스터가 이동할 목표 위치
        float distanceToPlayer = Vector3.Distance(transform.position, playerPos);
        if (distanceToPlayer <= attackDistance)
        {
            // 처음으로 공격 상태에 진입했을 때만 트리거 설정
            animator.Play("Attack");
            Debug.Log("보스 공격");
            Invoke("DestroyMonster", 2f);
            isAttacking = true; 
        }
        else
        {
            transform.position -= Vector3.forward * moveSpeed * Time.deltaTime;
            transform.LookAt(playerPos);
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (isAttacking)
            return;

        health -= damage;
        if (health <= 0)
        {
            isAttacking = true;
            animator.Play("Die");
            Invoke("DestroyMonster", 3f);
            return;
        }
        
        //animator.Play("GetHit");
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (isAttacking)
            return;
        
        if (other.gameObject.CompareTag("Bullet"))
        {
            TakeDamage(1);
        }
    }

    private void DestroyMonster()
    {
        Destroy(gameObject);
    }
    
    // 몬스터의 Attack 애니메이션 이벤트 함수
    public void ApplyDamage()
    {
        slashEffect.SetActive(true);
        PlaySFX("Slash");
        Player.Instance.TakeDamage(50);
    }


    public void SoundLeftStep()
    {
        PlaySFX("BossLeftStep");
    }
    
    public void SoundRightStep()
    {
        PlaySFX("BossRightStep");
    }

    private void PlaySFX(string clipName)
    {
        audioSource.clip = audioClips.Find(clip => clip.name == clipName);
        //audioSource.loop = false;
        audioSource.Play();
    }
}
