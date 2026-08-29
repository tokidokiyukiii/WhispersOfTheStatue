using UnityEngine;

public class QuestLog : MonoBehaviour
{
    public GameObject questpanel;
    public void OpenQuestLog()
    {
        QuestManager.Instance.RefreshUI(claimMode: false);
        questpanel.SetActive(true);
    }
}
