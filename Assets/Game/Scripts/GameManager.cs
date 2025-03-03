using UnityEngine;

public class GameManager : MonoBehaviour {
    InputManager inputManager;

    public GameObject pistol;
    public Animator animator;

    private string scopeAnimationBool = "ScopeActive";

    public float alertRadius = 20f;
    public LayerMask soldierLayer;
    GameObject player;

    private void Start() {
        inputManager = FindFirstObjectByType<InputManager>();
        player = GameObject.FindGameObjectWithTag("Player");
        pistol.SetActive(false);
        
        // Visualize all BoxColliders in the scene
        BoxCollider[] colliders = FindObjectsOfType<BoxCollider>();
        
        foreach (BoxCollider collider in colliders)
        {
            GameObject visualizer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualizer.transform.SetParent(collider.transform, false);
            visualizer.transform.localPosition = collider.center;
            visualizer.transform.localScale = collider.size;
            
            // Make the visualizer transparent and non-interactive
            visualizer.GetComponent<Collider>().enabled = false;
            Renderer renderer = visualizer.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Transparent/Diffuse"));
            mat.color = new Color(0, 1, 0, 0.3f);
            renderer.material = mat;
        }
    }

    private void Update() {
        // Switch between pistol and fist
        if (inputManager.switchWeaponInput == true) {
            pistol.SetActive(!pistol.activeSelf);
            inputManager.SwitchWeaponDone();
        }
        // Switch to scope animation when holding pistol and scope
        if (pistol.activeSelf == true && inputManager.scopeInput == true) {
            animator.SetBool(scopeAnimationBool, true);
        }
        else {
            animator.SetBool(scopeAnimationBool, false);
        }
        CheckAlertRadius();
    }

    // Enemies notify nearby soldiers in overlap sphere if found player
    void AlertNearbySoldiers() {
        Collider[] soldiers = Physics.OverlapSphere(player.transform.position, alertRadius, soldierLayer);
        foreach(Collider soldierCollider in soldiers) {
            Soldier soldier = soldierCollider.GetComponent<Soldier>();
            if (soldier != null) {
                soldier.AlertSoldier();
            }
        }
    }

    void CheckAlertRadius() {
        if (pistol.activeSelf) {
            AlertNearbySoldiers();
        }
    }

}
