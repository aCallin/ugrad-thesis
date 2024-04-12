using TMPro;
using UnityEngine;

public class TraditionalPortalQuestSystem : MonoBehaviour
{
    // Portal
    public GameObject portalPrefab;
    private GameObject portalInstance;
    private Transform portalCenter;
    private enum PortalState { Unopened, Open, Closing, Closed };
    private PortalState portalState = PortalState.Unopened;
    private readonly float portalClosingTime = 1.0f;
    private float portalClosingElapsedTime = 0.0f;
    private readonly float portalInteractRadius = 2.0f;
    public TextMeshProUGUI portalInteractText;

    // Player
    public Player player;
    public Transform playerCenter;

    // Quest
    public FavorSystem favorSystem;
    private int questType;
    private int eliminationCount;
    private int seconds;
    private string creatureName;
    private float creatureHealth;
    private float creatureSpeed;
    private bool doingQuest = false;
    private int questsCompleted = 0;
    private float secondsElapsedTime = 0;
    public Transform teleportPadTransform;
    public GameObject villager;
    public TextMeshProUGUI questDescriptionText;

    // Creatures
    public TraditionalCombatSystem combatSystem;
    private int creaturePrefabIndex;
    public GameObject[] creaturePrefabs;
    public GameObject[] creatureSpawnPoints;
    private readonly float spawnTime = 3.0f;
    private float spawnTimeElapsed = 0;

    // End game
    public Transform knightTransform;
    public GameObject knightQuestionMark;
    public BoxCollider2D gateCollider;
    public TraditionalDialogueSystem dialogueSystem;
    public BoxCollider2D playerCollider;
    public BoxCollider2D endGameCollider;
    public TextMeshProUGUI demoCompleteText;

    void Start()
    {
        // TODO: REMOVE ME
        OpenPortal();

        // Misc setup
        portalCenter = portalPrefab.transform;
        portalInteractText.enabled = false;
        questDescriptionText.enabled = false;
        demoCompleteText.enabled = false;
    }

    void Update()
    {
        if (portalState == PortalState.Open)
        {
            // Display "press e to interact ... " if player close enough to portal
            float playerDistance = (portalCenter.position - playerCenter.position).magnitude;
            if (playerDistance <= portalInteractRadius)
            {
                // Generate a quest if player prssed e
                portalInteractText.enabled = true;
                if (Input.GetKeyDown(KeyCode.E))
                    DoQuest();
            }
            else
                portalInteractText.enabled = false;

            // Quest logic
            if (doingQuest)
            {
                // Spawn enemies
                spawnTimeElapsed += Time.deltaTime;
                if (spawnTimeElapsed >= spawnTime)
                {
                    int spawnLocationIndex = Random.Range(0, creatureSpawnPoints.Length);
                    InstantiateCreature(creatureSpawnPoints[spawnLocationIndex].transform.position);
                    spawnTimeElapsed = 0;
                }

                // Count down seconds if doing protect type quest
                if (questType == 1)
                {
                    secondsElapsedTime += Time.deltaTime;
                    if (secondsElapsedTime >= 1.0f)
                    {
                        seconds--;
                        UpdateQuestDescriptionText();
                        secondsElapsedTime -= 1.0f;
                        if (seconds <= 0)
                            FinishQuest();
                    }
                }
            }
        }

        // Close and destory the portal if state == closing
        if (portalState == PortalState.Closing)
        {
            portalClosingElapsedTime += Time.deltaTime;
            if (portalClosingElapsedTime >= portalClosingTime)
            {
                Destroy(portalInstance);
                portalState = PortalState.Closed;
            }
        }

        // Check for collision with end game collider
        if (portalState == PortalState.Closed && playerCollider.IsTouching(endGameCollider))
        {
            demoCompleteText.enabled = true;
            player.SetSuspended(true);
        }
    }

    public void OpenPortal()
    {
        if (portalState == PortalState.Unopened)
        {
            portalInstance = Instantiate(portalPrefab);
            portalState = PortalState.Open;
        }
    }

    public void ClosePortal()
    {
        if (portalState == PortalState.Open)
        {
            portalInstance.GetComponent<Animator>().SetTrigger("ClosePortal");
            portalState = PortalState.Closing;
        }
    }

    public bool PortalIsOpen()
    {
        return portalState == PortalState.Open;
    }

    private void DoQuest()
    {
        GenerateQuest();
        player.transform.position = new Vector3(teleportPadTransform.position.x, teleportPadTransform.position.y + 1.0f, teleportPadTransform.position.z);
        questDescriptionText.enabled = true;
        UpdateQuestDescriptionText();
        spawnTimeElapsed = 0;
        doingQuest = true;
    }

    private void GenerateQuest()
    {
        questType = Random.Range(0, 2);
        villager.SetActive(questType == 1);
        if (favorSystem.GetFavor() <= 30)
        {
            eliminationCount = Random.Range(15, 21);
            seconds = Random.Range(35, 45);

            creaturePrefabIndex = 0;
            creatureName = "Evil Eyes";
            creatureHealth = Random.Range(4, 6);
            creatureSpeed = Random.Range(0.9f, 1.2f);
        }
        else if (favorSystem.GetFavor() <= 70)
        {
            eliminationCount = Random.Range(10, 15);
            seconds = Random.Range(30, 35);

            creaturePrefabIndex = 1;
            creatureName = "Goblins";
            creatureHealth = Random.Range(3, 4);
            creatureSpeed = Random.Range(0.7f, 0.9f);
        }
        else
        {
            eliminationCount = Random.Range(4, 10);
            seconds = Random.Range(20, 30);

            creaturePrefabIndex = 2;
            creatureName = "Skeletons";
            creatureHealth = Random.Range(2, 3);
            creatureSpeed = Random.Range(0.5f, 0.7f);
        }

        Debug.Log("Quest type: " + questType);
        Debug.Log("Elimination count: " + eliminationCount);
        Debug.Log("Seconds: " + seconds);
        Debug.Log("Creature prefab index: " + creaturePrefabIndex);
        Debug.Log("Creature name: " + creatureName);
        Debug.Log("Creature health: " + creatureHealth);
        Debug.Log("Creature speed: " + creatureSpeed);
    }

    private void InstantiateCreature(Vector3 position)
    {
        GameObject creatureInstance = Instantiate(creaturePrefabs[creaturePrefabIndex]);
        creatureInstance.GetComponent<TraditionalCreature>().SetHealth(creatureHealth);
        creatureInstance.GetComponent<TraditionalCreature>().SetSpeed(creatureSpeed);
        if (questType == 0)
            creatureInstance.GetComponent<TraditionalCreature>().SetTarget(playerCenter);
        else
            creatureInstance.GetComponent<TraditionalCreature>().SetTarget(villager.transform);
        creatureInstance.transform.position = position;
        combatSystem.AddCreature(creatureInstance);
    }

    public void CreatureDied()
    {
        if (doingQuest && questType == 0)
        {
            eliminationCount--;
            UpdateQuestDescriptionText();
            if (eliminationCount <= 0)
                FinishQuest();
        }
    }

    private void FinishQuest()
    {
        doingQuest = false;
        player.transform.position = new Vector3(0, 0.85f, -2);
        combatSystem.DestroyAllCreatures();
        questDescriptionText.enabled = false;
        questsCompleted++;

        if (questsCompleted == 3)
        {
            ClosePortal();
            knightTransform.position = new Vector3(5.25f, 2.0f, knightTransform.position.z);
            knightQuestionMark.SetActive(false);
            gateCollider.enabled = false;
            dialogueSystem.AllQuestsFinished();
        }
    }

    private void UpdateQuestDescriptionText()
    {
        if (questType == 0)
            questDescriptionText.text = "Eliminate " + eliminationCount + " " + creatureName;
        else
            questDescriptionText.text = "Protect the villager for " + seconds + " seconds from " + creatureName;
    }
}
