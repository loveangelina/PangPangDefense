using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateUI : MonoBehaviour
{
    public float rotationSpeed = 100f; // 초당 회전 속도

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        rectTransform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
}
