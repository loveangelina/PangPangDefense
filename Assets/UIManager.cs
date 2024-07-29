using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    public GameObject refreshImage;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void ActivateRefreshImage()
    {
        refreshImage.SetActive(true);
    }
    
    public void DeactivateRefreshImage()
    {
        refreshImage.SetActive(false);
    }

    public void ActivateGameOver()
    {
        
    }
}
