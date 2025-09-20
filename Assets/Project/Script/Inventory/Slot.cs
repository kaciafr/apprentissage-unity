using TMPro;
using UnityEngine;
using UnityEngine.UI;      
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ItemData item;

    public Image itemVisual;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null && TooltipSystem.instance != null)
        {
            TooltipSystem.instance.Show(item.description, item.nom);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipSystem.instance != null)
        {
            TooltipSystem.instance.Hide();
        }
    }
}
