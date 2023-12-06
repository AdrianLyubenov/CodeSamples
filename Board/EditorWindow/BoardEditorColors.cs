using UnityEngine;

public static class BoardEditorColors
{
    public static Color editTileHoveredColor = new Color(0.97f,1f,0.58f, 1f);
    public static Color editTileNormalColor = new Color(0.58f,0.98f,1f, 1f);
    public static Color editTileSelectedColor = new Color(1f,0.98f,0.17f, 1f);
    public static Color editTileNonGameplayColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    
    public static Color addTileHoveredColor = Color.green;
    public static Color addTileNormalColor = new Color(0.6f, 1.0f, 0.6f, 1f);
    
    public static Color removeTileHoveredColor = Color.red;
    public static Color removeTileNormalColor =  new Color(1.0f, 0.6f, 0.6f, 1f);
}

public static class ShiftingPiecesEditorColors
{
    public static Color hoveredTileColor = new Color(1.0f, 1.0f, 1.0f, 1f);
    public static Color selectableTileColor = new Color(.8f, .8f, 0.8f, 1f);
    public static Color selectedTileColor = new Color(0.6f, 1.0f, 0.6f, 1f);
    
}