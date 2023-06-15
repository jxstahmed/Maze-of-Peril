using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    [Header("Attachments")]
    [SerializeField] public WeaponStats WeaponData;
    [SerializeField] public Player PlayerScript;

    [Header("Payload")]
    [SerializeField] public float GFXHitDelay = 0.2f;
    [SerializeField] public float AttackingSafeZoneTime = 2f;

    [Header("States")]
    [SerializeField] public bool HasPressedAttack = false;
    [SerializeField] public float IsAttackingTime;
    [SerializeField] public List<int> AttackedEnemiesList = new List<int>();

    [Header("Stats")]
    [SerializeField] public int EnemyComboHitsCount = 0;
    [SerializeField] public float LastEnemyComboHitTime = 0;

    [Header("Feedback")]
    [SerializeField] public WeaponStatsProfile WeaponStatsProfile;
    [SerializeField] public List<TrailRenderer> TrailRenderers = new List<TrailRenderer>();

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
        if (IsAttacking()) return;
        Debug.Log("SwordAttack: Attack activated");

        IsAttackingTime = Time.time;
        HasPressedAttack = true;

        

        Debug.Log("Just hit this count: " + EnemyComboHitsCount);
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
