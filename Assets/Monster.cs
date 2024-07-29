using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int damage);
}

public class Monster : MonoBehaviour, IDamageable
{
    private Animator animator;
    public float moveSpeed = 2f;
    private bool isAttacking = false;   // isDying의 역할을 같이 함
    
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
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

    private void OnCollisionEnter(Collision other)
    {
        // 최초 총알 맞았을 때 한번만 실행되도록
        // TODO : enum state로 상태 나눠서 해당 상태로 가는건 bool 변수 안써도 되는지?
        if (isAttacking)
            return;
        
        if (other.gameObject.CompareTag("Bullet"))
        {
            TakeDamage();
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (isAttacking)
            return;
        
        animator.Play("GetHit");
        isAttacking = true;
    }

    private void OnEnable()
    {
        isAttacking = false;
    }

    // 몬스터의 Attack 애니메이션 이벤트 함수
    public void ApplyDamage()
    {
        Player.Instance.TakeDamage();
    }
}
