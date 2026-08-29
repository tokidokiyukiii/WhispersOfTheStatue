using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portrait;
    public Transform choiceContainer;
    public GameObject choiceButtonPrefab;

    public GameObject skipButton;
    public NPC currentNPC;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowDialogueUI(bool show)
    {
        dialoguePanel.SetActive(show);
    }
    public void SetNPCInfo(string npcName, Sprite portraits)
    {
        nameText.text = npcName;
        portrait.sprite = portraits;   
    }
    public void SetDialogueText(string text)
    {
        dialogueText.text = text;
    }
    public void ClearChoices()
    {
        foreach (Transform child in choiceContainer) Destroy(child.gameObject);
    }
    public void CreateChoiceButton(string choiceText, UnityEngine.Events.UnityAction onClick)
    {
        GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
        choiceButton.GetComponentInChildren<TMP_Text>().text = choiceText;
        choiceButton.GetComponent<Button>().onClick.AddListener(onClick);
    }
    public void SetCurrentNPC(NPC npc) => currentNPC = npc;

    public void SetSkipButtonActive(bool isActive)
    {
        if (skipButton != null) skipButton.SetActive(isActive);
    }

    // Call this from your UI Button's OnClick event
    public void OnSkipButtonClicked()
    {
        if (currentNPC != null)
        {
            currentNPC.SkipDialogue();
        }
    }
}
