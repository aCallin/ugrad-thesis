using UnityEngine;

public class Farmer : MonoBehaviour
{
    private readonly float xLeft = -4.5f;
    private readonly float xRight = -2.0f;
    private readonly float runSpeed = 2.0f;
    private readonly float minIdleDuration = 1.5f;
    private readonly float maxIdleDuration = 4.0f;
    private readonly float minRunDuration = 0.5f;
    private readonly float maxRunDuration = 2.0f;
    private readonly float hurtDuration = 0.333f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private enum State { Idle, Run, Hurt }
    private State state;
    private float stateDuration;
    private float stateElapsedTime;
    private int runDirection;

    public void TakeHit()
    {
        HandleFutureState(State.Hurt, true);
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        HandleFutureState(State.Idle, true);
        runDirection = 1;
    }

    void Update()
    {
        stateElapsedTime += Time.deltaTime;

        if (state == State.Idle)
            HandleFutureState(State.Run, false);
        else if (state == State.Run)
        {
            float newX = transform.position.x + runSpeed * runDirection * Time.deltaTime;
            if (newX <= xLeft || newX >= xRight)
            {
                runDirection = -runDirection;
                spriteRenderer.flipX = runDirection != 1;
                newX = (newX <= xLeft) ? xLeft + Mathf.Epsilon : xRight - Mathf.Epsilon;
            }
            Vector3 newPosition = new Vector3(newX, transform.position.y, transform.position.z);
            transform.position = newPosition;

            HandleFutureState(State.Idle, false);
        }
        else if (state == State.Hurt)
            HandleFutureState(State.Idle, false);
    }

    private void HandleFutureState(State futureState, bool abrupt)
    {
        if (stateElapsedTime >= stateDuration || abrupt)
        {
            state = futureState;
            switch (futureState)
            {
                case State.Idle:
                    stateDuration = Random.Range(minIdleDuration, maxIdleDuration);
                    animator.SetInteger("State", 0);
                    break;
                case State.Run:
                    stateDuration = Random.Range(minRunDuration, maxRunDuration);
                    animator.SetInteger("State", 1);
                    break;
                case State.Hurt:
                    stateDuration = hurtDuration;
                    animator.SetInteger("State", 2);
                    break;
            }
            stateElapsedTime = 0;
        }
    }
}
