
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [SerializeField] public float pickupRange = 2.6f;

    public Inventory inventory;

    public PickupBehaviour playerPickupBehaviour;

   



    void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, pickupRange))
        {
            if (hit.transform.CompareTag("Item"))

            {
                

                if (Input.GetKeyDown(KeyCode.E))
                {
                    playerPickupBehaviour.DoPickup(hit.transform.gameObject.GetComponent<Item>());
                }
            }
        }


    }

   

}
