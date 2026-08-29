using UnityEngine;
using System.Collections;

public class MapTransition : MonoBehaviour
{
    public Transform teleportTargetPosition;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            FadeTransition(collision.gameObject);
        }
    }
    async void FadeTransition(GameObject player)
    {
        await ScreenFader.instance.FadeOut();
        UpdatePlayerPosition(player);
        await ScreenFader.instance.FadeIn();
    }
    void UpdatePlayerPosition(GameObject player)
    {
        player.transform.position = teleportTargetPosition.position;
        return;
    }
}
