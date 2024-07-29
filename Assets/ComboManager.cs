using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance { get; private set; }
    public GameObject blueMissile;
    public GameObject firePoint;    // 지원사격 : 플레이어 뒤쪽
    public GameObject[] firePoints;
    public TextMeshProUGUI comboText;
    public float animationDuration = 1.0f;
    
    private Queue<float> matchTimestamps = new Queue<float>();
    private float timeSinceLastCombo = 0f;
    private const float resetTimeLimit = 10.0f; // 콤보 초기화 시간 간격
    private float lastMatchTime = 0f;
    public int requiredMatches = 3;
    public int comboCount;
    // TODO : 썰매 상점 만들어서 그거랑 연동하기
    //private int playerSpeedLevel = 0;
    // TODO : 프로퍼티로 만들어서 레벨에 따라 플레이어에 이펙트 주기 (최고레벨은 불나는 이펙트)
    public int currentGunRateLevel = 0; // TODO : private으로 바꾸기
    public int maxBonusLevel = 4;  // (0~4) 총 5단계
    public Action<int> gunRateBonus;
    public Action<int> gunRateDown;
    private Coroutine comboTextCoroutine;
    public int ComboCount
    {
        get { return comboCount; }
        private set
        {
            comboCount = value;

            /* if (comboTextCoroutine != null)
            {
                StopCoroutine(comboTextCoroutine);
            }
            StartCoroutine(AnimateText());
            comboText.text = comboCount.ToString();*/

            if (comboCount > 0 && comboCount % 10 == 0)
            {
                OnComboTen();
            }
        }
    }
    
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

    void Start()
    {
        ComboCount = 0;
        lastMatchTime = Time.time;
    }
    
    // 플레이어 개수는 어떤 조건에서 증가시켜?
    // 총알 속도 0.5 -> 총알 속도 1 -> 총알 속도 2 -> 빠른 총알 0.5 -> 빠른 총알 1 -> 플레이어 2명 (빠른 총알 1) (불타는 효과)
    // 대신 초기화되면 한단계씩 내림 

    void Update()
    {
        // 테스트용
        /*if (Input.GetKeyDown(KeyCode.A))
        {
            HandleComboTen();
        }*/
        
        // 현재 시간이 마지막 매치 시간에서 10초가 넘었으면 콤보 초기화
        if (Time.time - lastMatchTime >= resetTimeLimit)
        {
            ResetComboCount();
            lastMatchTime = Time.time;
        }
        
        // TODO : currentLevel이 max면 브금 속도가 빨라짐 & 불타는 이펙트 (불타는 이펙트는 레벨3도 )
    }
    
    // 콤보가 10의 배수일 때마다 지원사격
    void OnComboTen()
    {
        AudioManager.Instance.EffectPlay("Missile");
        // 스페셜 프로젝타일 발사
        if (firePoint != null)
        {
            StartCoroutine(ContinuousAttack());
        }
    }

    IEnumerator ContinuousAttack()
    {
        // 시간 간격을 두고 3발 연속 발사
        Instantiate(blueMissile, firePoints[Random.Range(0, firePoints.Length)].transform.position, blueMissile.transform.rotation);
        yield return new WaitForSeconds(0.25f);
        
        Instantiate(blueMissile, firePoints[Random.Range(0, firePoints.Length)].transform.position, blueMissile.transform.rotation);
        yield return new WaitForSeconds(0.1f);
        
        Instantiate(blueMissile, firePoints[Random.Range(0, firePoints.Length)].transform.position, blueMissile.transform.rotation);
    }

    public void IncreaseComboCount()
    {
        if (ComboCount > 0)
        {
            lastMatchTime = Time.time;
        }
        
        ComboCount++;

        // 첫번째 콤보를 큐에 넣음
        if (matchTimestamps.Count == 0)
        {
            matchTimestamps.Enqueue(Time.time);
        }
        // 두번째매치 이후부터는 이전 콤보와의 시간을 비교해서 10초 이내여야 큐에 넣음
        else if (Time.time - matchTimestamps.Peek() < 10f)
        {
            matchTimestamps.Enqueue(Time.time);

            if (matchTimestamps.Count >= requiredMatches)
            {
                if (currentGunRateLevel < maxBonusLevel)
                {
                    currentGunRateLevel++;
                    GiveBonus();
                }
                matchTimestamps.Clear(); // 보너스를 준 후 큐 초기화
            }
        }
        else
        {
            // 콤보 시간 큐 초기화
            matchTimestamps.Clear();
        }
        
        // 디버그용
        /*string str = "";
        foreach (var time in matchTimestamps)
        {
            str += time + ", ";
        }
        Debug.Log(str);*/
    }
    
    void GiveBonus()
    {
        Debug.Log("Bonus Awarded! : 현재 총 rate level : " + currentGunRateLevel);
        // 보너스 로직 구현
        
        AudioManager.Instance.EffectPlay("ComboLevelUp");

        Debug.Log($"총알 나가는 속도 증가 / 현재 단계{currentGunRateLevel}");
        gunRateBonus.Invoke(currentGunRateLevel);
    }

    void DowngradeBonus()
    {
        if (currentGunRateLevel == 0)
            return;

        Debug.Log($"총알 나가는 속도 감소 / 현재 단계{currentGunRateLevel}");
        gunRateDown.Invoke(currentGunRateLevel);
    }

    public void ResetComboCount()
    {
        ComboCount = 0;
        Debug.Log("콤보 초기화");
        
        AudioManager.Instance.EffectPlay("ComboLevelDown");
        
        // 콤보 큐 초기화
        matchTimestamps.Clear();
        
        if (currentGunRateLevel > 0)
        {
            currentGunRateLevel--;
            DowngradeBonus();
            
            // 콤보 초기화 된 후 update에서 또 초기화 되는 것을 방지해주기 위함 
            lastMatchTime = Time.time;
        }
    }
    
    IEnumerator AnimateText()
    {
        Vector3 initialScale = comboText.transform.localScale;
        Vector3 startPos = comboText.rectTransform.anchoredPosition;
        Vector3 targetScale = initialScale * 1.2f;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;
            float easedT = EaseOutElastic(t);

            comboText.transform.localScale = Vector3.Lerp(initialScale, targetScale, easedT);
            //comboText.rectTransform.anchoredPosition = startPos + new Vector3(0, easedT, 0); // 5f는 점프 높이

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 텍스트 크기가 커졌다가 다시 원래대로 작아짐
        animationDuration = 0.2f;
        elapsedTime = 0f; 
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            comboText.transform.localScale = Vector3.Lerp(targetScale, initialScale, t);

            yield return null;
        }

        comboText.transform.localScale = initialScale;
    }
    
    private float EaseOutElastic(float x)
    {
        const float c4 = (2 * Mathf.PI) / 3;

        return x == 0
            ? 0
            : x == 1
                ? 1
                : Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10 - 0.75f) * c4) + 1;
    }
}
