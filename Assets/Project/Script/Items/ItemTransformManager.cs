  using System.Collections.Generic;
  using System.Threading.Tasks;
  using PocketBaseSdk;
  using UnityEngine;
  using UnityEngine.Networking;
  using System.Linq;
  using System.Runtime.CompilerServices;
using System;

public class ItemTransformManager : MonoBehaviour
  {
      private const string ITEMS_COLLECTION = "items";
      private const string TRANSFORM_COLLECTION = "transforms";
      private const string DOWNLOAD_FOLDER = "Assets/Downloaded/";

      [Header("Test")]
      [SerializeField] private string testItemId = "";

      [Header("Synchronisation en temps réel")]
      [SerializeField] private bool enableRealTimeSync = false; // Désactivé temporairement
      [SerializeField] private float syncInterval = 5f; // Interval plus long

      private PocketBase pb;
      private readonly Dictionary<string, GameObject> loadedObjects = new();
      private readonly Dictionary<string, string> lastUpdateTimes = new();

      private void Start()
      {
          if (PocketBaseClient.Instance != null && PocketBaseClient.Instance.IsAuthenticated())
          {
              pb = PocketBaseClient.Instance.GetClient();
              if (enableRealTimeSync)
              {
                  Debug.Log("🔄 Démarrage sync ItemTransformManager");
                  InvokeRepeating(nameof(SyncTransforms), syncInterval, syncInterval);
              }
              else
              {
                  Debug.Log("ℹ️ Sync temps réel désactivée pour ItemTransformManager");
              }
          }
          else
          {
              Debug.LogWarning("⚠️ PocketBaseClient non disponible ou non authentifié - ItemTransformManager désactivé");
          }
      }

      public async Task<GameObject> LoadAndInstantiateObject(string itemId)
      {
          try
          {
              var itemData = await pb.Collection(ITEMS_COLLECTION).GetOne<SimpleItemData>(itemId);
              var transformData = await GetTransformData(itemId);
              if (transformData == null) return null;

              var fbxPath = await DownloadFBX(itemData);
              var prefab = LoadFBX(fbxPath);

              return InstantiateWithTransform(prefab, transformData, itemData.nom);
          }
          catch (Exception e)
          {
              Debug.LogError($"❌ Erreur LoadAndInstantiateObject: {e.Message}");
              return null;
          }
      }

      private async Task<TransformData> GetTransformData(string itemId)
      {
          var response = await pb.Collection(TRANSFORM_COLLECTION).GetList<TransformData>(
              filter: $"item = '{itemId}'", perPage: 1);
          return response.Items.FirstOrDefault();
      }

      private async Task<string> DownloadFBX(SimpleItemData itemData)
      {
          string fileUrl = $"https://kacia.fr/api/files/{ITEMS_COLLECTION}/{itemData.id}/{itemData.FBX}";

          using var request = UnityWebRequest.Get(fileUrl);
          if (pb.AuthStore.Token != null)
              request.SetRequestHeader("Authorization", $"Bearer {pb.AuthStore.Token}");

          await request.SendWebRequest();

          if (request.result != UnityWebRequest.Result.Success)
              throw new Exception($"Download failed: {request.error}");

          if (!System.IO.Directory.Exists(DOWNLOAD_FOLDER))
              System.IO.Directory.CreateDirectory(DOWNLOAD_FOLDER);

          string localPath = System.IO.Path.Combine(DOWNLOAD_FOLDER, itemData.FBX);
          System.IO.File.WriteAllBytes(localPath, request.downloadHandler.data);

          #if UNITY_EDITOR
          UnityEditor.AssetDatabase.Refresh();
          #endif

          return localPath;
      }

      private GameObject LoadFBX(string fbxPath)
      {
          #if UNITY_EDITOR
          var fbxAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
          if (fbxAsset != null) return fbxAsset;
          throw new Exception($"Impossible de charger le FBX: {fbxPath}");
          #else
          throw new NotImplementedException("Runtime FBX loading not implemented");
          #endif
      }

      private GameObject InstantiateWithTransform(GameObject prefab, TransformData data, string name)
      {
          var position = new Vector3(data.px, data.py, data.pz);
          var rotation = Quaternion.Euler(data.rx, data.ry, data.rz);
          var scale = new Vector3(data.sx, data.sy, data.sz);

          var obj = Instantiate(prefab, position, rotation);
          obj.transform.localScale = scale;
          obj.name = $"{name}_PB_{data.id}";

          // Enregistrer l'objet pour la synchronisation
          loadedObjects[data.id] = obj;
          lastUpdateTimes[data.id] = data.updated;

          return obj;
      }

      [ContextMenu("Test Simple")]
      public async void TestSimple()
      {
          try
          {
              var items = await pb.Collection(ITEMS_COLLECTION).GetList<SimpleItemData>(perPage: 5);
              var transforms = await pb.Collection(TRANSFORM_COLLECTION).GetList<TransformData>(perPage: 5);
              Debug.Log($"{items.Items.Count} items, {transforms.Items.Count} transforms");
          }
          catch (Exception e)
          {
              Debug.LogError($"Erreur: {e.Message}");
          }
      }

      [ContextMenu("Test avec ID")]
      public async void TestLoad()
      {
          if (string.IsNullOrEmpty(testItemId)) return;
          await LoadAndInstantiateObject(testItemId);
      }

      [ContextMenu("Charger objets")]
      public async void LoadAllObjectsInScene()
      {
          try
          {
              var transformsResponse = await pb.Collection(TRANSFORM_COLLECTION).GetList<TransformData>(perPage: 100);
              if (transformsResponse.Items.Count == 0) return;

              var itemsResponse = await pb.Collection(ITEMS_COLLECTION).GetList<SimpleItemData>(perPage: 100);

              int successCount = 0;
              int failCount = 0;

              foreach (var transform in transformsResponse.Items)
              {
                  try
                  {
                      var matchingItem = itemsResponse.Items.FirstOrDefault(item => item.id == transform.item);
                      if (matchingItem == null)
                      {
                          failCount++;
                          continue;
                      }

                      var fbxObj = await LoadAndInstantiateObject(transform.item);
                      if (fbxObj != null)
                      {
                         
                      }
                      else
                      {
                          failCount++;
                          continue;
                      }
                      successCount++;
                  }
                  catch
                  {
                      failCount++;
                  }
              }

              Debug.Log($"{successCount} objets chargés");
          }
          catch (Exception e)
          {
              Debug.LogError($"Erreur chargement: {e.Message}");
          }
      }

      [ContextMenu("Debug transforms")]
      public async void DebugListAllTransforms()
      {
          try
          {
              var transforms = await pb.Collection(TRANSFORM_COLLECTION).GetList<TransformData>(perPage: 100);
              foreach (var t in transforms.Items)
              {
                  Debug.Log($"{t.nom}: pos({t.px},{t.py},{t.pz}) rot({t.rx},{t.ry},{t.rz}) scale({t.sx},{t.sy},{t.sz})");
              }
          }
          catch (Exception e)
          {
              Debug.LogError($"Erreur debug: {e.Message}");
          }
      }

      [ContextMenu("Nettoyer")]
      public void ClearScene()
      {
          var objectsWithPB = GameObject.FindGameObjectsWithTag("Untagged")
              .Where(go => go.name.Contains("_PB_"))
              .ToArray();

          foreach (var obj in objectsWithPB)
          {
              if (Application.isPlaying)
                  Destroy(obj);
              else
                  DestroyImmediate(obj);
          }
      }

      private async void SyncTransforms()
      {
          if (pb == null || loadedObjects.Count == 0) return;

          try
          {
              // Récupérer tous les transforms
              var response = await pb.Collection(TRANSFORM_COLLECTION).GetList<TransformData>(perPage: 100);

              foreach (var transform in response.Items)
              {
                  // Vérifier si l'objet existe et s'il a été modifié
                  if (loadedObjects.ContainsKey(transform.id) && loadedObjects[transform.id] != null)
                  {
                      string lastUpdate = lastUpdateTimes.ContainsKey(transform.id) ? lastUpdateTimes[transform.id] : "";

                      if (transform.updated != lastUpdate)
                      {
                          UpdateObjectTransform(loadedObjects[transform.id], transform);
                          lastUpdateTimes[transform.id] = transform.updated;
                      }
                  }
              }
          }
          catch (Exception e)
          {
              Debug.LogWarning($"⚠️ Sync ItemTransform échouée: {e.Message}");
              // Désactiver la sync si elle échoue trop souvent
              if (e.Message.Contains("Client") || e.Message.Contains("Authentication"))
              {
                  Debug.LogWarning("🛑 Désactivation de la sync auto à cause d'erreurs répétées");
                  CancelInvoke(nameof(SyncTransforms));
                  enableRealTimeSync = false;
              }
          }
      }

      private void UpdateObjectTransform(GameObject obj, TransformData data)
      {
          if (obj == null) return;

          var newPosition = new Vector3(data.px, data.py, data.pz);
          var newRotation = Quaternion.Euler(data.rx, data.ry, data.rz);
          var newScale = new Vector3(data.sx, data.sy, data.sz);

          obj.transform.SetPositionAndRotation(newPosition, newRotation);
          obj.transform.localScale = newScale;

      }

      [ContextMenu("Sync")]
      public void ManualSync()
      {
          SyncTransforms();
      }

      [ContextMenu("Stop")]
      public void StopRealTimeSync()
      {
          CancelInvoke(nameof(SyncTransforms));
      }

      [ContextMenu("Restart")]
      public void RestartRealTimeSync()
      {
          StopRealTimeSync();
          if (enableRealTimeSync)
          {
              InvokeRepeating(nameof(SyncTransforms), syncInterval, syncInterval);
          }
      }
  }

  public static class UnityWebRequestExtensions
  {
      public static TaskAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
      {
          var tcs = new TaskCompletionSource<object>();
          asyncOp.completed += _ => tcs.SetResult(null);
          return ((Task)tcs.Task).GetAwaiter();
      }
  }