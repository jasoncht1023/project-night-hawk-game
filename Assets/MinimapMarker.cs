using UnityEngine;
using System.Collections.Generic;

public class MinimapMarker : MonoBehaviour {
    [Header("Marker Setup")]
    public Sprite playerSprite;
    public Sprite enemySprite;
    private string markerLayerName = "MinimapLayer";
    private string playerTag = "Player";

    private Camera minimapCamera;
    private Dictionary<GameObject, GameObject> objectToMarker = new Dictionary<GameObject, GameObject>();

    void Start() {
        minimapCamera = GameObject.FindGameObjectWithTag("MinimapCamera")?.GetComponent<Camera>();
        if (minimapCamera == null) {
            Debug.LogError("Minimap camera with tag 'MinimapCamera' not found. Make sure your minimap camera is tagged.");
            return;
        }

        CreatePlayerMarker();
        CreateEnemyMarkers();
    }

    void Update() {
        List<GameObject> objectsToRemove = new List<GameObject>();

        foreach (var kvp in objectToMarker) {
            if (kvp.Key == null) {
                objectsToRemove.Add(kvp.Key);
                Destroy(kvp.Value);
                continue;
            }

            Soldier soldier = kvp.Key.GetComponent<Soldier>();
            if (soldier != null && !soldier.enabled) {
                objectsToRemove.Add(kvp.Key);
                Destroy(kvp.Value);
                continue;
            }

            if (!kvp.Key.activeInHierarchy) {
                objectsToRemove.Add(kvp.Key);
                Destroy(kvp.Value);
            }
        }

        foreach (var obj in objectsToRemove) {
            objectToMarker.Remove(obj);
        }

        CreatePlayerMarker();
        CreateEnemyMarkers();
    }

    void CreatePlayerMarker() {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        if (objectToMarker.ContainsKey(player)) return;

        CreateMarkerForObject(player, playerSprite);
    }

    void CreateEnemyMarkers() {
        Soldier[] soldiers = FindObjectsByType<Soldier>(FindObjectsSortMode.None);

        foreach (Soldier soldier in soldiers) {
            GameObject enemyObj = soldier.gameObject;
            if (enemyObj.CompareTag(playerTag) || objectToMarker.ContainsKey(enemyObj)) continue;
            if (!soldier.enabled || !enemyObj.activeInHierarchy) continue;

            CreateMarkerForObject(enemyObj, enemySprite);
        }
    }

    void CreateMarkerForObject(GameObject obj, Sprite markerSprite) {
        GameObject markerObject = new GameObject("MinimapMarker_" + obj.name);
        SpriteRenderer markerRenderer = markerObject.AddComponent<SpriteRenderer>();
        markerRenderer.sprite = markerSprite;

        int minimapLayer = LayerMask.NameToLayer(markerLayerName);
        if (minimapLayer == -1) {
            Debug.LogError("Layer '" + markerLayerName + "' not found. Make sure you created it in Project Settings.");
            Destroy(markerObject);
            return;
        }
        markerObject.layer = minimapLayer;

        markerRenderer.sortingOrder = 1;

        markerObject.transform.parent = obj.transform;
        markerObject.transform.localPosition = new Vector3(0f, 100f, 0f);
        markerObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        objectToMarker.Add(obj, markerObject);
    }
}