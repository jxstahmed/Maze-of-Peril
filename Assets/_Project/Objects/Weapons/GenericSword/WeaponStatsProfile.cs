using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponStatsProfile : MonoBehaviour
{
    [Header("Overall")]
    [SerializeField] public bool isEquipEnabled;


    [Header("Attachments")]
    [SerializeField] public WeaponStats WeaponStats;
    [SerializeField] public List<TrailRenderer> TrailRenderers = new List<TrailRenderer>();

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
            EnemyAgentController enemy = other.GetComponent<EnemyAgentController>();
            if (enemy != null && weaponController != null)
            {
                // Debug.Log("SwordAttack: Collided with enemy, time: " + Time.time);
                weaponController.HitEnemy(WeaponStats, enemy);
            }
        } else if (other.tag == GameManager.Instance.PlayerTag && isEquipEnabled)
        {
            Debug.Log("WeaponSystem: TouchedPlayer");
            Player player = other.GetComponent<Player>();
            player.PickWeapon(WeaponStats);
            // todo, delete parent creates an issue, but we cant delete the parent
            Destroy(gameObject);
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == GameManager.Instance.EnemyTag)
        {
            EnemyAgentController enemy = other.GetComponent<EnemyAgentController>();
            if (enemy != null)
            {
                //Debug.Log("SwordAttack: Collided with enemy, time: " + Time.time);
                weaponController.HitEnemy(WeaponStats, enemy);
            }
        }
    }
}
