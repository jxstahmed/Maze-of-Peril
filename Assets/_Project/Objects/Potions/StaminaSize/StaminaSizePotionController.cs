using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaSizePotionController : MonoBehaviour
{
    [SerializeField] public string ID;
    [SerializeField] public int StaminaSizeAmount;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(GameManager.Instance.PlayerTag))
        {
            GameManager.Instance.AffectPlayerStaminaSize(StaminaSizeAmount);
            ObjectiveManager.Instance.CollectPotion(ID);
            GameObject.Destroy(gameObject);
        }
    }

}
