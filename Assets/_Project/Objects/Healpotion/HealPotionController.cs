using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPotionController : MonoBehaviour
{
    [SerializeField] public int HealAmount;


    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.LogWarning("aaaaaaaaaaaaaaaaaaaaaa");
        if (other.CompareTag(GameManager.Instance.PlayerTag))
        {
            GameManager.Instance.AttackPlayer(HealAmount);
            GameObject.Destroy(gameObject);
        }
    }

}
