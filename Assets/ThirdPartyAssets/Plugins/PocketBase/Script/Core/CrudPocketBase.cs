using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using PocketBaseSdk;
using System.Linq;

//===== Classe de Données =====
[System.Serializable]
public class TestRecord
{
    public string id;
    public string user_id;
    public string name;
    public int value;
    public string created;
    public string updated;
}

[System.Serializable]
public class TestRecordListResponse
{
    public TestRecord[] items;
    public int page;
    public int perPage;
    public int totalItems;
    public int totalPages;
}

public class CrudPocketBase : MonoBehaviour
{
    [Header("Configuration Collection PocketBase")]
    [SerializeField] private string collectionName = "tests";

    [Header("🎮 Interface CRUD - CREATE")]
    [SerializeField] private string newRecordName = "";
    [SerializeField] private int newRecordValue = 0;

    [Header("🔍 Interface CRUD - READ/UPDATE/DELETE")]
    [SerializeField] private string targetRecordId = "";
    [SerializeField] private string updateRecordName = "";
    [SerializeField] private int updateRecordValue = 0;

    private PocketBase pb;

    // === CONSTANTES DEBUG ===
    private const string ERROR_NO_CLIENT = "❌ PocketBaseClient manquant";
    private const string ERROR_NOT_AUTH = "❌ Non authentifié";
    private const string SUCCESS_PREFIX = "✅";
    private const string ERROR_PREFIX = "❌";

    // === MÉTHODES UTILITAIRES ===
    private bool ValidateConnection()
    {
        if (PocketBaseClient.Instance == null)
        {
            Debug.LogError($"{ERROR_NO_CLIENT} - Créez un GameObject avec le script PocketBaseClient !");
            return false;
        }

        if (!PocketBaseClient.Instance.IsAuthenticated())
        {
            Debug.LogWarning($"{ERROR_NOT_AUTH} - Connectez-vous d'abord !");
            return false;
        }

        return true;
    }

    private void LogSuccess(string operation, string details = "") =>
        Debug.Log($"{SUCCESS_PREFIX} {operation} {details}");

    private void LogError(string operation, System.Exception e)
    {
        Debug.LogError($"{ERROR_PREFIX} {operation}: {e.Message}");
        Debug.LogError($"Type: {e.GetType().Name}");
        if (e.InnerException != null)
            Debug.LogError($"Inner: {e.InnerException.Message}");
    }

    private void Start()
    {
        if (PocketBaseClient.Instance == null)
        {
            Debug.LogError(ERROR_NO_CLIENT);
            return;
        }

        pb = PocketBaseClient.Instance.GetClient();
        TestConnection();
    }

    public async Task<TestRecord> CreateRecord(string name, int value)
    {
        if (!ValidateConnection()) return null;

        var data = new
        {
            user_id = PocketBaseClient.Instance.GetCurrentUserId(),
            name,
            value
        };

        try
        {
            var result = await pb.Collection(collectionName).Create<TestRecord>(data);
            LogSuccess("Création", $"ID: {result.id}, Name: {result.name}");
            return result;
        }
        catch (System.Exception e)
        {
            LogError("Création", e);
            return null;
        }
    }

    public async Task<List<TestRecord>> GetMyRecords()
    {
        if (!ValidateConnection()) return new List<TestRecord>();

        try
        {
            var userId = PocketBaseClient.Instance.GetCurrentUserId();

            Debug.Log($"🔍 Requête: Collection={collectionName}, User={userId}");
            Debug.Log($"🔍 Note: Règle PocketBase filtre automatiquement par user connecté");

            // Pas de filtre nécessaire car la règle PocketBase le fait automatiquement
            var response = await pb.Collection(collectionName).GetList<TestRecord>(
                sort: "-created"
            );

            LogSuccess("Lecture", $"{response.Items.Count} enregistrements");
            return response.Items.ToList();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Détails erreur - Collection: {collectionName}");
            Debug.LogError($"❌ User ID: {(PocketBaseClient.Instance != null ? PocketBaseClient.Instance.GetCurrentUserId() : "NULL")}");
            LogError("Lecture", e);
            return new List<TestRecord>();
        }
    }

    public async Task<TestRecord> GetRecordById(string recordId)
    {
        try
        {
            var record = await pb.Collection(collectionName).GetOne<TestRecord>(recordId);
            LogSuccess("Lecture ID", record.name);
            return record;
        }
        catch (System.Exception e)
        {
            LogError($"Lecture ID {recordId}", e);
            return null;
        }
    }

    public async Task<TestRecord> UpdateRecord(string recordId, string newName, int newValue)
    {
        try
        {
            var updateData = new { name = newName, value = newValue };
            var result = await pb.Collection(collectionName).Update<TestRecord>(recordId, updateData);
            LogSuccess("Mise à jour", result.name);
            return result;
        }
        catch (System.Exception e)
        {
            LogError("Mise à jour", e);
            return null;
        }
    }

    public async Task<bool> DeleteRecord(string recordId)
    {
        try
        {
            await pb.Collection(collectionName).Delete(recordId);
            LogSuccess("Suppression", $"ID={recordId}");
            return true;
        }
        catch (System.Exception e)
        {
            LogError("Suppression", e);
            return false;
        }
    }

    public async void TestConnection()
    {
        try
        {
            await pb.Health.Check();
            LogSuccess("Connexion serveur", "");
        }
        catch (System.Exception e)
        {
            LogError("Connexion serveur", e);
        }
    }

    // ===== MÉTHODES DE DEBUG =====
    [ContextMenu("🔧 Test Collection Sans Filtre")]
    public async void TestCollectionAccess()
    {
        if (!ValidateConnection()) return;

        try
        {
            Debug.Log($"🧪 Test collection '{collectionName}' sans filtre...");
            var response = await pb.Collection(collectionName).GetList<TestRecord>();
            LogSuccess("Collection", $"{response.Items.Count} enregistrements au total");

            // Afficher quelques exemples
            foreach (var item in response.Items.Take(3))
            {
                Debug.Log($"Exemple: ID={item.id}, user_id={item.user_id}, name={item.name}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ La collection '{collectionName}' est inaccessible !");
            LogError("Collection", e);
        }
    }

    [ContextMenu("🔧 Status Utilisateur")]
    public void CheckUserStatus()
    {
        Debug.Log($"🔍 Instance existe: {PocketBaseClient.Instance != null}");
        if (!ValidateConnection()) return;
        var userId = PocketBaseClient.Instance.GetCurrentUserId();
        Debug.Log($"🔍 Auth: ✅, ID: {userId}, Collection: {collectionName}");
    }

    // ===== MÉTHODES UI =====
    [ContextMenu("Créer un enregistrement")]
    public async void CreateNewRecordFromUI()
    {
        if (string.IsNullOrEmpty(newRecordName)) { Debug.LogWarning("nom manquant"); return; }
        var result = await CreateRecord(newRecordName, newRecordValue);
        if (result != null) { newRecordName = ""; newRecordValue = 0; }
    }

    [ContextMenu(" Lire tous les enregistrement")]
    public async void ReadAllRecords()
    {
        var records = await GetMyRecords();
        foreach (var r in records)
            Debug.Log($"ID: {r.id} | {r.name} | {r.value}");
    }

    [ContextMenu("🔍 Lire en fonction de l' ID")]
    public async void ReadSingleRecord()
    {
        if (string.IsNullOrEmpty(targetRecordId)) { Debug.LogWarning(" ID introuvable"); return; }
        var record = await GetRecordById(targetRecordId);
        if (record != null) { updateRecordName = record.name; updateRecordValue = record.value; }
    }

    [ContextMenu("Modification de l'enregistrement")]
    public async void UpdateTargetRecord()
    {
        if (string.IsNullOrEmpty(targetRecordId) || string.IsNullOrEmpty(updateRecordName))
        { Debug.LogWarning("ID introuvable "); return; }
        await UpdateRecord(targetRecordId, updateRecordName, updateRecordValue);
    }

    [ContextMenu("🗑️ Supprimer")]
    public async void DeleteTargetRecord()
    {
        if (string.IsNullOrEmpty(targetRecordId)) { Debug.LogWarning("❌ ID introuvable"); return; }
        if (await DeleteRecord(targetRecordId)) targetRecordId = "";
    }

    [ContextMenu("🧪 Test CRUD")]
    public async void FullCRUDTest()
    {
        var record = await CreateRecord("Test CRUD", 999);
        if (record == null) return;

        await GetMyRecords();
        await UpdateRecord(record.id, "Test Modifié", 1337);
        await DeleteRecord(record.id);

        LogSuccess("Test CRUD", "terminé");
    }
}