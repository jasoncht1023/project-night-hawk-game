using UnityEngine;
using System.Collections.Generic;

public class MinimapMarker : MonoBehaviour {
    [Header("Marker Setup")]
    public Sprite playerSprite;     // Sprite for player markers
    public Sprite enemySprite;      // Sprite for enemy markers
    public string markerLayerName = "MinimapLayer";
    public string playerTag = "Player";

    private Camera minimapCamera;
    private Dictionary<GameObject, GameObject> objectToMarker = new Dictionary<GameObject, GameObject>();

    void Start() {
        // Find the minimap camera
        minimapCamera = GameObject.FindGameObjectWithTag("MinimapCamera")?.GetComponent<Camera>();
        if (minimapCamera == null) {
            Debug.LogError("Minimap camera with tag 'MinimapCamera' not found. Make sure your minimap camera is tagged.");
            return;
        }

        // Initial creation of markers
        CreatePlayerMarker();
        CreateEnemyMarkers();
    }

    void Update() {
        // Update marker positions for any tracked objects that still exist
        List<GameObject> objectsToRemove = new List<GameObject>();

        foreach (var kvp in objectToMarker) {
            if (kvp.Key == null) {
                // Object has been destroyed, mark for removal
                objectsToRemove.Add(kvp.Key);
                Destroy(kvp.Value); // Destroy the associated marker
            }
        }

        // Clean up destroyed objects from dictionary
        foreach (var obj in objectsToRemove) {
            objectToMarker.Remove(obj);
        }

        // Check for any new instances that might have been instantiated
        CreatePlayerMarker();
        CreateEnemyMarkers();
    }

    void CreatePlayerMarker() {
        // Find player using tag
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        // Skip if player already has a marker
        if (objectToMarker.ContainsKey(player)) return;

        // Create a marker for the player
        CreateMarkerForObject(player, playerSprite);
    }

    void CreateEnemyMarkers() {
        // Find all enemies using Soldier component
        Soldier[] soldiers = FindObjectsByType<Soldier>(FindObjectsSortMode.None);

        foreach (Soldier soldier in soldiers) {
            GameObject enemyObj = soldier.gameObject;

            // Skip if this is the player or already has a marker
            if (enemyObj.CompareTag(playerTag) || objectToMarker.ContainsKey(enemyObj)) continue;

            // Create a marker for this enemy
            CreateMarkerForObject(enemyObj, enemySprite);
        }
    }

    void CreateMarkerForObject(GameObject obj, Sprite markerSprite) {
        // Create a marker GameObject
        GameObject markerObject = new GameObject("MinimapMarker_" + obj.name);
        SpriteRenderer markerRenderer = markerObject.AddComponent<SpriteRenderer>();
        markerRenderer.sprite = markerSprite;

        // Set the marker's layer to the MinimapLayer
        int minimapLayer = LayerMask.NameToLayer(markerLayerName);
        if (minimapLayer == -1) {
            Debug.LogError("Layer '" + markerLayerName + "' not found. Make sure you created it in Project Settings.");
            Destroy(markerObject);
            return;
        }
        markerObject.layer = minimapLayer;

        // Set sorting order
        markerRenderer.sortingOrder = 1;

        // Parent the marker to the GameObject it represents
        markerObject.transform.parent = obj.transform;
        markerObject.transform.localPosition = new Vector3(0f, 30f, 0f);
        markerObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        // Add to tracking dictionary
        objectToMarker.Add(obj, markerObject);
    }
}