using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiddleMonster : MonoBehaviour
{
    public int health = 25;
    
    private Animator animator;
    public float moveSpeed = 2f;
    private bool isAttacking = false;   // isDying의 역할을 같이 함
    
    void Start()
    {
        animator = GetComponent<Animator>();
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
        if (distanceToPlayer <= 2f)
        {
            if (!isAttacking)
            {
                // 처음으로 공격 상태에 진입했을 때만 트리거 설정
                animator.Play("Attack");
                isAttacking = true; 
            }
        }
        else if (distanceToPlayer < 6f)
        {
            // 플레이어와 가까워지면 플레이어에게 이동
            transform.position = Vector3.MoveTowards(transform.position, playerPos, moveSpeed * Time.deltaTime);
            transform.LookAt(playerPos);
        }
        else
        {
            transform.position -= Vector3.forward * moveSpeed * Time.deltaTime;
            transform.LookAt(playerPos);
        }
    }

    public void TakeDamage(int damage = 0)
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
        
        animator.Play("GetHit");
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
        // TODO : 애니메이션 attack 시 소리 추가 
        Player.Instance.TakeDamage(10);
    }
}
