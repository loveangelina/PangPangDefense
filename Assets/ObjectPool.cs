using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private GameObject monsterPrefab; // 몬스터 프리팹
    private List<GameObject> pool = new List<GameObject>();
    private Transform poolParent; // 풀의 부모가 될 Transform

    // 생성자에서 프리팹과 풀의 크기를 전달받습니다.
    public ObjectPool(GameObject prefab, int initialSize, Transform parent = null)
    {
        monsterPrefab = prefab;
        poolParent = parent;
        InitializePool(initialSize);
    }

    private void InitializePool(int poolSize)
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = GameObject.Instantiate(monsterPrefab, poolParent);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        foreach (var obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }

        GameObject newObj = GameObject.Instantiate(monsterPrefab, poolParent);
        newObj.SetActive(false);
        pool.Add(newObj);
        return newObj;
    }
}
