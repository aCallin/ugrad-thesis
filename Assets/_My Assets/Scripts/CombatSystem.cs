using System.Collections;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    // Player
    public Player player;
    public Transform playerCenter;
    public BoxCollider2D playerAttackHitbox;
    private BoxCollider2D playerHitbox;

    // Farmer
    public BoxCollider2D farmerHitbox;
    public Farmer farmer;
    public FavorSystem favorSystem;
    private readonly int favorDecrement = -7;

    // Creatures
    public PortalQuestSystem portalQuestSystem;
    private ArrayList creatureInstances;

    // Villager
    public GameObject villager;
    private BoxCollider2D villagerHitbox;

    void Start()
    {
        creatureInstances = new ArrayList();
        playerHitbox = player.GetComponent<BoxCollider2D>();
        villagerHitbox = villager.GetComponent<BoxCollider2D>();
    }

    void LateUpdate()
    {
        // Player-farmer interaction
        if (player.Attacked() && playerAttackHitbox.IsTouching(farmerHitbox))
        {
            farmer.TakeHit();
            favorSystem.AddFavor(favorDecrement);
        }

        // Player-creature interaction
        for (int i = creatureInstances.Count - 1; i >= 0; i--)
        {
            GameObject creatureGameObject = creatureInstances[i] as GameObject;
            Creature creature = creatureGameObject.GetComponent<Creature>();
            BoxCollider2D creatureHitbox = creatureGameObject.GetComponent<BoxCollider2D>();

            // Take damage if hit by player
            if (player.Attacked() && playerAttackHitbox.IsTouching(creatureHitbox))
                creature.TakeDamage(player.GetDamage(), playerCenter.position);

            // Damage player if close enough
            if (creatureHitbox.IsTouching(playerHitbox))
                player.TryTakeHit(creature.GetCenter());

            // Damage villager if close enough
            if (villager.activeSelf == true && (creature.GetCenter() - villager.transform.position).magnitude <= 1.0f)
                villager.GetComponent<Villager>().TakeHit();

            // Destory dead creatures and remove from list
            if (creature.IsDead())
            {
                creatureInstances.RemoveAt(i);
                Destroy(creatureGameObject);
                portalQuestSystem.CreatureDied();
            }
        }
    }

    public void AddCreature(GameObject creature)
    {
        creatureInstances.Add(creature);
    }

    public void DestroyAllCreatures()
    {
        for (int i = creatureInstances.Count - 1; i >= 0; i--)
        {
            GameObject creatureGameObject = creatureInstances[i] as GameObject;

            creatureInstances.RemoveAt(i);
            Destroy(creatureGameObject);
        }
    }
}
