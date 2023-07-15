using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPotionController : MonoBehaviour
{
    [SerializeField] public string ID;
    [SerializeField] public int HealAmount;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(GameManager.Instance.PlayerTag))
        {
            GameManager.Instance.AffectPlayerHealth(HealAmount);
            ObjectiveManager.Instance.CollectPotion(ID);
            GameObject.Destroy(gameObject);
        }
    }

}
