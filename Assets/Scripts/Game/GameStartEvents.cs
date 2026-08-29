using UnityEngine;
using System.Collections;

public class GameStartEvents : MonoBehaviour
{
    [Header("Auto-Start on Game Load")]
    [Tooltip("Drag your Quest ScriptableObject here")]
    public Quest autoStartQuest;

    [Tooltip("Drag the NPC that should start talking here")]
    public NPC autoStartNPC;

    [Tooltip("Delay before dialogue starts (lets UI initialize)")]
    public float dialogueDelay = 0.5f;

    private void Start()
    {
        StartCoroutine(InitializeOnStart());
    }

    private IEnumerator InitializeOnStart()
    {
        // Auto-start dialogue ONLY (quest will start when dialogue ends)
        if (autoStartNPC != null)
        {
            // Set the quest to start AFTER dialogue ends
            if (autoStartQuest != null)
            {
                autoStartNPC.pendingQuestOnEnd = autoStartQuest;
            }

            yield return new WaitForSeconds(dialogueDelay);
            autoStartNPC.startDialogue();
            Debug.Log($"[GameStart] Dialogue started with: {autoStartNPC.name}");
        }
    }
}
