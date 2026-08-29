using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuestWarning : MonoBehaviour
{
    public Text questNameText;
    public Text descriptionText;

    private StairsLayer parentStairs;
    private Coroutine _autoDismissCoroutine;

    public void Show(string questName, string questID, StairsLayer stairs)
    {
        parentStairs = stairs;

        if (questNameText != null)
            questNameText.text = $"Quest Required: {questName}";

        if (descriptionText != null)
            descriptionText.text = "Complete this quest before you can proceed.";

        // Stop any existing dismiss coroutine to prevent conflicts
        if (_autoDismissCoroutine != null)
            StopCoroutine(_autoDismissCoroutine);

        gameObject.SetActive(true);

        // Start auto-dismiss after 3 seconds
        _autoDismissCoroutine = StartCoroutine(AutoDismissAfterDelay(3f));
    }

    private IEnumerator AutoDismissAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Dismiss();
    }

    private void Dismiss()
    {
        gameObject.SetActive(false);
        parentStairs?.OnQuestWarningDismissed();
        _autoDismissCoroutine = null;
    }
}
