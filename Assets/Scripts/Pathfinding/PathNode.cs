using UnityEngine;

/// <summary>
/// Node untuk A* Pathfinding Grid
/// Menyimpan data posisi, walkable status, dan A* costs
/// </summary>
public class PathNode
{
    public int x;
    public int y;
    public bool isWalkable;
    
    // A* Pathfinding costs
    public int gCost; // Jarak dari start node
    public int hCost; // Heuristic - estimasi jarak ke target
    public int fCost { get { return gCost + hCost; } } // Total cost
    
    public PathNode parentNode; // Untuk trace path balik
    
    private PathGrid grid;
    
    public PathNode(PathGrid grid, int x, int y, bool isWalkable)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        this.isWalkable = isWalkable;
    }
    
    /// <summary>
    /// Mendapatkan posisi world dari node ini
    /// </summary>
    public Vector3 GetWorldPosition()
    {
        return grid.GetWorldPosition(x, y);
    }
    
    public override string ToString()
    {
        return $"Node({x},{y}) Walkable:{isWalkable}";
    }
}
