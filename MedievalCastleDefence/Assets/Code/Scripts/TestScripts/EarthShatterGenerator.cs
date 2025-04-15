using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EarthShatterGenerator : MonoBehaviour
{
    [Header("AYARLAR")]
    [Range(10, 180)] public float angle = 90f;
    [Min(1)] public float radius = 8f;
    public Material meshMaterial;

    private Mesh _mesh;

    void Start()
    {
        GenerateMesh();
    }

    [ContextMenu("MESH OLUÞTUR")]
    public void GenerateMesh()
    {
        _mesh = new Mesh();
        _mesh.name = "EarthShatter_Mesh";

        int segments = Mathf.Clamp(Mathf.RoundToInt(angle / 5), 12, 72);
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        float angleStep = angle / segments;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -angle / 2 + i * angleStep;
            Vector3 dir = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward;
            vertices[i + 1] = dir * radius;
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = _mesh;

        if (meshMaterial != null)
            GetComponent<MeshRenderer>().material = meshMaterial;
    }

#if UNITY_EDITOR
    [ContextMenu("TÜMÜNÜ KAYDET")]
    public void SaveAllAssets()
    {
        if (_mesh == null) GenerateMesh();

        string folderPath = "Assets/EarthShatter_Output";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "EarthShatter_Output");
        }

        // Mesh kaydet
        string meshPath = $"{folderPath}/{_mesh.name}.asset";
        Mesh savedMesh = Instantiate(_mesh);
        AssetDatabase.CreateAsset(savedMesh, meshPath);

        // Prefab oluþtur
        GameObject prefabObj = new GameObject(_mesh.name + "_Prefab");
        prefabObj.AddComponent<MeshFilter>().sharedMesh = savedMesh;
        prefabObj.AddComponent<MeshRenderer>().material = meshMaterial != null ? meshMaterial : GetComponent<MeshRenderer>().material;

        string prefabPath = $"{folderPath}/{_mesh.name}_Prefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(prefabObj, prefabPath);
        DestroyImmediate(prefabObj);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"<color=green>KAYIT TAMAM!</color>\nMesh: {meshPath}\nPrefab: {prefabPath}", this);
    }

    // Static method for menu item
    [MenuItem("Tools/EarthShatter/Save All %#s", false, 1)]
    private static void SaveAllMenu()
    {
        EarthShatterGenerator generator = FindObjectOfType<EarthShatterGenerator>();
        if (generator != null)
        {
            generator.SaveAllAssets();
        }
        else
        {
            Debug.LogWarning("EarthShatterGenerator bulunamadý!");
        }
    }

    [MenuItem("Tools/EarthShatter/Select Output Folder", false, 2)]
    public static void SelectOutputFolder()
    {
        string folderPath = "Assets/EarthShatter_Output";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "EarthShatter_Output");
        }
        EditorUtility.FocusProjectWindow();
        Object folder = AssetDatabase.LoadAssetAtPath(folderPath, typeof(Object));
        Selection.activeObject = folder;
    }
#endif
}