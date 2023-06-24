using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    [Header("Logic")]
    [SerializeField] public float CancelAttackStateAfter = 0.15f;
    [SerializeField] public bool HasPressedAttack = false;

    [Header("Attachments")]
    [SerializeField] public WeaponStats WeaponData;
    [SerializeField] public Player PlayerScript;

    [Header("Payload")]
    [SerializeField] public float GFXHitDelay = 0.2f;
    [SerializeField] public float AttackingSafeZoneTime = 2f;

    [Header("Stats")]
    [SerializeField] public int EnemyComboHitsCount = 0;
    [SerializeField] public float LastEnemyComboHitTime = 0;

    [Header("Feedback")]
    [SerializeField]  private float CancelAttackTimer = 0f;
    [SerializeField] public WeaponStatsProfile WeaponStatsProfile;
    [SerializeField] public List<TrailRenderer> TrailRenderers = new List<TrailRenderer>();
    [SerializeField] public List<int> AttackedEnemiesList = new List<int>();

    private Animator animator;
    private Vector2 localPosition;

   
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
        localPosition = transform.localPosition;
    }

    private void Update()
    {
        if(HasPressedAttack)
        {
            CancelAttackTimer += Time.deltaTime;
            if(CancelAttackTimer >= CancelAttackStateAfter)
            {
                // Reset the attack state
                ResetAttack();
            }
        }
    }

    void FixedUpdate()
    {
        AdjustDirection();
        WeaponAnimation();
    }

    void AdjustDirection()
    {
        if (transform.localScale.x < 0)
        {
            transform.localPosition = new Vector2(-localPosition.x, localPosition.y);
        } else
        {
            transform.localPosition = new Vector2(localPosition.x, localPosition.y);
        }
    }


    private void WeaponAnimation()
    {
        if (animator == null) return;
        if (PlayerScript == null) return;

        animator.SetBool("isWalking", !HasPressedAttack && PlayerScript.isPlayerMoving && !PlayerScript.isPlayerRunning);

        animator.SetBool("isRunning", !HasPressedAttack && PlayerScript.isPlayerMoving && PlayerScript.isPlayerRunning);

        animator.SetBool("isFlipped", transform.localScale.x < 0);

        if(WeaponData != null)
        animator.SetInteger("swingType", WeaponData.SwingType);
    }

    public void ApplyNewSettings(WeaponStats weaponStatsProfile)
    {
        WeaponData = weaponStatsProfile;
        StartCoroutine(ApplyRenderSettings());
        
    }

    private IEnumerator ApplyRenderSettings()
    {
        yield return new WaitForSeconds(1);
        WeaponStatsProfile sample = null;
        List<TrailRenderer> sampleRenders = new List<TrailRenderer>();
        if (transform.GetChild(0) != null)
        {
            sample = transform.GetChild(0).GetComponent<WeaponStatsProfile>();
            if (sample != null && sample.TrailRenderers != null && sample.TrailRenderers.Count > 0)
            {
                sampleRenders = sample.TrailRenderers;
            }
        }

        TrailRenderers = sampleRenders;
        WeaponStatsProfile = sample;
    }

    public void EnableSwordTrails()
    {
        TrailRenderers.ForEach(trailRenderer => trailRenderer.emitting = true);

        
    }

    public void DisableSwordTrails()
    {
        TrailRenderers.ForEach(trailRenderer => trailRenderer.emitting = false);
    }
   

    public void ActivateAttackAllowance()
    {
        Debug.Log("SwordAttack: Attack activated");

        // Attack is executed, check if there's another attack in the process
        if(IsAttacking())
        {
            // It's executed and we are not yet to cancel it, return
            Debug.Log("SwordAttack: Attack rejected, another attack is active.");
            return;
        }

        // todo Adjust in case we do more animations
        AudioManager.Instance.PlayFromPosition(AudioManager.Instance.PlayerSlashSword1, gameObject.transform);

        Debug.Log("SwordAttack: Just hit this count: " + EnemyComboHitsCount);
        HasPressedAttack = true;

        
        // Update the combo in the animator to reflect the corrosponding animation
        animator.SetInteger("hitsCountIndex", EnemyComboHitsCount % WeaponData.MaxCombos);
        animator.SetBool("isAttacking", true);

        // Reduce stamina of player, regardless if the player hits someone or not
        GameManager.Instance.ChangePlayerStamina(-WeaponData.StaminaReductionRate);
    }

    private bool IsAttacking()
    {
        return HasPressedAttack && CancelAttackStateAfter > CancelAttackTimer;
    }

    private void ResetAttack()
    {
        Debug.Log("SwordAttack: Reset Attack, CancelAttackTimer: " + CancelAttackTimer);
        HasPressedAttack = false;
        CancelAttackTimer = 0f;
        // Change the animation
        animator.SetBool("isAttacking", false);
        // Remove the collected ids
        RemoveAttackedEnemies();
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
        Debug.Log("SwordAttack: Hit an enemy " + enemy.GetInstanceID());


        // In case the enemy got hit in the same slash, don't hit it again (in case we hit different collider points of the object)
        if (!CanAttackEnemy(enemy.GetInstanceID())) return;

        // No attack is possible, therefore, we don't register any hit
        if (!IsAttacking()) return;

        // Register the enemy
        AddAttackedEnemy(enemy.GetInstanceID());

        // Validate the combo
        ValidateComboTime();

        // Increase the combo;
        EnemyComboHitsCount++;
        // Used for combo validation
        LastEnemyComboHitTime = Time.time;


        // Affect the health of the nemy
        enemy.AttackEnemy(-weapon.Damage, PlayerScript.transform, WeaponData.PushForce);
        if (EnemyComboHitsCount > 0)
        {
            StopAllCoroutines();
            StartCoroutine(InitiateHitGFX(EnemyComboHitsCount));
        }


        Debug.Log("SwordAttack: Weapon[" + weapon.ID + "] has hit an enemy [" + enemy.name + "]");
    }

    private IEnumerator InitiateHitGFX(int EnemyComboHitsCount)
    {
        yield return new WaitForSeconds(GFXHitDelay);
        GameManager.Instance.CreateSlowMotionEffect(GameManager.Instance.SlowMotionDuration, GameManager.Instance.CanShakeCameraAfterHit);
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
        return !AttackedEnemiesList.Contains(enemyID);
    }
}
