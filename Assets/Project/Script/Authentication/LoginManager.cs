using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System;
using Plateformer;


public class LoginManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button forgotPasswordButton;

    [SerializeField] private TextMeshProUGUI statusText;

    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject passwordResetPanel;



    [SerializeField] private int limitletterpassword = 8;

    [SerializeField] private Button togglepassword;

    [SerializeField] private Sprite openEye;

    [SerializeField] private Sprite closeEye;

    private Image tooglebuttonImage;

    [SerializeField] private PlayerController playerController;

    [SerializeField] private ItemTransformManager itemTransformManager;

    [SerializeField] private SimpleRealTimeSync simpleRealTimeSync;



    private bool isVisible = false;

    void Start()
    {
        loginButton.onClick.AddListener(() => _ = LoginUser());
        registerButton.onClick.AddListener(() => _ = RegisterUser());
        forgotPasswordButton.onClick.AddListener(OnForgotPasswordClick);

        if (PocketBaseClient.Instance.IsAuthenticated())
        {
            OnLoginSuccess();
        }
        else
        {
            ShowLoginPanel();
            
            
        }

        emailInput.onSubmit.AddListener(OnInputEmail);
        passwordInput.onSubmit.AddListener(OnInputPassword);

        tooglebuttonImage = togglepassword.GetComponent<Image>();

        passwordInput.contentType = TMP_InputField.ContentType.Password;


        if (tooglebuttonImage != null && closeEye != null)
        {
            tooglebuttonImage.sprite = closeEye;
        }
    }

    private async Task LoginUser()
    {
        if (string.IsNullOrEmpty(emailInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            ShowStatus("Veuillez remplir tous les champs pour vous connecter", Color.red);
            return;
        }

      

        ShowStatus("Connexion en cours...", Color.yellow);
        ButtonInteract(false);

        try
        {
            bool success = await PocketBaseClient.Instance.Authenfication(emailInput.text, passwordInput.text);

            if (success)
            {
                ShowStatus("Connexion réussie", Color.green);
                await Task.Delay(1000);
                OnLoginSuccess();
            }
            else
            {
                ShowStatus("Échec de la connexion", Color.red);
            }
        }
        catch (Exception e)
        {
            ShowStatus($"Erreur : {e.Message}", Color.red);
            Debug.LogError($"Erreur de connexion : {e}");
        }
        finally
        {
            ButtonInteract(true);
        }
    }

    private async Task RegisterUser()
    {
        if (string.IsNullOrEmpty(emailInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            ShowStatus("Veuillez remplir tous les champs", Color.red);
            return;
        }

    

        if (passwordInput.text.Length < limitletterpassword)
        {
            ShowStatus("Le mot de passe doit contenir au moins 8 caractères", Color.red);
            return;
        }

        ShowStatus("Inscription en cours...", Color.blue);
        ButtonInteract(false);

        try
        {
            var pb = PocketBaseClient.Instance.GetClient();
            var createData = new
            {
                email = emailInput.text,
                password = passwordInput.text, // CORRECTION ICI
                passwordConfirm = passwordInput.text
            };

            var users = await pb.Collection("users").Create<object>(createData);
            ShowStatus("Inscription réussie ! Connexion...", Color.green);

            await Task.Delay(1000);
            await LoginUser();
        }
        catch (Exception e)
        {
            ShowStatus($"Erreur inscription : {e.Message}", Color.red);
            Debug.LogError($"Erreur d'inscription : {e}");
        }
        finally
        {
            ButtonInteract(true);
        }
    }

    private async void OnLoginSuccess()
    {
        ShowStatus("Chargement de l'inventaire...", Color.green);
        loginPanel.SetActive(false);
        gamePanel.SetActive(true);

        if (playerController != null)
            playerController.canMove = true;

        if (simpleRealTimeSync != null)
        {
            try
            {
                simpleRealTimeSync.TestSync();
                ShowStatus("Inventaire chargé", Color.green);

                if (itemTransformManager != null)
                {
                    itemTransformManager.LoadAllObjectsInScene();
                    ShowStatus("Objets chargés", Color.green);
                }

                ShowStatus("Prêt à jouer !", Color.green);
                await Task.Delay(2000);
                ShowStatus("", Color.white);
            }
            catch (Exception e)
            {
                ShowStatus("Erreur de chargement", Color.red);
                Debug.LogError($"Erreur: {e}");
            }
        }
        else
        {
            if (itemTransformManager != null)
            {
                itemTransformManager.LoadAllObjectsInScene();
            }
            ShowStatus("", Color.white);
        }
    }

    private void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        gamePanel.SetActive(false);
        if (passwordResetPanel != null)
            passwordResetPanel.SetActive(false);

        if (playerController != null)
            playerController.canMove = false;
        ClearField();
    }

    public void OnForgotPasswordClick()
    {
        loginPanel.SetActive(false);
        if (passwordResetPanel != null)
            passwordResetPanel.SetActive(true);
    }

  

    private void ShowStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
    }

    private void ButtonInteract(bool interactable)
    {
        loginButton.interactable = interactable;
        registerButton.interactable = interactable;
    }

    private void ClearField()
    {
        emailInput.text = "";
        passwordInput.text = "";
        ShowStatus("", Color.white);
    }

    public void Logout()
    {
        var pb = PocketBaseClient.Instance.GetClient();
        pb.AuthStore.Clear();
        ShowLoginPanel();
        ShowStatus("Déconnecté avec succès", Color.green);
    }

    private void OnInputEmail(string value)
    {
        passwordInput.Select();
        passwordInput.ActivateInputField();
    }

    private void OnInputPassword(string value)
    {
        _ = LoginUser();
    }

    public  void TogglePassword()
    {
        isVisible = !isVisible;
        if (isVisible)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Standard;
            if (tooglebuttonImage != null && openEye != null)
                tooglebuttonImage.sprite = openEye;
        }
        else
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;

            if (tooglebuttonImage != null && closeEye != null)
                tooglebuttonImage.sprite = closeEye;
        }
        passwordInput.ForceLabelUpdate();
    }

    public void BackToLogin()
    {
        ShowLoginPanel();
    }
}
