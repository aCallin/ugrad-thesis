using OpenAI_API.Chat;
using OpenAI_API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OpenAI_API.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;

// OpenAI_API reference: https://github.com/OkGoDoIt/OpenAI-API-dotnet

public class DialogueSystem : MonoBehaviour
{
    public TextMeshProUGUI startDialogueText;
    public Transform knightCenter;
    public Transform playerCenter;
    private readonly float maxTalkDistance = 2.0f;
    private bool inDialogue = false;

    public Player player;
    public GameObject dialogueContainer;
    public TextMeshProUGUI content;
    public TMP_InputField textbox;
    public Button leaveButton;

    private enum MessageFrom { Knight, Player, None };

    private OpenAIAPI api;
    private Conversation chat;

    class GPTJson
    {
        public string Response { get; set; }
        public int Politeness { get; set; }
    }

    public FavorSystem favorSystem;

    public PortalQuestSystem portalQuestSystem;

    void Start()
    {
        content.text = string.Empty;
        dialogueContainer.SetActive(false);
        textbox.onSubmit.AddListener(OnTextboxSubmit);
        leaveButton.onClick.AddListener(OnLeaveButtonClicked);
        AppendMessageToDialogueBox("A knight stands in the way of the gate...", MessageFrom.None);

        // Set up the assistant to act as the knight
        try
        {
            api = new OpenAIAPI(ApiKey.GetApiKey());
            chat = api.Chat.CreateConversation();
            chat.Model = Model.GPT4_Turbo;
            chat.RequestParameters.Temperature = 0.5; // 0 = strict, 0.9 = creative.
            chat.AppendSystemMessage("You are a knight in a garden. You are also the gatekeeper that is preventing the player from leaving the garden. The player has to complete three quests to leave the garden. You don't tell the player what quests they have to do, but that there is a portal to the right where they can do their quests. Output JSON. Your response to the player is in the key 'Response'. The player's politeness in their last message, on a scale of 0 meaning very rude and 100 meaning very polite, is in the key 'Politeness'.");
        }
        catch (Exception e)
        {
            Debug.Log("Error while initializing AI conversation: " + e.Message);
        }
    }

    void Update()
    {
        // Hide "press e to ..." if dialogue is already open
        if (inDialogue)
            startDialogueText.enabled = false;
        else
        {
            // Show "press e to ..." if player is close enough to knight
            float distanceFromKnight = (knightCenter.position - playerCenter.position).magnitude;
            if (distanceFromKnight <= maxTalkDistance)
            {
                startDialogueText.enabled = true;

                // Start dialogue if player pressed e
                if (Input.GetKeyDown(KeyCode.E))
                {
                    inDialogue = true;
                    player.SetSuspended(true);
                    dialogueContainer.SetActive(true);
                    textbox.text = string.Empty;
                }
            }
            else
                startDialogueText.enabled = false;
        }
    }

    public void EndGameReached()
    {
        chat.AppendSystemMessage("The player has now completed all three quests and can no longer do any more. As the gatekeeper, you can tell the player that they can now leave the garden through the gate.");
        Debug.Log("Dialogue end game reached");
    }

    private void AppendMessageToDialogueBox(string message, MessageFrom from)
    {
        switch (from)
        {
            case MessageFrom.Knight:
                content.text += "Knight: ";
                break;
            case MessageFrom.Player:
                content.text += "You: ";
                break;
            case MessageFrom.None:
                // No context...
                break;
            default:
                Debug.LogError("MessageFrom case not handled");
                break;
        }
        content.text += message + Environment.NewLine;
    }

    private async void OnTextboxSubmit(string content)
    {
        AppendMessageToDialogueBox(content, MessageFrom.Player);
        textbox.text = string.Empty;
        textbox.interactable = false;
        await GetResponseFromInput(content);
        textbox.interactable = true;
    }

    private async Task GetResponseFromInput(string input)
    {
        chat.AppendUserInput(input);
        string response = await chat.GetResponseFromChatbotAsync();

        // Deserialize the JSON response
        int jsonStart = response.IndexOf('{');
        int jsonEnd = response.LastIndexOf('}');
        string json = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
        Debug.Log("JSON: " + json);
        try
        {
            GPTJson gptJson = JsonSerializer.Deserialize<GPTJson>(json);

            // Add the response to the dialogue box
            AppendMessageToDialogueBox(gptJson.Response, MessageFrom.Knight);

            // Adjust favor based on user rudeness/politeness
            Debug.Log("Politeness: " + gptJson.Politeness);
            if (gptJson.Politeness >= 0 && gptJson.Politeness <= 10)
                favorSystem.AddFavor(-15);
            else if (gptJson.Politeness <= 25)
                favorSystem.AddFavor(-10);
            else if (gptJson.Politeness < 50)
                favorSystem.AddFavor(-5);
            else if (gptJson.Politeness >= 80 && gptJson.Politeness <= 90)
                favorSystem.AddFavor(5);
            else if (gptJson.Politeness <= 100)
                favorSystem.AddFavor(10);

            // Open the portal if it has been mentioned by the knight for the first time
            if (portalQuestSystem.PortalIsOpen() == false && gptJson.Response.ToLower().Contains("portal"))
                portalQuestSystem.OpenPortal();
        }
        catch (Exception e)
        {
            Debug.LogError("Error while deserializing JSON: " + e.Message);
        }
    }

    private void OnLeaveButtonClicked()
    {
        inDialogue = false;
        player.SetSuspended(false);
        dialogueContainer.SetActive(false);
    }
}
