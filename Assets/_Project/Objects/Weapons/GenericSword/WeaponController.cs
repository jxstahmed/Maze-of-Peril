using System.Collections;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Attachments")]
    [SerializeField] public Player PlayerScript;
    [SerializeField] public WeaponStats WeaponData;

    [Header("States")]
    [SerializeField] public bool HasPressedAttack = false;
    [SerializeField] public bool IsAttacking = false;

    [Header("Stats")]
    [SerializeField] public int EnemyComboHitsCount = 0;
    [SerializeField] public float LastEnemyComboHitTime = 0;

    private Animator animator;

    private void Awake()
    {
        GameManager.GameEvent += onGameEventListen;
    }

    // unsubscribe
    private void OnDestroy()
    {
        GameManager.GameEvent -= onGameEventListen;

    }

    void Start()
    {
        animator = GetComponent<Animator>();
        
    }

    void FixedUpdate()
    {

        WeaponAnimation();

        if(IsAttacking)
          WeaponHits();
    }

    private void WeaponAttack()
    {

    }

    private void WeaponAnimation()
    {
        if (animator == null) return;
        if (PlayerScript == null) return;

        animator.SetBool("isWalking", !HasPressedAttack && PlayerScript.isPlayerMoving && !PlayerScript.isPlayerRunning);

        animator.SetBool("isRunning", !HasPressedAttack && PlayerScript.isPlayerMoving && PlayerScript.isPlayerRunning);


        animator.SetInteger("swingType", WeaponData.SwingType);

        if(IsAttacking)
        {
            if (HasPressedAttack)
            {
                Debug.Log("Pressed attack, should trigger");
                animator.SetTrigger("isAttacking");
                animator.SetInteger("hitsCount", (EnemyComboHitsCount + 1));
                ResetAttackPress();
            }
            else
            {
                animator.SetInteger("hitsCount", 0);
            }
        } 

        

    }

    private void WeaponHits()
    {
        float lastComboDelay = WeaponData.ComboInbetweenTime + LastEnemyComboHitTime;
        if(Time.time > lastComboDelay)
        {
            //Debug.Log("No more combo time");
            ResetCombo();
        } else
        {
            Debug.Log("COMBO TIME ACTIVE");
        }
    }

    public void ActivateAttackAllowance()
    {
     

        Debug.Log("Attack activated");
        IsAttacking = true;
        HasPressedAttack = true;

        // Reduce stamina
        GameManager.Instance.ChangePlayerStamina(-WeaponData.StaminaReductionRate);
    }


    public void HitEnemy(WeaponStats weapon, Enemy enemy)
    {
        if(!IsAttacking) return;

        Debug.Log("Weapon[" + weapon.ID + "] has hit an enemy [" + enemy.name + "]");
        // Save time
        LastEnemyComboHitTime = Time.time;
        IncreaseCombo();
    }

    public void ResetAttackPress()
    {
        Debug.Log("Resetting attack press");
        HasPressedAttack = false;
    }

    public void ResetCombo()
    {
        EnemyComboHitsCount = 0;
    }

    public void IncreaseCombo()
    {
        EnemyComboHitsCount++;
    }

    public void ResetAttackState()
    {
        IsAttacking = false;
    }

    private void onGameEventListen(Hashtable payload)
    {
        if ((GameState)payload["state"] == GameState.AttackPlayer)
        {
           
        }
    }
}
