using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    public GameObject interactIcon;

    private void Start()
    {
        interactIcon.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            interactableInRange?.Interact();
            if(!interactableInRange.CanInteract())
            {
                interactIcon.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out IInteractable interactable)&&interactable.CanInteract())
        {
            interactableInRange = interactable;
            interactIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out IInteractable interactable)&&interactable==interactableInRange)
        {
            interactableInRange = null;
            interactIcon.SetActive(false);
        }
    }
}
