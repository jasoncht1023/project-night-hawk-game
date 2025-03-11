using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DetectionSensor : MonoBehaviour {
    public float distance = 40f;
    public float angle = 30f;
    public float height = 7f;
    public GameObject eyePosition;
    public Color meshColor = Color.red;
    public int scanFrequnecy = 60;
    public LayerMask layers;
    public LayerMask occlusionLayers;
    public List<GameObject> Objects {
        get {
            objects.RemoveAll(obj => !obj);
            return objects;
        }
    }
    private List<GameObject> objects = new List<GameObject>();

    Collider[] colliders = new Collider[50];

    Mesh mesh;
    int count;
    float scanInterval;
    float scanTimer;

    void Start() {
        scanInterval = 1.0f / scanFrequnecy;
    }

    void Update() {
        scanTimer -= Time.deltaTime;
        if (scanTimer < 0) {
            scanTimer += scanInterval;
            Scan();
        }
    }

    private void Scan() {
        count = Physics.OverlapSphereNonAlloc(eyePosition.transform.position, distance, colliders, layers, QueryTriggerInteraction.Collide);
        
        objects.Clear();
        for (int i = 0; i < count; i++) {
            GameObject obj = colliders[i].gameObject;
            if (IsInSight(obj)) {
                objects.Add(obj);
            }
        }
    }

    public bool IsInSight(GameObject obj) {
        Vector3 origin = eyePosition.transform.position;

        CapsuleCollider capsule = obj.GetComponent<CapsuleCollider>();
        if (capsule == null) {
            // Check transform if no active capsule is found
            return CheckPointVisibility(origin, obj.transform.position);
        }

        Vector3 capsuleCenter = obj.transform.TransformPoint(capsule.center);
        float capsuleHeight = capsule.height;

        Vector3 bottomPoint = capsuleCenter - Vector3.up * (capsuleHeight / 2f);
        Vector3 topPoint = capsuleCenter + Vector3.up * (capsuleHeight / 2f);

        // "In sight" if any point is visible
        return CheckPointVisibility(origin, bottomPoint) || CheckPointVisibility(origin, capsuleCenter) || CheckPointVisibility(origin, topPoint);
    }

    private bool CheckPointVisibility(Vector3 origin, Vector3 targetPoint) {
        Vector3 direction = targetPoint - origin;

        // Vertical check
        if (direction.y < -height / 2f || direction.y > height / 2f) {
            return false;
        }

        // Horizontal angle check
        Vector3 horizontalDirection = direction;
        horizontalDirection.y = 0;
        float deltaAngle = Vector3.Angle(horizontalDirection, eyePosition.transform.forward);
        if (deltaAngle > angle) {
            return false;
        }

        // Detect if any obstacles blocking the vision
        Vector3 adjustedOrigin = origin;
        if (Physics.Linecast(adjustedOrigin, targetPoint, occlusionLayers)) {
            return false;
        }

        return true;
    }

    private void OnValidate() {
        mesh = CreateWedgeMesh();
        scanInterval = 1.0f / scanFrequnecy;
    }

    // For debug only
    private void OnDrawGizmos() {
        if (mesh) {
            Gizmos.color = meshColor;
            Gizmos.DrawMesh(mesh, eyePosition.transform.position, eyePosition.transform.rotation);
        }

        Gizmos.DrawWireSphere(eyePosition.transform.position, distance);
        for (int i = 0; i < count; i++) {
            Gizmos.DrawSphere(colliders[i].transform.position, 0.2f);
        }

        Gizmos.color = Color.green;
        foreach (var obj in objects) {
            Gizmos.DrawSphere(obj.transform.position, 0.5f);
            Gizmos.DrawLine(eyePosition.transform.position, obj.transform.position);
        }
    }

    Mesh CreateWedgeMesh() {
        Mesh mesh = new Mesh();

        int segments = 10;
        int numTriangle = (segments * 4) + 2 + 2;   // 4 triangles for each segment, 2 for each left and right side
        int numVertices = numTriangle * 3;

        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numVertices];

        Vector3 bottomCenter = Vector3.down * height / 2f;
        Vector3 bottomLeft = Quaternion.Euler(0, -angle, 0) * Vector3.forward * distance + Vector3.down * height / 2f;
        Vector3 bottomRight = Quaternion.Euler(0, angle, 0) * Vector3.forward * distance + Vector3.down * height / 2f;

        Vector3 topCenter = bottomCenter + Vector3.up * height;
        Vector3 topLeft = bottomLeft + Vector3.up * height;
        Vector3 topRight = bottomRight + Vector3.up * height;

        int vert = 0;

        // Left side
        vertices[vert++] = bottomCenter;
        vertices[vert++] = bottomLeft;
        vertices[vert++] = topLeft;

        vertices[vert++] = topLeft;
        vertices[vert++] = topCenter;
        vertices[vert++] = bottomCenter;

        // Right side
        vertices[vert++] = bottomCenter;
        vertices[vert++] = topCenter;
        vertices[vert++] = topRight;

        vertices[vert++] = topRight;
        vertices[vert++] = bottomRight;
        vertices[vert++] = bottomCenter;

        // Divide the triangular sensor to segments
        float currentAngle = -angle;
        float deltaAngle = (angle * 2) / segments;
        for (int i = 0; i < segments; i++) {
            bottomLeft = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * distance + Vector3.down * height / 2f;
            bottomRight = Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * distance + Vector3.down * height / 2f;

            topLeft = bottomLeft + Vector3.up * height;
            topRight = bottomRight + Vector3.up * height;

            // Far side
            vertices[vert++] = bottomLeft;
            vertices[vert++] = bottomRight;
            vertices[vert++] = topRight;

            vertices[vert++] = topRight;
            vertices[vert++] = topLeft;
            vertices[vert++] = bottomLeft;

            // Top
            vertices[vert++] = topCenter;
            vertices[vert++] = topLeft;
            vertices[vert++] = topRight;

            // Bottom
            vertices[vert++] = bottomCenter;
            vertices[vert++] = bottomRight;
            vertices[vert++] = bottomLeft;

            currentAngle += deltaAngle;
        }

        for (int i = 0; i < numVertices; i++) {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    public List<GameObject> Filter(string layerName, int size) {
        int layer = LayerMask.NameToLayer(layerName);
        List<GameObject> objectList = new List<GameObject>();

        foreach (var obj in Objects) {
            if (obj.layer == layer) {
                objectList.Add(obj);
            }

            if (objectList.Count == size) {
                break;
            }
        }

        return objectList;
    }
}
