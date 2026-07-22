using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class StageGenerator : MonoBehaviour
{
    [Header("Category Roots")]
    public Transform brickWallRoot;
    public Transform metalWallRoot;

    public Transform concreteFloorRoot;
    public Transform glassFloorRoot;

    public Transform itemSpawnRoot;
    public Transform enemySpawnRoot;

    public Transform movingWallRoot;
    public Transform breakableWallRoot;

    public Transform fakeWallRoot;

    public Transform lightRoot;


    [Header("Maze Image")]
    public Texture2D mazeImage;


    [Header("Maze Parent")]
    public Transform mazeParent;


    [Header("Color Database")]
    public List<ColorDatabase> colorDatabase = new();


    [Header("Floor Prefabs")]
    public GameObject concreteFloorPrefab;
    public GameObject glassFloorPrefab;


    private const int GridSize = 60;


    public void Generate()
    {

        Color32[] pixels = mazeImage.GetPixels32();
        Debug.Log($"Width = {mazeImage.width}");
        Debug.Log($"Height = {mazeImage.height}");
        Debug.Log($"Path = {AssetDatabase.GetAssetPath(mazeImage)}");
        Debug.Log(pixels[0]);
        Debug.Log(pixels[1]);
        Debug.Log(pixels[2]);
        Debug.Log("Generate開始");
    Debug.Log(mazeImage.name);
    Debug.Log($"{mazeImage.width} x {mazeImage.height}");
        Debug.Log(mazeImage.GetPixel(10, 10));
        if (mazeParent == null)
        {
            Debug.LogWarning("Maze Parent が設定されていません。");
            return;
        }

        if (mazeImage == null)
        {
            Debug.LogWarning("Maze Image が設定されていません。");
            return;
        }

        // Root取得（無ければ生成）
        brickWallRoot = FindOrCreateRoot("BrickWall");
        metalWallRoot = FindOrCreateRoot("MetalWall");

        concreteFloorRoot = FindOrCreateRoot("ConcreteFloor");
        glassFloorRoot = FindOrCreateRoot("GlassFloor");

        itemSpawnRoot = FindOrCreateRoot("ItemSpawn");
        enemySpawnRoot = FindOrCreateRoot("EnemySpawn");

        movingWallRoot = FindOrCreateRoot("MovingWall");
        breakableWallRoot = FindOrCreateRoot("BreakableWall");

        fakeWallRoot = FindOrCreateRoot("FakeWall");

        lightRoot = FindOrCreateRoot("Light");


        // 既存オブジェクト削除
        ClearMaze();


        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                Color32 color = mazeImage.GetPixel(x, y);

                Debug.Log($"({x},{y}) RGB = {color.r * 255f:F1}, {color.g * 255f:F1}, {color.b * 255f:F1}");

                Vector3 position = new Vector3(
                    x,
                    0f,
                    -y
                );
                // 白 → コンクリート床
                if (IsSameColor(color, new Color32(255, 255, 255, 255)))
                {
                    SpawnFloor(
                        concreteFloorPrefab,
                        concreteFloorRoot,
                        position,
                        x,
                        y
                    );

                    continue;
                }


                // 水色 → ガラス床
                if (IsSameColor(color, new Color32(0, 255, 255, 255)))
                {
                    SpawnFloor(
                        glassFloorPrefab,
                        glassFloorRoot,
                        position,
                        x,
                        y
                    );

                    continue;
                }


                // 床を生成
                SpawnFloor(
                    concreteFloorPrefab,
                    concreteFloorRoot,
                    position,
                    x,
                    y
                );


                // 特殊Prefab取得
                ColorDatabase data = GetDatabase(color);

                if (data == null)
                    continue;


                // カテゴリRoot取得
                Transform parent = GetCategoryRoot(data.category);

                if (parent == null)
                    continue;


                // 特殊Prefab生成
                SpawnPrefab(
                    data.prefab,
                    parent,
                    position,
                    x,
                    y
                );
            }
        }

#if UNITY_EDITOR

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );

        EditorSceneManager.SaveOpenScenes();

#endif

        Debug.Log("Stage Generate 完了");
    }



    /// <summary>
    /// 床生成
    /// </summary>
    private void SpawnFloor(
        GameObject prefab,
        Transform parent,
        Vector3 position,
        int x,
        int y
    )
    {
        SpawnPrefab(
            prefab,
            parent,
            position,
            x,
            y
        );
    }



    /// <summary>
    /// Prefab生成
    /// </summary>
    private void SpawnPrefab(
        GameObject prefab,
        Transform parent,
        Vector3 position,
        int x,
        int y
    )
    {
        if (prefab == null || parent == null)
            return;

#if UNITY_EDITOR

        GameObject obj =
            (GameObject)PrefabUtility.InstantiatePrefab(
                prefab,
                parent
            );

#else

        GameObject obj =
            Instantiate(
                prefab,
                parent
            );

#endif

        obj.name = prefab.name;

        obj.transform.localPosition = position;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;


        GridObject grid = obj.GetComponent<GridObject>();

        if (grid == null)
        {
            grid = obj.AddComponent<GridObject>();
        }

        grid.SetGridPosition(
            x,
            y
        );
        Debug.Log(AssetDatabase.GetAssetPath(mazeImage));
    }
    /// <summary>
    /// Prefabの変更をシーン全体へ反映
    /// </summary>
    public void UpdateStage()
    {
#if UNITY_EDITOR

        Transform[] roots =
        {
            brickWallRoot,
            metalWallRoot,

            concreteFloorRoot,
            glassFloorRoot,

            itemSpawnRoot,
            enemySpawnRoot,

            movingWallRoot,
            breakableWallRoot,

            fakeWallRoot,

            lightRoot
        };

        foreach (Transform root in roots)
        {
            if (root == null)
                continue;

            foreach (Transform child in root)
            {
                PrefabUtility.RevertPrefabInstance(
                    child.gameObject,
                    InteractionMode.AutomatedAction
                );
            }
        }

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );

        Debug.Log("Stage Update 完了");

#endif
    }



    /// <summary>
    /// ステージ削除
    /// </summary>
    public void ClearMaze()
    {
        if (mazeParent == null)
            return;

        brickWallRoot = mazeParent.Find("BrickWall");
        metalWallRoot = mazeParent.Find("MetalWall");

        concreteFloorRoot = mazeParent.Find("ConcreteFloor");
        glassFloorRoot = mazeParent.Find("GlassFloor");

        itemSpawnRoot = mazeParent.Find("ItemSpawn");
        enemySpawnRoot = mazeParent.Find("EnemySpawn");

        movingWallRoot = mazeParent.Find("MovingWall");
        breakableWallRoot = mazeParent.Find("BreakableWall");

        fakeWallRoot = mazeParent.Find("FakeWall");

        lightRoot = mazeParent.Find("Light");


        ClearChildren(brickWallRoot);
        ClearChildren(metalWallRoot);

        ClearChildren(concreteFloorRoot);
        ClearChildren(glassFloorRoot);

        ClearChildren(itemSpawnRoot);
        ClearChildren(enemySpawnRoot);

        ClearChildren(movingWallRoot);
        ClearChildren(breakableWallRoot);

        ClearChildren(fakeWallRoot);

        ClearChildren(lightRoot);


        Debug.Log("Mazeを削除しました。");
    }
    /// <summary>
    /// カテゴリに対応するRootを返す
    /// </summary>
    private Transform GetCategoryRoot(StageCategory category)
    {
        switch (category)
        {
            case StageCategory.BrickWall:
                return brickWallRoot;

            case StageCategory.MetalWall:
                return metalWallRoot;

            case StageCategory.ConcreteFloor:
                return concreteFloorRoot;

            case StageCategory.GlassFloor:
                return glassFloorRoot;

            case StageCategory.ItemSpawn:
                return itemSpawnRoot;

            case StageCategory.EnemySpawn:
                return enemySpawnRoot;

            case StageCategory.MovingWall:
                return movingWallRoot;

            case StageCategory.BreakableWall:
                return breakableWallRoot;

            case StageCategory.FakeWall:
                return fakeWallRoot;

            case StageCategory.Light:
                return lightRoot;

            default:
                return null;
        }
    }



    /// <summary>
    /// 指定色に最も近いColorDatabaseを取得
    /// </summary>
    private ColorDatabase GetDatabase(Color32 color)
    {
        foreach (ColorDatabase data in colorDatabase)
        {
            Color32 dbColor = (Color32)data.color;

            if (dbColor.r == color.r &&
                dbColor.g == color.g &&
                dbColor.b == color.b)
            {
                return data;
            }
        }

        Debug.LogWarning($"未登録の色です : {color}");
        return null;

    }


    /// <summary>
    /// 色比較
    /// </summary>
    private bool IsSameColor(Color32 a, Color32 b)
    {
        return
            a.r == b.r &&
            a.g == b.g &&
            a.b == b.b;
    }



    /// <summary>
    /// Root取得（無ければ生成）
    /// </summary>
    private Transform FindOrCreateRoot(string rootName)
    {
        Transform root = mazeParent.Find(rootName);

        if (root != null)
            return root;

        GameObject obj = new GameObject(rootName);

        obj.transform.SetParent(
            mazeParent,
            false
        );

        return obj.transform;
    }



    /// <summary>
    /// 子オブジェクト削除
    /// </summary>
    private void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        while (parent.childCount > 0)
        {
#if UNITY_EDITOR

            DestroyImmediate(
                parent.GetChild(0).gameObject
            );

#else

            Destroy(
                parent.GetChild(0).gameObject
            );

#endif
        }
    }
}