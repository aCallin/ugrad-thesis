using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TraditionalDialogueSystem : MonoBehaviour
{
    // Leading up to dialogue
    public TextMeshProUGUI startDialogueText;
    public Player player;
    public Transform playerCenter;
    public Transform knightCenter;
    private readonly float talkRadius = 2.0f;
    private bool inDialogue = false;

    // Dialogue container
    public GameObject dialogueContainer;
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI[] responseSlots;
    public Button leaveButton;

    // Dialogue options
    public FavorSystem favorSystem;
    private class DialogueInstance
    {
        public string MainText { get; set; }
        public string[] Responses;
        public int[] FavorImpacts;
        public int[] NextResponses;
    }
    private DialogueInstance[] dialogueInstances;
    private DialogueInstance currentDialogueInstance;

    void Start()
    {
        startDialogueText.enabled = false;

        // Dialogue container
        dialogueContainer.SetActive(false);
        for (int i = 0; i < responseSlots.Length; i++)
        {
            int responseIndex = i; // Need this for some reason
            responseSlots[i].GetComponent<Button>().onClick.AddListener(() => SelectResponse(responseIndex));
        }
        leaveButton.onClick.AddListener(CloseDialogue);

        // Dialogue instances
        dialogueInstances = new DialogueInstance[]
        {
            new DialogueInstance() // 0
            {
                MainText = "[A knight stands in the way of the gate...]",
                Responses = new string[]
                {
                    "Hello? Who are you?",
                    "[Stare at the knight blankly]",
                    "You're in my way...",
                    null
                },
                FavorImpacts = new int[] { 2, 0, -5, 0 },
                NextResponses = new int[] { 1, 2, 8, 0 }
            },
            new DialogueInstance() // 1
            {
                MainText = "Morning, traveller, and welcome to the Garden. I am the knight and gatekeeper of these lands, and it is my sworn duty to guide visitors such as yourself on your journey through here.",
                Responses = new string[]
                {
                    "Did you say 'Garden'? - what exactly is this place?",
                    "How did I get here?",
                    "Seems to me like you take your job too seriously...",
                    null
                },
                FavorImpacts = new int[] { 0, 0, -5, 0 },
                NextResponses = new int[] { 5, 3, 9, 0 }
            },
            new DialogueInstance() // 2
            {
                MainText = "... I see that you are still shaken upon your arrival here. Tell me, traveler, what's on your mind?",
                Responses = new string[]
                {
                    "What is this place? Where am I?",
                    "Who are you?",
                    "And why should I tell you anything?",
                    "Nothing... I think I'll take a look around"
                },
                FavorImpacts = new int[] { 0, 0, -5, 0 },
                NextResponses = new int[] { 3, 4, 8, 10 }
            },
            new DialogueInstance() // 3
            {
                MainText = "The Garden is a mysterious and magical land where visitors from all walks of life may find themselves. The workings of this place are unknown; seek out what the Gardens asks of you to take your leave.",
                Responses = new string[]
                {
                    "I see... and who are you?",
                    "What do you mean by 'mysterious and magical'?",
                    "How do I get out of here?",
                    "Fool... there's nothing 'magical' about this place",
                },
                FavorImpacts = new int[] { 5, 0, 0, -7 },
                NextResponses = new int[] { 4, 5, 6, 7 }
            },
            new DialogueInstance() // 4
            {
                MainText = "I am the knight and gatekeeper of these lands, and it is my sworn duty to guide visitors such as yourself on your journey through here. My identity is of no importance, however.",
                Responses = new string[]
                {
                    "Tell me more about this 'Garden'...",
                    "How do I leave this place?",
                    "Thank you, I think I'll take a look around...",
                    null
                },
                FavorImpacts = new int[] { 0, 0, 5, 0 },
                NextResponses = new int[] { 5, 6, 10, 0 }
            },
            new DialogueInstance() // 5
            {
                MainText = "You'll come to notice that these lands are unlike others you may have been to. Where the outside world is indifferent, the Garden notices your actions, good or bad, and shapes itself accordingly.",
                Responses = new string[]
                {
                    "That's good and all, but how do I get out of here?",
                    "Could you tell me more about yourself?",
                    "This 'Garden' of yours isn't noticing much at all",
                    null,
                },
                FavorImpacts = new int[] { 0, 5, -5, 0 },
                NextResponses = new int[] { 6, 4, 7, 0 }
            },
            new DialogueInstance() // 6
            {
                MainText = "Leaving this place will be no easy task, traveller. Three quests lie ahead of you before you can earn your leave. Take them on at any time by approaching the portal on your right.",
                Responses = new string[]
                {
                    "Could you remind me who you are again?",
                    "Thank you, I'll be on my way then...",
                    null,
                    null
                },
                FavorImpacts = new int[] { 2, 5, 0, 0 },
                NextResponses = new int[] { 4, 10, 0, 0 }
            },
            new DialogueInstance() // 7
            {
                MainText = "So be it. It is not my belief in these attributes of the Garden that make them true. I advise you take a look around for yourself; I'll be here when you are ready.",
                Responses = new string[]
                {
                    "Fine, I'll take a look around then...",
                    null,
                    null,
                    null
                },
                FavorImpacts = new int[] { 0, 0, 0, 0 },
                NextResponses = new int[] { 10, 0, 0, 0 }
            },
            new DialogueInstance() // 8
            {
                MainText = "Anger is only natural when you find yourself in an unfamiliar place, though it has no use here. Tell me, traveller, what's on your mind?",
                Responses = new string[]
                {
                    "What is this place?",
                    "Who are you?",
                    "How do I leave this 'Garden'?",
                    "Nothing. I'm going to take a look around..."
                },
                FavorImpacts = new int[] { 0, 0, 0, 0 },
                NextResponses = new int[] { 3, 4, 6, 10 }
            },
            new DialogueInstance() // 9
            {
                MainText = "My role here is only one small contribution to something much larger at work. I can guide you, traveller, but it will be you who must earn your leave out of these lands. Now, what's on your mind?",
                Responses = new string[]
                {
                    "What is this place? How did I get here?",
                    "What are you doing here?",
                    "How do I get out of here?",
                    null
                },
                FavorImpacts = new int[] { 0, 0, 0, 0 },
                NextResponses = new int[] { 3, 4, 6, 0 }
            },
            new DialogueInstance() // 10
            {
                MainText = "Of course. I will be here whenever you need my guidance...",
                Responses = new string[]
                {
                    "How did I get here, again?",
                    "What's special about this 'Garden'?",
                    "Tell me about yourself",
                    "How do I leave this place?"
                },
                FavorImpacts = new int[] { 0, 0, 2, 0 },
                NextResponses = new int[] { 3, 5, 4, 6 }
            },
            new DialogueInstance() // 11
            {
                MainText = "Welcome back, traveller. How can I help you?",
                Responses = new string[]
                {
                    "How did I get here, again?",
                    "What's special about this 'Garden'?",
                    "Tell me about yourself",
                    "How do I leave this place?"
                },
                FavorImpacts = new int[] { 0, 0, 2, 0 },
                NextResponses = new int[] { 3, 5, 4, 6 }
            },
            new DialogueInstance() // 12
            {
                MainText = "You have completed all three quests, and as such the Garden permits you to leave. Take care, traveller. May you visit again in the near future.",
                Responses = new string[]
                {
                    "",
                    "",
                    "",
                    ""
                },
                FavorImpacts = new int[] { 0, 0, 0, 0 },
                NextResponses = new int[] { 0, 0, 0, 0 }
            }
        };
        currentDialogueInstance = dialogueInstances[0];
        PopulateDialogueTexts(currentDialogueInstance.MainText, currentDialogueInstance.Responses);
    }

    void Update()
    {
        if (inDialogue)
        {
            // Don't show "press e to talk" (already in dialogue)
            startDialogueText.enabled = false;

            // Close dialogue with escape
            if (Input.GetKeyDown(KeyCode.Escape))
                CloseDialogue();
        }
        else
        {
            // Show "press e to talk" if player is close enough to knight
            float distanceFromKnight = (knightCenter.position - playerCenter.position).magnitude;
            if (distanceFromKnight <= talkRadius)
            {
                startDialogueText.enabled = true;

                // Start dialogue if player pressed e
                if (Input.GetKeyDown(KeyCode.E))
                    OpenDialogue();
            }
            else
                startDialogueText.enabled = false;
        }
    }

    private void OpenDialogue()
    {
        inDialogue = true;
        player.SetSuspended(true);
        dialogueContainer.SetActive(true);
    }

    private void PopulateDialogueTexts(string mainText, string[] responses)
    {
        this.mainText.text = mainText;
        for (int i = 0; i < responseSlots.Length; i++)
        {
            if (responses[i] == null)
            {
                responseSlots[i].text = string.Empty;
                responseSlots[i].enabled = false;
            }
            else
            {
                responseSlots[i].enabled = true;
                responseSlots[i].text = responses[i];
            }
        }
    }

    private void SelectResponse(int responseIndex)
    {
        favorSystem.AddFavor(currentDialogueInstance.FavorImpacts[responseIndex]);
        currentDialogueInstance = dialogueInstances[currentDialogueInstance.NextResponses[responseIndex]];
        PopulateDialogueTexts(currentDialogueInstance.MainText, currentDialogueInstance.Responses);
    }

    private void CloseDialogue()
    {
        dialogueContainer.SetActive(false);
        player.SetSuspended(false);
        inDialogue = false;

        currentDialogueInstance = dialogueInstances[11];
        PopulateDialogueTexts(currentDialogueInstance.MainText, currentDialogueInstance.Responses);
    }

    public void AllQuestsFinished()
    {
        currentDialogueInstance = dialogueInstances[12];
        PopulateDialogueTexts(currentDialogueInstance.MainText, currentDialogueInstance.Responses);
    }
}
