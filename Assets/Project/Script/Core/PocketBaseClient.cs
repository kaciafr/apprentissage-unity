using UnityEngine;
  using PocketBaseSdk;
  using System.Threading.Tasks;

  public class PocketBaseClient : MonoBehaviour
  {
      [SerializeField] private string serverUrl = "http://localhost:8090";
      private PocketBase pb;

      public static PocketBaseClient Instance { get; private set; }

      private void Awake()
      {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePocketBase();
            Debug.Log("✅ PocketBaseClient initialisé et conservé entre les scènes");
        }
        else
        {
            Debug.LogWarning("⚠️ PocketBaseClient dupliqué détruit - gardez un seul GameObject PocketBaseClient !");
            Destroy(gameObject);
        }
      }

      private void InitializePocketBase()
      {
          pb = new PocketBase(serverUrl);
          Debug.Log($"PocketBase client initialized with URL: {serverUrl}");
      }

      public PocketBase GetClient() => pb;

      public async Task<bool> Authenfication(string email, string password)
      {
          try
          {
              var authData = await pb.Collection("users").AuthWithPassword(email, password);
              Debug.Log("User authenticated successfully!");
              return true;
          }
          catch (System.Exception e)
          {
              Debug.LogError($"Authentification failed : {e.Message}");
              return false;
          }
      }

    public async Task<bool> RequestPasswordReset(string email)
    {
        try
        {
            await pb.Collection("users").RequestPasswordReset(email);
            Debug.Log($"la requte de changement de mot de vous a ete envoyer au : {email}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"La requete a echoué: {e.Message}");
            return false; 
        }

    }

    public async Task<bool> ConfirmPasswordReset(string token, string password, string passwordConfirm)
    {
        try
        {
            await pb.Collection("users").ConfirmPasswordReset(token, password, passwordConfirm);
            Debug.Log("le mot de passe à été modifier avec succés ");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"La confirmation du nouveau mot de passe à echouer {e.Message}");
            return false; 
        }
    }
      public bool IsAuthenticated() => pb.AuthStore.IsValid();

      public string GetCurrentUserId() => pb.AuthStore.Model?.Id ?? "";
  }