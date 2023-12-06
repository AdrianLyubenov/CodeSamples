#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;


// TODO : Add/Break Links functionality
public class BoardEditorWindow : OdinEditorWindow
{
    public BoardTile TileToSpawn
    {
        get
        {
            if (_tileToSpawn == null)
            {
                _tileToSpawn = _editedBoard.tiles.Any()
                    ? _editedBoard.tiles.First()
                    : DesignToolsContent.BasicTilePrefab();
            }

            return _tileToSpawn;
        }
        protected set => _tileToSpawn = value;
    }

    private BoardTile _tileToSpawn;

    private Board _editedBoard;

    private BoardTileset _workingTileset;
    private PieceSet _pieceSet;

    private BoardTile _addPresetTile;

    private GameObject _tempObj;

    private List<BoardTile> _multiSelection;

    private bool flagUpdateTilesetMatrix;
    
    
    #region Editor Initialization

    // Add menu named "My Window" to the Window menu
    [MenuItem("GameTools/BoardEditor")]
    public static void Init()
    {
        BoardEditorWindow window = (BoardEditorWindow) EditorWindow.GetWindow<BoardEditorWindow>("Board Editor");
        window.Show();
        window.OnSelectionChange();
    }

    private void SetDefaultTileset()
    {
        if(_workingTileset == null)
        {
            _workingTileset = AssetDatabase.LoadAssetAtPath<BoardTileset>("Assets/Editor/BoardEditor/Tilesets/TestTileset.asset");
        }
    }

    protected override void OnEnable()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        SceneView.duringSceneGui += HandleSceneView;
        _editedBoard = null;
        TileToSpawn = null;
        _tempObj = new GameObject();
        _tempObj.hideFlags = HideFlags.HideAndDontSave;
        _addPresetTile = _tempObj.AddComponent<BoardTile>();
        _addPresetTile.gameObject.name = "Create Preset";
        _multiSelection = new List<BoardTile>();
        this._editorMode = BoardEditorMode.Idle;
        if (_workingTileset != null)
        {
            TilesetUpdateMatrix();
        }
        AutoSelectBoard();
        SceneView.RepaintAll();
    }

    private void OnDisable()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        SceneView.duringSceneGui -= HandleSceneView;
        DestroyImmediate(_tempObj);
        _editedBoard = null;
        TileToSpawn = null;
        SelectTile(null);
        SceneView.RepaintAll();
    }

    #endregion

    #region State Management

    private enum BoardEditorMode
    {
        Idle,
        AddOrRemove,
        Edit
    }

    private bool IsEditMode => _editorMode == BoardEditorMode.Edit;
    private bool IsAddOrRemoveMode => _editorMode == BoardEditorMode.AddOrRemove;

    private BoardEditorMode _editorMode;

    private void SetEditorMode(BoardEditorMode editorMode)
    {
        this._editorMode = editorMode;
        _editedTile = null;
        SceneView.RepaintAll();
        Repaint();
    }

    #endregion

    #region ODIN Editor Fields

    [ShowInInspector]
    [ShowIf("EditModeCond")]
    private BoardTile _editedTile;
    
    [BoxGroup("Select a Tile")]
    [TableMatrix(ResizableColumns = false,Transpose = true, HideColumnIndices = true, HideRowIndices = true, DrawElementMethod = "DrawBoardTileGridElement" , SquareCells = true)]
    [ShowIf("TileSelectionCond")]
    public BoardTile[,] tilesetMatrix;

    [BoxGroup("Select a Tile")]
    [TableMatrix(ResizableColumns = false,Transpose = true, HideColumnIndices = true, HideRowIndices = true, DrawElementMethod = "DrawBoardTileGridElement" , SquareCells = true)]
    [ShowIf("EditSelectionCond")]
    public BoardTile[,] editTilesetMatrix;
    
    [BoxGroup("Select a Piece")]
    [TableMatrix(ResizableColumns = false,Transpose = true, HideColumnIndices = true, HideRowIndices = true, DrawElementMethod = "DrawPieceTileGridElement" , SquareCells = true)]
    [ShowIf("PieceSelectionCond")]
    public Piece[,] pieceSetMatrix;
    
    
    [DisableIf("IsAddOrRemoveMode")]
    [ShowIf("BoardSelectedCond")]
    [HorizontalGroup("Split", 0.5f)]
    [Button(ButtonSizes.Large), GUIColor(1.0f, 0.6f, 1)]
    private void ToggleAddOrRemove()
    {
        SetEditorMode(BoardEditorMode.AddOrRemove);
    }

    [DisableIf("IsEditMode")]
    [ShowIf("BoardSelectedCond")]
    [HorizontalGroup("Split", 0.5f)]
    [Button(ButtonSizes.Large), GUIColor(1.0f, 1.0f, 0.4f)]
    private void ToggleEdit()
    {
        SetEditorMode(BoardEditorMode.Edit);
    }
    
    [Button(ButtonSizes.Large), GUIColor(0.80f, 1.0f, 0.6f)]
    [ShowIf("SelectBoardCond")]
    private void SelectBoard()
    {
        Selection.activeGameObject = GameObject.FindObjectOfType<Board>().gameObject;
        OnSelectionChange();
        Debug.Log("Selecting Board");
    }
    
    [Button(ButtonSizes.Large), GUIColor(0.50f, 1.0f, 0.5f)]
    [ShowIf("CreateBoardCond")]
    private void CreateBoard()
    {
        var boardPrefab = DesignToolsContent.BasicBoardPrefab();
        var boardInstance = GameObject.Instantiate(boardPrefab);
        Selection.activeGameObject = boardInstance.gameObject;
        OnSelectionChange();
        Debug.Log("Creating Board");
    }

    #region Odin Reflection Functions
    private bool PieceSelectionCond => _editorMode == BoardEditorMode.Edit && _pieceSet != null && pieceSetMatrix != null;
    private bool TileSelectionCond => _editorMode == BoardEditorMode.AddOrRemove && _workingTileset != null && tilesetMatrix != null;
    private bool EditSelectionCond => _editorMode == BoardEditorMode.Edit && _workingTileset != null && editTilesetMatrix != null;
    private bool BoardSelectedCond => _editedBoard != null;
    private bool EditModeCond => this._editorMode.Equals(BoardEditorMode.Edit);
    
    private bool SelectBoardCond()
    {
        var cond = _editedBoard == null;
        var anyBoard = GameObject.FindObjectOfType<Board>();
        cond &= anyBoard != null;
        return cond;
    }
    
    private bool CreateBoardCond()
    {
        var cond = _editedBoard == null;
        var anyBoard = GameObject.FindObjectOfType<Board>();
        cond &= anyBoard == null;
        return cond;
    }

    private Texture2D GetPresetAddTexture()
    {
        return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/BoardEditor/Textures/addtex.png");
    }


    private void CreateNewTilePreset()
    {
        throw new NotImplementedException();
        // if (this._workingTileset.Values.data.Contains(_editedTile)) return; // no duplicates
        //
        // var tilesetPath = AssetDatabase.GetAssetPath(this._workingTileset);
        // var dir = Path.GetDirectoryName(tilesetPath);
        // var subDirName = (_workingTileset.name + "-tiles");
        // var tilesetTileFolderPath = dir + Path.DirectorySeparatorChar + subDirName;
        // var fileName = tilesetTileFolderPath + Path.DirectorySeparatorChar + _editedTile.name;
        // var fullAssetPath = fileName + ".prefab";
        // if (!AssetDatabase.IsValidFolder(tilesetTileFolderPath))
        // {
        //     AssetDatabase.CreateFolder(dir, subDirName);
        // }
        // var existingAsset = AssetDatabase.LoadAssetAtPath<BoardTile>(fullAssetPath);
        // if(existingAsset != null)
        // {
        //     fullAssetPath = fileName + RandomExtensions.GetRandomLowercaseString(8) + ".prefab";
        // }
        // //AssetDatabase.CreateAsset(_editedTile.gameObject, fullAssetPath);
        // PrefabUtility.SaveAsPrefabAssetAndConnect(_editedTile.gameObject, fullAssetPath, InteractionMode.UserAction);
        // var asset = AssetDatabase.LoadAssetAtPath<BoardTile>(fullAssetPath);
        // _workingTileset.data.Add(asset);
        // asset.GridPosition = new Vector3Int(0, 0, 0);
        // asset.transform.position = Vector3.zero;
        // var guid = AssetDatabase.GUIDFromAssetPath(fullAssetPath);
        // var tilesetGuid = AssetDatabase.GUIDFromAssetPath(tilesetPath);
        // AssetDatabase.SaveAssetIfDirty(guid);
        // AssetDatabase.SaveAssetIfDirty(tilesetGuid);
        // flagUpdateTilesetMatrix = true;
    }

    private void ReplaceSelectionWith(BoardTile tile)
    {
        List<BoardTile> newMultiSelection = new List<BoardTile>();
        foreach (var item in _multiSelection) 
        {
            //first = GameObject.Instantiate(tile.gameObject, item.transform.position, item.transform.rotation, item.transform.parent);
            var parent = item.transform.parent;
            var position = item.transform.position;
            var roatation = item.transform.rotation;
            var gp = item.GridPosition;
            BoardRemoveTile(item);
            var newTile = PrefabUtility.InstantiatePrefab(tile.gameObject, parent);
            var go = (GameObject) newTile;
            go.transform.position = position;
            go.transform.rotation = roatation;
            var newBt =  go.GetComponent<BoardTile>();
            newBt.GridPosition = gp;
            newMultiSelection.Add(newBt);
        }
        _multiSelection.Clear();
        _multiSelection.AddRange(newMultiSelection);
        _editedTile = _multiSelection.First();
        Selection.activeObject = _editedTile.gameObject;
        CustomEditorHandles.ZeroControlState();
        BoardUpdate();
    }
    
    private Piece DrawPieceTileGridElement(Rect rect, Piece value)
    {
        if (value != null)
        {
            var tex = value == _addPresetTile ? GetPresetAddTexture(): AssetPreview.GetAssetPreview(value.gameObject);
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                if (_editorMode == BoardEditorMode.Edit)
                {
                    _editedTile.SetPiece(value);
                }
                GUI.changed = true;
                Event.current.Use();
            }

            if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rect, Color.gray);
            }

            //Border
            if (value == _tileToSpawn)
            {
                EditorGUI.DrawRect(rect, new Color(0.4f, 0.7f, 1.0f, 0.6f));
            }
            //Texture
            EditorGUI.DrawPreviewTexture(rect.Padding(rect.size.magnitude/15), tex);
            
            //Name
            var nameRect = rect.Padding(2).AlignBottom(16);
            GUI.Label(nameRect, value.gameObject.name, SirenixGUIStyles.CenteredWhiteMiniLabel);
        }
        return value;
    }
    private BoardTile DrawBoardTileGridElement(Rect rect, BoardTile value)
    {
        if (value != null)
        {
            var tex = value == _addPresetTile ? GetPresetAddTexture(): AssetPreview.GetAssetPreview(value.gameObject);
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                // Selection.activeGameObject = value.gameObject;
                if (value == _addPresetTile)
                {
                    CreateNewTilePreset();
                } else {
                    if(_editorMode == BoardEditorMode.AddOrRemove)
                    {
                        _tileToSpawn = value;
                    }
                    else if (_editorMode == BoardEditorMode.Edit)
                    {
                        ReplaceSelectionWith(value);
                    }
                }
                GUI.changed = true;
                Event.current.Use();
            }
            if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rect, Color.gray);
            }

            //Border
            if (value == _tileToSpawn)
            {
                EditorGUI.DrawRect(rect, new Color(0.4f, 0.7f, 1.0f, 0.6f));
            }
            //Texture
            EditorGUI.DrawPreviewTexture(rect.Padding(rect.size.magnitude/15), tex);
            
            //Name
            var nameRect = rect.Padding(2).AlignBottom(16);
            GUI.Label(nameRect, value.gameObject.name, SirenixGUIStyles.CenteredWhiteMiniLabel);
        }
        return value;
    }
    #endregion


    #endregion

    #region Active Selection Handling

    private void OnSelectionChange()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        if (Selection.activeGameObject == null)
            return;
        AutoSelectBoard();
        //TilesetUpdateMatrix();
        SceneView.RepaintAll();
        this.Repaint();
    }

    private void AutoSelectBoard()
    {
        var activeGo = Selection.activeGameObject;
        if (activeGo == null) { return; }
        Board newEditedBoard;
        activeGo.TryGetComponentInParents(out newEditedBoard);

        if (newEditedBoard != _editedBoard)
        {
            _editedBoard = newEditedBoard;
            SelectTile(null);
        }


        if (_editedBoard != null)
        {
            BoardUpdateGrid();
            BoardUpdateTiles();
            SceneView.RepaintAll();
        }
    }

    #endregion


    #region Scene View Editing

    public void HandleSceneView(SceneView scene)
    {
        Handles.BeginGUI();
        Handles.color = new Color(0.4f, 0.8f, 1.0f, 1.0f);
        HandleUtility.AddDefaultControl(0);
        if (_editedBoard != null)
        {
            if (IsEditMode)
            {
                DrawNeighbourLinks();
                DrawEditHandles(scene);
            }
            else if (IsAddOrRemoveMode)
            {
                DrawTileAddOrDeleteHandles(scene);
            }
        }

        Handles.EndGUI();
    }

    private List<Vector3Int> validExpansionPositions = new List<Vector3Int>();

    private bool expansionPositionsDirty = false;
    private void UpdateExpansionPositions()
    {
        validExpansionPositions = _editedBoard.tiles
            .SelectMany(t => Utilities.HexGrid.GetNeighbourPositions(t.GridPosition))
            .Where(pos => !_editedBoard.tiles.Any(tile => tile.GridPosition == pos))
            .ToList();
        expansionPositionsDirty = false;
    }

    private void DrawTileAddOrDeleteHandles(SceneView scene)
    {

        if (expansionPositionsDirty)
            UpdateExpansionPositions();

        foreach (var gPos in validExpansionPositions)
        {
            var worldPos = _editedBoard.Grid.CellToWorld(gPos);
            var size = 50f / scene.size;
            var result = CustomEditorHandles.TileAddButton(worldPos, gPos, _editedBoard, size);
            if (result == ButtonResult.Select)
            {
                BoardAddTile(TileToSpawn, gPos);
                //SceneView.RepaintAll();
            }
        }

        foreach (var tile in _editedBoard.tiles.ToList())
        {
            var worldPos = tile.transform.position;
            var size = 30f / scene.size;
            var result = CustomEditorHandles.TileDeleteButton(worldPos, size);
            if (result == ButtonResult.Select)
            {
                BoardRemoveTile(tile);
                //SceneView.RepaintAll();
            }
        }
    }

    private void DrawEditHandles(SceneView scene)
    {
        foreach (var tile in _editedBoard.tiles)
        {
            var size = 50 / scene.size;
            var btnResult = CustomEditorHandles.TileEditButton(tile.transform.position, size, tile.isGameplayTile, _multiSelection.Contains(tile));
            switch (btnResult)
            {
                case ButtonResult.Nothing:
                    break;
                case ButtonResult.Select:
                    SelectTile(tile,false);
                    //SceneView.RepaintAll();
                    Repaint();
                    break;
                case ButtonResult.MultiSelect:
                    if (_multiSelection.Contains(tile))
                    {
                        UnselectTile(tile);
                    }
                    else
                    {
                        SelectTile(tile, true);
                    }
                    Repaint();
                    break;
                case ButtonResult.Unselect:
                    SelectTile(null);
                    //SceneView.RepaintAll();
                    Repaint();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Allow Movement of Tile;
        if (_editedTile != null)
        {
            var tf = _editedTile.transform;
            var pos = Handles.FreeMoveHandle(tf.position, tf.rotation, 20f,
                _editedBoard.Grid.cellSize, Handles.ArrowHandleCap);
            if(Vector3.Distance(pos, posActiveMoveHandleLast) > 0.05f)
            {
                BoardMoveTile(_editedTile, pos);
            }
            posActiveMoveHandleLast = pos;
        }
    }
    Vector3 posActiveMoveHandleLast = Vector3.zero;

    private void DrawNeighbourLinks()
    {
        foreach (var tile in _editedBoard.tiles)
        {

            if (!tile.isGameplayTile) continue;
            foreach (var neighbour in tile.Neighbours)
            {
                if (!neighbour.isGameplayTile) continue;
                Handles.color = _editedTile == tile 
                    || _editedTile == neighbour 
                    || _multiSelection.Contains(neighbour)
                    || _multiSelection.Contains(tile) ? Color.white : Color.green;
                CustomEditorHandles.DrawNeighbourLink(tile.transform.position, neighbour.transform.position);
            }
        }
    }

    #endregion

    #region Editor OnGUI

    private Vector2 _scrollViewPos;
    
    private UnityEditor.Editor _tileEditor;
    
    private void UnselectTile(BoardTile tile)
    {
        _multiSelection.Remove(tile);
        if (_editedTile == tile)
        {
            if (_multiSelection.Any())
            {
                SelectTile(_multiSelection.First());
            }
            else
            {
                SelectTile(null);
            }
        }
        else
        {
            Selection.SetActiveObjectWithContext(_editedTile, _editedBoard);
            Selection.selectionChanged.Invoke();
            EditorGUIUtility.PingObject(_editedTile);
            EditorApplication.RepaintHierarchyWindow();
        }
    }

    private void SelectTile(BoardTile tile, bool multiple = false)
    {
        if (_editedTile != null)
        {
            if (!multiple)
            {
                _editedTile = tile;
            }
        }
        else
        {
            _editedTile = tile;
        }
        if (_editedTile != null)
        {
            var editor = OdinEditor.CreateEditor(_editedTile);
            _tileEditor = editor;

            // Handle Highlighting of Tileset
            var prefabParent = PrefabUtility.GetCorrespondingObjectFromSource(_editedTile);
            if(prefabParent != null)
            {
                _tileToSpawn = prefabParent;
            }
            else
            {
                _tileToSpawn = null;
            }
        }
        else
        {
            _tileEditor = null;
        }

        if (!multiple) { _multiSelection.Clear(); }
        if (!_multiSelection.Contains(tile))
        {
            this._multiSelection.Add(tile);
        }
        
        Selection.SetActiveObjectWithContext(_editedTile, _editedBoard);
        Selection.selectionChanged.Invoke();
        EditorGUIUtility.PingObject(_editedTile);
        EditorApplication.RepaintHierarchyWindow();
        BoardUpdate();
    }
    
    protected override void OnGUI()
    {
        SirenixEditorGUI.InfoMessageBox("While this window is open, only scene objects with Tiles can be selected in the Scene View!");
        GUILayout.Label("Board Settings", EditorStyles.boldLabel);
        EditorGUILayout.ObjectField("Board", _editedBoard, typeof(Board), true, GUILayoutOptions.EmptyGUIOptions);
        if (flagUpdateTilesetMatrix)
        {
            flagUpdateTilesetMatrix = false;
            TilesetUpdateMatrix();
        }
        if (_editedBoard != null)
        {
            var newTileset = (BoardTileset) EditorGUILayout.ObjectField("Working Tileset", _workingTileset, typeof(BoardTileset));
            if (!Equals(newTileset, _workingTileset))
            {
                _workingTileset = newTileset;
                TilesetUpdateMatrix();
            }
            var newPieceSet = (PieceSet) EditorGUILayout.ObjectField("Piece Set", _pieceSet, typeof(PieceSet));
            if (!Equals(newPieceSet, _pieceSet))
            {
                _pieceSet = newPieceSet;
                PiecesetUpdateMatrix();
            }
            if (_tileEditor != null && _editedTile != null)
            {
                OdinEditor.DrawFoldoutInspector(_editedTile, ref _tileEditor);
            }
        }
        base.OnGUI(); // Buttons
    }
    
    //Used via reflection by Odin Editor
    #endregion

    #region Data Editing

    private void PiecesetUpdateMatrix()
    {
        int step = 5;
        int upper = (int) Math.Ceiling(_pieceSet.Values.Count / (double) step);
        pieceSetMatrix = new Piece[upper, step];
        for (int i = 0; i < upper; i++)
        {
            for (int j = 0; j < step; j++)
            {
                var linearIndex = (i * step) + j;
                if (_pieceSet.Values.Count <= linearIndex)
                {
                    Debug.Log($"Qutting normal at {linearIndex}");
                    break;
                }
                pieceSetMatrix[i, j] = _pieceSet.Values[linearIndex].value.editorAsset.GetComponent<Piece>();
            }
        }
        GUIHelper.RequestRepaint();
        Repaint();
    }

    private void TilesetUpdateMatrix()
    {
       //Debug.Log($"Tileset items count = {_workingTileset.data.Count}");
        int step = 5;
        int upper = (int) Math.Ceiling(_workingTileset.Values.Count / (double) step);
        tilesetMatrix = new BoardTile[upper, step];
        for (int i = 0; i < upper; i++)
        {
            for (int j = 0; j < step; j++)
            {
                var linearIndex = (i * step) + j;
                if (_workingTileset.Values.Count <= linearIndex)
                {
                    Debug.Log($"Qutting normal at {linearIndex}");
                    break;
                }
                tilesetMatrix[i, j] = _workingTileset.Values[linearIndex].value.editorAsset.GetComponent<BoardTile>();
            }
        }

        int upperEdit = (int) Math.Ceiling((_workingTileset.Values.Count+1) / (double) step);
        editTilesetMatrix = new BoardTile[upperEdit, step];
        for (int i = 0; i < upperEdit; i++)
        {
            for (int j = 0; j < step; j++)
            {
                var linearIndex = (i * step) + j;
                if (_workingTileset.Values.Count == linearIndex)
                {
                    Debug.Log($"Qutting extra at {linearIndex}");
                    editTilesetMatrix[i, j] = _addPresetTile;
                    break;
                }
                else
                {
                    editTilesetMatrix[i, j] = _workingTileset.Values[linearIndex].value.editorAsset.GetComponent<BoardTile>();
                }
            }
        }
        GUIHelper.RequestRepaint();
        Repaint();
    }

    private void BoardAddTile(BoardTile tileToSpawn, Vector3Int gridPosition)
    {
        if(_editedBoard == null) return;
        var newTile = (GameObject)PrefabUtility.InstantiatePrefab(tileToSpawn.gameObject, _editedBoard.tilesParent.transform);
        newTile.transform.position = _editedBoard.Grid.GetCellCenterWorld(gridPosition);
        newTile.GetComponent<BoardTile>().GridPosition = gridPosition;
        BoardUpdate();
    }

    private void BoardUpdateActiveCollection()
    {

    }

    private void BoardRemoveTile(BoardTile editedTile)
    {
        _editedBoard.tiles.Remove(editedTile);
        _editedBoard.tiles.ForEach((t) =>
        {
            if (t.Neighbours.Contains(editedTile))
            {
                t.Neighbours.Remove(editedTile);
            }
        });
        DestroyImmediate(editedTile.gameObject);
        BoardUpdate();
    }

    private void BoardMoveTile(BoardTile tile, Vector3 worldPos)
    {
        tile.transform.position = worldPos;
        BoardUpdate();
    }
    
    private void BoardUpdate()
    {
        expansionPositionsDirty = true;
        BoardUpdateGrid();
        BoardUpdateTiles();
    }

    private void BoardUpdateGrid()
    {
        if (_editedBoard != null)
        {
            if (_editedBoard.gridReferenceTile != null)
            {
                var grid = _editedBoard.Grid;

                var tileRenderingSize = _editedBoard.gridReferenceTile.GetRenderingBounds();
                var newGridSize = new Vector3(tileRenderingSize.size.y, tileRenderingSize.size.x, 1);

                if (Vector3.Distance(_editedBoard.Grid.cellSize, newGridSize) > 0.0001f)
                {
                    _editedBoard.Grid.cellSize = newGridSize;
                }
            }
        }
    }

    private void BoardUpdateTiles()
    {
        if (_editedBoard == null) return;

        var foundTiles = _editedBoard.transform.GetComponentsInChildren<BoardTile>();
        var newTiles = foundTiles.Except(_editedBoard.tiles).ToList();
        var removedTiles = _editedBoard.tiles.Except(foundTiles).ToList();

        foreach (var toRemove in removedTiles)
        {
            if (_editedBoard.tiles.Contains(toRemove))
            {
                _editedBoard.tiles.Remove(toRemove);
            }
        }

        foreach (var newTile in newTiles)
        {
            if (!_editedBoard.tiles.Contains(newTile))
            {
                _editedBoard.tiles.Add(newTile);
            }
        }

        foreach (var tile in _editedBoard.tiles)
        {
            tile.SetBoard(_editedBoard);
            tile.SetGrid(_editedBoard.Grid);
        }


        BoardPositionTiles();
        BoardSetTileNeighbours();
    }

    private void BoardPositionTiles()
    {
        foreach (var tile in _editedBoard.tiles)
        {
            tile.SetBoard(_editedBoard);
            tile.SetGrid(_editedBoard.Grid);
            // Debug.Log("Positioning tile");
            tile.SetGridPosition(_editedBoard.Grid.WorldToCell(tile.transform.position));
            if (_editedBoard.tiles.Any(other => other.GridPosition == tile.GridPosition && other != tile))
            {
                BoardPositionTile(tile);
            }
            else
            {
                tile.transform.position = _editedBoard.Grid.GetCellCenterWorld(tile.GridPosition);
            }
        }
    }

    private void BoardPositionTile(BoardTile tile)
    {
        var startPos = tile.GridPosition;
        if (Utilities.Pathfinding.PathfindBFS(startPos
            , _editedBoard.CheckPositionIsOccupied
            , _editedBoard.CheckPositionIsNotOccupied
            , _editedBoard.NeighboursForPosition
            , out List<Vector3Int> result)
        )
        {
            for (int i = 0; i < result.Count - 1; i++)
            {
                var item = result[i];
                var item2 = result[i + 1];
                Debug.DrawLine(_editedBoard.Grid.GetCellCenterWorld(item), _editedBoard.Grid.GetCellCenterWorld(item2),
                    Color.red, 0.5f);
            }

            var newPos = result.Last();
            tile.SetGridPosition(newPos);
            tile.UpdateWorldPosition();
            // tile.transform.position = grid.GetCellCenterWorld(tile.GridPosition);
        }
    }

    private void BoardSetTileNeighbours()
    {
        foreach (var tile in _editedBoard.tiles)
        {
            var foundNeighbours = _editedBoard.tiles
                .Where(bt => Utilities.HexGrid.GetNeighbourPositions(tile.GridPosition).Contains(bt.GridPosition))
                .ToList();
            tile.SetNeighbours(foundNeighbours);
            // Debug.Log(foundNeighbours.Count);
        }
    }

    #endregion
}

#endif