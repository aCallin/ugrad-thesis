using UnityEngine;

public class Villager : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private readonly float invincibilityTime = 1.0f;
    private readonly float flashTime = 0.15f;
    private float invincibilityElapsedTime;
    private float flashElapsedTime;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        invincibilityElapsedTime = 0;
        flashElapsedTime = flashTime;
    }

    void Update()
    {
        if (invincibilityElapsedTime < invincibilityTime)
            invincibilityElapsedTime += Time.deltaTime;
        if (flashElapsedTime < flashTime)
        {
            flashElapsedTime += Time.deltaTime;
            if (flashElapsedTime >= flashTime)
                spriteRenderer.color = Color.white;
        }
    }

    public void TakeHit()
    {
        if (invincibilityElapsedTime >= invincibilityTime)
        {
            invincibilityElapsedTime = 0;
            flashElapsedTime = 0;
            spriteRenderer.color = Color.red;
        }
    }
}
