using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponStatsProfile : MonoBehaviour
{

    [SerializeField] public WeaponStats WeaponStats;

    private WeaponController weaponController;

    void Start()
    {
        weaponController = this.gameObject.transform.parent.GetComponent<WeaponController>();
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.tag == "Enemy")
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                
                weaponController.HitEnemy(WeaponStats, enemy);
                // enemy.applyDamage(damage);

            }
        }
    }
}
