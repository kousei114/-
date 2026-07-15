using UnityEngine;

public class GridObject : MonoBehaviour
{
    [Header("Grid Position")]
    public Vector2Int gridPosition;

    /// <summary>
    /// À•W‚ğİ’è
    /// </summary>
    public void SetGridPosition(int x, int y)
    {
        gridPosition = new Vector2Int(x, y);
    }
}