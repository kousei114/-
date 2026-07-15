using UnityEngine;

public enum StageCategory
{
    BrickWall,
    MetalWall,

    ConcreteFloor,
    GlassFloor,

    ItemSpawn,
    EnemySpawn,

    MovingWall,
    BreakableWall,

    FakeWall,

    Light
}

[System.Serializable]
public class ColorDatabase
{
    [Header("Display Name")]
    public string name;

    [Header("Map Color")]
    public Color color = Color.white;

    [Header("Prefab")]
    public GameObject prefab;

    [Header("Category Root")]
    public StageCategory category;
}