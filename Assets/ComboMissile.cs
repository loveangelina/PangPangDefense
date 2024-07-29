using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ComboMissile : MonoBehaviour
{
    [SerializeField] protected float speed = 15f;
    [SerializeField] protected float hitOffset = 0f;
    [SerializeField] protected bool UseFirePointRotation;
    [SerializeField] protected Vector3 rotationOffset = new Vector3(0, 0, 0);
    [SerializeField] protected GameObject hit;
    [SerializeField] protected ParticleSystem hitPS;
    [SerializeField] protected GameObject flash;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected Collider col;
    [SerializeField] protected Light lightSourse;
    [SerializeField] protected GameObject[] Detached;
    [SerializeField] protected ParticleSystem projectilePS;
    private bool startChecker = false;
    [SerializeField]protected bool notDestroy = false;
    [SerializeField] private float degree = 45f;    // 포물선으로 날아갈 각도
    private Vector3 startPos;
    [SerializeField] private Vector3[] targetPositions;
    private Vector3 targetPos;
    [SerializeField] private float heightArc = 2f;
    public bool isTouched = false;  // 목표 위치에 닿았는지
    public float explosionRadius = 5f; // 폭발 반경
    public GameObject explosionPrefab;

    protected virtual void Start()
    {
        if (!startChecker)
        {
            if (flash != null)
            {
                flash.transform.parent = null;
            }
        }
        if (notDestroy)
            StartCoroutine(DisableTimer(5));
        else
            Destroy(gameObject, 5);
        startChecker = true;

        startPos = transform.position;

        // 목표 위치 중 랜덤한 위치로 
        targetPos = targetPositions[Random.Range(0, targetPositions.Length)];
    }

    protected virtual IEnumerator DisableTimer(float time)
    {
        yield return new WaitForSeconds(time);
        if(gameObject.activeSelf)
            gameObject.SetActive(false);
        yield break;
    }

    protected virtual void OnEnable()
    {
        if (startChecker)
        {
            if (flash != null)
            {
                flash.transform.parent = null;
            }
            if (lightSourse != null)
                lightSourse.enabled = true;
            col.enabled = true;
            rb.constraints = RigidbodyConstraints.None;
            
            // 포물선으로 다리 중앙 쪽으로 날아감
        }
    }
    
    private void LaunchProjectile()
    {
        float rad = degree * Mathf.Deg2Rad;

        // x와 y 방향의 속도를 계산
        float horizontalSpeed = speed * Mathf.Cos(rad);
        float verticalSpeed = speed * Mathf.Sin(rad);
        rb.velocity = new Vector3(0, verticalSpeed, horizontalSpeed);

        // 중력의 영향을 받도록 설정
        rb.useGravity = true;
    }

    private void FixedUpdate()
    {
        Vector3 currentPosition = transform.position;

        float totalDistance = Vector3.Distance(startPos, targetPos);

        // x, y, z 방향으로 이동 거리 계산
        float nextX = Mathf.MoveTowards(currentPosition.x, targetPos.x, speed * Time.deltaTime);
        float nextY = Mathf.MoveTowards(currentPosition.y, targetPos.y, speed * Time.deltaTime);
        float nextZ = Mathf.MoveTowards(currentPosition.z, targetPos.z, speed * Time.deltaTime);

        // 3D 공간에서의 이동 비율 계산
        float distanceTravelled = Vector3.Distance(startPos, new Vector3(nextX, nextY, nextZ));
        float progress = distanceTravelled / totalDistance;

        // Y좌표를 위한 선형 보간
        float baseY = Mathf.Lerp(startPos.y, targetPos.y, progress);

        // 포물선 높이 계산
        float arc = heightArc * progress * (1 - progress);

        // 다음 위치 계산
        Vector3 nextPosition = new Vector3(nextX, baseY + arc, nextZ);

        // Rigidbody의 속도를 다음 위치로 설정하여 이동
        Vector3 direction = nextPosition - currentPosition;
        rb.velocity = direction.normalized * speed;

        if (!isTouched && Vector3.Distance(nextPosition, targetPos) < 1f)
            Arrived();
    }
    
    void Arrived()
    {
        isTouched = true;
        HitFloor();
    }

    void HitFloor()
    {
        // 연기 이펙트
        GameObject explosion = Instantiate(explosionPrefab);
        explosion.transform.position = targetPos;
        AudioManager.Instance.EffectPlay("MissileCollision");
        
        // collision 시 충돌
        rb.constraints = RigidbodyConstraints.FreezeAll;
        if (lightSourse != null)
            lightSourse.enabled = false;
        col.enabled = false;
        projectilePS.Stop();
        projectilePS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        
        //CameraShake.Instance.SetShakeAmount(shakeAmount);
        CameraShake.Instance.Shake();

        //Removing trail from the projectile on cillision enter or smooth removing. Detached elements must have "AutoDestroying script"
        foreach (var detachedPrefab in Detached)
        {
            if (detachedPrefab != null)
            {
                ParticleSystem detachedPS = detachedPrefab.GetComponent<ParticleSystem>();
                detachedPS.Stop();
            }
        }
        
        // 폭발 반경 내의 모든 몬스터를 죽임
        Collider[] colliders = Physics.OverlapSphere(targetPos, explosionRadius);

        foreach (Collider collider in colliders)
        {
            IDamageable  monster = collider.GetComponent<IDamageable>();
            if (monster != null)
            {
                monster.TakeDamage(10);
            }
        }
        
        if (notDestroy)
            StartCoroutine(DisableTimer(hitPS.main.duration));
        else
        {
            if (hitPS != null)
            {
                Destroy(gameObject, hitPS.main.duration);
            }
            else
                Destroy(gameObject, 1);
        }
    }
}
