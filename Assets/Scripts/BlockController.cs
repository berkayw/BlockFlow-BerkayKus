using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BlockController : MonoBehaviour
{
    private Grid grid;
    private Vector3 startPos;
    private Vector3 dragOffset;
    public float hoverHeight = 0.2f;
    public BlockInstanceManager _blockInstanceManager;
    
    public List<Vector2Int> shapeOffsets; // BlockDataSO’dan gelecek
    public List<Vector3Int> occupiedCells = new List<Vector3Int>(); 
    
    void Start()
    {
        grid = FindObjectOfType<Grid>();
        _blockInstanceManager = GetComponentInParent<BlockInstanceManager>();
        foreach (var shapeOffset in _blockInstanceManager.blockInstance.blockData.shapeOffsets)
        {
            shapeOffsets.Add(shapeOffset);
        }
        RegisterToGrid();
    }

    void OnMouseDown()
    {
        Cursor.visible = false;
        startPos = transform.parent.position;
        
        BoardManager.Instance.RemoveShapeFromGrid(grid.WorldToCell(startPos), shapeOffsets);
        
        Vector3 mouseWorld = GetMouseWorldPosition();
        dragOffset = transform.parent.position - new Vector3(mouseWorld.x, transform.parent.position.y, mouseWorld.z); //tıklanan yer ile blok merkezi arasındaki offset
    }

    private void OnMouseDrag()
    {
        Vector3 worldMouse = GetMouseWorldPosition();
        Vector3 targetPos = new Vector3(worldMouse.x + dragOffset.x, hoverHeight, worldMouse.z + dragOffset.z);

        Vector3Int currentCell = grid.WorldToCell(transform.parent.position);
        Vector3Int targetCell = grid.WorldToCell(targetPos);

        //gitmek istenen yon
        Vector3Int dir = targetCell - currentCell;

        if (dir.x != 0) dir.x = Math.Sign(dir.x);
        if (dir.z != 0) dir.z = Math.Sign(dir.z);
        dir.y = 0;
        
        //tek yonlu
        if (_blockInstanceManager.blockInstance.blockType == BlockType.OnlyHorizontal)
        {
            if (dir.z != 0) 
                return; // dikey hareketler iptal 
        }
        if (_blockInstanceManager.blockInstance.blockType == BlockType.OnlyVertical)
        {
            if (dir.x != 0) 
                return; // yatay hareketler iptal 
        }
        //buzlu
        if (_blockInstanceManager.blockInstance.blockType == BlockType.Iced && _blockInstanceManager.blockInstance.iceCount > 0)
        {
            return;
        }
        
        //gitmek istenen yer ile arada mesafe varsa
        if (dir != Vector3Int.zero)
        {
            if (CanMoveTo(currentCell, dir))
            {
                //devamli kontrol icin lerp ile -> cunku karsısına engel cıkabilir
                Vector3 nextPos = grid.GetCellCenterWorld(currentCell + dir) + Vector3.up * hoverHeight; 
                transform.parent.position = Vector3.Lerp(transform.parent.position, nextPos, 0.5f);
            }
        }
        else
        {
            transform.parent.position = targetPos; //free and smooth drag
            
        }
    }
    
    
    void OnMouseUp()
    {
        Cursor.visible = true;

        Vector3Int cellPos = grid.WorldToCell(transform.parent.position);

        if (BoardManager.Instance.IsShapePlacementValid(cellPos, shapeOffsets))
        {
            transform.parent.position = grid.GetCellCenterWorld(cellPos);
            RegisterToGrid();
        }
        else
        {
            transform.parent.position = startPos;
            RegisterToGrid();
        }
        
        GameEventSystem.Instance.BlockPlacedEvent.Invoke();
    }
    
    private bool CanMoveTo(Vector3Int currentCell, Vector3Int dir)
    {
        Vector3Int nextCell = currentCell + dir;

        if (!BoardManager.Instance.IsShapePlacementValid(nextCell, shapeOffsets, this))
            return false;

        //capraz gecis engelleme
        if (dir.x != 0 && dir.z != 0) 
        {
            //kose kontrol
            Vector3Int side1 = currentCell + new Vector3Int(dir.x, 0, 0);
            Vector3Int side2 = currentCell + new Vector3Int(0, 0, dir.z);

            if (!BoardManager.Instance.IsShapePlacementValid(side1, shapeOffsets, this))
                return false;
            if (!BoardManager.Instance.IsShapePlacementValid(side2, shapeOffsets, this))
                return false;
        }

        return true;
    }
    
    public Vector3 GetMouseWorldPosition()
    {
        Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y,
            Camera.main.WorldToScreenPoint(transform.parent.position).z);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(position);

        return worldPosition;
    }
    
    private void RegisterToGrid()
    {
        Vector3Int rootCell = grid.WorldToCell(transform.position);
        occupiedCells.Clear();

        foreach (var offset in shapeOffsets)
        {
            Vector3Int cell = rootCell + new Vector3Int(offset.x, 0, offset.y);
            occupiedCells.Add(cell);
        }

        BoardManager.Instance.AddShapeToGrid(rootCell, shapeOffsets);
    }
    
    
    
    
}