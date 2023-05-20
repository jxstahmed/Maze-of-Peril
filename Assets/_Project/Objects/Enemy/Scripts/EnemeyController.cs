using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering.Universal;

public class EnemeyController : MonoBehaviour
{

    [Header("Overall")]
    [SerializeField] bool CanMove = true;

    [Header("Attachments")]
    [SerializeField] Rigidbody2D TargetPlayer;
    [SerializeField] Light2D SelfSpot;
    [SerializeField] Light2D SightSpot;

    [Header("Sight Lamp")]
    [SerializeField] bool EnableSelfLamp = true;
    [SerializeField] bool EnableSightLamp = true;

    [Header("Follow")]
    [SerializeField] string PlayerTag = "Player";
    [SerializeField] bool CanFollowPlayer = true;
    [SerializeField] bool SightInMovingDirection = false;
    [SerializeField] float MaxSightRadius = 6f;
    [SerializeField] float FollowSpeed = 0.2f;

    [Header("Follow States")]
    [SerializeField] Rigidbody2D PlayerFoundBody;
    [SerializeField] bool CanSeePlayer = false;
    [SerializeField] bool IsFollowing = false;

    [Header("Patrol")]
    [SerializeField] string CollisionTag = "Walls";
    [SerializeField] bool CanPatrol = true;
    [SerializeField] float PatrolSpeed = 0.6f;
    [SerializeField] float PatrolRadius = 25f;
    [SerializeField] bool RandomizeDirectionAtStart = false;
    [SerializeField] bool RandomizeDirectionAfterEnd = false;
    [SerializeField] List<Vector2> PatrolAvailablePoints = new List<Vector2>();
    [SerializeField] Vector2 LocalPatrolStartingPoint;
    [SerializeField] Vector2 GlobalPatrolStartingPoint;

    [Header("Patrol States")]
    [SerializeField] PATROL_POINTS PatrolUpcomingPoint;
    [SerializeField] bool IsPatrolReady = false;
    [SerializeField] bool IsPatroling = false;
    [SerializeField] bool HasCollided = false;
    [SerializeField] bool IsResetting = false;


    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;

    void Start() {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();


        PatrolAvailablePoints.AddRange(new List<Vector2> {
            new Vector2(0, 0),
            new Vector2(0, 0),
            new Vector2(0, 0),
            new Vector2(0, 0),
        });
    }

    void FixedUpdate() {
        // Flip
        //DirectionalAdjustments();

        // Validate the starting point that we are relative to
        ValidateStartingComapssPoint();

        // Validate if we can see the player
        SeePlayer();


        IsPatroling = IsPatrolReady && CanPatrol;
        IsFollowing = CanFollowPlayer && CanSeePlayer;

        // Do patroling
        if (IsPatroling && !IsFollowing)
        {
            Debug.Log("Patroling");
            Patrol();
        } else if(IsFollowing)
        {
            Debug.Log("Following player");
            FollowPlayer();
        }


        SelfLight();
        SightLight();
    }

    void DirectionalAdjustments()
    {
        Vector2 target_position = Vector2.zero;

        if(IsPatroling && !IsFollowing)
        {
            target_position.x = transform.position.x - PatrolAvailablePoints[(int)PatrolUpcomingPoint].x;
            target_position.y = transform.position.y - PatrolAvailablePoints[(int)PatrolUpcomingPoint].y;

        }
        else if (IsFollowing)
        {
            target_position = GameManager.Instance.getTargetPosition(transform, TargetPlayer.transform);
        }



        bool isRight = target_position.x < 0;
        bool isTop = target_position.y < 0;


        // Flip if different position
        //spriteRenderer.flipX = !isRight;


    }

    void FollowPlayer()
    {
        if(!CanSeePlayer || PlayerFoundBody == null)
        {
            return;
        }


        rigidBody.position = Vector2.MoveTowards(transform.position, PlayerFoundBody.transform.position, FollowSpeed * Time.deltaTime);
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


        Vector2 currentUpcomingPoint = PatrolAvailablePoints[(int)PatrolUpcomingPoint];
        Vector2 currentPosition = transform.localPosition;

        float movingAngle = Mathf.Atan2(currentUpcomingPoint.y - currentPosition.y, currentUpcomingPoint.x - currentPosition.x) * Mathf.Rad2Deg;


        SightSpot.pointLightOuterRadius = PatrolRadius;
        SightSpot.transform.rotation = Quaternion.Euler(SightSpot.transform.localRotation.x, SightSpot.transform.localRotation.y, movingAngle -90);
    }

    void SeePlayer()
    {
        // Provide initial information for the all-directional raycast
        

        Vector2 currentUpcomingPoint = PatrolAvailablePoints[(int)PatrolUpcomingPoint];
        Vector2 currentPosition = transform.localPosition;

        float movingAngle = 0f;



        if (PlayerFoundBody != null)
        {
           
            movingAngle = Mathf.Atan2(PlayerFoundBody.transform.position.y - transform.position.y, PlayerFoundBody.transform.position.x - transform.position.x) * Mathf.Rad2Deg;
        } else
        {
            movingAngle = Mathf.Atan2(currentUpcomingPoint.y - currentPosition.y, currentUpcomingPoint.x - currentPosition.x) * Mathf.Rad2Deg;
        }

        float minAngle = 0f;
        float maxAngle = 360f;


        if (SightInMovingDirection )
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

            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, transform.parent.TransformDirection(direction), MaxSightRadius);

            

            Rigidbody2D playerHitrb = null;
            bool hasHitCollision = false;

            foreach(RaycastHit2D hit in hits)
            {
                if(hit.collider.tag.Contains(CollisionTag))
                {
                    hasHitCollision = true;
                    break;
                }

                if (hit.collider.tag.Contains(PlayerTag))
                {
                    playerHitrb = hit.rigidbody;
                    break;
                }
            }

            if(!hasHitCollision && playerHitrb != null)
            {
                Debug.DrawRay(transform.position, transform.parent.TransformDirection(direction) * MaxSightRadius, Color.red);

                PlayerFoundBody = playerHitrb;
                playerSeen = true;
            } 


            //Debug.DrawRay(transform.position, direction * MaxSightRadius, Color.yellow);

            if (playerSeen) break;
        }


        CanSeePlayer = playerSeen;

        if (!CanSeePlayer) PlayerFoundBody = null;


    }

    void Patrol()
    {
        Vector2 currentUpcomingPoint = PatrolAvailablePoints[(int)PatrolUpcomingPoint];
        Vector2 currentPosition = transform.localPosition;



        // Debug.Log("Current position: [x:" + currentPosition.x + "|y:" + currentPosition.y + "]");

        //Debug.Log("Upcoming position: [x:" + currentUpcomingPoint.x + "|y:" + currentUpcomingPoint.y + "]");

        if(IsResetting)
        {
            if(currentPosition == LocalPatrolStartingPoint)
            {
                // reached reset point
                Debug.Log("Reached reset point");
                IsResetting = false;
            } else
            {
                Debug.Log("Velocity: " + rigidBody.velocity);

                rigidBody.position = Vector2.MoveTowards(transform.position, transform.parent.TransformPoint(LocalPatrolStartingPoint), PatrolSpeed * Time.deltaTime);
                return;
            }
            
        }


        if(HasCollided)
        {
            rigidBody.velocity = Vector2.zero;
            // Either collided or reached the destintation
            int length = System.Enum.GetValues(typeof(PATROL_POINTS)).Length;
            int next_index = (int)PatrolUpcomingPoint;


            if(RandomizeDirectionAfterEnd && (next_index + 1) >= length)
            { 
                // prevent that we get the same direction as we have
                while ((int)PatrolUpcomingPoint == next_index)
                {
                    System.Random r = new System.Random();
                    next_index = r.Next(0, length - 1);
                }
            }
            else
            {
                next_index = (next_index + 1) % length;
            }

            
            // Go to starting point
            IsResetting = true;
            PatrolUpcomingPoint = (PATROL_POINTS) System.Enum.GetValues(typeof(PATROL_POINTS)).GetValue(next_index);

            HasCollided = false;
            return;
        }

     
        rigidBody.position = Vector2.MoveTowards(transform.position, transform.parent.TransformPoint(currentUpcomingPoint), PatrolSpeed * Time.deltaTime);
    }


    void ValidateStartingComapssPoint() {
        if(!IsPatrolReady) {
            IsPatrolReady = true;
            LocalPatrolStartingPoint = transform.localPosition;
            GlobalPatrolStartingPoint = transform.position;

            // Randomize the upcoming starting point
            if(RandomizeDirectionAtStart)
            {
                int length = System.Enum.GetValues(typeof(PATROL_POINTS)).Length;
                System.Random r = new System.Random();
                int index = r.Next(0, length - 1);
                PatrolUpcomingPoint = (PATROL_POINTS) System.Enum.GetValues(typeof(PATROL_POINTS)).GetValue(index);
            }

            FindAvailableDirectionPoints();
        }
    }

    void FindAvailableDirectionPoints() {
        foreach(PATROL_POINTS patrolDirection in System.Enum.GetValues(typeof(PATROL_POINTS))) {
            Vector2 availablePoint = FindAvailableDirectionPoint(patrolDirection);
            PatrolAvailablePoints[(int) patrolDirection] = availablePoint;
        }
    }

    Vector2 FindAvailableDirectionPoint(PATROL_POINTS direction) {
        Vector2 directionPoint = new Vector2(0, 0);
        Debug.Log(direction);

        //Physics.RaycastAll(myPoint, rayDirections[i], hits[i])

        switch(direction) {
            case PATROL_POINTS.TOP:
                Debug.Log("TOP");
                directionPoint = getRaycastVector(Vector2.up);
                break;
            case PATROL_POINTS.RIGHT:
                Debug.Log("RIGHT");
                directionPoint = getRaycastVector(Vector2.right);

                break;
            case PATROL_POINTS.LEFT:
                Debug.Log("LEFT");
                directionPoint = getRaycastVector(Vector2.left);

                break;
            case PATROL_POINTS.BOTTOM:
                Debug.Log("BOTTOM");
                directionPoint = getRaycastVector(Vector2.down);

                break;
        }

         return directionPoint;
    }

    Vector2 getRaycastVector(Vector2 direction)
    {
        Vector2 hitPoint = new Vector2(0, 0);
        RaycastHit2D[] hits = Physics2D.RaycastAll(GlobalPatrolStartingPoint, direction, PatrolRadius);

        Debug.DrawRay(transform.position, direction * PatrolRadius, Color.yellow);


        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.tag.Contains(CollisionTag))
            {
                Debug.Log("Hit name: " + hit.collider.name + ", y: " + hit.point.y);
                // Convert from world point to local point
                hitPoint = transform.parent.InverseTransformPoint(hit.point);
                break;
            }
        }

        return hitPoint;


    }

    void FindPatrolPath() {
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == CollisionTag)
        {

            HasCollided = true;
        }
        else if (col.gameObject.tag == PlayerTag && CanFollowPlayer && CanSeePlayer)
        {

            
        }
    }

    enum PATROL_POINTS {
        TOP,
        BOTTOM,
        RIGHT,
        LEFT
    }
}
