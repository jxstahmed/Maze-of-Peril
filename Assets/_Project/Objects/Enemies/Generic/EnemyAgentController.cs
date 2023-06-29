using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class EnemyAgentController : MonoBehaviour
{

    [Header("Overall")]
    [Tooltip("This ID will be used as a reference for the ObjectiveManager")]
    [SerializeField] public string ID;
    [SerializeField] public float Health;

    [Header("States")]
    [SerializeField] public bool CanAttack = false;
    [SerializeField] public bool CanMove = false;
    [SerializeField] public bool CanFollow = false;
    [SerializeField] public bool CanPatrol = false;
    [SerializeField] public bool IsDead = false;


    [Header("Attachments")]
    [SerializeField] EnemiesStats EnemyData;
    [SerializeField] Slider UIHealth;
    [SerializeField] SpriteRenderer AttachmentWeapon;
    [SerializeField] SpriteRenderer DropShadowAttachment;
    [SerializeField] SpriteRenderer ExcelemationMarkAttachment;
    [SerializeField] Light2D SelfSpot;
    [SerializeField] Light2D SightSpot;
    [Tooltip("To avoid calculations, we can preselect the player and make the enemy's target to literally follow the player (e.g. trap rooms)")]
    [SerializeField] Rigidbody2D PlayerByDefault;


    [Header("Sight Lamp")]
    [SerializeField] bool EnableSelfLamp = true;
    [SerializeField] bool EnableSightLamp = true;

    [Header("Follow")]
    [SerializeField] List<string> FollowCollisionTag = new List<string>();
    [SerializeField] bool LookBehindWalls = false;
    [SerializeField] bool SightInMovingDirection = false;
    [SerializeField] float ClosestRadiusToPlayer = 0.3f;
    [SerializeField] float WaitAfterChaseEndDuration = 1f;


    [Header("Patrol")]
    [SerializeField] string CollisionTag = "PatrolPoint";
    [SerializeField] List<Transform> PatrolPoints = new List<Transform>();
    [SerializeField] int UpcomingPatrolPointIndex = -1;
    [SerializeField] float PatrolStopToLookDuration = 1f;


    [Header("Feedback")]
    [SerializeField] Transform DestinationTarget;
    [SerializeField] float LastLockedSightTimer = 0f;
    [SerializeField] bool IsWaitingAfterChaseEnd = false;
    [SerializeField] bool CanSeePlayer = false;
    [SerializeField] bool IsMoving = false;
    [SerializeField] bool IsFollowing = false;
    [SerializeField] bool IsPatroling = false;
    [SerializeField] bool HasCollided = false;
    [SerializeField] bool HasSetPatrolStopTime = false;
    [SerializeField] Rigidbody2D FoundPlayerBySight;
    [SerializeField] bool isEnemyBeingAttacked = false;
    [SerializeField] bool isAttacking = false;
    [SerializeField] private bool attackOnCooldown;

    [Header("DeathReplacements")]
    [SerializeField] bool DeathReplacementHasCollider;
    [SerializeField] bool CreateTombstone;
    [SerializeField] List<GameObject> Tombstones = new List<GameObject>();


    private AudioSource audioSource;
    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;
    private NavMeshAgent agent;

    private float PatrolDurationTimer = 0;
    private float WaitAfterChaseEndTimer = 0;
    private float LastPlayerAttackTime = 0;
    private float internalIncrementTimer = 0f;
    private float internalHealthCooldowTimer = 0f;
    private Animator animator;

    

    [Header("Knockback")]
    [SerializeField] float powBase = 3f;
    [SerializeField] float powStartX = 0.5f;
    [SerializeField] float powExpMultiplier = 5f;

    

    private void Awake()
    {
        GameManager.GameEvent += onGameEventListen;
    }

    private void OnDestroy()
    {
        GameManager.GameEvent -= onGameEventListen;

    }


    private Shader shaderGUItext;
    private Shader shaderSpritesDefault;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        shaderGUItext = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Default"); // or whatever sprite shader is being used


        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        Health = EnemyData.OverallHealth;

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        AudioManager.Instance.CreateTimer(EnemyData.Name + gameObject.GetInstanceID(), AudioManager.Instance.EnemyMovementRate);

    }

    private void Update()
    {
        if (IsDead) {
            if (ExcelemationMarkAttachment != null)
                ExcelemationMarkAttachment.enabled = false;
            if (UIHealth != null)
                UIHealth.transform.position = new Vector2(0, float.MaxValue); //ich wei� ist jetzt keine cleane l�sung aber funktioniert
            return; 
        }

        if (IsDead) return;

        if(HasSetPatrolStopTime && CanPatrol)
            PatrolDurationTimer += Time.deltaTime;
        
        if(IsWaitingAfterChaseEnd)
            WaitAfterChaseEndTimer += Time.deltaTime;

        if(CanFollow)
            LastLockedSightTimer += Time.deltaTime;

        if (ExcelemationMarkAttachment != null)
        {
            if (IsFollowing)
                ExcelemationMarkAttachment.enabled = true;
            else
                ExcelemationMarkAttachment.enabled = false;
        }


        internalIncrementTimer += Time.deltaTime;
        internalHealthCooldowTimer += Time.deltaTime;


        if (internalIncrementTimer >= EnemyData.IncrementEverySeconds)
        {
            internalIncrementTimer = 0;
            EnemeytatsIncrement();
        }
    }

    void FixedUpdate()
    {
        IsMoving = agent.velocity.x != 0 || agent.velocity.y != 0;


        if(IsDead)
        {
            agent.SetDestination(gameObject.transform.position);
        }

        UpdateUI();

        bool OldCanSeePlayer = CanSeePlayer;

        animator.SetBool("isMoving", !IsDead && IsMoving && !isAttacking);

        if (IsDead || !CanMove) return;

        SeePlayer();

        bool ShouldWaitForChaseEnd = (WaitAfterChaseEndDuration > 0 && OldCanSeePlayer && !CanSeePlayer) || IsWaitingAfterChaseEnd;


        IsPatroling = CanMove && CanPatrol && !CanSeePlayer;
        IsFollowing = CanMove && CanFollow && CanSeePlayer && FoundPlayerBySight != null;
        EnemyAudio();



        if (ShouldWaitForChaseEnd && !CanSeePlayer)
        {
            if((WaitAfterChaseEndDuration > 0 && OldCanSeePlayer && !CanSeePlayer))
            {
                // first frame
                WaitAfterChaseEndTimer = 0;
                IsWaitingAfterChaseEnd = true;
            }

            ValidateEndChaseWait();
        } else
        {
            if (IsPatroling)
            {
                Patrol();
            }

            if (IsFollowing)
            {
                Follow();
            }
        }


        AdjustDirection();
        SelfLight();
        SightLight();
    }

    private void EnemyAudio()
    {
        // It should only apply the audio if the object is within range, otherwise... we would have an immense volume of footsteps
        // We could also check the distance to the playe r
        Camera main = Camera.main;
        Vector2 viewPos = main.WorldToViewportPoint(transform.position);

        // normalized values, if within [0,1] then within the camera bounds, otherwise, it's outside
        if (!(viewPos.x >= 0 && viewPos.y >= 0 && viewPos.x <= 1 && viewPos.y <= 1)) return; 


        if (IsMoving)
        {
            AudioManager.Instance.PlayeOneShotFromAudioInstance(audioSource, AudioManager.Instance.EnemyWalks, EnemyData.Name + gameObject.GetInstanceID());
        }
    }

    void ValidateEndChaseWait()
    {
        if(WaitAfterChaseEndTimer >= WaitAfterChaseEndDuration)
        {
            IsWaitingAfterChaseEnd = false;
        } else
        {
            agent.SetDestination(gameObject.transform.position);
        }
    }

    void AdjustDirection()
    {
        Vector2 target_position = Vector2.zero;
        if (IsFollowing && FoundPlayerBySight != null)
        {
            target_position = GameManager.Instance.getTargetPosition(transform, FoundPlayerBySight.transform);
        } else if (IsPatroling && DestinationTarget != null)
        {
            target_position = GameManager.Instance.getTargetPosition(transform, DestinationTarget.transform);
        }

        bool isRight = target_position.x < 0;
        bool isTop = target_position.y < 0;


        // Flip if different position
        spriteRenderer.flipX = isRight;

        if(AttachmentWeapon != null)
        {
            AttachmentWeapon.flipX = !isRight;
            float x = Mathf.Abs(AttachmentWeapon.transform.localPosition.x);
            if (!isRight)
            {
                x = -1 * x;
            }

            AttachmentWeapon.transform.localPosition = new Vector2(x, AttachmentWeapon.transform.localPosition.y);
        }

        if(DropShadowAttachment != null)
        {
            DropShadowAttachment.flipX = isRight;
            float x = Mathf.Abs(DropShadowAttachment.transform.localPosition.x);
            if (isRight)
            {
                x = -1 * x;
            }

            DropShadowAttachment.transform.localPosition = new Vector2(x, DropShadowAttachment.transform.localPosition.y);
        }

        //switches orientation of healthbar so it stays on top of head
        Vector2 healthbarpos = UIHealth.transform.localPosition;
        Vector2 shadowPos = DropShadowAttachment.transform.localPosition;
        if (isRight)
        {
            shadowPos.x = Mathf.Abs(shadowPos.x);
            healthbarpos.x = Mathf.Abs(healthbarpos.x);
        }
        else
        {
            shadowPos.x = -Mathf.Abs(shadowPos.x);
            healthbarpos.x = -Mathf.Abs(healthbarpos.x);
        }
        DropShadowAttachment.transform.localPosition = shadowPos;
        UIHealth.transform.localPosition = healthbarpos;
    }

    void SeePlayer()
    {
        if(PlayerByDefault != null)
        {
            // We could add a float for the required distance, but we want to avoid any further calculation and just let the player get followed by such enemies until he kills them
            // this could be a problem if the enemy can pass walls
            CanSeePlayer = true;
            FoundPlayerBySight = PlayerByDefault;
            return;
        }

        // Provide initial information for the all-directional raycast
        Vector2 currentPosition = transform.localPosition;

        float movingAngle = 0f;

        if(DestinationTarget != null)
        {
            Vector3 dir = DestinationTarget.position - transform.position;
            movingAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;  // Assuming you want degrees not radians?

        }

        if (FoundPlayerBySight != null)
        {
            Vector3 dir = new Vector3(FoundPlayerBySight.position.x, FoundPlayerBySight.position.y, 0) - transform.position;
            movingAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }

         

        float minAngle = 0f;
        float maxAngle = 360f;


        if (SightInMovingDirection)
        {
            minAngle = movingAngle - 45;
            maxAngle = movingAngle + 45;
        }

        float angleIncrement = 2.5f;

        // Calculate the angle and initiate a raycast
        bool playerSeen = false;
        for (float angle = minAngle; angle <= maxAngle; angle += angleIncrement)
        {
            float calculatedAngle = angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(calculatedAngle), Mathf.Sin(calculatedAngle));

            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, transform.parent.TransformDirection(direction), EnemyData.MaxSightRadius);



            Rigidbody2D playerHitrb = null;
            bool hasHitCollision = false;

            foreach (RaycastHit2D hit in hits)
            {
                
                if (FollowCollisionTag.Contains(hit.collider.tag) && !LookBehindWalls)
                {
                    hasHitCollision = true;
                    break;
                }

                if (hit.collider.tag.Contains(GameManager.Instance.PlayerTag))
                {
                    playerHitrb = hit.rigidbody;
                    break;
                }
            }

            if (!hasHitCollision && playerHitrb != null)
            {
                Debug.DrawRay(transform.position, transform.parent.TransformDirection(direction) * EnemyData.MaxSightRadius, Color.red);

                FoundPlayerBySight = playerHitrb;
                playerSeen = true;
                playerSeen = true;
            }


            if (playerSeen) break;
        }


        CanSeePlayer = playerSeen;
        if (!CanSeePlayer) FoundPlayerBySight = null;
    }

    void Follow()
    {
        if (!CanFollow) return;

        if(!CanSeePlayer || FoundPlayerBySight == null)
        {
            Debug.Log("Couldn't find the player | TargetPlayer is null");
            return;
        }

        agent.speed = EnemyData.FollowSpeed;

        if (!(LastLockedSightTimer >= EnemyData.FollowAfterLockedSightingForSeconds)) return;

        float distanceToPlayer = Vector2.Distance(transform.position, FoundPlayerBySight.transform.position);
        if (distanceToPlayer >= ClosestRadiusToPlayer)
        {
            agent.SetDestination(FoundPlayerBySight.position);
        } else
        {
            agent.SetDestination(gameObject.transform.position);
        }
    }

    void Patrol()
    {
        if (!CanPatrol) return;

        if (PatrolPoints == null || PatrolPoints.Count == 0)
        {
            Debug.Log("Patrol points are not set");
            return;
        }

        agent.speed = EnemyData.PatrolSpeed;

        // set initial point
        if (UpcomingPatrolPointIndex == -1 || UpcomingPatrolPointIndex >= PatrolPoints.Count)
        {
            UpcomingPatrolPointIndex = 0;
        }

        // get point 
        DestinationTarget = PatrolPoints[UpcomingPatrolPointIndex];
        if(HasCollided)
        {
            Debug.Log("AI has Collided with the point");
            if(!HasSetPatrolStopTime)
            {
                Debug.Log("PatrolPause has been set.");
                PatrolDurationTimer = 0;
                HasSetPatrolStopTime = true;
            }


            if(PatrolStopToLookDuration <= 0 || PatrolDurationTimer >= (PatrolStopToLookDuration))
            {
                Debug.Log("Patrol is now moving onto the next point.");
                // no stop to look, go for the next patrol point
                HasCollided = false;
                // Get the next point, whatever the size of points is
                UpcomingPatrolPointIndex = (UpcomingPatrolPointIndex + 1) % PatrolPoints.Count;
                Debug.Log("Going to index " + UpcomingPatrolPointIndex);
                HasSetPatrolStopTime = false;
                DestinationTarget = PatrolPoints[UpcomingPatrolPointIndex];
            }
            else 
            {
                Debug.Log("Patrol is pausing to see.");
                agent.SetDestination(gameObject.transform.position);
                // keep waiting, here we should be letting the SeePlayer() function works if the CanFollow is enabled
            }
        } else
        {
            Debug.Log("Patroling to a point.");
            // we are patrolling
            agent.SetDestination(DestinationTarget.position);
        }

    }

   

    void SelfLight()
    {
        if (SelfSpot != null) SelfSpot.gameObject.SetActive(EnableSelfLamp);
    }

    void SightLight()
    {
        if (SightSpot != null) SightSpot.gameObject.SetActive(EnableSightLamp);
        if (!EnableSightLamp || SightSpot == null)
        {
            return;
        }



        Vector2 currentPosition = transform.localPosition;

        float movingAngle = 0f;

        if (DestinationTarget != null)
        {
            Vector3 dir = DestinationTarget.position - transform.position;
            movingAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
         
        }

        if (FoundPlayerBySight != null)
        {
            Vector3 dir = new Vector3(FoundPlayerBySight.position.x, FoundPlayerBySight.position.y, 0) - transform.position;
            movingAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        }


        SightSpot.pointLightOuterRadius = EnemyData.PatrolRadius;
        SightSpot.transform.rotation = Quaternion.Euler(SightSpot.transform.localRotation.x, SightSpot.transform.localRotation.y, movingAngle - 90);
         
        
    }

    void StopMovement()
    {
        CanMove = false;
        CanPatrol = false;
        CanFollow = false;
        animator.SetBool("isMoving", false);
    }
    void ContinueMovement()
    {
        CanMove = true;
        CanPatrol = true;
        CanFollow = true;
    }
    void EnemeytatsIncrement()
    {

        if (isEnemyBeingAttacked) internalHealthCooldowTimer = 0;


        if (internalHealthCooldowTimer >= EnemyData.RegenerateHealthCooldownWhenHit)
        {
            internalHealthCooldowTimer = 0;
            AffectHealth(EnemyData.HealthRegenerationRate);
        }

    }

    public void AttackEnemy(float damage, Transform player, float force)
    {
        if(GameManager.Instance.Settings.EnemyBloodFeedback)
        {
            GameManager.Instance.InitiateLabel(GameManager.Instance.Settings.EnemyBloodFeedbackLabel, "" + damage, transform);
        }

        AudioManager.Instance.PlayFromPosition(AudioManager.Instance.EnemyGotHit, gameObject.transform);

        animator.SetTrigger("takeDamage");
        AffectHealth(damage);

        DestinationTarget = player.transform;
        knockback(player, force);
    }

    private void knockback(Transform player, float force)
    {
        Vector3 direction = (transform.position - player.position).normalized;
        rigidBody.AddForce(direction * force, ForceMode2D.Impulse);
        StartCoroutine(StopKnockback());
    }

    //stops the knockback exponentially for a smoother knockback
    private IEnumerator StopKnockback()
    {
        float KnockbackFactor = powStartX;
        while (rigidBody.velocity.x != 0 || rigidBody.velocity.y != 0)
        {
            //if calculation goes wrong: enemy cant get knocked back longer than 0.75 seconds
            if(KnockbackFactor - powStartX > 0.75)
            {
                rigidBody.velocity = Vector2.zero;
                break;
            }

            //force function to iterate in gametime
            yield return new WaitForSeconds(Time.deltaTime);
            KnockbackFactor += Time.deltaTime;

            //calculate factor for exponential decrease of velocity
            float Factor = Mathf.Pow(powBase, KnockbackFactor * powExpMultiplier) * Time.deltaTime;

            //if velocity in x or y dir reaches or surpasses 0
            if (Mathf.Abs(rigidBody.velocity.x) - Mathf.Abs(rigidBody.velocity.normalized.x) * Factor <= 0)
                rigidBody.velocity = new Vector2(0, rigidBody.velocity.y);
            if (Mathf.Abs(rigidBody.velocity.y) - Mathf.Abs(rigidBody.velocity.normalized.y) * Factor <= 0)
                rigidBody.velocity = new Vector2(rigidBody.velocity.x,0);
            Vector2 dir = rigidBody.velocity;
            dir.Normalize();
            rigidBody.velocity -= dir * Factor;
        }
        yield return 0;
    }
    /*private IEnumerator PushEnemy(Transform player, float force)
    {
        yield return new WaitForSeconds(0.1f);

        Vector3 direction = (transform.position - player.position).normalized;
        Debug.Log(direction);
        rigidBody.AddForce(direction * force, ForceMode2D.Impulse);

        StartCoroutine(ResetKnockback());
    }

    private IEnumerator ResetKnockback()
    {
        yield return new WaitForSeconds(0.15f);
        rigidBody.velocity = Vector2.zero;
    }*/


    public void AffectHealth(float health)
    {
        float newHealth = Health + health;

        if (newHealth > EnemyData.OverallHealth) newHealth = EnemyData.OverallHealth;
        else if (newHealth < 0) newHealth = 0;

        Health = newHealth;


        if (newHealth <= 0)
        {
            KillEnemy();
        }
    }

    private void KillEnemy()
    {
        AudioManager.Instance.PlayFromPosition(AudioManager.Instance.EnemyDies, gameObject.transform);
        IsDead = true;
        animator.SetTrigger("isDead");
        ObjectiveManager.Instance.CollectEnemy(ID);
    }

    private void RemoveEnemy()
    {
        Destroy(gameObject);
    }

    private void UpdateUI()
    {
        UIHealth.value = Health > 0 ? Health / EnemyData.OverallHealth : 0;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag(CollisionTag))
        {
            Debug.Log("AI has collided in OnTriggerEnter");
            HasCollided = true;
        }
       /* else if (col.gameObject.CompareTag(GameManager.Instance.PlayerTag) && CanFollow && CanSeePlayer)
        {
            Debug.Log("OnTriggerEnter Player");
            LastPlayerAttackTime = Time.time;

        }*/
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(GameManager.Instance.PlayerTag))
        {
            Debug.Log("Enemy, inside CompareTag");
            // turn to player when attacking
            bool dir = other.transform.position.x - transform.position.x > 0;
            spriteRenderer.flipX = dir; 
            // Current time & delay
            // First touch at second 2
            // We test at second 5, attack pause is 2 => first touch + 2 => 5 > 4, we can hit
            // set the time of last_hit to 5 and then retry 
            // player gets away and OnExit is trigger
            // player gets back at 7.2 and when he first touches => he gets hit
            /*float allowedPlayerAttackTime = LastPlayerAttackTime + EnemyData.AttackCooldown;
            if (LastPlayerAttackTime != 0 && Time.time >= allowedPlayerAttackTime)
            {
                // hitting 
                Debug.Log("OnTriggerStay2D");
                LastPlayerAttackTime = Time.time;
                Debug.Log("Attacking Player");
                if (CanMove)
                {
                    isAttacking = false;
                    animator.SetTrigger("attack");
                    GameManager.Instance.AttackPlayer(-EnemyData.Damage);
                }
            }*/

            //dein alter code ist auskommentier noch da, daran habe ich auch nichts ge�ndert.
            //Attackcooldown funktioniert jetzt so, dass wenn ein gegner angreift eine coroutine startet, die den cooldown wartet und dann dem gegner wieder erlaubt anzugreifen.
            // Alles klar
            if (!IsDead && CanAttack && !attackOnCooldown && !isAttacking)
            {
                AttackPlayer();
            }
        }
    }

    private void AttackPlayer()
    {
        AudioManager.Instance.PlayFromPosition(AudioManager.Instance.EnemyAttacks, gameObject.transform);
        Debug.LogWarning("Attacking Player");
        attackOnCooldown = true;
        isAttacking = true;
        animator.SetTrigger("attack");
        animator.SetBool("isAttacking", true);
        StopMovement();
        StartCoroutine(EnemyAttackCooldown());
    }

    //waits N seconds and then allows the enemy to hit the player again
    private IEnumerator EnemyAttackCooldown()
    {
        yield return new WaitForSeconds(EnemyData.AttackCooldown);
        Debug.LogWarning("enemy can now attack again");
        attackOnCooldown = false;
        animator.SetBool("isAttacking", false);
        yield return 0;
    }
    private void DamagePlayer()
    {
        GameManager.Instance.AttackPlayer(-EnemyData.Damage);
    }
   /* private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(GameManager.Instance.PlayerTag))
        {
            Debug.Log("OnTriggerExit2D");
            LastPlayerAttackTime = 0;
        }
    }*/

    public void ResetAttack()
    {
        if (!isAttacking)
            return;
        isAttacking = false;
        ContinueMovement();

    }

    public void HideAttachment()
    {
        if (AttachmentWeapon != null)
        {
            AttachmentWeapon.enabled = false;
        }
    }

    public void ShowAttachment()
    {
        if(AttachmentWeapon != null)
        {
            AttachmentWeapon.enabled = true;
        }
    }

    void WhiteSprite()
    {
        spriteRenderer.material.shader = shaderGUItext;
        spriteRenderer.color = Color.white;
    }

    void NormalSprite()
    {
        spriteRenderer.material.shader = shaderSpritesDefault;
        spriteRenderer.color = Color.white;
    }

    private void onGameEventListen(Hashtable payload)
    {
        if ((GameState)payload["state"] == GameState.StopEnemies)
        {
            StopMovement();
            CanAttack = false;
        }
    }


    /**
     * Replaces the enemy with one of the gameObject in "Tombstones" upon death
     * */
    private void replaceWithTombsstone()
    {
        if (!CreateTombstone || Tombstones == null || Tombstones.Count == 0)
        {
            Destroy(gameObject);
            return;
        }
        int rand_int = Random.Range(0, Tombstones.Count);
        GameObject tombstone = Tombstones[rand_int];
        if (tombstone != null)
        {
            GameObject deathReplacement = Instantiate(tombstone, transform.position, Quaternion.identity);
            BoxCollider2D rbt = deathReplacement.GetComponent<BoxCollider2D>();
            rbt.enabled = DeathReplacementHasCollider;

        }
        
        Destroy(gameObject);
    }
}
