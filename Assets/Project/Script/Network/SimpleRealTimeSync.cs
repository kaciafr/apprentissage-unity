using UnityEngine;
using System.Threading.Tasks;
using PocketBaseSdk;
using System.Linq;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Version simplifiée du système de synchronisation temps réel
/// Étape par étape pour éviter les erreurs de compilation
/// </summary>
public class SimpleRealTimeSync : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;

    [Header("Collections PocketBase")]
    private const string ITEMS_COLLECTION = "items";
    private const string INVENTORIES_COLLECTION = "inventories";

    [Header("Settings")]
    [SerializeField] private float checkInterval = 5f;
    [SerializeField] private bool enableAutoSync = true;

    private PocketBase pb;
    [Header("Server Settings")]
    [SerializeField] private string serverUrl = "https://kacia.fr"; // Votre serveur

    private void Start()
    {
        Debug.Log("🚀 SimpleRealTimeSync - Initialisation...");

        if (PocketBaseClient.Instance != null)
        {
            Debug.Log("✅ PocketBaseClient.Instance trouvé");
            pb = PocketBaseClient.Instance.GetClient();

            if (pb != null)
            {
                Debug.Log("✅ pb (PocketBase client) initialisé");

                if (PocketBaseClient.Instance.IsAuthenticated())
                {
                    Debug.Log("✅ Utilisateur authentifié");

                    if (enableAutoSync)
                    {
                        InvokeRepeating(nameof(CheckForUpdates), checkInterval, checkInterval);
                        Debug.Log($"✅ Synchronisation temps réel activée (intervalle: {checkInterval}s)");
                    }
                    else
                    {
                        Debug.Log("⚠️ Auto-sync désactivé");
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ Utilisateur non authentifié");
                }
            }
            else
            {
                Debug.LogError("❌ pb (PocketBase client) est null après GetClient()");
            }
        }
        else
        {
            Debug.LogError("❌ PocketBaseClient.Instance est null");
        }
    }

    /// <summary>
    /// Vérifie les mises à jour sur le serveur
    /// </summary>
    private async void CheckForUpdates()
    {
        if (!PocketBaseClient.Instance.IsAuthenticated())
            return;

        try
        {
            Debug.Log("🔄 Vérification des mises à jour...");
            await CheckInventoryChanges();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erreur sync: {e.Message}");
        }
    }

    /// <summary>
    /// Vérifie les changements d'inventaire
    /// </summary>
    private async Task CheckInventoryChanges()
    {
        string userId = PocketBaseClient.Instance.GetCurrentUserId();

        try
        {
            // Requête simple pour vérifier s'il y a des changements
            var response = await pb.Collection(INVENTORIES_COLLECTION)
                .GetList<SimpleInventoryData>(filter: $"user = '{userId}'");

            Debug.Log($"📦 Trouvé {response.Items.Count} items dans l'inventaire serveur");

            // Comparer avec l'inventaire local
            if (response.Items.Count != inventory.content.Count)
            {
                Debug.Log("🔄 Différence détectée, synchronisation...");
                await SyncInventoryFromServer();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erreur vérification inventaire: {e.Message}");
        }
    }

    /// <summary>
    /// Synchronise l'inventaire depuis le serveur
    /// </summary>
    private async Task SyncInventoryFromServer()
    {
        Debug.Log("📥 Synchronisation depuis le serveur...");

        // Vérifications de sécurité
        if (PocketBaseClient.Instance == null)
        {
            Debug.LogError("❌ PocketBaseClient.Instance est null !");
            return;
        }

        if (pb == null)
        {
            Debug.LogError("❌ pb (PocketBase) est null !");
            return;
        }

        string userId = PocketBaseClient.Instance.GetCurrentUserId();

        try
        {
            var response = await pb.Collection(INVENTORIES_COLLECTION)
                .GetList<SimpleInventoryData>(filter: $"user = '{userId}'");

            // Vider l'inventaire local
            inventory.content.Clear();

            // Charger tous les items disponibles (simple)
            var itemsResponse = await pb.Collection(ITEMS_COLLECTION)
                .GetList<SimpleItemData>();

            foreach (var serverSlot in response.Items)
            {
                // Trouver l'item correspondant
                var itemData = itemsResponse.Items.FirstOrDefault(item => item.id == serverSlot.item);

                if (itemData != null)
                {
                            // Créer un ItemData Unity simple
                    var unityItem = await CreateUnityItem(itemData);

                    // Ajouter selon la quantité
                    for (int i = 0; i < serverSlot.count; i++)
                    {
                        inventory.content.Add(unityItem);
                    }
                }
            }

            inventory.RefreshInventory();
            Debug.Log($"✅ Inventaire synchronisé: {inventory.content.Count} items");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erreur synchronisation: {e.Message}");
        }
    }

    /// <summary>
    /// Crée un ItemData Unity à partir des données PocketBase
    /// </summary>
    private async Task<ItemData> CreateUnityItem(SimpleItemData pbItem)
    {
        var itemData = ScriptableObject.CreateInstance<ItemData>();
        itemData.itemId = pbItem.id;

        // Protection contre les noms vides
        itemData.nom = string.IsNullOrEmpty(pbItem.nom) ? $"Item_{pbItem.id}" : pbItem.nom;
        itemData.description = string.IsNullOrEmpty(pbItem.description) ? "Aucune description" : pbItem.description;
        itemData.prix = pbItem.prix;

        // Debug.Log($"🔧 Création ItemData: nom='{itemData.nom}', desc='{itemData.description}', prix={itemData.prix}");

        // Charger l'image depuis PocketBase
        if (!string.IsNullOrEmpty(pbItem.icon))
        {
            itemData.visualObject = await LoadImageFromPocketBase(pbItem.id, pbItem.icon);

            if (itemData.visualObject != null)
                Debug.Log($"✅ Image chargée pour {itemData.nom}");
            else
                Debug.LogWarning($"❌ Échec chargement image pour {itemData.nom} - item sans sprite");
        }
        else
        {
            Debug.LogWarning($"⚠️ Pas d'icône définie pour {itemData.nom} - item sans sprite");
            itemData.visualObject = null; // Pas de sprite par défaut
        }

        return itemData;
    }


    /// <summary>
    /// Charge une image depuis PocketBase et la convertit en Sprite
    /// </summary>
    private async Task<Sprite> LoadImageFromPocketBase(string recordId, string fileName)
    {
        try
        {
            // URL complète du fichier PocketBase
            string imageUrl = $"{serverUrl}/api/files/{ITEMS_COLLECTION}/{recordId}/{fileName}";

            // Debug.Log($"🖼️ Chargement image: {fileName}");

            // Télécharger l'image
            using UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);
            var operation = www.SendWebRequest();

            // Attendre le téléchargement avec timeout
            float timeout = 10f;
            float timer = 0f;

            while (!operation.isDone && timer < timeout)
            {
                timer += Time.unscaledDeltaTime;
                await Task.Yield();
            }

            if (timer >= timeout)
            {
                Debug.LogError($"❌ Timeout lors du téléchargement de {fileName}");
                return null;
            }

            // Debug.Log($"🖼️ Résultat: {www.result}, Code: {www.responseCode}");

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Convertir en Sprite
                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

                if (texture != null)
                {
                    // S'assurer que la texture est lisible
                    if (!texture.isReadable)
                    {
                        var renderTexture = RenderTexture.GetTemporary(texture.width, texture.height);
                        Graphics.Blit(texture, renderTexture);
                        var readable = new Texture2D(texture.width, texture.height);
                        RenderTexture.active = renderTexture;
                        readable.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                        readable.Apply();
                        RenderTexture.active = null;
                        RenderTexture.ReleaseTemporary(renderTexture);
                        texture = readable;
                    }

                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    sprite.name = fileName;

                    return sprite;
                }
                else
                {
                    Debug.LogError($"❌ Texture null pour {fileName}");
                    return null;
                }
            }
            else
            {
                Debug.LogError($"❌ Erreur HTTP chargement image {fileName}:");
                Debug.LogError($"   URL: {imageUrl}");
                Debug.LogError($"   Code: {www.responseCode}");
                Debug.LogError($"   Résultat: {www.result}");
                Debug.LogError($"   Erreur: {www.error}");

                if (www.downloadHandler != null && !string.IsNullOrEmpty(www.downloadHandler.text))
                {
                    Debug.LogError($"   Réponse serveur: {www.downloadHandler.text}");
                }

                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Exception téléchargement image {fileName}: {e.Message}");
            Debug.LogError($"   Stack trace: {e.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Ajoute un item via PocketBase
    /// </summary>
    public async Task AddItemToServer(string itemId, int count = 1)
    {
        if (!PocketBaseClient.Instance.IsAuthenticated())
        {
            Debug.LogWarning("❌ Non authentifié");
            return;
        }

        try
        {
            string userId = PocketBaseClient.Instance.GetCurrentUserId();

            var data = new
            {
                user = userId,
                item = itemId,
                count,
                position = 0 // Position simple pour commencer
            };

            await pb.Collection(INVENTORIES_COLLECTION).Create<object>(data);
            Debug.Log($"✅ Item {itemId} x{count} ajouté au serveur");

            // Synchroniser immédiatement
            await SyncInventoryFromServer();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erreur ajout item: {e.Message}");
        }
    }

    // Méthodes de test
    [ContextMenu("🔄 Test Sync")]
    public async void TestSync()
    {
        await SyncInventoryFromServer();
    }

    [ContextMenu("📦 Test Add Item")]
    public async void TestAddItem()
    {
        // D'abord récupérer le premier item disponible
        try
        {
            var itemsResponse = await pb.Collection(ITEMS_COLLECTION).GetList<SimpleItemData>();

            if (itemsResponse.Items.Count > 0)
            {
                string firstItemId = itemsResponse.Items[0].id;
                Debug.Log($"🧪 Test avec le premier item trouvé: {firstItemId} ({itemsResponse.Items[0].nom})");
                await AddItemToServer(firstItemId, 1);
            }
            else
            {
                Debug.LogError("❌ Aucun item trouvé dans la collection 'items' ! Ajoutez d'abord des items dans PocketBase.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erreur récupération items: {e.Message}");
        }
    }

    [ContextMenu("🖼️ Test Image Download")]
    public async void TestImageDownload()
    {
        try
        {
            var itemsResponse = await pb.Collection(ITEMS_COLLECTION).GetList<SimpleItemData>();

            if (itemsResponse.Items.Count > 0)
            {
                var firstItem = itemsResponse.Items[0];
                Debug.Log($"🖼️ === TEST DOWNLOAD IMAGE ===");
                Debug.Log($"🖼️ Item ID: {firstItem.id}");
                Debug.Log($"🖼️ Item nom: '{firstItem.nom}'");
                Debug.Log($"🖼️ Item description: '{firstItem.description}'");
                Debug.Log($"🖼️ Item prix: {firstItem.prix}");
                Debug.Log($"🖼️ Nom fichier icon: '{firstItem.icon}'");

                if (string.IsNullOrEmpty(firstItem.icon))
                {
                    Debug.LogWarning("⚠️ Le champ 'icon' est vide pour cet item !");

                    // Tester avec tous les items pour voir s'il y en a un avec une icône
                    Debug.Log("🔍 Recherche d'items avec icônes...");
                    foreach (var item in itemsResponse.Items)
                    {
                        if (!string.IsNullOrEmpty(item.icon))
                        {
                            Debug.Log($"✅ Item trouvé avec icône: {item.id} - nom: '{item.nom}' - icon: '{item.icon}'");
                            firstItem = item;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(firstItem.icon))
                    {
                        Debug.LogError("❌ Aucun item avec icône trouvé!");
                        return;
                    }
                }

                string imageUrl = $"{serverUrl}/api/files/{ITEMS_COLLECTION}/{firstItem.id}/{firstItem.icon}";
                Debug.Log($"🖼️ URL complète: {imageUrl}");

                // Test de téléchargement
                var sprite = await LoadImageFromPocketBase(firstItem.id, firstItem.icon);

                if (sprite != null)
                {
                    Debug.Log($"✅ Image téléchargée avec succès !");
                    Debug.Log($"✅ Dimensions: {sprite.texture.width}x{sprite.texture.height}");
                    Debug.Log($"✅ Format: {sprite.texture.format}");

                    // Test d'ajout direct à l'inventaire
                    var testItem = ScriptableObject.CreateInstance<ItemData>();
                    testItem.nom = firstItem.nom;
                    testItem.description = firstItem.description;
                    testItem.prix = firstItem.prix;
                    testItem.visualObject = sprite;

                    Debug.Log($"🧪 Test ajout à l'inventaire...");
                    inventory.AddItem(testItem);
                }
                else
                {
                    Debug.LogError($"❌ Échec téléchargement image");
                }
            }
            else
            {
                Debug.LogError("❌ Aucun item trouvé pour tester");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erreur test image: {e.Message}");
        }
    }

    [ContextMenu("📋 Lister tous les items")]
    public async void ListAllItems()
    {
        Debug.Log("🔍 Début listage des items...");

        // Vérifications de sécurité
        if (PocketBaseClient.Instance == null)
        {
            Debug.LogError("❌ PocketBaseClient.Instance est null !");
            return;
        }

        if (pb == null)
        {
            Debug.LogError("❌ pb (PocketBase) est null ! Tentative de réinitialisation...");
            pb = PocketBaseClient.Instance.GetClient();

            if (pb == null)
            {
                Debug.LogError("❌ Impossible d'initialiser pb !");
                return;
            }
        }

        try
        {
            var itemsResponse = await pb.Collection(ITEMS_COLLECTION).GetList<SimpleItemData>();
            Debug.Log($"📋 {itemsResponse.Items.Count} items trouvés dans la collection:");

            for (int i = 0; i < itemsResponse.Items.Count; i++)
            {
                var item = itemsResponse.Items[i];
                Debug.Log($"  {i+1}. ID: {item.id} | Nom: '{item.nom}' | Desc: '{item.description}' | Prix: {item.prix} | Icon: '{item.icon}' | FBX: '{item.FBX}'");

                // Vérifier si l'item est valide
                if (string.IsNullOrEmpty(item.nom))
                    Debug.LogWarning($"    ⚠️ Item {item.id} a un nom vide!");
                if (string.IsNullOrEmpty(item.icon))
                    Debug.LogWarning($"    ⚠️ Item {item.id} n'a pas d'icône!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erreur listage items: {e.Message}");
        }
    }


    /// <summary>
    /// Corrige les items avec des données manquantes dans PocketBase
    /// </summary>
    [ContextMenu("🔧 Corriger Items PocketBase")]
    public async void FixPocketBaseItems()
    {
        if (!PocketBaseClient.Instance.IsAuthenticated())
        {
            Debug.LogError("❌ Non authentifié!");
            return;
        }

        try
        {
            var itemsResponse = await pb.Collection(ITEMS_COLLECTION).GetList<SimpleItemData>();
            Debug.Log($"🔧 Vérification de {itemsResponse.Items.Count} items...");

            for (int i = 0; i < itemsResponse.Items.Count; i++)
            {
                var item = itemsResponse.Items[i];
                bool needsUpdate = false;
                var updateData = new Dictionary<string, object>();

                // Corriger nom vide
                if (string.IsNullOrEmpty(item.nom))
                {
                    updateData["nom"] = $"Item_{i + 1}";
                    needsUpdate = true;
                    Debug.Log($"🔧 Correction nom pour {item.id}");
                }

                // Corriger description vide
                if (string.IsNullOrEmpty(item.description))
                {
                    updateData["description"] = $"Description de l'item {i + 1}";
                    needsUpdate = true;
                    Debug.Log($"🔧 Correction description pour {item.id}");
                }

                // Corriger prix 0
                if (item.prix <= 0)
                {
                    updateData["prix"] = 100; // Prix par défaut
                    needsUpdate = true;
                    Debug.Log($"🔧 Correction prix pour {item.id}");
                }

                // Appliquer les corrections
                if (needsUpdate)
                {
                    await pb.Collection(ITEMS_COLLECTION).Update<object>(item.id, updateData);
                    Debug.Log($"✅ Item {item.id} corrigé");
                }
            }

            Debug.Log("✅ Correction terminée! Relancez le test sync.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erreur correction: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        CancelInvoke();
    }
}

// Structures SimpleItemData et SimpleInventoryData sont définies dans RealTimeInventoryManager.cs