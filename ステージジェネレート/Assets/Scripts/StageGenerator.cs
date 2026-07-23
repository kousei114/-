using System.Collections.Generic;
using UnityEngine;

public class StageGenerator : MonoBehaviour
{
    [Header("Maze Images")]
    [Tooltip("下ステージ（通常。床が上向き）")]
    public Texture2D mazeImage;

    [Tooltip("上ステージ（重力反転で裏面を歩く。未設定なら上ステージは生成されない）")]
    public Texture2D mazeImageUpper;


    [Header("Two-Stage Settings")]
    [Tooltip("上ステージを生成するか")]
    public bool generateUpperStage = true;

    [Tooltip("下ステージ床から上ステージ天井までの高さ。\n下の壁の高さ + 上の壁の高さ + プレイヤー身長 より大きくすること")]
    public float stageHeight = 6f;


    [Header("Maze Parent")]
    public Transform mazeParent;


    [Header("Color Database")]
    public List<ColorDatabase> colorDatabase = new();


    [Header("Floor Prefabs")]
    public GameObject concreteFloorPrefab;
    public GameObject glassFloorPrefab;


    private const int GridSize = 60;

    // カテゴリRootの名前一覧（StageCategory の enum 名と一致させる）
    private static readonly string[] CategoryRootNames =
    {
        "BrickWall", "MetalWall",
        "ConcreteFloor", "GlassFloor",
        "ItemSpawn", "EnemySpawn",
        "MovingWall", "BreakableWall",
        "FakeWall", "Light"
    };


    public void Generate()
    {
        if (mazeParent == null)
        {
            Debug.LogWarning("Maze Parent が設定されていません。");
            return;
        }

        if (mazeImage == null)
        {
            Debug.LogWarning("Maze Image（下ステージ）が設定されていません。");
            return;
        }


        // 既存ステージを削除
        ClearMaze();


        // ── 下ステージ：通常（床が上向き、y = 0）
        Transform lower = FindOrCreateContainer("LowerStage");
        BuildStage(lower, mazeImage);
        lower.localPosition = Vector3.zero;
        lower.localRotation = Quaternion.identity;


        // ── 上ステージ：重力反転で裏面を歩く
        // X軸まわりに180°回転して床面を下向き（天井化）にし、y = stageHeight へ持ち上げる
        if (generateUpperStage && mazeImageUpper != null)
        {
            Transform upper = FindOrCreateContainer("UpperStage");
            BuildStage(upper, mazeImageUpper);
            upper.localPosition = new Vector3(0f, stageHeight, 0f);
            upper.localRotation = Quaternion.Euler(180f, 0f, 0f);
        }


        SaveScene();

        Debug.Log("Stage Generate 完了");
    }


    // 1枚分のステージを container 直下に生成する
    private void BuildStage(Transform container, Texture2D image)
    {
        // カテゴリRootを container 直下に用意
        foreach (string rootName in CategoryRootNames)
            FindOrCreateRoot(container, rootName);


        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                int px = Mathf.Clamp(
                    Mathf.FloorToInt((x + 0.5f) * image.width / GridSize),
                    0,
                    image.width - 1
                );

                int py = Mathf.Clamp(
                    Mathf.FloorToInt((y + 0.5f) * image.height / GridSize),
                    0,
                    image.height - 1
                );


                Color color = image.GetPixel(px, py);

                Vector3 position = new Vector3(x, 0f, -y);


                // 白 → コンクリート床
                if (IsSameColor(color, Color.white))
                {
                    CreateFloor(container, concreteFloorPrefab, position);
                    continue;
                }


                // 水色 → ガラス床
                if (IsSameColor(color, Color.cyan))
                {
                    CreateFloor(container, glassFloorPrefab, position);
                    continue;
                }


                // その他 → 床（コンクリート）＋ Color Database の Prefab
                CreateFloor(container, concreteFloorPrefab, position);

                ColorDatabase data = GetDatabase(color);
                if (data == null)
                    continue;

                Transform parent = GetCategoryRoot(container, data.category);

                GameObject obj = Instantiate(data.prefab, parent);
                obj.name = data.prefab.name;
                obj.transform.localPosition = position;
            }
        }
    }


    private void CreateFloor(Transform container, GameObject floorPrefab, Vector3 position)
    {
        if (floorPrefab == null)
            return;

        // 元実装どおり、床はすべて ConcreteFloor Root にまとめる
        Transform root = GetCategoryRoot(container, StageCategory.ConcreteFloor);

        GameObject floor = Instantiate(floorPrefab, root);
        floor.name = floorPrefab.name;
        floor.transform.localPosition = position;
    }


    public void ClearMaze()
    {
        if (mazeParent == null)
            return;

        // 2ステージ構造のコンテナごと削除
        DestroyContainer("LowerStage");
        DestroyContainer("UpperStage");

        // 旧構造（mazeParent 直下にカテゴリRootが並ぶ形）が残っていれば掃除
        foreach (string rootName in CategoryRootNames)
        {
            Transform legacy = mazeParent.Find(rootName);
            if (legacy != null)
                ClearChildren(legacy);
        }

        Debug.Log("Maze を削除しました。");
    }


    private void DestroyContainer(string name)
    {
        Transform container = mazeParent.Find(name);
        if (container == null)
            return;

#if UNITY_EDITOR
        DestroyImmediate(container.gameObject);
#else
        Destroy(container.gameObject);
#endif
    }


    private Transform GetCategoryRoot(Transform container, StageCategory category)
    {
        // StageCategory の名前とRoot名は一致させてある
        return container.Find(category.ToString());
    }


    private bool IsSameColor(Color a, Color b, float tolerance = 0.05f)
    {
        return Mathf.Abs(a.r - b.r) <= tolerance &&
               Mathf.Abs(a.g - b.g) <= tolerance &&
               Mathf.Abs(a.b - b.b) <= tolerance;
    }


    private ColorDatabase GetDatabase(Color color)
    {
        ColorDatabase nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (ColorDatabase pair in colorDatabase)
        {
            float distance =
                Mathf.Pow(color.r - pair.color.r, 2) +
                Mathf.Pow(color.g - pair.color.g, 2) +
                Mathf.Pow(color.b - pair.color.b, 2);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = pair;
            }
        }

        if (nearestDistance > 0.1f)
        {
            Debug.LogWarning($"未登録の色です : {color}");
            return null;
        }

        return nearest;
    }


    private Transform FindOrCreateContainer(string name)
    {
        return FindOrCreateRoot(mazeParent, name);
    }


    private Transform FindOrCreateRoot(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
            return child;

        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        return obj.transform;
    }


    private void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        while (parent.childCount > 0)
        {
#if UNITY_EDITOR
            DestroyImmediate(parent.GetChild(0).gameObject);
#else
            Destroy(parent.GetChild(0).gameObject);
#endif
        }
    }


    private void SaveScene()
    {
#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
#endif
    }
}
