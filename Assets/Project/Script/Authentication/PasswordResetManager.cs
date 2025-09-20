using UnityEngine;
using System.Threading.Tasks;
using System;

public class PasswordResetManager : MonoBehaviour
{
    
    public static event Action<string> OnResetEmailSent;
    public static event Action<bool> OnPasswordResetCompleted;
    public static event Action<string> OnResetError;

    public static PasswordResetManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task<bool> SendPasswordResetEmail(string email)
    {
        try
        {
            
            if (!IsValidEmailFormat(email))
            {
                OnResetError?.Invoke("Format d'email invalide");
                return false;
            }

            Debug.Log($"Envoi de la demande de réinitialisation pour : {email}");

           
            bool success = await PocketBaseClient.Instance.RequestPasswordReset(email);

            if (success)
            {
                OnResetEmailSent?.Invoke(email);
                Debug.Log("Email de réinitialisation envoyé avec succès");

              
                PlayerPrefs.SetString("reset_email", email);
                PlayerPrefs.Save();
            }
            else
            {
                OnResetError?.Invoke("Impossible d'envoyer l'email de réinitialisation");
            }

            return success;
        }
        catch (Exception e)
        {
            Debug.LogError($"Erreur lors de l'envoi de l'email de réinitialisation : {e.Message}");
            OnResetError?.Invoke($"Erreur : {e.Message}");
            return false;
        }
    }

    public async Task<bool> ConfirmPasswordReset(string token, string newPassword, string confirmPassword)
    {
        try
        {
            // Validations côté client
            if (string.IsNullOrEmpty(token))
            {
                OnResetError?.Invoke("Token de réinitialisation requis");
                return false;
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                OnResetError?.Invoke("Le mot de passe doit contenir au moins 6 caractères");
                return false;
            }

            if (newPassword != confirmPassword)
            {
                OnResetError?.Invoke("Les mots de passe ne correspondent pas");
                return false;
            }

            Debug.Log("Confirmation de la réinitialisation du mot de passe...");

          
            bool success = await PocketBaseClient.Instance.ConfirmPasswordReset(token, newPassword, confirmPassword);

            if (success)
            {
                Debug.Log("Mot de passe réinitialisé avec succès");

                
                PlayerPrefs.DeleteKey("reset_email");
                PlayerPrefs.Save();

                OnPasswordResetCompleted?.Invoke(true);
            }
            else
            {
                OnResetError?.Invoke("Échec de la réinitialisation du mot de passe");
                OnPasswordResetCompleted?.Invoke(false);
            }

            return success;
        }
        catch (Exception e)
        {
            Debug.LogError($"Erreur lors de la confirmation de réinitialisation : {e.Message}");
            OnResetError?.Invoke($"Erreur : {e.Message}");
            OnPasswordResetCompleted?.Invoke(false);
            return false;
        }
    }

    public string GetStoredResetEmail()
    {
        return PlayerPrefs.GetString("reset_email", "");
    }

    public void ClearStoredResetEmail()
    {
        PlayerPrefs.DeleteKey("reset_email");
        PlayerPrefs.Save();
    }

    private bool IsValidEmailFormat(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public bool IsResetInProgress()
    {
        return !string.IsNullOrEmpty(GetStoredResetEmail());
    }
}