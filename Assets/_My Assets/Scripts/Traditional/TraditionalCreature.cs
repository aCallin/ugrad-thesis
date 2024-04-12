using UnityEngine;

public class TraditionalCreature : MonoBehaviour
{
    // Stats
    private float health;
    private float speed;

    // Movement
    private Transform target;
    private readonly float knockbackStrength = 3.0f;
    private Vector3 knockback = Vector3.zero;
    private SpriteRenderer spriteRenderer;
    private readonly float noMovementThreshold = 0.15f;

    // Animations
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // Movement and facing direction
        knockback -= knockback * Time.deltaTime;
        if (Mathf.Abs(knockback.x) < noMovementThreshold)
            knockback.x = 0;
        if (Mathf.Abs(knockback.y) < noMovementThreshold)
            knockback.y = 0;
        transform.position += knockback * Time.deltaTime;
        Vector3 offset = (target.position - transform.position);
        if (offset.magnitude > noMovementThreshold)
        {
            Vector3 moveDirection = (target.position - transform.position).normalized;
            transform.position += (moveDirection * speed) * Time.deltaTime;
            animator.SetBool("Is Moving", true);
        }
        else
            animator.SetBool("Is Moving", false);
        spriteRenderer.flipX = (target.position.x < transform.position.x);
    }

    public void SetHealth(float health)
    {
        this.health = health;
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void TakeHit(float damage, Vector3 from)
    {
        health -= damage;
        animator.SetTrigger("Take Hit");
        knockback = (transform.position - from).normalized * knockbackStrength;
    }

    public bool IsDead()
    {
        return health <= 0;
    }
}
