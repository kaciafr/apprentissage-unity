using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;


public class Tooltip : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI header;

    [SerializeField]
    private TextMeshProUGUI content;

    [SerializeField]
    private LayoutElement layoutElement;

    [SerializeField]
    private int maxCharacter;

    [SerializeField]
    RectTransform rectTransform;

    public void SetText(string contentText, string headerText = "")
    {
        if (string.IsNullOrEmpty(headerText))
        {
            header.gameObject.SetActive(false);
        }
        else
        {
            header.gameObject.SetActive(true);
            header.text = headerText;
        }

        content.text = contentText;

        int headerLength = header.text.Length;
        int contentLength = content.text.Length;

        layoutElement.enabled = (headerLength > maxCharacter || contentLength > maxCharacter);
    }

    private void Update()
    {
        Vector2 mousePosition = Input.mousePosition;
        float pivotX = mousePosition.x / Screen.width;
        float pivotY = mousePosition.y / Screen.height;

        rectTransform.pivot = new Vector2(pivotX, pivotY);
        transform.position = mousePosition;
    }
    
}
