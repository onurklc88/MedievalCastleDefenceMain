using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VMeshGenerator : MonoBehaviour
{
    [Header("V Shape Settings")]
    [Range(30, 170)] public float angle = 90f;
    [Min(1f)] public float length = 8f;
    [Min(0.1f)] public float depth = 5f;
    [Min(0.05f)] public float thickness = 0.3f;
    public Material meshMaterial;

    private Mesh _mesh;

    void Start()
    {
        GenerateVMesh();
    }

    [ContextMenu("Generate V Mesh")]
    public void GenerateVMesh()
    {
        _mesh = new Mesh();
        _mesh.name = "V_Mesh_" + System.Guid.NewGuid().ToString().Substring(0, 8);

        // Vertex Calculation (8 points)
        Vector3[] vertices = new Vector3[8];
        Vector3 leftDir = Quaternion.Euler(0, -angle / 2, 0) * Vector3.forward;
        Vector3 rightDir = Quaternion.Euler(0, angle / 2, 0) * Vector3.forward;

        // Bottom Points
        vertices[0] = Vector3.zero;
        vertices[1] = leftDir * (length - thickness);
        vertices[2] = rightDir * (length - thickness);
        vertices[3] = leftDir * length;
        vertices[4] = rightDir * length;

        // Top Points
        vertices[5] = vertices[0] + Vector3.up * depth;
        vertices[6] = vertices[3] + Vector3.up * depth;
        vertices[7] = vertices[4] + Vector3.up * depth;

        // Triangle Formation (24 indices = 12 triangles)
        int[] triangles = {
            // Left Leg (outer)
            0, 3, 5,
            3, 6, 5,
            // Right Leg (outer)
            0, 5, 4,
            5, 7, 4,
            // Left Leg (inner)
            0, 1, 5,
            1, 6, 5,
            // Right Leg (inner)
            0, 5, 2,
            5, 7, 2
        };

        // UV Coordinates
        Vector2[] uvs = new Vector2[8];
        for (int i = 0; i < vertices.Length; i++)
        {
            // Project vertices to 2D plane for UVs
            uvs[i] = new Vector2(
                vertices[i].x / (length * 2f) + 0.5f,
                vertices[i].z / (length * 2f) + 0.5f
            );
        }

        // Calculate Tangents (for normal mapping)
        Vector4[] tangents = new Vector4[8];
        for (int i = 0; i < vertices.Length; i++)
        {
            tangents[i] = new Vector4(1f, 0f, 0f, -1f);
        }

        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.uv = uvs;
        _mesh.tangents = tangents;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = _mesh;

        // Apply material if exists
        if (meshMaterial != null)
            GetComponent<MeshRenderer>().material = meshMaterial;
    }

#if UNITY_EDITOR
    [ContextMenu("Save All Assets")]
    public void SaveAllAssets()
    {
        if (_mesh == null)
        {
            Debug.LogWarning("No mesh to save! Generating first...");
            GenerateVMesh();
        }

        string folderPath = "Assets/VMesh_Assets";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "VMesh_Assets");
        }

        // 1. Save Mesh
        string meshPath = $"{folderPath}/{_mesh.name}.asset";
        Mesh meshToSave = Instantiate(_mesh);
        AssetDatabase.CreateAsset(meshToSave, meshPath);

        // 2. Create and Save Prefab
        GameObject prefabObj = new GameObject(_mesh.name + "_Prefab");
        MeshFilter mf = prefabObj.AddComponent<MeshFilter>();
        mf.sharedMesh = meshToSave;

        MeshRenderer mr = prefabObj.AddComponent<MeshRenderer>();
        mr.sharedMaterial = meshMaterial != null ? meshMaterial : new Material(Shader.Find("Standard"));

        string prefabPath = $"{folderPath}/{_mesh.name}_Prefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(prefabObj, prefabPath);
        DestroyImmediate(prefabObj);

        // 3. Finalize
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(prefabPath);

        Debug.Log($"<color=green>V Mesh saved successfully!</color>\n" +
                 $"Mesh: {meshPath}\n" +
                 $"Prefab: {prefabPath}");
    }

    [MenuItem("Tools/VMesh/Save All Assets %#v", priority = 1)]
    private static void SaveAllAssetsMenu()
    {
        VMeshGenerator generator = FindObjectOfType<VMeshGenerator>();
        if (generator != null)
        {
            generator.SaveAllAssets();
        }
        else
        {
            Debug.LogWarning("No VMeshGenerator found in scene!");
        }
    }

    [MenuItem("Tools/VMesh/Select Output Folder", priority = 2)]
    private static void SelectOutputFolder()
    {
        string folderPath = "Assets/VMesh_Assets";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "VMesh_Assets");
        }
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath(folderPath, typeof(Object));
    }
#endif
}