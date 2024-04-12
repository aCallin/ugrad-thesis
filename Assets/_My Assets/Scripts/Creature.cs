using UnityEngine;

public class Creature : MonoBehaviour
{
    // Stats
    private float health;
    private float speed;

    // Movement
    private Vector3 creatureCenter;
    private Transform target;
    private readonly float knockbackStrength = 3.0f;
    private Vector3 knockback;

    // Animations
    private SpriteRenderer spriteRenderer;
    private float step;
    private readonly float amplitude = 0.1f;
    private readonly float hurtAnimationTime = 0.15f;
    private float hurtAnimationElapsedTime;

    void Start()
    {
        knockback = Vector3.zero;
        hurtAnimationElapsedTime = hurtAnimationTime;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // Movement
        knockback -= knockback * Time.deltaTime;
        if (Mathf.Abs(knockback.x) < Mathf.Epsilon)
            knockback.x = 0;
        if (Mathf.Abs(knockback.y) < Mathf.Epsilon)
            knockback.y = 0;
        transform.position += knockback * Time.deltaTime;
        creatureCenter = transform.position + new Vector3(0.75f, 0.75f, 0);
        Vector3 moveDirection = (target.position - creatureCenter).normalized;
        transform.position += (moveDirection * speed) * Time.deltaTime;

        // Hurt animation
        if (hurtAnimationElapsedTime < hurtAnimationTime)
        {
            hurtAnimationElapsedTime += Time.deltaTime;
            if (hurtAnimationElapsedTime >= hurtAnimationTime)
                spriteRenderer.color = Color.white;
        }

        // Grow / shrink animation
        step += Time.deltaTime * 2;
        if (step >= 360)
            step -= 360;
        float offset = Mathf.Sin(step) * amplitude;
        spriteRenderer.size = new Vector2(1.5f, 1.5f + offset);
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

    public Vector3 GetCenter()
    {
        return creatureCenter;
    }

    public void TakeDamage(float damage, Vector3 from)
    {
        health -= damage;

        // Start hurt animation
        hurtAnimationElapsedTime = 0;
        spriteRenderer.color = Color.red;

        // Add knockback
        knockback = (creatureCenter - from).normalized * knockbackStrength;
    }

    public bool IsDead()
    {
        return health <= 0;
    }
}
