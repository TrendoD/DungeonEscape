using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A* Pathfinding Algorithm Implementation
/// Mencari path optimal dari start ke target menghindari WALL
/// </summary>
public class Pathfinding : MonoBehaviour
{
    public static Pathfinding Instance { get; private set; }
    
    private PathGrid grid;
    
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14; // sqrt(2) * 10 ≈ 14
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        grid = PathGrid.Instance;
    }
    
    /// <summary>
    /// Mencari path dari startPos ke endPos menggunakan A* algorithm
    /// </summary>
    /// <returns>List of waypoints (Vector3) dari start ke end, atau null jika tidak ada path</returns>
    public List<Vector3> FindPath(Vector3 startPos, Vector3 endPos)
    {
        if (grid == null)
        {
            grid = PathGrid.Instance;
            if (grid == null)
            {
                Debug.LogWarning("PathGrid not found!");
                return null;
            }
        }
        
        PathNode startNode = grid.GetNodeFromWorldPosition(startPos);
        PathNode endNode = grid.GetNodeFromWorldPosition(endPos);
        
        if (startNode == null || endNode == null)
        {
            Debug.LogWarning($"Start or end node is null! Start: {startNode}, End: {endNode}");
            return null;
        }
        
        // Jika start node tidak walkable, cari node walkable terdekat
        if (!startNode.isWalkable)
        {
            startNode = FindNearestWalkableNode(startNode);
            if (startNode == null)
            {
                Debug.LogWarning("Could not find walkable start node!");
                return null;
            }
        }
        
        // Jika target node tidak walkable, cari node walkable terdekat
        if (!endNode.isWalkable)
        {
            endNode = FindNearestWalkableNode(endNode);
            if (endNode == null)
            {
                Debug.LogWarning("Could not find walkable end node!");
                return null;
            }
        }
        
        List<PathNode> openList = new List<PathNode> { startNode };
        HashSet<PathNode> closedSet = new HashSet<PathNode>();
        
        // Reset semua node costs
        for (int x = 0; x < grid.gridWidth; x++)
        {
            for (int y = 0; y < grid.gridHeight; y++)
            {
                PathNode node = grid.GetNode(x, y);
                if (node != null)
                {
                    node.gCost = int.MaxValue;
                    node.parentNode = null;
                }
            }
        }
        
        startNode.gCost = 0;
        startNode.hCost = CalculateHeuristic(startNode, endNode);
        
        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            
            // Sudah sampai target
            if (currentNode == endNode)
            {
                return CalculatePath(endNode);
            }
            
            openList.Remove(currentNode);
            closedSet.Add(currentNode);
            
            foreach (PathNode neighbor in grid.GetNeighbors(currentNode))
            {
                if (closedSet.Contains(neighbor)) continue;
                if (!neighbor.isWalkable)
                {
                    closedSet.Add(neighbor);
                    continue;
                }
                
                // Semua gerakan sekarang straight (tidak ada diagonal)
                int moveCost = MOVE_STRAIGHT_COST;
                
                int tentativeGCost = currentNode.gCost + moveCost;
                
                if (tentativeGCost < neighbor.gCost)
                {
                    neighbor.parentNode = currentNode;
                    neighbor.gCost = tentativeGCost;
                    neighbor.hCost = CalculateHeuristic(neighbor, endNode);
                    
                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }
        
        // Tidak ada path ditemukan
        return null;
    }
    
    /// <summary>
    /// Mencari node walkable terdekat dari node yang tidak walkable
    /// </summary>
    private PathNode FindNearestWalkableNode(PathNode node)
    {
        int searchRadius = 1;
        int maxRadius = 5;
        
        while (searchRadius <= maxRadius)
        {
            for (int dx = -searchRadius; dx <= searchRadius; dx++)
            {
                for (int dy = -searchRadius; dy <= searchRadius; dy++)
                {
                    PathNode checkNode = grid.GetNode(node.x + dx, node.y + dy);
                    if (checkNode != null && checkNode.isWalkable)
                    {
                        return checkNode;
                    }
                }
            }
            searchRadius++;
        }
        
        return null;
    }
    
    /// <summary>
    /// Hitung heuristic (estimasi jarak ke target) menggunakan Manhattan distance
    /// </summary>
    private int CalculateHeuristic(PathNode a, PathNode b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(dx - dy);
        return MOVE_DIAGONAL_COST * Mathf.Min(dx, dy) + MOVE_STRAIGHT_COST * remaining;
    }
    
    /// <summary>
    /// Mendapatkan node dengan F cost terendah dari list
    /// </summary>
    private PathNode GetLowestFCostNode(List<PathNode> nodeList)
    {
        PathNode lowestNode = nodeList[0];
        
        for (int i = 1; i < nodeList.Count; i++)
        {
            if (nodeList[i].fCost < lowestNode.fCost ||
                (nodeList[i].fCost == lowestNode.fCost && nodeList[i].hCost < lowestNode.hCost))
            {
                lowestNode = nodeList[i];
            }
        }
        
        return lowestNode;
    }
    
    /// <summary>
    /// Build path dari end node ke start node, lalu reverse
    /// </summary>
    private List<Vector3> CalculatePath(PathNode endNode)
    {
        List<Vector3> path = new List<Vector3>();
        PathNode currentNode = endNode;
        
        while (currentNode != null)
        {
            path.Add(currentNode.GetWorldPosition());
            currentNode = currentNode.parentNode;
        }
        
        path.Reverse();
        
        // Smooth path - hapus waypoint yang tidak perlu
        return SmoothPath(path);
    }
    
    /// <summary>
    /// Simplify path dengan menghapus intermediate nodes yang satu garis lurus
    /// </summary>
    private List<Vector3> SmoothPath(List<Vector3> path)
    {
        if (path.Count <= 2) return path;
        
        List<Vector3> smoothed = new List<Vector3>();
        smoothed.Add(path[0]);
        
        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector3 prev = path[i - 1];
            Vector3 current = path[i];
            Vector3 next = path[i + 1];
            
            Vector3 dir1 = (current - prev).normalized;
            Vector3 dir2 = (next - current).normalized;
            
            // Jika arah berubah, tambahkan waypoint ini
            if (Vector3.Dot(dir1, dir2) < 0.99f)
            {
                smoothed.Add(current);
            }
        }
        
        smoothed.Add(path[path.Count - 1]);
        
        return smoothed;
    }
}
