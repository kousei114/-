using System.Collections.Generic;
using UnityEngine;

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


        // Root作成
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


        // 既存ステージ削除
        ClearMaze();


        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                int px = Mathf.Clamp(
                    Mathf.FloorToInt((x + 0.5f) * mazeImage.width / GridSize),
                    0,
                    mazeImage.width - 1
                );

                int py = Mathf.Clamp(
                    Mathf.FloorToInt((y + 0.5f) * mazeImage.height / GridSize),
                    0,
                    mazeImage.height - 1
                );


                Color color = mazeImage.GetPixel(px, py);


                Vector3 position = new Vector3(x, 0f, -y);



                // 白 → コンクリート床
                if (IsSameColor(color, Color.white))
                {
                    CreateFloor(
                        concreteFloorPrefab,
                        position,
                        x,
                        y
                    );

                    continue;
                }



                // 水色 → ガラス床
                if (IsSameColor(color, Color.cyan))
                {
                    CreateFloor(
                        glassFloorPrefab,
                        position,
                        x,
                        y
                    );

                    continue;
                }



                // その他は床＋特殊Prefab
                CreateFloor(
                    concreteFloorPrefab,
                    position,
                    x,
                    y
                );


                ColorDatabase data = GetDatabase(color);


                if (data == null)
                    continue;



                Transform parent = GetCategoryRoot(data.category);



                GameObject obj = Instantiate(
                    data.prefab,
                    parent
                );


                obj.name = data.prefab.name;

                obj.transform.localPosition = position;
            }
        }



#if UNITY_EDITOR

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene()
        );


        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

#endif


        Debug.Log("Stage Generate 完了");
    }



    private void CreateFloor(
        GameObject floorPrefab,
        Vector3 position,
        int x,
        int y
    )
    {
        if (floorPrefab == null)
            return;


        GameObject floor = Instantiate(
            floorPrefab,
            concreteFloorRoot
        );


        floor.name = floorPrefab.name;

        floor.transform.localPosition = position;
    }



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
        }


        return null;
    }





    private bool IsSameColor(
        Color a,
        Color b,
        float tolerance = 0.05f
    )
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
            Debug.LogWarning(
                $"未登録の色です : {color}"
            );

            return null;
        }



        return nearest;
    }





    private Transform FindOrCreateRoot(string name)
    {
        Transform child = mazeParent.Find(name);



        if (child != null)
            return child;



        GameObject obj = new GameObject(name);


        obj.transform.SetParent(
            mazeParent
        );


        obj.transform.localPosition = Vector3.zero;



        return obj.transform;
    }





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