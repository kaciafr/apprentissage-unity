using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using PocketBaseSdk;
using System.Linq;


[System.Serializable]
public class SimpleItemData
{
    public string id;
    public string nom;
    public string description;
    public float prix;
    public string icon;
    public string FBX;
}

/// <summary>
/// Structure simplifiée pour l'inventaire
/// </summary>
[System.Serializable]
public class SimpleInventoryData
{
    public string id;
    public string user;
    public string item;
    public int count;
    public int position;
}

public class RealTimeInventoryManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;
    

    [Header("Collections PocketBase")]
    private const string ITEMS_COLLECTION = "items";
    private const string INVENTORIES_COLLECTION = "inventories";

    [Header("Sync Settings")]
    [SerializeField] private float syncInterval = 3f;
    [SerializeField] private bool enableRealTimeSync = true;
    [SerializeField] private bool debugMode = true;

    private PocketBase pb;
    private Dictionary<string, ItemData> cachedItems = new Dictionary<string, ItemData>();

    public static RealTimeInventoryManager Instance { get; private set; }

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

    private void Start()
    {
        if (PocketBaseClient.Instance == null)
        {
            LogDebug("PocketBaseClient non trouvé ");
            return;
        }

        pb = PocketBaseClient.Instance.GetClient();

        if (PocketBaseClient.Instance.IsAuthenticated())
        {
            _ = InitializeRealTimeSync();
        }
    }

    private async Task InitializeRealTimeSync()
    {
        LogDebug(" syncronisation en temps réel...");

        await LoadAllItemsFromServer();


        await LoadInventoryFromServer();

        
        if (enableRealTimeSync)
        {
            InvokeRepeating(nameof(CheckForUpdates), syncInterval, syncInterval);
            LogDebug($"Syncronisation temps réel activée (intervalle: {syncInterval}s)");
        }
    }

   
    /// Charge tous les items de la collection "items"
   
    public async Task LoadAllItemsFromServer()
    {
        try
        {
            LogDebug("Chargement des items depuis le serveur");

            var response = await pb.Collection(ITEMS_COLLECTION).GetList<SimpleItemData>();

            cachedItems.Clear();
            foreach (var item in response.Items)
            {
                
                var itemData = ScriptableObject.CreateInstance<ItemData>();
                itemData.itemId = item.id;
                itemData.nom = item.nom;
                itemData.description = item.description;
                itemData.prix = item.prix;
                cachedItems[item.id] = itemData;
            }

            LogDebug($" {cachedItems.Count} items chargés et mis en cache");
        }
        catch (System.Exception e)
        {
            LogDebug($"Erreur chargement items: {e.Message}");
        }
    }


    /// Charge l'inventaire depuis la collection "inventories"

    public async Task LoadInventoryFromServer()
    {
        try
        {
            LogDebug(" Chargement inventaire depuis le serveur...");

            string userId = PocketBaseClient.Instance.GetCurrentUserId();
            var response = await pb.Collection(INVENTORIES_COLLECTION)
                .GetList<SimpleInventoryData>(filter: $"user = '{userId}'", sort: "position");

            inventory.content.Clear();

            foreach (var slot in response.Items)
            {
                if (cachedItems.TryGetValue(slot.item, out ItemData itemData))
                {
                    // Ajouter à l'inventaire Unity (répéter selon count)
                    for (int i = 0; i < slot.count; i++)
                    {
                        inventory.content.Add(itemData);
                    }
                }
            }

            inventory.RefreshInventory();
            LogDebug($" Inventaire chargé: {inventory.content.Count} items totaux");
        }
        catch (System.Exception e)
        {
            LogDebug($" Erreur chargement inventaire: {e.Message}");
        }
    }

    
    /// Vérifie les mises à jour sur le serveur
    
    private async void CheckForUpdates()
    {
        if (!PocketBaseClient.Instance.IsAuthenticated())
            return;

        try
        {
            // Vérifier les nouveaux items
            await CheckForNewItems();

            // Vérifier les changements d'inventaire
            await CheckForInventoryChanges();
        }
        catch (System.Exception e)
        {
            LogDebug($" Erreur vérification updates: {e.Message}");
        }
    }

 
    /// Vérifie s'il y a de nouveaux items dans la collection
    private async Task CheckForNewItems()
    {
        var response = await pb.Collection(ITEMS_COLLECTION).GetList<SimpleItemData>();

        foreach (var item in response.Items)
        {
            if (!cachedItems.ContainsKey(item.id))
            {
                LogDebug($"🆕 Nouvel item détecté: {item.nom}");
                var itemData = ScriptableObject.CreateInstance<ItemData>();
                itemData.itemId = item.id;
                itemData.nom = item.nom;
                itemData.description = item.description;
                itemData.prix = item.prix;
                cachedItems[item.id] = itemData;
            }
        }
    }


    /// Vérifie les changements d'inventaire sur le serveur
    private async Task CheckForInventoryChanges()
    {
        string userId = PocketBaseClient.Instance.GetCurrentUserId();
        var response = await pb.Collection(INVENTORIES_COLLECTION)
            .GetList<SimpleInventoryData>(filter: $"user = '{userId}'", sort: "position");

        // Comparer avec l'état actuel
        bool hasChanges = false;

        // Comparer simple : si le nombre d'items est différent
        if (response.Items.Count != inventory.content.Count)
        {
            hasChanges = true;
            LogDebug($"Changement détecté: nombre d'items différent ({response.Items.Count} vs {inventory.content.Count})");
        }

        if (hasChanges)
        {
            LogDebug(" Synchronisation de l'inventaire...");
            await LoadInventoryFromServer();
        }
    }

    /// Ajoute un item à l'inventaire via PocketBase

    public async Task AddItemToInventory(string itemId, int count = 1, int position = -1)
    {
        try
        {
            string userId = PocketBaseClient.Instance.GetCurrentUserId();

            // Si position non spécifiée, trouver la prochaine position libre
            if (position == -1)
            {
                position = FindNextFreePosition();
            }

            var data = new
            {
                user = userId,
                item = itemId,
                count = count,
                position = position
            };

            await pb.Collection(INVENTORIES_COLLECTION).Create<object>(data);
            LogDebug($"Item ajouté: {itemId} x{count} à la position {position}");

            // Synchroniser immédiatement
            await LoadInventoryFromServer();
        }
        catch (System.Exception e)
        {
            LogDebug($" Erreur ajout item: {e.Message}");
        }
    }


    /// Trouve la prochaine position libre dans l'inventaire

    private int FindNextFreePosition()
    {
        // Version simple : retourne la prochaine position disponible
        return inventory.content.Count;
    }


    private void LogDebug(string message)
    {
        if (debugMode)
            Debug.Log($"[RealTimeInventory] {message}");
    }

    private void OnDestroy()
    {
        CancelInvoke();
    }

    // Méthodes publiques pour tester
    [ContextMenu(" Force Sync")]
    public async void ForceSyncFromServerMenu()
    {
        await ForceSyncFromServer();
    }

    public async Task ForceSyncFromServer()
    {
        await LoadAllItemsFromServer();
        await LoadInventoryFromServer();
    }

    [ContextMenu("Test Add Item")]
    public async void TestAddItem()
    {
        // Ajouter le premier item disponible pour test
        if (cachedItems.Count > 0)
        {
            string firstItemId = cachedItems.Keys.First();
            await AddItemToInventory(firstItemId, 1);
        }
    }
}