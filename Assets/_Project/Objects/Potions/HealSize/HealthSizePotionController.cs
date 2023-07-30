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
            AudioManager.Instance.PlayFromPosition(AudioManager.Instance.PotionIsPickedUp1, gameObject.transform);
            GameManager.Instance.AffectPlayerHealthSize(HealthSizeAmount);
            ObjectiveManager.Instance.CollectPotion(ID);
            GameObject.Destroy(gameObject);
        }
    }

}
