using OpenAI_API.Chat;
using OpenAI_API;
using TMPro;
using UnityEngine;
using OpenAI_API.Models;
using System;
using System.Threading.Tasks;
using OpenAI_API.Images;
using System.Net.Http;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

public class PortalQuestSystem : MonoBehaviour
{
    // Portal prefab, instance, state
    public GameObject portalPrefab;
    private GameObject portalInstance;
    private enum PortalState { Unopened, Open, Closing, Closed };
    private PortalState portalState = PortalState.Unopened;
    private float closingElapsedTime = 0.0f;
    private readonly float closingTime = 1.0f;

    // Player-portal interaction
    public Transform playerCenter;
    private Transform portalCenter;
    private readonly float interactRadius = 2.0f;
    public TextMeshProUGUI interactText;

    // Player-quest interaction
    public FavorSystem favorSystem;
    public Player player;
    private OpenAIAPI api;
    private Conversation eliminationQuestChat;
    private Conversation protectQuestChat;
    private HttpClient httpClient;
    public TextMeshProUGUI generatingText;
    private bool generatingQuest = false;
    private class EliminationQuestInfo
    {
        public int EliminationCount { get; set; }
        public string CreatureName { get; set; }
        public float CreatureHealth { get; set; }
        public float CreatureSpeed { get; set; }
    }
    private EliminationQuestInfo eliminationQuestInfo;
    private class ProtectQuestInfo
    {
        public int Seconds { get; set; }
        public string CreatureName { get; set; }
        public float CreatureHealth { get; set; }
        public float CreatureSpeed { get; set; }
    }
    private ProtectQuestInfo protectQuestInfo;
    private int questType;
    private float protectQuestElapsedTime = 0;
    private int questsCompleted = 0;
    public Transform teleportPadTransform;
    private bool doingQuest = false;
    public TextMeshProUGUI questDescriptionText;
    public GameObject villager;

    // Creatures
    public CombatSystem combatSystem;
    public GameObject creaturePrefab;
    private Sprite creatureSprite;
    public GameObject[] creatureSpawnPoints;
    private readonly float spawnTime = 3.0f;
    private float spawnTimeElapsed = 0;

    // End game
    public Transform knightTransform;
    public GameObject knightQuestionMark;
    public BoxCollider2D gateCollider;
    public DialogueSystem dialogueSystem;
    public BoxCollider2D playerCollider;
    public BoxCollider2D endGameCollider;
    public TextMeshProUGUI demoCompleteText;

    void Start()
    {
        // TODO: REMOVE ME
        //OpenPortal();

        // Misc setup
        portalCenter = portalPrefab.transform;
        interactText.enabled = false;
        generatingText.enabled = false;
        questDescriptionText.enabled = false;
        demoCompleteText.enabled = false;

        // Quest assistant setup
        try
        {
            api = new OpenAIAPI(ApiKey.GetApiKey());

            eliminationQuestChat = api.Chat.CreateConversation();
            eliminationQuestChat.Model = Model.GPT4_Turbo;
            eliminationQuestChat.RequestParameters.Temperature = 0.5; // 0 = strict, 0.9 = creative.
            eliminationQuestChat.AppendSystemMessage("You generate quests for the player. Quests are in the format of \"Eliminate x y\", where x is a number between 1 and 4, and y is a fantasy creature name. Be creative with the creature name. The creature's health is between 2 and 5. The creature's speed is between 0.5 and 1.2. x, the creature's health, and the creature's speed are based on the player's 'Favor', where favor is between 0 and 100. Low favor means x and creature health and speed should be higher, and high favor means x and creature health and speed should be lower. Output JSON. x is in the key 'EliminationCount', y is in the key 'CreatureName', creature health is in the key 'CreatureHealth', and creature speed is in the key 'CreatureSpeed'.");

            protectQuestChat = api.Chat.CreateConversation();
            protectQuestChat.Model = Model.GPT4_Turbo;
            protectQuestChat.RequestParameters.Temperature = 0.5;
            protectQuestChat.AppendSystemMessage("You generate quests for the player. Quests are in the format of \"Protect the villager for x seconds\", where x is a number between 10 and 30. Come up with a fantasy creature name. The creature's health is between 2 and 5. The creature's speed is between 0.5 and 1.2. x, the creature's health, and the creature's speed are based on the player's 'Favor', where favor is between 0 and 100. Low favor means x and creature health and speed should be higher, and high favor means x and creature health and speed should be lower. Output JSON. x is in the key 'Seconds', the creature's name is in the key 'CreatureName', creature health is in the key 'CreatureHealth', and creature speed is in the key 'CreatureSpeed'.");
        }
        catch (Exception e)
        {
            Debug.LogError("Error creating quest conversation(s): " + e.Message);
        }

        // HTTP client for downloading DALL E images
        httpClient = new HttpClient();
    }

    void Update()
    {
        if (portalState == PortalState.Open)
        {
            // Display "press e to interact ... " if player close enough to portal
            float playerDistance = (portalCenter.position - playerCenter.position).magnitude;
            if (playerDistance <= interactRadius)
            {
                // Generate a quest if player prssed e
                interactText.enabled = (generatingQuest) ? false : true;
                if (Input.GetKeyDown(KeyCode.E))
                    DoQuest();
            }
            else
                interactText.enabled = false;

            // Quest logic
            if (doingQuest)
            {
                // Spawn enemies
                spawnTimeElapsed += Time.deltaTime;
                if (spawnTimeElapsed >= spawnTime)
                {
                    int spawnLocationIndex = UnityEngine.Random.Range(0, creatureSpawnPoints.Length);
                    InstantiateCreature(creatureSpawnPoints[spawnLocationIndex].transform.position);
                    spawnTimeElapsed = 0;
                }

                // Count down seconds if doing protect type quest
                if (questType == 1)
                {
                    protectQuestElapsedTime += Time.deltaTime;
                    if (protectQuestElapsedTime >= 1.0f)
                    {
                        protectQuestInfo.Seconds--;
                        UpdateQuestDescriptionText();
                        protectQuestElapsedTime -= 1.0f;
                        if (protectQuestInfo.Seconds <= 0)
                            FinishQuest();
                    }
                }
            }
        }

        // Close and destory the portal if state == closing
        if (portalState == PortalState.Closing)
        {
            closingElapsedTime += Time.deltaTime;
            if (closingElapsedTime >= closingTime)
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

    private async void DoQuest()
    {
        Debug.Log("Generating quest and enemy...");
        generatingQuest = true;
        player.SetSuspended(true);
        generatingText.enabled = true;

        await GenerateQuest();
        await GenerateCreatureTexture();
        player.transform.position = new Vector3(teleportPadTransform.position.x, teleportPadTransform.position.y + 1.0f, teleportPadTransform.position.z);
        questDescriptionText.enabled = true;
        UpdateQuestDescriptionText();
        spawnTimeElapsed = 0;
        doingQuest = true;
        
        generatingText.enabled = false;
        player.SetSuspended(false);
        generatingQuest = false;
    }

    private async Task GenerateQuest()
    {
        try
        {
            questType = UnityEngine.Random.Range(0, 2);
            Debug.Log("Quest type: " + questType);
            string response;
            if (questType == 0)
            {
                // Elminiation type quest
                eliminationQuestChat.AppendUserInput("Generate a quest for the player. The player's favor is " + favorSystem.GetFavor() + ".");
                response = await eliminationQuestChat.GetResponseFromChatbotAsync();
                villager.SetActive(false);
                Debug.Log("Elimination quest chatbot response: " + response);
            }
            else
            {
                // Protect type quest
                protectQuestChat.AppendUserInput("Generate a quest for the player. The player's favor is " + favorSystem.GetFavor() + ".");
                response = await protectQuestChat.GetResponseFromChatbotAsync();
                villager.SetActive(true);
                Debug.Log("Protect quest chatbot response: " + response);
            }

            // Deserialize the response from JSON
            int jsonStart = response.IndexOf('{');
            int jsonEnd = response.LastIndexOf('}');
            string json = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
            if (questType == 0)
                eliminationQuestInfo = JsonSerializer.Deserialize<EliminationQuestInfo>(json);
            else
                protectQuestInfo = JsonSerializer.Deserialize<ProtectQuestInfo>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("Error getting response from quest chatbot: " + e.Message);
        }
    }

    // TODO: remove me
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    private async Task GenerateCreatureTexture()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        try
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Assets\\_My Assets\\Sprites\\Creature.png");
            string metaPath = path + ".meta";

            // Delete previous Creature sprite
            if (File.Exists(path))
                File.Delete(path);
            if (File.Exists(metaPath))
                File.Delete(metaPath);

            // DALL E request
            string creatureName = (questType == 0) ? eliminationQuestInfo.CreatureName : protectQuestInfo.CreatureName;
            var result = await api.ImageGenerations.CreateImageAsync(new ImageGenerationRequest(creatureName + " as pixel art. The background should be a solid black color and contrast the creature.", Model.DALLE3, ImageSize._1024));
            Debug.Log(result.Data[0].Url);

            // Download the generated image from its URL
            var buffer = await httpClient.GetByteArrayAsync(result.Data[0].Url);
            await File.WriteAllBytesAsync(path, buffer);

            // Load the creature's sprite
            Texture2D creatureTexture = new Texture2D(0, 0);
            creatureTexture.LoadImage(File.ReadAllBytes(path));
            Texture2D finalTexture = RemoveBackground(creatureTexture);
            creatureSprite = Sprite.Create(finalTexture, new Rect(0, 0, finalTexture.width, finalTexture.height), Vector2.zero);
        }
        catch (Exception e)
        {
            Debug.LogError("Error getting response from image generation: " + e.Message);
        }
    }

    private Texture2D RemoveBackground(Texture2D source)
    {
        int width = source.width;
        int height = source.height;

        // Create empty output texture and make it transparent
        Texture2D output = new(width, height);
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                output.SetPixel(x, y, Color.clear);

        // Postprocessing setup
        Color backgroundColor = source.GetPixel(width / 10, height / 10); // Assume near top-left is background color
        Queue<Vector2Int> pixelQueue = new();
        bool[,] marked = new bool[width, height];

        // Find the pixels, starting from the center and moving out vertically, that is not the background
        // Enqueue these two non-background pixels to start the postprocessing from
        Vector2Int middle = new Vector2Int(width / 2, height / 2);
        Vector2Int top = new Vector2Int(middle.x, middle.y + 1);
        Vector2Int bottom = new Vector2Int(middle.x, middle.y - 1);
        bool foundTop = false;
        bool foundBottom = false;
        do
        {
            // Top
            if (top.y < height)
            {
                Color topColor = source.GetPixel(top.x, top.y);
                if (IsSimilarColor(topColor, backgroundColor))
                    top.y++;
                else
                {
                    pixelQueue.Enqueue(top);
                    foundTop = true;
                }
            }
            else
                foundTop = true;
            // Bottom
            if (bottom.y >= 0)
            {
                Color bottomColor = source.GetPixel(bottom.x, bottom.y);
                if (IsSimilarColor(bottomColor, backgroundColor))
                    bottom.y--;
                else
                {
                    pixelQueue.Enqueue(bottom);
                    foundBottom = true;
                }
            }
            else
                foundBottom = true;
        }
        while (!foundTop || !foundBottom);

        // Postprocessing algorithm (spread outwards from middle)
        while (pixelQueue.Count > 0)
        {
            // Take out the next pixel in the queue and add it to the output texture
            Vector2Int pixelPosition = pixelQueue.Dequeue();
            Color pixelColor = source.GetPixel(pixelPosition.x, pixelPosition.y);
            output.SetPixel(pixelPosition.x, pixelPosition.y, pixelColor);

            // Queue neighbouring pixels if they are in bounds, not marked, and not the white background
            Vector2Int[] neighbouringPixels =
            {
                new(pixelPosition.x - 1, pixelPosition.y),
                new(pixelPosition.x + 1, pixelPosition.y),
                new(pixelPosition.x, pixelPosition.y + 1),
                new(pixelPosition.x, pixelPosition.y - 1)
            };
            for (int i = 0; i < neighbouringPixels.Length; i++)
            {
                Vector2Int neighbourPosition = neighbouringPixels[i];
                if (neighbourPosition.x >= 0 && neighbourPosition.x < width && neighbourPosition.y >= 0 && neighbourPosition.y < height && !marked[neighbourPosition.x, neighbourPosition.y])
                {
                    Color neighbourColor = source.GetPixel(neighbourPosition.x, neighbourPosition.y);
                    if (IsSimilarColor(neighbourColor, backgroundColor) == false)
                    {
                        pixelQueue.Enqueue(neighbourPosition);
                        marked[neighbourPosition.x, neighbourPosition.y] = true;
                    }
                }
            }
        }

        // Apply changes to the output texture and return it
        output.Apply();
        return output;
    }

    private bool IsSimilarColor(Color colorA, Color colorB)
    {
        float similarColorThreshold = 0.04f;

        float rDistance = Mathf.Abs(colorA.r - colorB.r);
        float gDistance = Mathf.Abs(colorA.g - colorB.g);
        float bDistance = Mathf.Abs(colorA.b - colorB.b);

        return rDistance <= similarColorThreshold && gDistance <= similarColorThreshold && bDistance <= similarColorThreshold;
    }

    private void InstantiateCreature(Vector3 position)
    {
        GameObject creatureInstance = Instantiate(creaturePrefab);
        creatureInstance.GetComponent<SpriteRenderer>().sprite = creatureSprite;
        creatureInstance.GetComponent<SpriteRenderer>().size = new Vector2(1.5f, 1.5f);
        if (questType == 0)
        {
            creatureInstance.GetComponent<Creature>().SetHealth(eliminationQuestInfo.CreatureHealth);
            creatureInstance.GetComponent<Creature>().SetSpeed(eliminationQuestInfo.CreatureSpeed);
            creatureInstance.GetComponent<Creature>().SetTarget(playerCenter);
        }
        else
        {
            creatureInstance.GetComponent<Creature>().SetHealth(protectQuestInfo.CreatureHealth);
            creatureInstance.GetComponent<Creature>().SetSpeed(protectQuestInfo.CreatureSpeed);
            creatureInstance.GetComponent<Creature>().SetTarget(villager.transform);
        }
        creatureInstance.transform.position = position;
        combatSystem.AddCreature(creatureInstance);
    }

    public void CreatureDied()
    {
        if (doingQuest && questType == 0)
        {
            eliminationQuestInfo.EliminationCount--;
            UpdateQuestDescriptionText();
            if (eliminationQuestInfo.EliminationCount <= 0)
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
            dialogueSystem.EndGameReached();
        }
    }

    private void UpdateQuestDescriptionText()
    {
        if (questType == 0)
            questDescriptionText.text = "Eliminate " + eliminationQuestInfo.EliminationCount + " " + eliminationQuestInfo.CreatureName;
        else
            questDescriptionText.text = "Protect the villager for " + protectQuestInfo.Seconds + " seconds from " + protectQuestInfo.CreatureName;
    }
}
