using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    [Header("Attachments")]
    [SerializeField] public WeaponStats WeaponData;
    [SerializeField] public Player PlayerScript;
    [SerializeField] public GameObject WeaponAnimationAnimator;

    [Header("Payload")]
    [SerializeField] public float AttackingSafeZoneTime = 2f;

    [Header("States")]
    [SerializeField] public bool HasPressedAttack = false;
    [SerializeField] public float IsAttackingTime;
    [SerializeField] public List<int> AttackedEnemiesList = new List<int>();

    [Header("Stats")]
    [SerializeField] public int EnemyComboHitsCount = 0;
    [SerializeField] public float LastEnemyComboHitTime = 0;

    private Animator animator;
    private Animator weaponAnimator;

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
        weaponAnimator = WeaponAnimationAnimator.GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        WeaponAnimation();
        slashAnimationDirection();
    }


    private void WeaponAnimation()
    {
        if (animator == null) return;
        if (PlayerScript == null) return;

        animator.SetBool("isWalking", !HasPressedAttack && PlayerScript.isPlayerMoving && !PlayerScript.isPlayerRunning);

        animator.SetBool("isRunning", !HasPressedAttack && PlayerScript.isPlayerMoving && PlayerScript.isPlayerRunning);

        weaponAnimator.SetBool("isFlipped", transform.localScale.x < 0);
        animator.SetBool("isFlipped", transform.localScale.x < 0);

        animator.SetInteger("swingType", WeaponData.SwingType);
    }

    private void slashAnimationDirection()
    {
        bool isFlipped = transform.localScale.x < 0;
        SpriteRenderer spriteR = WeaponAnimationAnimator.GetComponent<SpriteRenderer>();
        spriteR.flipX = isFlipped;

        float x = Mathf.Abs(WeaponAnimationAnimator.transform.localPosition.x);
        if(isFlipped)
        {
            x = -1 * x;
        }

        WeaponAnimationAnimator.transform.localPosition = new Vector2(x, WeaponAnimationAnimator.transform.localPosition.y);

    }

    public void TriggerSlash()
    {
        weaponAnimator.SetTrigger("strike0");
    }

    public void ActivateAttackAllowance()
    {
        if (IsAttacking()) return;
        Debug.Log("SwordAttack: Attack activated");

        IsAttackingTime = Time.time;
        HasPressedAttack = true;

        EnemyComboHitsCount++;

        
        animator.SetInteger("hitsCountIndex", EnemyComboHitsCount % WeaponData.MaxCombos);



        Debug.Log("SwordAttack: ActivateAttackAllowance, EnemyComboHitsCount: " + (EnemyComboHitsCount % WeaponData.MaxCombos));

        animator.SetBool("isAttacking", true);

        // Reduce stamina of player
        GameManager.Instance.ChangePlayerStamina(-WeaponData.StaminaReductionRate);
    }

    public bool IsAttacking()
    {
        //Debug.Log((IsAttackingTime + AttackingSafeZoneTime) + " >= " + Time.time);
        // return ((IsAttackingTime + AttackingSafeZoneTime) >= Time.time) && HasPressedAttack;
        return HasPressedAttack;
        //return animator.GetBool("IsAttacking") == true;
    }

    public void ValidateComboTime()
    {
        float lastComboDelay = WeaponData.ComboInbetweenTime + LastEnemyComboHitTime;
        if (Time.time > lastComboDelay)
        {
            Debug.Log("SwordAttack: ComboReset");
            LastEnemyComboHitTime = 0f;
            EnemyComboHitsCount = 0;
        } else
        {
            Debug.Log("SwordAttack: ComboAllowed");
        }
    }

    public void HitEnemy(WeaponStats weapon, EnemyAgentController enemy)
    {

        Debug.Log("SwordAttack: " + enemy.GetInstanceID());

        if (CanAttackEnemy(enemy.GetInstanceID()))
        {
            return;
        }

        if (!IsAttacking())
        {
            Debug.Log("SwordAttack: Requested to hit enemy, but not allowed. IsAttacking: " + IsAttackingTime + ", time: " + Time.time);
            return;
        }
       

        AddAttackedEnemy(enemy.GetInstanceID());

        // validate the combo
        ValidateComboTime();

        

        // increase the combo;
        EnemyComboHitsCount++;
        LastEnemyComboHitTime = Time.time;

        // Affect the health of the nemy
        enemy.AttackEnemy(-weapon.Damage);

        Debug.Log("SwordAttack: Weapon[" + weapon.ID + "] has hit an enemy [" + enemy.name + "]");
    }


    // Used by the animation swing0 | swing0flipped
    public void ResetAttackState()
    {
        Debug.Log("SwordAttack: Reset Attack, " + Time.time);
        IsAttackingTime = 0f;
        HasPressedAttack = false;
        animator.SetBool("isAttacking", false);
        RemoveAttackedEnemies();
    }

    private void onGameEventListen(Hashtable payload)
    {
        if ((GameState)payload["state"] == GameState.AttackPlayer)
        {
           
        }
    }

    private void AddAttackedEnemy(int enemyId)
    {
        AttackedEnemiesList.Add(enemyId);
    }

    private void RemoveAttackedEnemies()
    {
        AttackedEnemiesList.Clear();
    }

    private bool CanAttackEnemy(int enemyID)
    {
        return AttackedEnemiesList.Contains(enemyID);
    }
}
