using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("References")]
    public RectTransform playerMarker;
    public Transform player;
    public GameObject mapPanel;
    public Transform mapAnchor;
    public AudioManager audioManager;

    [Header("Settings")]
    public float worldToUIScale = 8.5f;
    public Vector2 mapSizeWorldUnits = new Vector2(100, 85);

    private bool isOpen;

    private void Start()
    {
        isOpen = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            OpenMap();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isOpen)
        {
            CloseMap();
        }

        if (isOpen && playerMarker != null && player != null && mapAnchor != null)
        {
            UpdatePlayerMarker();
        }
    }

    public void OpenMap()
    {
        isOpen = true;
        mapPanel.SetActive(true);
        PauseController.SetPause(true);
        UpdatePlayerMarker();
        audioManager.PlayMapSound();
    }

    public void CloseMap()
    {
        isOpen = false;
        mapPanel.SetActive(false);
        PauseController.SetPause(false);
        audioManager.PlayMapSound();
    }

    private void UpdatePlayerMarker()
    {
        // Get player position relative to map anchor
        Vector3 worldPos = player.position - mapAnchor.position;

        // Convert to UI space & clamp to map bounds
        Vector2 uiPos = new Vector2(
            Mathf.Clamp(worldPos.x, 0, mapSizeWorldUnits.x),
            Mathf.Clamp(worldPos.y, 0, mapSizeWorldUnits.y) + 10
        ) * worldToUIScale;

        playerMarker.anchoredPosition = uiPos;
    }
}
