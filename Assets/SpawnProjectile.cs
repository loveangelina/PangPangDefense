using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnProjectile : MonoBehaviour
{
    // TODO : 중복되는 것들은 player로 빼기 (vfx, projectile, flashduration 등)
    public GameObject firePoint;
    public List<GameObject> vfx = new List<GameObject>();   // 나중에 총알 종류 많아질 거 대비
    public Material[] originalMaterial;
    public Material[] redMaterial;
    private GameObject projectile;          // 총알
    private SkinnedMeshRenderer[] skinnedMeshRenderer;
    private float timeSinceLastShot = 0f; // 마지막 발사 이후 경과 시간
    public float flashDuration = 0.2f; // 피격 효과 지속 시간
    public float fireRate = 1f;  // 발사 시간 간격
    
    void Start()
    {
        skinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();
        projectile = vfx[0];
    }

    void Update()
    {
       /*timeSinceLastShot += Time.deltaTime;   
        // 발사 시간 간격에 따라 총알 발사
        if (timeSinceLastShot >= fireRate)
        {
            SpawnVFX();
            timeSinceLastShot = 0f; // 발사 후 경과 시간 초기화
        }*/

    }

    // 애니메이션에서 호출되는 함수 
    // TODO : firepoint 배열아닌 변수로 만들기 
    public void SpawnBullet()
    {
        //Debug.Log("총알이 발사될 때의 player 위치 : " + transform.parent.position);
        if (firePoint != null)
        {
            Instantiate(projectile, firePoint.transform.position, projectile.transform.rotation);
            AudioManager.Instance.EffectPlay("GunShoot");
            
            // TODO : projectile 오브젝트가 아무것에도 콜라이더 되지 않았을때 일정z 이상 지나가면 destroy되게 스크립트 넣어주기 
            // 아니면 저멀리 안보이는 벽 만들어서 자동으로 닿게 (어떤게 더나을지는 나중에 물어보기)
        }
    }

    public void TakeDamage()
    {
        // TODO : 사운드 추가 
        
        // 몬스터와 부딪히면 피격효과
        for (int i = 0; i < originalMaterial.Length; i++)
        {
            skinnedMeshRenderer[i].material = redMaterial[i];
        }

        // 일정 시간이 지난 후 원래 색상으로 돌아오게 합니다.
        Invoke("ResetColor", flashDuration);
    }
    
    void ResetColor()
    {
        for (int i = 0; i < originalMaterial.Length; i++)
        {
            skinnedMeshRenderer[i].material = originalMaterial[i];
        }
    }
}
