using UnityEngine;
using System.Collections.Generic;

public class PanelsUI : MonoBehaviour
{
    public static PanelsUI Instance;

    [System.Serializable]
    public class PanelReference
    {
        public string panelName;
        public GameObject panelObject;
    }

    public List<PanelReference> panels = new List<PanelReference>();
    public QuestManager questManager;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OpenPanel(string panelName)
    {
        foreach (var p in panels)
        {
            if (p.panelName == panelName && p.panelObject != null)
            {
                // Set mode based on which panel is opening
                if (panelName == "Quests") // Match your exact panel name in PanelsUI
                    QuestManager.Instance.RefreshUI(claimMode: true); // Guild mode

                p.panelObject.SetActive(true);
                return;
            }
        }
        Debug.LogWarning($"Panel '{panelName}' not found.");
    }

    public void ClosePanel(string panelName)
    {
        foreach (var p in panels)
        {
            if (p.panelName == panelName && p.panelObject != null)
            {
                p.panelObject.SetActive(false);
                PauseController.SetPause(false);
                return;
            }
        }
    }
}
