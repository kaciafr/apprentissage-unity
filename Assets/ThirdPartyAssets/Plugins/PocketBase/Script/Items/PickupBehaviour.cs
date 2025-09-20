using Plateformer;
using Unity.VisualScripting;
using UnityEngine;

public class PickupBehaviour : MonoBehaviour
{
    [SerializeField] private Inventory inventory;

    [SerializeField] private Animator playerAnimator;

    [SerializeField] private PlayerController playerMove;

    private Item currentItem;

    public void DoPickup(Item item)
    {
        if (inventory.IsFull())
        {
            Debug.Log("Inventory full , can't pick up :" + item.name);
            return;
        }
        currentItem = item;

        playerAnimator.SetTrigger("Pickup");
        playerMove.canMove = false;

    }

    public void AddItemToInventory()
    {
        inventory.AddItem(currentItem.itemData);
        Destroy(currentItem.gameObject);
        currentItem = null;
    }

    public void ReEnablePlayerMovement()
    {
        playerMove.canMove = true; 
    } 
}
