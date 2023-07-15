using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPotionController : MonoBehaviour
{
    [SerializeField] public int HealAmount;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(GameManager.Instance.PlayerTag))
        {
            GameManager.Instance.AffectPlayerHealth(HealAmount);
            GameObject.Destroy(gameObject);
        }
    }

}
