using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponStatsProfile : MonoBehaviour
{

    [SerializeField] public WeaponStats WeaponStats;
    [SerializeField] public bool isEquipEnabled;

    private WeaponController weaponController;
    private Animator animator;

    void Start()
    {
        weaponController = this.gameObject.transform.parent.GetComponent<WeaponController>();
        animator = GetComponent<Animator>();
        animator.enabled = isEquipEnabled;
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == GameManager.Instance.EnemyTag)
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                // Debug.Log("SwordAttack: Collided with enemy, time: " + Time.time);
                weaponController.HitEnemy(WeaponStats, enemy);
            }
        } else if (other.tag == GameManager.Instance.PlayerTag && isEquipEnabled)
        {
            Debug.Log("WeaponSystem: TouchedPlayer");
            Player player = other.GetComponent<Player>();
            player.PickWeapon(WeaponStats);
            Destroy(gameObject);
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == GameManager.Instance.EnemyTag)
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                //Debug.Log("SwordAttack: Collided with enemy, time: " + Time.time);
                weaponController.HitEnemy(WeaponStats, enemy);
            }
        }
    }
}
