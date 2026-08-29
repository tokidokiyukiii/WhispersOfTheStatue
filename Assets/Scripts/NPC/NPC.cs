using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC: MonoBehaviour,IInteractable
{
    public NPCdialogue dialogue;
    private DialogueController dialogueUI;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    public string talkObjectiveQuestID;

    private NPCdialogue _currentDialogue;
    public NPCdialogue activeQuestDialogue;
    public NPCdialogue postQuestDialogue;

    [Header("Quest Settings")]
    public string npcID;
    public Quest[] offeredQuests;
    [HideInInspector] public Quest pendingQuestOnEnd;

    private QuestManager questManager;

    private void Start()
    {
        dialogueUI = DialogueController.Instance;
    }

    public bool CanInteract()
    { 
        return !isDialogueActive; 
    }
    public void Interact()
    {
        if (dialogue == null) return;
        if (isDialogueActive)
        {
            nextLine();
        }
        else
        {
            startDialogue();
        }
    }
    public void OfferQuest()
    {
        if (dialogue.offeredQuest != null && QuestManager.Instance != null)
        {
            QuestManager.Instance.StartQuest(dialogue.offeredQuest);
        }
    }
    public void startDialogue()
    {
        // Priority: Completed > Active > Default
        NPCdialogue useDialogue = dialogue;

        if (dialogue.offeredQuest != null && QuestManager.Instance != null)
        {
            string id = dialogue.offeredQuest.questID;
            if (QuestManager.Instance.IsQuestCompleted(id) && postQuestDialogue != null)
                useDialogue = postQuestDialogue;
            else if (QuestManager.Instance.IsQuestActive(id) && activeQuestDialogue != null)
                useDialogue = activeQuestDialogue;
        }

        _currentDialogue = useDialogue; // store selected dialogue

        isDialogueActive = true;
        dialogueIndex = 0;

        // Use _currentDialogue for NPC info
        dialogueUI.SetNPCInfo(_currentDialogue.npcName, _currentDialogue.npcPortrait);
        dialogueUI.ShowDialogueUI(true);
        PauseController.SetPause(true);

        dialogueUI.SetCurrentNPC(this);
        dialogueUI.SetSkipButtonActive(_currentDialogue.enableSkipButton);

        DisplayCurrentLine();
    }
    void TryOfferQuest()
    {
        if (dialogue.offeredQuest != null && QuestManager.Instance != null)
        {
            QuestManager.Instance.StartQuest(dialogue.offeredQuest);
            Debug.Log($"Quest accepted: {dialogue.offeredQuest.questName}");
        }
    }

    public void nextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogue.dialogueLines[dialogueIndex]);
            isTyping = false;
        }

        dialogueUI.ClearChoices();

        if (_currentDialogue.endDialogueLines.Length > dialogueIndex && _currentDialogue.endDialogueLines[dialogueIndex])
        {
            endDialogue();
            return;
        }

        if (_currentDialogue.questOfferDialogueIndex == dialogueIndex)
        {
            TryOfferQuest();
        }

        foreach (DialogueChoice dialogueChoice in _currentDialogue.choices)
        {
            if (dialogueChoice.dialogueIndex == dialogueIndex)
            {
                DisplayChoices(dialogueChoice);
                return;
            }
        }

        if (++dialogueIndex < _currentDialogue.dialogueLines.Length)
        {
            DisplayCurrentLine();
        }
        else
        {
            endDialogue();
        }
    }

    public IEnumerator typeLine()
    {
        isTyping = true;
        /*dialogueUI.SetDialogueText("");
        foreach (char letter in _currentDialogue.dialogueLines[dialogueIndex])
        {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text += letter);
            yield return new WaitForSeconds(_currentDialogue.typingSpeed);
        }
        isTyping = false;
        if (_currentDialogue.autoProgressLines.Length > dialogueIndex && _currentDialogue.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(_currentDialogue.autoProgressDelay);
            nextLine();
        }*/
        string fullText = _currentDialogue.dialogueLines[dialogueIndex];

        // 1. Set full text ONCE so TMP parses Rich Text tags
        dialogueUI.dialogueText.text = fullText;
        dialogueUI.dialogueText.ForceMeshUpdate(); // ← Important for TMP!
        dialogueUI.dialogueText.maxVisibleCharacters = 0; // Start hidden

        // 2. Type character by character
        for (int i = 0; i < fullText.Length; i++)
        {
            // Reveal tags instantly (no wait)
            if (fullText[i] == '<')
            {
                while (i < fullText.Length && fullText[i] != '>') i++;
                dialogueUI.dialogueText.maxVisibleCharacters = i + 1;
                continue;
            }

            // Reveal normal characters with delay
            dialogueUI.dialogueText.maxVisibleCharacters = i + 1;
            yield return new WaitForSeconds(_currentDialogue.typingSpeed);
        }

        isTyping = false;

        // Auto-progress if enabled
        if (_currentDialogue.autoProgressLines.Length > dialogueIndex &&
            _currentDialogue.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(_currentDialogue.autoProgressDelay);
            nextLine();
        }
    }

    void DisplayChoices(DialogueChoice choice)
    {
        for(int i = 0; i < choice.choices.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndexes[i];
            // Get the panel name for this specific choice
            string panelName = (choice.panelNamesToOpen.Length > i) ? choice.panelNamesToOpen[i] : "";

            // Pass the panel name to the callback
            dialogueUI.CreateChoiceButton(choice.choices[i], () => ChooseOption(nextIndex, panelName));
        }
    }
    void ChooseOption(int nextIndex, string panelName)
    {
        dialogueUI.ClearChoices();

        // Find which choice was clicked by matching the callback
        foreach (var choice in _currentDialogue.choices)
        {
            if (choice.dialogueIndex == dialogueIndex)
            {
                // Find which index in choices[] triggered this
                for (int i = 0; i < choice.choices.Length; i++)
                {
                    // Simple match: if this choice's nextIndex matches, it was clicked
                    if (choice.nextDialogueIndexes[i] == nextIndex &&
                        i == choice.questAcceptChoiceIndex &&
                        _currentDialogue.offeredQuest != null)
                    {
                        //QuestManager.Instance?.StartQuest(_currentDialogue.offeredQuest);
                        QuestManager.Instance.StartQuest(
                        _currentDialogue.offeredQuest,
                        updateUI: true,
                        showPopup: false  // ← Suppress immediate popup
                    );

                        // Set pending quest so popup shows AFTER dialogue ends
                        pendingQuestOnEnd = _currentDialogue.offeredQuest;
                        Debug.Log($"Quest accepted: {_currentDialogue.offeredQuest.questName}");
                        break;
                    }
                }
                break;
            }
        }

        // If a panel is specified, open it and end dialogue
        if (!string.IsNullOrEmpty(panelName))
        {
            PanelsUI.Instance.OpenPanel(panelName);
            endDialogue();
            PauseController.SetPause(true);
        }
        else
        {
            // Otherwise, continue the dialogue flow
            dialogueIndex = nextIndex;
            DisplayCurrentLine();
        }
    }

    void DisplayCurrentLine()
    {
        StopAllCoroutines();
        StartCoroutine(typeLine());
    }

    public void endDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueUI.SetDialogueText("");
        dialogueUI.ShowDialogueUI(false);
        PauseController.SetPause(false);

        if (!string.IsNullOrEmpty(talkObjectiveQuestID) && QuestManager.Instance != null)
        {
            QuestManager.Instance.OnTalkedToNPC(talkObjectiveQuestID);
        }

        dialogueUI.SetSkipButtonActive(false);

        if (pendingQuestOnEnd != null && QuestManager.Instance != null)
        {
            var questToStart = pendingQuestOnEnd;
            pendingQuestOnEnd = null; // Clear immediately to avoid re-trigger

            StartCoroutine(StartQuestWithDelay(questToStart));
        }
    }

    public void SkipDialogue()
    {
        if (!isDialogueActive) return;
        endDialogue();
    }
    private IEnumerator StartQuestWithDelay(Quest quest)
    {
        // Wait for dialogue panel to fully hide
        yield return new WaitForSecondsRealtime(0.2f);

        // Start quest WITHOUT showing popup yet
        QuestManager.Instance.StartQuest(quest, updateUI: true, showPopup: false);

        // Small extra delay so popup feels like a "reward" after dialogue
        yield return new WaitForSecondsRealtime(0.1f);

        // Now show the popup manually
        QuestManager.Instance.ShowQuestAcceptedPopup(quest);
    }
}

