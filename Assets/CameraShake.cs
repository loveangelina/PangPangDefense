using System.Collections;
using System.Collections.Generic;
using CartoonFX;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }
    
    public float shakeAmount;
    public float shakeTime;
    private float currentShakeTime;
    private Vector3 initialPos;
    private Coroutine shakeCoroutine;
    
    void Awake()
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
        initialPos = transform.position;
        currentShakeTime = shakeTime;
    }
    
    public void SetShakeAmount(float time)
    {
        shakeAmount = time;
        currentShakeTime = shakeAmount;
    }

    public void Shake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(ShakeCamera());
    }

    IEnumerator ShakeCamera()
    {
        while (currentShakeTime > 0)
        {
            transform.position = Random.insideUnitSphere * shakeAmount + initialPos;
            currentShakeTime -= Time.deltaTime;
            yield return null;
        }

        transform.position = initialPos;
        currentShakeTime = shakeTime;
    }
}
