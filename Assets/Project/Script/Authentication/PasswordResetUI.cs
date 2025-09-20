using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class PasswordResetUI : MonoBehaviour
{
    [Header("Email Request Panel")]
    public GameObject emailRequestPanel;
    public TMP_InputField emailInputField;
    public Button sendResetEmailButton;
    public TextMeshProUGUI emailStatusText;


    [Header("Navigation")]
    public Button backToLoginButton;

    [Header("References")]
    public LoginManager loginManager;

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        
        ShowEmailRequestPanel();

        
        sendResetEmailButton.onClick.AddListener(() => OnSendResetEmail());
        backToLoginButton.onClick.AddListener(() => OnBackToLogin());

        
        ClearStatusTexts();
    }

    private void ShowEmailRequestPanel()
    {
        emailRequestPanel.SetActive(true);
    }


    private void ClearStatusTexts()
    {
        emailStatusText.text = "";
    }

    public async void OnSendResetEmail()
    {
        
        if (string.IsNullOrEmpty(emailInputField.text) || !IsValidEmail(emailInputField.text))
        {
            emailStatusText.text = "Veuillez entrer une adresse email valide";
            emailStatusText.color = Color.red;
            return;
        }

        
        sendResetEmailButton.interactable = false;
        emailStatusText.text = "Envoi en cours...";
        emailStatusText.color = Color.yellow;

        try
        {
        
            bool success = await PocketBaseClient.Instance.RequestPasswordReset(emailInputField.text);

            if (success)
            {
                emailStatusText.text = "Email de réinitialisation envoyé ! Vérifiez votre boîte mail.";
                emailStatusText.color = Color.green;

                
            }
            else
            {
                emailStatusText.text = "Erreur lors de l'envoi. Vérifiez votre adresse email.";
                emailStatusText.color = Color.red;
            }
        }
        finally
        {
            sendResetEmailButton.interactable = true;
        }
    }


    public void OnBackToLogin()
    {
        if (loginManager != null)
        {
            loginManager.BackToLogin();
        }
        else
        {
            Debug.LogError("LoginManager reference manquante !");
        }
    }

    private bool IsValidEmail(string email)
    {
        return email.Contains("@") && email.Contains(".");
    }
}