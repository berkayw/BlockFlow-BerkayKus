using System;
using UnityEngine;
using UnityEngine.Events;

public class BlockController : MonoBehaviour
{
    private Grid grid;
    private Vector3 startPos;
    private Vector3 dragOffset;
    public float hoverHeight = 0.2f;
    
    void Start()
    {
        grid = FindObjectOfType<Grid>();
    }

    void OnMouseDown()
    {
        
        Cursor.visible = false;
        startPos = transform.parent.position;
        
        Vector3 mouseWorld = GetMouseWorldPosition();
        dragOffset = transform.parent.position - new Vector3(mouseWorld.x, transform.parent.position.y, mouseWorld.z); //tıklanan yer ile arasındaki offset
    }

    private void OnMouseDrag()
    {
        Vector3 worldMouse = GetMouseWorldPosition();
        Vector3 targetPos = new Vector3(worldMouse.x + dragOffset.x, hoverHeight, worldMouse.z + dragOffset.z);
        transform.parent.position = targetPos; 

    }
    void OnMouseUp()
    {
        Cursor.visible = true;        

        Vector3Int cellPos = grid.WorldToCell(transform.parent.position);
        Vector3 snapPos = grid.GetCellCenterWorld(cellPos);

        snapPos.y = 0;

        if (IsValidPlacement(cellPos))
        {
            transform.parent.position = snapPos;
            GameEventSystem.instance.BlockPlacedEvent.Invoke();
        }
        else
            transform.parent.position = startPos;
    }
 
    
    public Vector3 GetMouseWorldPosition()
    {
        Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y,
            Camera.main.WorldToScreenPoint(transform.parent.position).z);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(position);

        return worldPosition;
    }
    
    //grid doluluk kontrolu eklicem
    bool IsValidPlacement(Vector3Int cellPos)
    {
        return true;
    }

    
    
}