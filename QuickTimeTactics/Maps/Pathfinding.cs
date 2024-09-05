using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Pathfinding : GridMap
{
    public Vector3I StartPosition;
    public Vector3I EndPosition;

    private AStar3D pathfindingAStar = new AStar3D();
    private AStar3D aStarWithOccupiedTiles = new AStar3D();
    public List<Vector3> Path = new List<Vector3>();

    public string WalkableTileName = "WalkableTile";
    private const string WalkableHighlightedTileName = "WalkableHighlightedTile";
    private const string NonWalkableTileName = "NonWalkableTile";
    public string PlayerOccupiedTileName = "PlayerOccupiedTile";
    public string EnemyOccupiedTileName = "EnemyOccupiedTile";

    private void UpdateAStar()
    {
        pathfindingAStar.Clear();
        aStarWithOccupiedTiles.Clear();

        foreach (Vector3I cell in GetUsedCells())
        {
            if (IsWalkableCell(cell) || cell == StartPosition)
            {
                int cellId = GetCellIdFromPosition(cell);
                pathfindingAStar.AddPoint(cellId, MapToLocal(cell), 1);
                aStarWithOccupiedTiles.AddPoint(cellId, MapToLocal(cell), 1);
            }
            if (IsWalkableCell(cell) || IsOccupiedCell(cell))
            {
                int cellId = GetCellIdFromPosition(cell);
                aStarWithOccupiedTiles.AddPoint(cellId, MapToLocal(cell), 1);
            }
        }
        foreach (int cellId in pathfindingAStar.GetPointIds())
        {
            ConnectCellNeighbors(cellId, pathfindingAStar);
        }
        foreach (int cellId in aStarWithOccupiedTiles.GetPointIds())
        {
            ConnectCellNeighbors(cellId, aStarWithOccupiedTiles);
        }
    }

    private bool IsWalkableCell(Vector3I cell)
    {
        return GetCellItem(cell) == GetMeshLibraryItemIdByName(WalkableTileName);
    }

    private void ConnectCellNeighbors(int cellId, AStar3D aStar)
    {
        Vector3I cellPosition = GetPositionFromCellId(cellId);
        Vector3I[] directions = { Vector3I.Right, Vector3I.Left, Vector3I.Forward, Vector3I.Back };

        foreach (Vector3I direction in directions)
        {
            // Calculate neighbor positions
            Vector3I horizontalNeighbor = cellPosition + direction;
            Vector3I upperNeighbor = horizontalNeighbor + Vector3I.Up;
            Vector3I lowerNeighbor = horizontalNeighbor + Vector3I.Down;

            // Check for direct horizontal connections
            if (aStar == pathfindingAStar)
            {
                if (IsWalkableCell(horizontalNeighbor) && !IsOccupiedCell(horizontalNeighbor))
                {
                    ConnectIfPossible(cellPosition, horizontalNeighbor, aStar);
                }
            }
            else
            {
                if (IsWalkableCell(horizontalNeighbor) || IsOccupiedCell(horizontalNeighbor))
                {
                    ConnectIfPossible(cellPosition, horizontalNeighbor, aStar);
                }
            }

            // Check for upper diagonal connections
            if (aStar == pathfindingAStar)
            {
                if (IsWalkableCell(upperNeighbor) && !IsOccupiedCell(upperNeighbor))
                {
                    ConnectIfPossible(cellPosition, upperNeighbor, aStar);
                }
            }
            else
            {
                if (IsWalkableCell(upperNeighbor) || IsOccupiedCell(upperNeighbor))
                {
                    ConnectIfPossible(cellPosition, upperNeighbor, aStar);
                }
            }

            // Check for lower diagonal connections
            if (aStar == pathfindingAStar)
            {
                if (IsWalkableCell(lowerNeighbor) && !IsOccupiedCell(lowerNeighbor))
                {
                    ConnectIfPossible(cellPosition, lowerNeighbor, aStar);
                }
            }
            else
            {
                if (IsWalkableCell(lowerNeighbor) || IsOccupiedCell(lowerNeighbor))
                {
                    ConnectIfPossible(cellPosition, lowerNeighbor, aStar);
                }
            }
        }
    }

    private void ConnectIfPossible(Vector3I from, Vector3I to, AStar3D aStar)
    {
        int fromId = GetCellIdFromPosition(from);
        int toId = GetCellIdFromPosition(to);
        if (aStar.HasPoint(toId))
        {
            aStar.ConnectPoints(fromId, toId, true);
        }
    }

    private void FindPath()
    {
        int startId = GetCellIdFromPosition(StartPosition);
        int endId = GetCellIdFromPosition(EndPosition);
        List<Vector3> fullPath = new List<Vector3>();
        if (IsOccupiedCell(EndPosition))
        {
            fullPath = new List<Vector3>(aStarWithOccupiedTiles.GetPointPath(startId, endId));
        }
        else
        {
            fullPath = new List<Vector3>(pathfindingAStar.GetPointPath(startId, endId));
        }
        Path.Clear();
        foreach (Vector3 pos in fullPath)
        {
            Path.Add(pos);
        }
    }

    private bool IsOccupiedCell(Vector3I cell)
    {
        int item = GetCellItem(cell);
        return item == GetMeshLibraryItemIdByName(PlayerOccupiedTileName) || item == GetMeshLibraryItemIdByName(EnemyOccupiedTileName);
    }
    public List<Vector3> CalculatePath(Vector3I start, Vector3I end)
    {
        StartPosition = start;
        EndPosition = end;
        UpdateAStar();
        FindPath();
        return Path;
    }

    public void UpdatePath(Vector3I newEnd)
    {
        EndPosition = newEnd;
        UpdateAStar();
        FindPath();
        HighlightPath();
    }


    private void HighlightPath()
    {
        int highlightedTileId = GetMeshLibraryItemIdByName(WalkableHighlightedTileName);
        foreach (Vector3 worldPosition in Path)
        {
            Vector3I gridPosition = LocalToMap(worldPosition);
            SetCellItem(gridPosition, highlightedTileId);
        }
    }
    public void ClearHighlightedPath()
    {
        int normalTileId = GetMeshLibraryItemIdByName(WalkableTileName);
        foreach (Vector3I cell in GetUsedCells())
        {
            if (GetCellItem(cell) == GetMeshLibraryItemIdByName(WalkableHighlightedTileName))
            {
                SetCellItem(cell, normalTileId);
            }
        }
    }

    private int GetCellIdFromPosition(Vector3I position)
    {
        // Flat conversion assuming unique IDs across floors
        return position.X + position.Y * 1000 + position.Z * 1000000;
    }

    private Vector3I GetPositionFromCellId(int cellId)
    {
        int x = cellId % 1000;
        int y = (cellId / 1000) % 1000;
        int z = cellId / 1000000;
        return new Vector3I(x, y, z);
    }
    public int GetPathBetweenUnits(Vector3I startPosition, Vector3I endPosition)
    {
        UpdateAStar();
        int startId = GetCellIdFromPosition(startPosition);
        int endId = GetCellIdFromPosition(endPosition);
        if (aStarWithOccupiedTiles.HasPoint(startId) && aStarWithOccupiedTiles.HasPoint(endId))
        {
            var path = aStarWithOccupiedTiles.GetPointPath(startId, endId);
            return path.Count(); // The length of the path is the number of steps in the path.
        }
        return int.MaxValue; // Return a large value if no path exists.
    }

    public int GetMeshLibraryItemIdByName(string name)
    {
        foreach (var key in MeshLibrary.GetItemList())
        {
            if (MeshLibrary.GetItemName(key) == name)
            {
                return key;
            }
        }
        GD.PrintErr($"MeshLibrary item with name '{name}' not found.");
        return -1; // Return an invalid ID if not found
    }
}
