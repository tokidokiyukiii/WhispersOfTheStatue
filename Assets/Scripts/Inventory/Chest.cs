using UnityEngine;

public class Chest : MonoBehaviour,IInteractable
{
    public bool IsOpened { get; private set; }
    public string ID { get; private set; }
    public GameObject itemPrefab;
    public Sprite openedSprite;
    public AudioManager audioManager;

    private void Start()
    {
        ID ??= GenerateUniqueID(gameObject);
    }

    public static string GenerateUniqueID(GameObject obj)
    {
        return $"{obj.scene.name}_{obj.transform.position.x}_{obj.transform.position.y}";
    }

    public bool CanInteract()
    {
        return !IsOpened;
    }

    public void Interact()
    {
        if(!CanInteract()) return;
        OpenChest();
    }

    private void OpenChest()
    {
        SetOpen(true);
        if(itemPrefab)
        {
            GameObject droppedItem = Instantiate(itemPrefab,transform.position+Vector3.down,Quaternion.identity);
        }
        audioManager.PlayChestSound();
    }

    public void SetOpen(bool open)
    {
        if(IsOpened = open)
        {
            GetComponent<SpriteRenderer>().sprite = openedSprite;
        }
    }
}
