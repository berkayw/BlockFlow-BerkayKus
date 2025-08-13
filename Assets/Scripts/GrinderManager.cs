using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GrinderManager : MonoBehaviour
{
    public Color color;
    Renderer grinderRenderer;
    public Vector3 direction;
    public Vector3 rayStartOffset;
    public GameDataSO gameData;
    private BlockController _blockController;
    public GrinderType grinderType;
    public int size;

    public GameObject blockDestroyVFX;

    public void Awake()
    {
        grinderRenderer = GetComponent<Renderer>();    
    }

    private void OnEnable()
    {
        GameEventSystem.Instance.BlockPlacedEvent.AddListener(CheckGrinders);
    }
    private void OnDisable()
    {
        GameEventSystem.Instance.BlockPlacedEvent.RemoveListener(CheckGrinders);
    }
    
    public void InitializeGrinder(GrinderColor _color, Vector3 _direction, Vector3 _rayStartOffset, GrinderType _grinderType, int _size)
    {
        SetGrinderColor(_color);
        SetDirectionandRayPoint(_direction, _rayStartOffset);
        SetSize(_grinderType, _size);
    }
    
    
    public void SetGrinderColor(GrinderColor _color)
    {
        switch (_color)
        {
            case GrinderColor.Red: 
                color = gameData.colorMaterials[0].color; 
                break;
            case GrinderColor.Green: 
                color = gameData.colorMaterials[1].color; 
                break;
            case GrinderColor.Blue: 
                color = gameData.colorMaterials[2].color;
                break;
            case GrinderColor.Yellow:
                color = gameData.colorMaterials[3].color; 
                break;
        }

        grinderRenderer.material.color = color;
    }

    public void SetDirectionandRayPoint(Vector3 _direction, Vector3 _rayStartOffset)
    {
        direction = _direction;
        rayStartOffset = _rayStartOffset;
    }

    public void SetSize(GrinderType _grinderType, int _size)
    {
        grinderType = _grinderType;
        size = _size;
    }

    public void CheckGrinders()
    {
        RaycastHit hit;
        if (Physics.Raycast( transform.position + rayStartOffset,-direction.normalized, out hit, 1f))
        {
            if (hit.collider.CompareTag("Block"))
            {
                BlockInstanceManager bim = hit.collider.GetComponentInParent<BlockInstanceManager>();
                MeshRenderer blockRenderer = bim.blockPrefab.GetComponentInChildren<MeshRenderer>();
                
                if (blockRenderer.material.color == grinderRenderer.material.color)
                {
                    if (grinderType  == GrinderType.Horizontal)
                    {
                        if (bim.blockInstance.blockData.width <= size)
                        {
                            ClearShapeCellsAndDestroy(bim);
                        }
                    }
                    else if (grinderType == GrinderType.Vertical)
                    {
                        if (bim.blockInstance.blockData.height <= size)
                        {
                            ClearShapeCellsAndDestroy(bim);
                        }
                    }
                }
            }
        }
    }

    public void ClearShapeCellsAndDestroy(BlockInstanceManager _blockInstanceManager)
    {
        //grid temizle
        Vector3Int rootCell = BoardManager.Instance.grid.WorldToCell(_blockInstanceManager.transform.position);
        List<Vector2Int> shapeOffsets = _blockInstanceManager.blockInstance.blockData.shapeOffsets;
        BoardManager.Instance.RemoveShapeFromGrid(rootCell, shapeOffsets);
        
        //obje destroy
        BoardManager.Instance.spawnedBlocks.Remove(_blockInstanceManager);
        GameEventSystem.Instance.BlockDestroyedEvent.Invoke(transform.position + new Vector3(0,1f, 0), grinderRenderer.material.color);
        Destroy(_blockInstanceManager.gameObject);
        BoardManager.Instance.remainingBlocks--;
        
        //buzları azalt
        List<BlockInstanceManager> icedBlocks = BoardManager.Instance.GetIcedBlocks();
        foreach (var icedBlock in icedBlocks)
        {
            icedBlock.blockInstance.iceCount--;
            icedBlock.RefreshIceText();
            if (icedBlock.blockInstance.iceCount <= 0)
            {
                icedBlock.blockInstance.blockType = BlockType.Normal;
                icedBlock.SetBlockVisuals();
            }

            GameEventSystem.Instance.IceBreakEvent.Invoke(icedBlock.transform.position + new Vector3(0,1f, 0));
        }
        
        
    }
    
    private void OnDrawGizmos()
    {
        // Ray başlangıç noktası
        Vector3 start = transform.position + rayStartOffset;
        // Ray yönü
        Vector3 dir = -direction.normalized;

        // Ray çizimi
        Gizmos.color = Color.red;
        Gizmos.DrawLine(start, start + dir * 1f); // 1f uzunluk
        
    }
}

