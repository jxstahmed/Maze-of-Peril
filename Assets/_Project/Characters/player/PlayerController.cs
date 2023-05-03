using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed = 1f;
    public float collisionOffset = 0.05f;
    [SerializeField] private ContactFilter2D movementFilter;
    private Vector2 movementInput;
    private Rigidbody2D _rb;
    List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    Animator animator;
    SpriteRenderer spriteRenderer;



    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate(){
        // If movement input is not 0 try to move
        if(movementInput != Vector2.zero){
            int Count = _rb.Cast(
                movementInput, // X and Y values between -1 and 1 that represent the directon of the body to look for collisons
                movementFilter, // The settings that determine where a collsion can occur on such as layers to collide with
                castCollisions, // List of collisons to store the found collsions into after the Cast is finished
                moveSpeed * Time.fixedDeltaTime + collisionOffset); // The amount to cast equal to the movement plus an offset
            if(Count == 0)
                _rb.MovePosition(_rb.position + movementInput * moveSpeed * Time.fixedDeltaTime);
            else //test if only blocked in one direction
            {
                Vector2 testVerticalCollision = movementInput;
                testVerticalCollision.y = 0;
                Count = _rb.Cast(
                    testVerticalCollision, // X and Y values between -1 and 1 that represent the directon of the body to look for collisons
                    movementFilter, // The settings that determine where a collsion can occur on such as layers to collide with
                    castCollisions, // List of collisons to store the found collsions into after the Cast is finished
                    moveSpeed * Time.fixedDeltaTime + collisionOffset); // The amount to cast equal to the movement plus an offset
                if (Count == 0)
                    _rb.MovePosition(_rb.position + testVerticalCollision * moveSpeed * Time.fixedDeltaTime);
                else
                {
                    Vector2 testHorizontalCollision = movementInput;
                    testHorizontalCollision.x = 0;
                    Count = _rb.Cast(
                        testHorizontalCollision, // X and Y values between -1 and 1 that represent the directon of the body to look for collisons
                        movementFilter, // The settings that determine where a collsion can occur on such as layers to collide with
                        castCollisions, // List of collisons to store the found collsions into after the Cast is finished
                        moveSpeed * Time.fixedDeltaTime + collisionOffset); // The amount to cast equal to the movement plus an offset
                    if (Count == 0)
                        _rb.MovePosition(_rb.position + testHorizontalCollision * moveSpeed * Time.fixedDeltaTime);
                }
            }
            animator.SetBool("isMoving_H", true);
        }
        else
            animator.SetBool("isMoving_H", false);

        // Set direction to facing
        if (movementInput.x < 0)
            spriteRenderer.flipX = true;
        else if(movementInput.x > 0)
            spriteRenderer.flipX = false;
    }

    void OnMove(InputValue movementValue)
    {
        movementInput = movementValue.Get<Vector2>();
    }
}
