using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    public float damage = 50f;
    public Collider2D swordCollider;
    [SerializeField] Vector2 rightAttackOffset;
    [SerializeField] Vector2 upAttackOffset;
    [SerializeField] Vector2 DownAttackOffset;
    
    // Start is called before the first frame update
    void Start()
    {
        swordCollider.enabled = false;
        rightAttackOffset = swordCollider.offset;
    }

    public void AttackRight()
    {
        print("right");
        swordCollider.enabled = true;
        swordCollider.offset = rightAttackOffset;
    }
    public void AttackLeft()
    {
        print("left");
        swordCollider.enabled = true;
        swordCollider.offset = new Vector2(rightAttackOffset.x * -1, rightAttackOffset.y);
    }
    public void AttackUp()
    {
        print("up");
        swordCollider.enabled = true;
        swordCollider.offset = upAttackOffset;
    }
    public void AttackDown()
    {
        print("down");
        swordCollider.enabled = true;
        swordCollider.offset = DownAttackOffset;
    }
    public void StopAttack()
    {
        swordCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        print("enemy");
        if (other.tag != "Enemy")
            return;
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
            enemy.Health -= damage;
       
    }
}
