using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPbar : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    public Slider effectSlider; // 슬라이더 뒤쪽에서 데미지 감소 이펙트를 주는 슬라이더
    private Slider slider;
    private Coroutine lerpCoroutine;

    private void Start()
    {
        slider = GetComponent<Slider>();
    }

    // 움직임 이후에 HpBar를 출력
    private void LateUpdate()
    {
        var screenPos = Camera.main.WorldToScreenPoint(player.position + offset); //월드좌표(3D)를 스크린좌표(2D)로 변경, offset은 오브젝트 머리 위치
        slider.gameObject.transform.position = screenPos; //그 좌표를 localPos에 저장, 거기에 hpbar를 출력
    }

    public void SetSliderValue(int health)
    {
        if (lerpCoroutine != null)
        {
            StopCoroutine(lerpCoroutine);
        }

        lerpCoroutine = StartCoroutine(LerpSliderValue(health));
    }
    
    private IEnumerator LerpSliderValue(float targetValue)
    {
        // 앞쪽 슬라이더 값이 바로 줄어들음 
        slider.value = targetValue;
        
        // 뒤쪽 이펙트 슬라이더의 값이 서서히 줄어들음
        float startValue = effectSlider.value;
        float timeElapsed = 0f;
        float duration = 0.5f; // 슬라이더가 변경되는 시간 (0.5초)

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            effectSlider.value = Mathf.Lerp(startValue, targetValue, timeElapsed / duration);
            yield return null;
        }
        
        // 최종 값을 보장하기 위해 반복이 끝난 후 값을 설정
        effectSlider.value = targetValue;
    }

}
