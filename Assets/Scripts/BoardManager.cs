using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GrinderColor
{
    Red, Green, Blue, Yellow
}
public enum GrinderType
{
    Horizontal, Vertical
}

[System.Serializable]
public class GrinderSpawn
{
    public Vector2Int position; // kenar pozisyonu
    public GrinderColor color;
    public int size = 1; // 1, 2 veya 3
}

[System.Serializable]
public class ObstacleSpawn
{
    public Vector2Int position;
}

[System.Serializable]
public class BlockSpawn
{
    public Vector2Int position;   // Grid pozisyonu
    public int shapeID;           // BlockDataSO içindeki shapeID
    public BlockType blockType;   // Normal, Iced, hor, ver
    public BlockColor blockColor; // Red, Green, Blue, Yellow
    public int iceCount;
}



public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;
    public int remainingBlocks;

    [Header("Level Settings")]
    public int levelIndex = 1; // 1-5 
    public LevelData currentLevel;
    private int width;
    private int height;

    public Grid grid; // Unity Grid sistemi
    public HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>(); // xlistx -> aynı degerde iki veri olmasın diye hashSet, search daha hizli

    [Header("Prefabs")]
    public GameObject cellPrefab;
    public GameObject wallPrefab;
    public GameObject cornerPrefab;
    public GameObject grinder1Prefab;
    public GameObject grinder2Prefab;
    public GameObject grinder3Prefab;
    public GameObject obstaclePrefab;
    public GameObject blockPrefab;
    public List<BlockDataSO> allBlockData;

    private Transform boardParent;

    public List<BlockInstanceManager> spawnedBlocks;
    
    private void Awake()
    {
        Instance = this;

        LoadLevel(levelIndex);
    }

    private void Update()
    {
        if (remainingBlocks <= 0)
        {
            levelIndex++;
            if (levelIndex >= 5)
            {
                levelIndex = 1; 
            }
            occupiedCells.Clear();
            LoadLevel(levelIndex);
        }
    }

    public void SetCameraPos()
    {
        Transform mainCamTransform = Camera.main.transform;
        float camXOffset = (float)currentLevel.width / 2;
        mainCamTransform.position = new Vector3(camXOffset, mainCamTransform.position.y, mainCamTransform.position.z);
    }
    
    public void LoadLevel(int index)
    {
        string path = $"Levels/Level_{index}";
        TextAsset json = Resources.Load<TextAsset>(path);
        if (json == null)
        {
            Debug.LogError($"Level JSON bulunamadı: {path}");
            return;
        }
        currentLevel = JsonUtility.FromJson<LevelData>(json.text);
        remainingBlocks = currentLevel.blocks.Count;
        SetCameraPos();
        BuildBoard();
    }

    public void BuildBoard()
    {
        if (currentLevel == null)
        {
            Debug.Log("Level null");
            return;
        }

        if (boardParent != null)
            DestroyImmediate(boardParent.gameObject);

        boardParent = new GameObject("Board").transform;
        boardParent.SetParent(transform);

        width = currentLevel.width;
        height = currentLevel.height;

        // Ground
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 pos = grid.GetCellCenterWorld(new Vector3Int(x, 0, z));
                Instantiate(cellPrefab, pos, cellPrefab.transform.rotation, boardParent);
            }
        }

        // Walls, Corners and Grinders
        for (int x = -1; x <= width; x++)
        {
            for (int z = -1; z <= height; z++)
            {
                bool isOutside = (x < 0 || z < 0 || x >= width || z >= height);
                if (!isOutside) continue;

                bool isCorner = (x < 0 && z < 0) ||
                                (x < 0 && z >= height) ||
                                (x >= width && z < 0) ||
                                (x >= width && z >= height);

                Vector3 refCellPos = grid.GetCellCenterWorld(new Vector3Int(
                    Mathf.Clamp(x, 0, width - 1),
                    0,
                    Mathf.Clamp(z, 0, height - 1)
                ));

                if (isCorner)
                {
                    SpawnCorner(refCellPos, x, z);
                }
                else
                {
                    SpawnWall(refCellPos, x, z);
                    GrinderSpawn grinder = currentLevel.grinders.Find(g => g.position == new Vector2Int(x, z));
                    if (grinder != null)
                    {
                        SpawnGrinder(refCellPos, x, z, grinder);
                    }
                }
            }
        }

        // Obstacles
        foreach (var obs in currentLevel.obstacles)
        {
            if (IsInsideBoard(obs.position.x, obs.position.y))
            {
                Vector3 pos = grid.GetCellCenterWorld(new Vector3Int(obs.position.x, 0, obs.position.y));
                Instantiate(obstaclePrefab, pos, obstaclePrefab.transform.rotation, boardParent);
                
                Vector3Int rootCell = grid.WorldToCell(pos);
                List<Vector2Int> obstacleShapeOffsets = new List<Vector2Int>
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 0),
                    new Vector2Int(1, 1)
                };
                AddShapeToGrid(rootCell, obstacleShapeOffsets);
                
            }
        }

        // Blocks
        foreach (var b in currentLevel.blocks)
        {
            if (IsInsideBoard(b.position.x, b.position.y))
            {
                Vector3 pos = grid.GetCellCenterWorld(new Vector3Int(b.position.x, 0, b.position.y));
                GameObject blockObj = Instantiate(blockPrefab, pos, Quaternion.identity, boardParent);

                BlockInstanceManager inst = blockObj.GetComponent<BlockInstanceManager>();
                inst.blockInstance = new BlockInstanceData
                {
                    blockData = allBlockData.Find(x => x.shapeID == b.shapeID),
                    blockType = b.blockType,
                    blockColor = b.blockColor,
                    iceCount = b.iceCount
                };
                spawnedBlocks.Add(inst);
            }
        }
    }

    private void SpawnCorner(Vector3 cellPos, int x, int z)
    {
        float half = grid.cellSize.x / 2f;
        Vector3 offset = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        if (x < 0 && z < 0) { rot = Quaternion.Euler(-90, 0, -180); offset = new Vector3(-half, 0, -half); }
        else if (x < 0 && z >= height) { rot = Quaternion.Euler(-90, 0, -90); offset = new Vector3(-half, 0, half); }
        else if (x >= width && z < 0) { rot = Quaternion.Euler(-90, 0, -270); offset = new Vector3(half, 0, -half); }
        else if (x >= width && z >= height) { rot = Quaternion.Euler(-90, 0, 0); offset = new Vector3(half, 0, half); }

        Instantiate(cornerPrefab, cellPos + offset, rot, boardParent);
    }

    private void SpawnWall(Vector3 cellPos, int x, int z)
    {
        float half = grid.cellSize.x / 2f;
        Vector3 offset = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        if (z < 0)
        {
            rot = Quaternion.Euler(-90, 0, -90);
            offset = new Vector3(-half, 0, -half);
        }
        else if (z >= height)
        {
            rot = Quaternion.Euler(-90, 0, -90);
            offset = new Vector3(-half, 0, 0.8f);
        }
        else if (x < 0)
        {
            rot = Quaternion.Euler(-90, 0, -180);
            offset = new Vector3(-0.8f, 0, -half);
        }
        else if (x >= width)
        {
            rot = Quaternion.Euler(-90, 0, -180);
            offset = new Vector3(half, 0, -half);
        }

        Instantiate(wallPrefab, cellPos + offset, rot, boardParent);
    }

    private void SpawnGrinder(Vector3 cellPos, int x, int z, GrinderSpawn grinder)
    {
        GameObject prefab = null;
        if (grinder.size == 1) prefab = grinder1Prefab;
        else if (grinder.size == 2) prefab = grinder2Prefab;
        else if (grinder.size == 3) prefab = grinder3Prefab;

        float half = grid.cellSize.x / 2f;
        float halfSize = grinder.size / 2f;
        Vector3 offset = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        Vector3 grinderDirection = Vector3.zero;
        Vector3 grinderRayStartOffset = Vector3.zero;
        GrinderType grinderType = GrinderType.Horizontal; //def
        
        if (z < 0)
        {
            rot = Quaternion.Euler(-90, 0, -90);
            offset = new Vector3(-half, 0, -0.8f);
            grinderDirection = Vector3.back;
            grinderRayStartOffset = new Vector3(halfSize, half, 0);
            grinderType = GrinderType.Horizontal;
        }
        else if (z >= height)
        {
            rot = Quaternion.Euler(-90, 0, -270);
            offset = new Vector3(half, 0, 0.8f);
            grinderDirection = Vector3.forward;
            grinderRayStartOffset = new Vector3(-halfSize, half, 0);
            grinderType = GrinderType.Horizontal;
        }
        else if (x < 0)
        {
            rot = Quaternion.Euler(-90, 0, 0);
            offset = new Vector3(-0.8f, 0, half);
            grinderDirection = Vector3.left;
            grinderRayStartOffset = new Vector3(0, half, -halfSize);
            grinderType = GrinderType.Vertical;
        }
        else if (x >= width)
        {
            rot = Quaternion.Euler(-90, 0, -180);
            offset = new Vector3(0.8f, 0, -half);
            grinderDirection = Vector3.right;
            grinderRayStartOffset = new Vector3(0, half, halfSize);
            grinderType = GrinderType.Vertical;
        }

        GameObject gObj = Instantiate(prefab, cellPos + offset, rot, boardParent);
        gObj.GetComponent<GrinderManager>().InitializeGrinder(grinder.color, grinderDirection, grinderRayStartOffset, grinderType, grinder.size);
    }

    private bool IsInsideBoard(int gx, int gz)
    {
        return gx >= 0 && gz >= 0 && gx < width && gz < height;
    }
    
    
    public bool IsShapePlacementValid(Vector3Int rootCell, List<Vector2Int> shapeOffsets, BlockController ignoreBlock = null)
    {
        foreach (var offset in shapeOffsets)
        {
            Vector3Int checkCell = rootCell + new Vector3Int(offset.x, 0, offset.y);

            //tahta dısına cıkma
            if (checkCell.x < 0 || checkCell.x >= width || checkCell.z < 0 || checkCell.z >= height)
                return false;
            
            //blogun kapladigi kendi hucreleri
            if (ignoreBlock != null && ignoreBlock.occupiedCells.Contains(checkCell))
                continue;

            //dolu hucreler
            if (occupiedCells.Contains(checkCell))
                return false;
        }
        return true;
    }

    public void AddShapeToGrid(Vector3Int rootCell, List<Vector2Int> shapeOffsets)
    {
        foreach (var offset in shapeOffsets)
        {
            Vector3Int cell = rootCell + new Vector3Int(offset.x, 0, offset.y);
            occupiedCells.Add(cell);
        }
    }

    public void RemoveShapeFromGrid(Vector3Int rootCell, List<Vector2Int> shapeOffsets)
    {
        foreach (var offset in shapeOffsets)
        {
            Vector3Int cell = rootCell + new Vector3Int(offset.x, 0, offset.y);
            occupiedCells.Remove(cell);
        }
    }


    public List<BlockInstanceManager> GetIcedBlocks()
    {
        List<BlockInstanceManager> icedBlocks = new List<BlockInstanceManager>();
        foreach (var s in spawnedBlocks)
        {
            if (s.blockInstance.iceCount > 0 && s.blockInstance.blockType == BlockType.Iced)
            {
                icedBlocks.Add(s);
            }
        }

        return icedBlocks;
    }
}
