using UnityEngine;

[CreateAssetMenu(fileName ="NewNPCDialogue",menuName ="NPC Dialogue")]
public class NPCdialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public string[] dialogueLines;
    public float typingSpeed = 0.05f;
    public bool[] autoProgressLines;
    public bool[] endDialogueLines;
    public float autoProgressDelay = 1.5f;
    public DialogueChoice[] choices;
    public Quest offeredQuest;
    public int questOfferDialogueIndex = -1;
    public bool enableSkipButton = false;
}

[System.Serializable]
public class DialogueChoice
{
    public int dialogueIndex;
    public string[] choices;
    public int[] nextDialogueIndexes;
    public string[] panelNamesToOpen;
    public int questAcceptChoiceIndex = -1;
}