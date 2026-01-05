using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Grid Manager untuk A* Pathfinding
/// Generate grid nodes berdasarkan ukuran map dan detect WALL layer
/// </summary>
public class PathGrid : MonoBehaviour
{
    public static PathGrid Instance { get; private set; }
    
    [Header("Grid Settings")]
    public int gridWidth = 50;
    public int gridHeight = 50;
    public float cellSize = 1f;
    public Vector3 gridOrigin = Vector3.zero;
    
    [Header("Obstacle Detection")]
    public LayerMask wallLayer; // Assign "WALL" layer di Inspector
    
    [Header("Agent Settings")]
    [Tooltip("Radius enemy dalam tiles. Set 0 untuk tidak ada padding (default)")]
    public int agentRadius = 0; // Default 0 agar koridor sempit tetap walkable
    
    [Header("Debug")]
    public bool showGrid = true;
    
    private PathNode[,] grid;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        CreateGrid();
    }
    
    /// <summary>
    /// Buat grid pathfinding berdasarkan settings
    /// </summary>
    public void CreateGrid()
    {
        grid = new PathNode[gridWidth, gridHeight];
        
        // Step 1: Pertama, detect semua wall tiles
        bool[,] isBlocked = new bool[gridWidth, gridHeight];
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPos = GetWorldPosition(x, y);
                isBlocked[x, y] = IsPositionBlocked(worldPos);
            }
        }
        
        // Step 2: Expand blocked area berdasarkan agent radius
        // Ini membuat "padding" di sekitar tembok
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                bool isWalkable = !isBlocked[x, y];
                
                // Jika walkable, cek apakah ada wall di sekitarnya (dalam radius)
                if (isWalkable && agentRadius > 0)
                {
                    isWalkable = !HasWallNearby(isBlocked, x, y, agentRadius);
                }
                
                grid[x, y] = new PathNode(this, x, y, isWalkable);
            }
        }
        
        Debug.Log($"PathGrid created: {gridWidth}x{gridHeight}, CellSize: {cellSize}, AgentRadius: {agentRadius}");
    }
    
    /// <summary>
    /// Cek apakah ada wall dalam radius tertentu dari tile
    /// </summary>
    private bool HasWallNearby(bool[,] isBlocked, int centerX, int centerY, int radius)
    {
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                
                int checkX = centerX + dx;
                int checkY = centerY + dy;
                
                if (checkX >= 0 && checkX < gridWidth && checkY >= 0 && checkY < gridHeight)
                {
                    if (isBlocked[checkX, checkY])
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    
    /// <summary>
    /// Cek apakah posisi tertentu blocked oleh wall
    /// Menggunakan OverlapPoint dan short-range raycast untuk akurasi
    /// </summary>
    private bool IsPositionBlocked(Vector3 worldPos)
    {
        // Metode 1: Cek titik tepat di tengah cell
        if (Physics2D.OverlapPoint(worldPos, wallLayer))
        {
            return true;
        }
        
        // Metode 2: Cek dengan OverlapBox kecil di tengah
        Vector2 boxSize = new Vector2(cellSize * 0.3f, cellSize * 0.3f);
        if (Physics2D.OverlapBox(worldPos, boxSize, 0f, wallLayer))
        {
            return true;
        }
        
        // Metode 3: Raycast pendek dari 4 arah (hanya cek wall yang melewati cell ini)
        float checkRadius = cellSize * 0.35f;
        Vector2[] directions = new Vector2[]
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right
        };
        
        foreach (Vector2 dir in directions)
        {
            Vector2 startPos = (Vector2)worldPos;
            RaycastHit2D hit = Physics2D.Raycast(startPos, dir, checkRadius, wallLayer);
            
            if (hit.collider != null)
            {
                return true;
            }
        }
        
        return false;
    }


    
    /// <summary>
    /// Konversi grid position ke world position
    /// </summary>
    public Vector3 GetWorldPosition(int x, int y)
    {
        return gridOrigin + new Vector3(x * cellSize + cellSize / 2, y * cellSize + cellSize / 2, 0);
    }
    
    /// <summary>
    /// Konversi world position ke grid position
    /// </summary>
    public void GetGridPosition(Vector3 worldPos, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPos.x - gridOrigin.x) / cellSize);
        y = Mathf.FloorToInt((worldPos.y - gridOrigin.y) / cellSize);
        
        // Clamp ke dalam grid bounds
        x = Mathf.Clamp(x, 0, gridWidth - 1);
        y = Mathf.Clamp(y, 0, gridHeight - 1);
    }
    
    /// <summary>
    /// Mendapatkan node di posisi grid tertentu
    /// </summary>
    public PathNode GetNode(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return grid[x, y];
        }
        return null;
    }
    
    /// <summary>
    /// Mendapatkan node di world position tertentu
    /// </summary>
    public PathNode GetNodeFromWorldPosition(Vector3 worldPos)
    {
        GetGridPosition(worldPos, out int x, out int y);
        return GetNode(x, y);
    }
    
    /// <summary>
    /// Mendapatkan semua neighbor nodes (4 arah saja - tanpa diagonal)
    /// Ini mencegah enemy stuck di pojok tembok
    /// </summary>
    public List<PathNode> GetNeighbors(PathNode node)
    {
        List<PathNode> neighbors = new List<PathNode>();
        
        // Hanya 4 arah: atas, bawah, kiri, kanan (TANPA diagonal)
        // Ini lebih aman untuk navigasi di dungeon dengan banyak pojok
        int[,] directions = new int[,]
        {
            { 0, 1 },   // Atas
            { 0, -1 },  // Bawah
            { -1, 0 },  // Kiri
            { 1, 0 }    // Kanan
        };
        
        for (int i = 0; i < 4; i++)
        {
            int checkX = node.x + directions[i, 0];
            int checkY = node.y + directions[i, 1];
            
            PathNode neighbor = GetNode(checkX, checkY);
            if (neighbor != null && neighbor.isWalkable)
            {
                neighbors.Add(neighbor);
            }
        }
        
        return neighbors;
    }
    
    /// <summary>
    /// Refresh walkable status untuk node di sekitar posisi tertentu
    /// Berguna jika ada dynamic obstacles
    /// </summary>
    public void RefreshNodeAtPosition(Vector3 worldPos)
    {
        GetGridPosition(worldPos, out int x, out int y);
        PathNode node = GetNode(x, y);
        if (node != null)
        {
            Vector3 nodeWorldPos = GetWorldPosition(x, y);
            Vector2 boxSize = new Vector2(cellSize * 0.8f, cellSize * 0.8f);
            node.isWalkable = !Physics2D.OverlapBox(nodeWorldPos, boxSize, 0f, wallLayer);
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showGrid || grid == null) return;
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPos = GetWorldPosition(x, y);
                
                if (grid[x, y] != null)
                {
                    Gizmos.color = grid[x, y].isWalkable ? new Color(0, 1, 0, 0.2f) : new Color(1, 0, 0, 0.3f);
                    Gizmos.DrawCube(worldPos, Vector3.one * cellSize * 0.9f);
                }
            }
        }
    }
}
