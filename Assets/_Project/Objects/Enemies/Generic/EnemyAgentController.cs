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
    [SerializeField] string ID;
    [SerializeField] Transform DestinationTarget;

    [Header("States")]
    [SerializeField] bool CanMove = false;
    [SerializeField] bool CanFollow = false;
    [SerializeField] bool CanPatrol = false;
    [SerializeField] bool IsDead = false;


    [Header("Attachments")]
    [SerializeField] EnemiesStats EnemyData;
    [SerializeField] public Slider UIHealth;
    [SerializeField] Light2D SelfSpot;
    [SerializeField] Light2D SightSpot;


    [Header("Sight Lamp")]
    [SerializeField] bool EnableSelfLamp = true;
    [SerializeField] bool EnableSightLamp = true;

    [Header("Follow")]
    [SerializeField] float LastLockedSightTimer = 0f;
    [SerializeField] bool LookBehindWalls = false;
    [SerializeField] bool SightInMovingDirection = false;
    [SerializeField] float ClosestRadiusToPlayer = 0.3f;


    [Header("Patrol")]
    [SerializeField] string CollisionTag = "PatrolPoint";
    [SerializeField] List<Transform> PatrolPoints = new List<Transform>();
    [SerializeField] int UpcomingPatrolPointIndex = -1;
    [SerializeField] float PatrolStopToLookDuration = 1f;


    [Header("Feedback")]
    [SerializeField] bool CanSeePlayer = false;
    [SerializeField] bool IsMoving = false;
    [SerializeField] bool IsFollowing = false;
    [SerializeField] bool IsPatroling = false;
    [SerializeField] bool HasCollided = false;
    [SerializeField] bool HasSetPatrolStopTime = false;
    [SerializeField] Rigidbody2D FoundPlayerBySight;


    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;
    private NavMeshAgent agent;

    private float PatrolDurationTimer = 0;
    private float LastPlayerAttackTime = 0;
    private float internalIncrementTimer = 0f;
    private float internalHealthCooldowTimer = 0f;
    private bool isEnemyBeingAttacked = false;
    private Animator animator;


    private void Awake()
    {
        GameManager.GameEvent += onGameEventListen;
    }

    private void OnDestroy()
    {
        GameManager.GameEvent -= onGameEventListen;

    }



    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        EnemyData.Health = EnemyData.OverallHealth;

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        agent.speed = EnemyData.FollowSpeed;
    }

    private void Update()
    {
        if (IsDead) return;

        PatrolDurationTimer += Time.deltaTime;
        LastLockedSightTimer += Time.deltaTime;
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
        UpdateUI();

        SeePlayer();


        IsPatroling = CanMove && CanPatrol && !CanSeePlayer;
        IsFollowing = CanMove && CanFollow && CanSeePlayer && FoundPlayerBySight != null;


        animator.SetBool("isMoving", IsPatroling || IsFollowing);

        if (IsDead || !CanMove) return;

        if(IsPatroling)
        {
            Patrol();
        } 
        
        if (IsFollowing)
        {
            Follow();
        }

        AdjustDirection();
        SelfLight();
        SightLight();
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

    }

    void SeePlayer()
    {

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
                if (hit.collider.tag.Contains(CollisionTag) && !LookBehindWalls)
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
            }


            if (playerSeen) break;
        }


        CanSeePlayer = playerSeen;
        if (!CanSeePlayer) FoundPlayerBySight = null;
    }

    void Follow()
    {
        if(!CanSeePlayer || FoundPlayerBySight == null)
        {
            Debug.Log("Couldn't find the player | TargetPlayer is null");
            return;
        }

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
        if (PatrolPoints == null || PatrolPoints.Count == 0)
        {
            Debug.Log("Patrol points are not set");
            return;
        }

        // set initial point
        if(UpcomingPatrolPointIndex == -1 || UpcomingPatrolPointIndex >= PatrolPoints.Count)
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

    void EnemeytatsIncrement()
    {

        if (isEnemyBeingAttacked) internalHealthCooldowTimer = 0;


        if (internalHealthCooldowTimer >= EnemyData.RegenerateHealthCooldownWhenHit)
        {
            internalHealthCooldowTimer = 0;
            AffectHealth(EnemyData.HealthRegenerationRate);
        }

    }

    public void AttackEnemy(float damage)
    {

        animator.SetTrigger("takeDamage");
        AffectHealth(damage);
    }

    public void AffectHealth(float health)
    {
        float newHealth = EnemyData.Health + health;

        if (newHealth > EnemyData.OverallHealth) newHealth = EnemyData.OverallHealth;
        else if (newHealth < 0) newHealth = 0;

        EnemyData.Health = newHealth;


        if (newHealth <= 0)
        {
            KillEnemy();
        }
    }

    private void KillEnemy()
    {
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
        UIHealth.value = EnemyData.Health > 0 ? EnemyData.Health / EnemyData.OverallHealth : 0;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag(CollisionTag))
        {
            Debug.Log("AI has collided in OnTriggerEnter");
            HasCollided = true;
        }
        else if (col.gameObject.CompareTag(GameManager.Instance.PlayerTag) && CanFollow && CanSeePlayer)
        {
            Debug.Log("OnTriggerEnter Player");
            LastPlayerAttackTime = Time.time;

        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(GameManager.Instance.PlayerTag))
        {
            Debug.Log("Enemy, inside CompareTag");
            // Current time & delay
            // First touch at second 2
            // We test at second 5, attack pause is 2 => first touch + 2 => 5 > 4, we can hit
            // set the time of last_hit to 5 and then retry 
            // player gets away and OnExit is trigger
            // player gets back at 7.2 and when he first touches => he gets hit

            float allowedPlayerAttackTime = LastPlayerAttackTime + EnemyData.AttackCooldown;
            if (LastPlayerAttackTime != 0 && Time.time >= allowedPlayerAttackTime)
            {
                // hitting 
                Debug.Log("OnTriggerStay2D");
                LastPlayerAttackTime = Time.time;
                Debug.Log("Attacking Player");
                animator.SetTrigger("attack");
                GameManager.Instance.AttackPlayer(-EnemyData.Damage);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(GameManager.Instance.PlayerTag))
        {
            Debug.Log("OnTriggerExit2D");
            LastPlayerAttackTime = 0;
        }
    }

    private void onGameEventListen(Hashtable payload)
    {
        if ((GameState)payload["state"] == GameState.StopEnemies)
        {
            StopMovement();
        }
    }
}
