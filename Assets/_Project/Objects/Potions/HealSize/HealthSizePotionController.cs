using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSizePotionController : MonoBehaviour
{
    [SerializeField] public string ID;
    [SerializeField] public int HealthSizeAmount;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(GameManager.Instance.PlayerTag))
        {
            GameManager.Instance.AffectPlayerHealthSize(HealthSizeAmount);
            ObjectiveManager.Instance.CollectPotion(ID);
            GameObject.Destroy(gameObject);
        }
    }

}
