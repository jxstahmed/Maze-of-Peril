using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class WeaponsManager : MonoBehaviour
{

    public static WeaponsManager Instance;
    
   
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int FindWeaponIndex(string id)
    {
        Debug.Log("WeaponSystem: FindWeaponIndex(" + id + ")");
        if (id == null) return -1;

        int index = -1;

        for (int i = 0; i < GameManager.Instance.WeaponsPackData.Swords.Count; i++)
        {
            if (GameManager.Instance.WeaponsPackData.Swords[i].ID.Contains(id))
            {
                index = i;
                break;
            }
        }

        return index;
    }
}
