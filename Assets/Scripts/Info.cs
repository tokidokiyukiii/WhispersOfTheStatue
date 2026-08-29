using UnityEngine;

public class Info : MonoBehaviour
{
    public GameObject infoPanel;

    public void Show()
    {
        infoPanel.SetActive(true);
    }
    public void Hide()
    {
        infoPanel.SetActive(false);
    }
}
