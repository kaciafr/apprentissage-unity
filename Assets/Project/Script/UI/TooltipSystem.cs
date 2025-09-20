using TMPro;
using UnityEngine;
using UnityEngine.UI;
[ExecuteInEditMode]
public class TooltipSystem : MonoBehaviour
{
    
    

    static public TooltipSystem instance;
    [SerializeField]
    private Tooltip tooltip;
    private void Awake()
    {
        instance = this;
    }

    public void Show(string content , string header = "")
    {
        if (tooltip != null)
        {
            tooltip.SetText(content, header);
            tooltip.gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if (tooltip != null)
        {
            tooltip.gameObject.SetActive(false);
        }
    }
}
