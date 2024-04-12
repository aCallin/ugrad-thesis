using UnityEngine;

public class LabBox : MonoBehaviour
{
    public TraditionalCreature creature;

    void Start()
    {
        creature.SetTarget(transform);
        creature.SetSpeed(1.0f);
        creature.SetHealth(3.0f);
    }

    void Update()
    {
        // Move box with cursor
        Vector3 screenToWorldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(screenToWorldPoint.x, screenToWorldPoint.y, 0);

        // Creature stuff
        if (Input.GetKeyDown(KeyCode.H))
            creature.TakeHit(1.0f, transform.position);
    }
}
