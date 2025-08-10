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
        startPos = transform.position;
        
        Vector3 mouseWorld = GetMouseWorldPosition();
        dragOffset = transform.position - new Vector3(mouseWorld.x, transform.position.y, mouseWorld.z); //tıklanan yer ile arasındaki offset
    }

    private void OnMouseDrag()
    {
        Vector3 worldMouse = GetMouseWorldPosition();
        Vector3 targetPos = new Vector3(worldMouse.x + dragOffset.x, hoverHeight, worldMouse.z + dragOffset.z);
        transform.position = targetPos; 

    }
    void OnMouseUp()
    {
        Cursor.visible = true;        

        Vector3Int cellPos = grid.WorldToCell(transform.position);
        Vector3 snapPos = grid.GetCellCenterWorld(cellPos);

        snapPos.y = 0;

        if (IsValidPlacement(cellPos))
        {
            transform.position = snapPos;
            GameEventSystem.instance.BlockPlacedEvent.Invoke();
        }
        else
            transform.position = startPos;
    }
 
    
    public Vector3 GetMouseWorldPosition()
    {
        Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y,
            Camera.main.WorldToScreenPoint(transform.position).z);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(position);

        return worldPosition;
    }
    
    //grid doluluk kontrolu eklicem
    bool IsValidPlacement(Vector3Int cellPos)
    {
        return true;
    }

    
    
}