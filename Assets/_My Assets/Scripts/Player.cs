using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject playerCenter;

    private readonly float speed = 4.0f;
    private readonly float damage = 1.0f;
    private readonly float takeHitRate = 1.0f;
    private readonly float knockbackStrength = 3.0f;

    private Animator animator;
    private Rigidbody2D rigidbody2d;
    private int currentAttack;
    private float timeSinceAttack;
    private bool attacked;
    private float delayToIdle;
    private bool suspended;
    private float takeHitElapsedTime;
    private Vector2 knockback;

    public bool Attacked()
    {
        return attacked;
    }

    public void SetSuspended(bool suspended)
    {
        this.suspended = suspended;
    }

    public float GetDamage()
    {
        return damage;
    }

    public void TryTakeHit(Vector3 from)
    {
        if (takeHitElapsedTime >= takeHitRate)
        {
            takeHitElapsedTime = 0;
            animator.SetTrigger("Hurt");

            knockback = (playerCenter.transform.position - from).normalized * knockbackStrength;
        }
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody2d = GetComponent<Rigidbody2D>();
        currentAttack = 1;
        timeSinceAttack = 0;
        attacked = false;
        delayToIdle = 0.0f;
        suspended = false;
        takeHitElapsedTime = takeHitRate;
        knockback = Vector2.zero;
    }

    void Update()
    {
        // Movement
        knockback -= knockback * Time.deltaTime;
        if (Mathf.Abs(knockback.x) < Mathf.Epsilon)
            knockback.x = 0;
        if (Mathf.Abs(knockback.y) < Mathf.Epsilon)
            knockback.y = 0;

        float inputX = 0;
        float inputY = 0;
        if (!suspended)
        {
            inputX = Input.GetAxis("Horizontal");
            inputY = Input.GetAxis("Vertical");
        }
        if (inputX > 0)
            GetComponent<SpriteRenderer>().flipX = false;
        else if (inputX < 0)
            GetComponent<SpriteRenderer>().flipX = true;
        Vector2 velocity = new Vector2(inputX, inputY) * speed;
        if (velocity.magnitude > speed)
            velocity = velocity.normalized * speed;
        rigidbody2d.velocity = velocity + knockback;

        // Attack animations
        attacked = false;
        timeSinceAttack += Time.deltaTime;
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.K)) && timeSinceAttack > 0.25f && !suspended)
        {
            attacked = true;
            if (timeSinceAttack > 1.0f)
                currentAttack = 1;
            animator.SetTrigger("Attack" + currentAttack);
            currentAttack++;
            if (currentAttack > 3)
                currentAttack = 1;
            timeSinceAttack = 0.0f;
        }
        // Run animation
        else if (Mathf.Abs(inputX) > 0 || Mathf.Abs(inputY) > 0)
        {
            delayToIdle = 0.05f;
            animator.SetInteger("AnimState", 1);
        }
        // Idle animation
        else
        {
            delayToIdle -= Time.deltaTime;
            if (delayToIdle < 0)
                animator.SetInteger("AnimState", 0);
        }
        
        // Hurt timing
        if (takeHitElapsedTime < takeHitRate)
            takeHitElapsedTime += Time.deltaTime;
    }
}
