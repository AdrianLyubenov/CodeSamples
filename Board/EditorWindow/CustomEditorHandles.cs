#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CustomEditorHandles
{
    private static int _tileSelectedControl = 0;

    private static Dictionary<Vector3, int> _tileControlIds = new Dictionary<Vector3, int>();

    #region Helper Methods
    
    public static void DrawNeighbourLink(Vector3 wPos1, Vector3 wPos2)
    {
        var tp = HandleUtility.WorldToGUIPoint(wPos1);
        var np = HandleUtility.WorldToGUIPoint(wPos2);

        var shortTp = Vector2.Lerp(tp, np, 0.25f);
        var shortNp = Vector2.Lerp(tp, np, 0.75f);
        Handles.DrawLine(shortTp,shortNp);
    }
    
    #endregion
    
    #region Buttons
    
    public static ButtonResult TileAddButton(Vector3 worldPos, Vector3Int gridPos, Board board, float size)
    {
        var guiPos = HandleUtility.WorldToGUIPoint(worldPos);
        return TileSelectButton(
            worldPos,
            drawHover: () =>
            {
                Handles.color = BoardEditorColors.addTileHoveredColor; 
                Handles.DrawSolidDisc(guiPos, Vector3.back, size * 1.5f);
                Handles.DrawWireDisc(guiPos, Vector3.back, size * 1.7f);
                Handles.DrawWireDisc(guiPos, Vector3.back, size * 2.0f);
                foreach (var neighbour in Utilities.HexGrid.GetNeighbourPositions(gridPos)
                    .Where(n => board.tiles.Any(t => t.GridPosition == n)))
                {
                    var neighbourWorldPos = board.Grid.CellToWorld(neighbour);
                    DrawNeighbourLink(neighbourWorldPos, worldPos);
                }
            },
            drawNormal: () =>
            {
                Handles.color = BoardEditorColors.addTileNormalColor;
                Handles.DrawSolidDisc(guiPos, Vector3.back, size);
            }
        );
    }
    
    public static ButtonResult TileDeleteButton(Vector3 worldPos, float size)
    {
        var guiPos = HandleUtility.WorldToGUIPoint(worldPos);
        return TileSelectButton(
            worldPos,
            drawHover: () =>
            {
                Handles.color = BoardEditorColors.removeTileHoveredColor;
                Handles.DrawSolidDisc(guiPos, Vector3.back, size * 1.5f);
                Handles.DrawWireDisc(guiPos, Vector3.back, size * 1.7f);
                Handles.DrawWireDisc(guiPos, Vector3.back, size * 2.0f);
            },
            drawNormal: () =>
            {
                Handles.color = BoardEditorColors.removeTileNormalColor;
                Handles.DrawSolidDisc(guiPos, Vector3.back, size);
            }
        );
    }

    
    public static ButtonResult TileEditButton(Vector3 worldPos, float size, bool isGameplay, bool isSelected)
    {
        var guiPos = HandleUtility.WorldToGUIPoint(worldPos);
        return TileSelectButton(
            worldPos,
            drawHover: () =>
            {
                Handles.color = BoardEditorColors.editTileHoveredColor;
                Handles.DrawSolidDisc(guiPos, Vector3.back, size * 1.5f);
            },
            drawNormal: () =>
            {
                if (isSelected)
                {
                    Handles.color = BoardEditorColors.editTileSelectedColor;
                    Handles.DrawSolidDisc(guiPos, Vector3.back, size * 1.2f);
                    Handles.DrawWireDisc(guiPos, Vector3.back, size * 1.5f);
                    Handles.DrawWireDisc(guiPos, Vector3.back, size * 1.7f);
                }
                else
                {
                    Handles.color = isGameplay ? BoardEditorColors.editTileNormalColor : BoardEditorColors.editTileNonGameplayColor;
                    Handles.DrawSolidDisc(guiPos, Vector3.back, size);
                }
            },
            drawSelected: () =>
            {
                Handles.color = BoardEditorColors.editTileSelectedColor;
                Handles.DrawSolidDisc(guiPos, Vector3.back, size * 1.5f);
                Handles.DrawWireDisc(guiPos, Vector3.back, size * 1.7f);
                Handles.DrawWireDisc(guiPos, Vector3.back, size * 2.0f);
            }
        );
    }

    public static void ZeroControlState()
    {
        _tileSelectedControl = 0;
        GUIUtility.hotControl = 0;
    }

    public static ButtonResult GenericDraggableButton(Vector3 worldPos, float size, bool isSelected)
    {
        var guiPos = HandleUtility.WorldToGUIPoint(worldPos);
        return TileDraggableButton(
            worldPos,
            size,
            drawHover: () =>
            {
                Handles.color = ShiftingPiecesEditorColors.hoveredTileColor;
                Handles.DrawSolidDisc(guiPos, Vector3.back, size * 1.5f);
            },
            drawNormal: () =>
            {
                if (isSelected)
                {
                    Handles.color = ShiftingPiecesEditorColors.selectedTileColor;
                    Handles.DrawSolidDisc(guiPos, Vector3.back, size * 1.2f);
                    Handles.DrawWireDisc(guiPos, Vector3.back, size * 1.5f);
                    Handles.DrawWireDisc(guiPos, Vector3.back, size * 1.7f);
                }
                else
                {
                    Handles.color = ShiftingPiecesEditorColors.selectableTileColor;
                    Handles.DrawSolidDisc(guiPos, Vector3.back, size);
                }
            },
            drawSelected: () =>
            {
                // Will never happen
            }
        );

    }

    public static ButtonResult TileDraggableButton(Vector3 worldPos, float size, Action drawNormal = null, Action drawHover = null, Action drawSelected = null)
    {
        int controlID = GUIUtility.GetControlID(FocusType.Passive); // Gets a new ControlID for the handle
        Vector3 screenPosition = Handles.matrix.MultiplyPoint(worldPos);
        ButtonResult result = ButtonResult.Nothing;
        Vector3 pos = HandleUtility.WorldToGUIPoint(worldPos);
        switch (Event.current.GetTypeForControl(controlID))
        {
            case EventType.Layout:
                HandleUtility.AddControl(
                    controlID,
                    HandleUtility.DistanceToCircle(screenPosition, 0.3f) // Determines mouse-over range
                );
                break;
            case EventType.MouseDown:
            case EventType.MouseDrag:
                HandleUtility.AddControl(
                    controlID,
                    HandleUtility.DistanceToCircle(screenPosition, 0.3f) // Determines mouse-over range
                );
                if (HandleUtility.nearestControl == controlID)
                {
                    GUIUtility.hotControl = controlID;
                    if (Event.current.button == 0)// left Click
                    {
                        result = ButtonResult.Select;
                        if (Event.current.alt)
                        {
                            result = ButtonResult.Unselect;
                        }
                        Event.current.Use();
                    }
                }
                break;
            case EventType.MouseUp:
                result = ButtonResult.StopDrag;
                break;
            case EventType.Repaint:
                if (HandleUtility.nearestControl == controlID) // This is the part that switches according to mouse over, and the rectangles act as the visual element of the button
                    drawHover?.Invoke();
                else
                    drawNormal?.Invoke();
                break;
        }
        return result;
    }

    private static ButtonResult TileSelectButton(Vector3 worldPos, Action drawNormal = null,  Action drawHover = null, Action drawSelected = null)
    {
        int controlID = GUIUtility.GetControlID(FocusType.Passive); // Gets a new ControlID for the handle
        Vector3 screenPosition = Handles.matrix.MultiplyPoint(worldPos);
        ButtonResult result = ButtonResult.Nothing;
        Vector3 pos = HandleUtility.WorldToGUIPoint(worldPos);
        switch (Event.current.GetTypeForControl(controlID))
        {
            case EventType.Layout:
            case EventType.MouseMove:
                HandleUtility.AddControl(
                    controlID,
                    HandleUtility.DistanceToCircle(screenPosition, 0.1f) // Determines mouse-over range
                );
                break;
            case EventType.MouseDown:
                if (HandleUtility.nearestControl == controlID && Event.current.button == 0) // Left Click
                {
                    GUIUtility.hotControl = controlID;
                    _tileSelectedControl = controlID;
                    if (Event.current.shift)
                    {
                        result = ButtonResult.MultiSelect;
                    }
                    else
                    {
                        result = ButtonResult.Select;
                    }
                    // Event.current.Use();
                }
                if (Event.current.button == 1) // Right Click
                {
                    _tileSelectedControl = 0; // clear tile
                    result = ButtonResult.Unselect;
                }
                break;
            case EventType.Repaint:
                if(HandleUtility.nearestControl == controlID) // This is the part that switches according to mouse over, and the rectangles act as the visual element of the button
                    drawHover?.Invoke();
                else if (controlID == _tileSelectedControl)
                    drawSelected?.Invoke();
                else
                    drawNormal?.Invoke();
                break;
        }
        return result;
    }
    #endregion
}
public enum ButtonResult
{
    Nothing = 0,
    Select = 1,
    Unselect = 2,
    MultiSelect = 3,
    StopDrag = 4
}
#endif