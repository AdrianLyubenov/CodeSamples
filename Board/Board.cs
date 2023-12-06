using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.AddressableAssets;
using System;
using System.Text;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets.Settings;
using UnityEditor;
using Sirenix.OdinInspector;
using Spine.Unity;
#endif

[RequireComponent(typeof(Grid))]
public class Board : MonoBehaviour
{
    public const int Min_Pieces_To_Merge = 3;

    public BoardTile testTile;

    [Space]
    public Tile gridReferenceTile;

    public GameObject tilesParent;

    public float mergeTime = 0.4f;

    public float mergeSequenceEndTime = 0.4f;

    public Grid Grid => GetComponent<Grid>();

    [SerializeField] [HideInInspector] public List<BoardTile> tiles = new List<BoardTile>();

    [SerializeField] private BoardData boardData;

    [HideInInspector]
    public List<MergeRuleset> _mergeRules;
    [HideInInspector]
    public List<MergeRule> _mergeRulesFlat;
    [HideInInspector]
    public List<string> _playablePieces;
    
    private Dictionary<Vector3Int, BoardTile> tilesMappedByGridPos;

    public Dictionary<Vector3Int, BoardTile> GetTilesByGridPosition()
    {
        if (tilesMappedByGridPos == null || tilesMappedByGridPos.Count != tiles.Count)
        {
            tilesMappedByGridPos = tiles.ToDictionary(x => x.GridPosition, y => y);
        }

        return tilesMappedByGridPos;
    }

    public bool UpdatePlayablePieces()
    {
        if (_mergeRulesFlat != null)
        {
            _playablePieces =
                _mergeRulesFlat.SelectMany(y => new string[] { y.requiredPieceId, y.resultPieceId }).Distinct()
                    .ToList();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetMergeData(List<MergeRuleset> rules)
    {
        this._mergeRules = rules;
        this._mergeRulesFlat = _mergeRules.SelectMany(mr => mr.mergeRules.ToList()).ToList();
    }

    public BoardTile GetRandomBoardTile()
    {
        if (tiles.Count > 0)
        {
            return tiles.Where(t => t.isGameplayTile).OrderBy(t => Random.Range(0, 100f)).First();
        }

        return null;
    }

    /// <summary>
    /// Checks whether there's neighbouring pieces, sorts them by type, 
    /// and calcualtes which type that is the most abundant & most likely to result in a merge.
    /// </summary>
    /// <returns>Suceeds if there's neighbouring pieces or fails if there arent. Outputs best guess as out parameter. </returns>
    public bool TryGetBestComformingPiece(BoardTile tile, out Piece bestPiece, bool mustMerge = true)
    {
        if (!tile.CanBeOccupied)
        {
            bestPiece = null;
            return false;
        }

        // First check, if there is a possible piece that will make a merge
        var neighbourPieces = tile.Neighbours.Select(x => x as BoardTile)
            .Where(x => x.isGameplayTile && x.pieceController != null && x.pieceController.masterPiece != null)
            .Select(x => x.pieceController.masterPiece).ToList();
        neighbourPieces = neighbourPieces.Where(x => GameManager.Instance.GetPiece(x.GetFullId()) != null).ToList();

        Dictionary<string,int> idOccurrences = new Dictionary<string, int>();
        
        foreach (var p in neighbourPieces)
        {
            if (mustMerge)
            {
                var ruleForTile = _mergeRulesFlat.FirstOrDefault(r => r.requiredPieceId == p.GetFullId());
                
                if (FindPotentialMerges(tile, p, out var mergeData))
                {
                    if (!idOccurrences.ContainsKey(p.GetFullId()) && ruleForTile != null)
                        idOccurrences.Add(p.GetFullId(), mergeData.Count);
                }
            }
            else
            {
                if (!idOccurrences.ContainsKey(p.GetFullId())) 
                {
                    idOccurrences.Add(p.GetFullId(), 1); 
                }
                else
                {
                    idOccurrences[p.GetFullId()]++; 
                }
            }
        }

        if (idOccurrences.Any())
        {
            var possibleMerges = idOccurrences.Select(
                x => (count: x.Value, piece: neighbourPieces.First(n => n.GetFullId() == x.Key)
            ));
            var result = possibleMerges
                .OrderByDescending(p => p.piece.level)
                .ThenBy(p => p.piece.GetFullId())
                .First();
            bestPiece = result.piece;
            return true;
        }

        bestPiece = null;
        return false;
    }

    public BoardTile GetRandomFreeBoardTile()
    {
        var res = tiles
            .Where(t => t.pieceController == null && t.isGameplayTile)
            .OrderBy(r => Random.Range(0,1f))
            .FirstOrDefault();
        if (res == null)
        {
            Debug.LogError("Could not find free board tile");
        }
        return res;
    }


#if UNITY_EDITOR
    [Button(ButtonSizes.Large, Name = "Sort Tiles"), GUIColor(0.5f, 0.5f, 1)]
#endif
    public void SortTiles()
    {
        tiles = tiles.OrderByDescending(x => x._gridPosition.y % 2 == 0 ? ((x._gridPosition.x * 2) - 1) : x._gridPosition.x * 2).ToList();
        
        for (int n = 0; n < tiles.Count; n++)
        {
            tiles[n].transform.SetAsLastSibling();
            SpriteRenderer[] Sprites = tiles[n].gameObject.GetComponentsInChildren<SpriteRenderer>();

            int baseLevel = 100;

            foreach(SpriteRenderer s in Sprites)
            {
                s.sortingOrder = baseLevel - (tiles[n].GridPosition.y % 2 == 0
                    ? ((tiles[n].GridPosition.x * 2) - 1)
                    : (tiles[n].GridPosition.x * 2));
                baseLevel -= 1;
            }
        }
    }
    
#if UNITY_EDITOR

    [Button(ButtonSizes.Large, Name = "Check grid position"), GUIColor(0.5f, 1f, 1)]
    public void Check()
    {
        Debug.Log(testTile.GridPosition);
    }

    [Button(ButtonSizes.Large, Name = "Open Board Editor"), GUIColor(0.3f, 0.8f, 1)]
    public static void OpenBoardEditor()
    {
        var windowExists = EditorWindow.HasOpenInstances<BoardEditorWindow>();
        if (windowExists)
        {
            EditorWindow.FocusWindowIfItsOpen<BoardEditorWindow>();
        }
        else
        {
            BoardEditorWindow.Init();
        }
    }


#endif

    //TODO, remove from here
    public Sequence PlayMergePiecesAnimation(BoardTile tileToMergeInto, List<BoardTile> mergeTiles, Piece newPiece, int cascadeCount = 0)
    {
        Sequence seq = DOTween.Sequence().SetAutoKill(true);

        seq.AppendCallback(() => 
        {
            foreach (var tile in mergeTiles)
            {
                if (tile != tileToMergeInto)
                {
                    var pieceInst = Instantiate(tile.GetPieceController().pieceInstance, tile.transform);
                    pieceInst.transform.position = tile.GetPieceController().pieceInstance.transform.position;

                    tile.SetPiece(null);
                    pieceInst.transform.DOMove(tileToMergeInto.transform.position, mergeTime)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() => Destroy(pieceInst.gameObject));
                }
            }
        });

        seq.AppendInterval(mergeSequenceEndTime);

        seq.AppendCallback(() =>
        {
            var mergeEffect = Instantiate(ResourcesProvider.Instance.GetMergeEffect());
            mergeEffect.transform.position = tileToMergeInto.transform.position;
            if(mergeEffect.transform.GetComponent<PSSizeControl>() != null)
                mergeEffect.transform.GetComponent<PSSizeControl>().scaleNumber += (0.8f * (float)cascadeCount);
            else
                mergeEffect.transform.localScale = new Vector3(1.0f + (0.5f * (float)cascadeCount), 1.0f + (0.5f * (float)cascadeCount), 1.0f + (0.5f * (float)cascadeCount));
            Destroy(mergeEffect, 2f);
           // mergeEffect.transform.localScale = new Vector3(2f, 2f, 1f);

            Debug.Log(ResourcesProvider.Instance.GetMergeEffect().transform.localScale);
            Debug.Log(mergeEffect.transform.localScale);

            var аudioController = ServiceLocator.Instance.GetInstanceOfType<AudioController>();
            if (аudioController != null)
            {
                аudioController.PlayEffect(SoundEffect.Merge, Mathf.Pow(1.2f, cascadeCount));
            }

            tileToMergeInto.SetPiece(newPiece);
        });

        return seq;
    }

    #region Public API

    public void ClearPieces()
    {
        foreach (var tile in tiles)
        {
            tile.SetPiece(null);
        }
    }

    #endregion

    #region Board State Logic

    public List<Vector3Int> NeighboursForPosition(Vector3Int pos) => Utilities.HexGrid.GetNeighbourPositions(pos);
    public bool CheckPositionIsOccupied(Vector3Int pos) => this.tiles.Any(sl => sl.GridPosition == pos);
    public bool CheckPositionIsNotOccupied(Vector3Int pos) => !CheckPositionIsOccupied(pos);
    /*
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            CheckBoardFull(null);
        }
    }*/

    public void CheckBoardFull(BoardTile lastTile)
    {
        var occupiableTiles = tiles.Where(t => t.CanBeOccupied);
        var unoccupiedTileCount = occupiableTiles.Count(t => t.pieceController == null); 
        Debug.Log($"Unoccupied tiles = {unoccupiedTileCount}");
        if (unoccupiedTileCount == 0)
        {
            GameplayEventsManager.OnGameplayEventRaised.OnNext(new BoardFilledEvent() { lastTile = lastTile });
        }
    }
    
    public bool TryGetBoardTileGrid(Vector3Int gPos, out BoardTile tile)
    {
        if (this.tiles.Any(t => t.GridPosition == gPos))
        {
            tile = this.tiles.First(t => t.GridPosition == gPos);
            return true;
        }

        tile = null;
        return false;
    }
    public bool TryGetBoardTileWorld(Vector2 worldPos, out BoardTile tile)
    {
        var gPos = this.Grid.WorldToCell(worldPos);
        if (this.tiles.Any(t => t.GridPosition == gPos))
        {
            tile = this.tiles.First(t => t.GridPosition == gPos);
            return true;
        }

        tile = null;
        return false;
    }
    
    public bool TryGetPieceControllerFromTile(BoardTile tile, out PieceController pieceController)
    {
        if (tile && tile.pieceController)
        {
            pieceController = tile.pieceController;
            return true;
        }

        pieceController = null;
        return false;
    }

    // Does not work
    public bool FindPotentialMerges(BoardTile tile, Piece pieceToUse, out List<BoardTile> mergeData)
    {
        mergeData = Utilities.Pathfinding.SearchBFS(tile, (bt) =>
        {
            if (pieceToUse != null && bt == tile)
            {
                return true;
            }
            else
            {
                if (bt.pieceController == null || pieceToUse == null)
                {
                    return false;
                }

                return bt.pieceController.masterPiece.Equals(pieceToUse);
            }
        },
            (bt) => bt.Neighbours.Select(t => (BoardTile)t).ToList());

        return mergeData.Count >= 3;
    }

    public bool FindMergeSequence(BoardTile tile, Piece startingPiece, out List<List<BoardTile>> mergeData)
    {
        // Piece has no merge rules.
        mergeData = new List<List<BoardTile>>();
        
        //if (tile.pieceController == null) return false;
        if (!this._mergeRulesFlat.Any(rule => rule.requiredPieceId == startingPiece.GetFullId()))
            return false;

        List<BoardTile> forPieceId = null;
        bool isMerge = false;
        var searchedPiece = startingPiece;
        do
        {
            var ruleForPiece = this._mergeRulesFlat.FirstOrDefault(rule => rule?.requiredPieceId == searchedPiece?.GetFullId());
            if (ruleForPiece != null)
            {
                forPieceId = Utilities.Pathfinding.SearchBFS(tile, (bt) =>
                    {
                        if (bt == tile) return true;
                        if (bt.pieceController == null || mergeInProgressTiles.Contains(bt)) //  TODO : || tile.PieceData is WildcardPiece)
                        {
                            return false;
                        }
                        var val = bt.pieceController.masterPiece.GetFullId().Equals(searchedPiece.GetFullId());
                        return val;
                    },
                    (bt) => bt.Neighbours.Where(t => (BoardTile)t != tile).Select(t => (BoardTile)t).ToList());
                isMerge = forPieceId.Count >= Board.Min_Pieces_To_Merge;
                if (isMerge)
                {
                    mergeData.Add(forPieceId);
                }
                searchedPiece = ResourcesProvider.Instance.GetPieceById(ruleForPiece.resultPieceId);
            }
            else
            {
                isMerge = false;
            }
            //Debug.Log($"Found {forPieceId.Count} simillar pieces");
        } while (isMerge);

        return mergeData.Count > 0;
    }

    public bool FindMerges(BoardTile tile, out List<BoardTile> mergeData)
    {
        // Piece has no merge rules.
        mergeData = null;
        
        if (tile.pieceController == null) return false;
        if (!this._mergeRulesFlat.Any(rule => rule.requiredPieceId == tile.pieceController.masterPiece.GetFullId()))
            return false;

        var ruleForPiece = this._mergeRulesFlat.First(rule => rule.requiredPieceId == tile.pieceController.masterPiece.GetFullId());
        mergeData = Utilities.Pathfinding.SearchBFS(tile, (bt) =>
            {
                if (bt.pieceController == null || tile.pieceController == null || mergeInProgressTiles.Contains(bt) ) //  TODO : || tile.PieceData is WildcardPiece)
                {
                    return false;
                }
                return bt.pieceController.Equals(tile.pieceController);
            },
            (bt) => bt.Neighbours.Select(t => (BoardTile) t).ToList());

        return mergeData.Count >= Min_Pieces_To_Merge;
    }

    public Piece GetNextPiece(Piece piece)
    {
        var ruleForPiece = this._mergeRulesFlat.First(rule => rule.requiredPieceId == piece.GetFullId());

        return ResourcesProvider.Instance.GetPieceById(ruleForPiece?.resultPieceId);
    }

    private List<Tile> mergeInProgressTiles = new List<Tile>();
    public void CheckForMerges(BoardTile tile)
    {
        if (tile.pieceController == null) return;
        if (mergeInProgressTiles.Contains(tile)) return;
        if (FindMerges(tile, out var tilesToMerge))
        {
            // tilesToMerge.ForEach(t => t.PieceController.SetMergeFlag(true));
            mergeInProgressTiles = mergeInProgressTiles.Union(tilesToMerge).ToList();
            
            var ruleForTile = _mergeRulesFlat.First(r => r.requiredPieceId == tile.pieceController.masterPiece.GetFullId());
            PiecesMergedEvent newPiecesMergedEventNew = new PiecesMergedEvent()
            {
               // mergeRule = ruleForTile,
                oldPiece = ResourcesProvider.Instance.GetPieceById(ruleForTile?.requiredPieceId),
                newPiece = ResourcesProvider.Instance.GetPieceById(ruleForTile?.resultPieceId),
                mergedTiles = tilesToMerge,
                tileMergedInto = tile
            };
            GameplayEventsManager.OnGameplayEventRaised.OnNext(newPiecesMergedEventNew);
        }
    }

    public void NotifyMergeComplete(List<BoardTile> tiles)
    {
        mergeInProgressTiles = mergeInProgressTiles.Except(tiles).ToList();
        mergeInProgressTiles.ForEach(t =>
        {
            if (t.pieceController == null) return;
            // t.PieceController.SetMergeFlag(false);
        });
    }

    private List<TileData.AnimationData> GetAnimationForObject(GameObject obj)
    {
        return obj.GetComponents<DOTweenAnimation>().Select(x => new TileData.AnimationData()
        {
            type = x.animationType, delay = x.delay, fromAlpha = x.endValueFloat, fromPos = new Vector3FloatStripped(x.endValueV3.x, x.endValueV3.y, x.endValueV3.z),
            easeType = x.easeType, duration = x.duration
        }).ToList();
    }
    
    private void SetAnimationForObject(GameObject obj, List<TileData.AnimationData> anims)
    {
        foreach (var anim in anims.Where(anim => anim.type == DOTweenAnimation.AnimationType.LocalMove))
        {
            var targetPos = obj.transform.localPosition;
            obj.transform.localPosition += new Vector3(anim.fromPos.x, anim.fromPos.y, anim.fromPos.z);
            obj.transform.DOLocalMove(targetPos, anim.duration).SetEase(anim.easeType).SetDelay(anim.delay).SetAutoKill(true).Play();
        }
    }
    public void RemoveTile(BoardTile tile)
    {
        var tileNeighbours = tile.Neighbours;
        foreach (var tn in tileNeighbours)
        {
            (tn as GridTile).RemoveNeighbour(tile);
        }

        tiles.Remove(tile);
    }
    #endregion

    public bool CheckIsMerging(Tile tile)
    {
        return mergeInProgressTiles.Contains(tile);
    }

    public BoardData GetBoardData(bool update = false)
    {
        if (update)
        {
#if UNITY_EDITOR
            // UpdateTilePieceAndOverlayIds();
            UpdateTileData();
#endif
        }
        return boardData;
    }
    public void SetBoardData(BoardData data)
    {
        boardData = data;
    }
    
#if UNITY_EDITOR
    
    [Button]
    public void UpdateTilePieceAndOverlayIds()
    {
        List<(string name, Color color, int level)> duplicatePieces = new List<(string name, Color color, int level)>();
        List<string> duplicateTiles = new List<string>();
        List<(string name, Color color)> duplicateOverlays = new List<(string name, Color color)>();
        Dictionary<string, string> allTiles = new Dictionary<string, string>();
        Dictionary<(string name, Color color, int level), string> staticPieces =
            new Dictionary<(string name, Color color, int level), string>();
        StringBuilder log = new StringBuilder();

        var pieces = AddressableHelpers.GetAllPieces();
        var animatedPieces = pieces.Where(x =>
            ((GameObject)new AssetReference(x.guid).editorAsset).GetComponentInChildren<MeshRenderer>());
        var animatedPiecesDict = animatedPieces.ToDictionary(
            x => ((GameObject)new AssetReference(x.guid).editorAsset).GetComponentInChildren<MeshRenderer>().sharedMaterial.name,
            x => x.address);


        foreach (var assetEntry in pieces.Except(animatedPieces))
        {
            var assetRefObj = ((GameObject)new AssetReference(assetEntry.guid).editorAsset);
            var key = (
                assetRefObj.GetComponentInChildren<SpriteRenderer>().sprite.name,
                assetRefObj.GetComponentInChildren<SpriteRenderer>().color,
                assetRefObj.GetComponentInChildren<Piece>().level);
            if (!staticPieces.ContainsKey(key))
            {
                staticPieces.Add(key, assetEntry.address);
            }
            else
            {
                duplicatePieces.Add(key);
                Debug.LogWarning("Two pieces found with the same key" + key);
            }
        }

        foreach (var tile in AddressableHelpers.GetAllTiles())
        {
            var key = ((GameObject)new AssetReference(tile.guid).editorAsset).GetComponentInChildren<SpriteRenderer>()
                .sprite
                .name;
            if (!allTiles.ContainsKey(key))
            {
                allTiles.Add(key, tile.address);
            }
            else
            {
                duplicateTiles.Add(key);
                Debug.LogWarning("Two tiles found with the same key" + key);
            }
        }

        Dictionary<(string name, Color color), string> allOverlays =
            new Dictionary<(string name, Color color), string>();
        foreach (var overlay in AddressableHelpers.GetAllOverlays())
        {
            var assetRef = (GameObject)new AssetReference(overlay.guid).editorAsset;
            var key = (assetRef.GetComponentInChildren<SpriteRenderer>().sprite.name,
                 assetRef.GetComponentInChildren<SpriteRenderer>().color);
            if (!allOverlays.ContainsKey(key))
            {
                allOverlays.Add(key, overlay.address);
            }
            else
            {
                duplicateOverlays.Add(key);
                Debug.LogWarning("Two overlays found with the same key" + key);
            }
        }
        
        bool error = false;
        
        foreach (var tile in tiles)
        {
            if (tile.pieceController != null && tile.pieceController.pieceInstance != null)
            {
                var pieceInstance = tile.pieceController.pieceInstance;
                var animation = pieceInstance.GetComponentInChildren<MeshRenderer>(true);
                var fullPieceID = "";
                if (animation != null)
                {
                    var key = animation.sharedMaterial.name;
                    if (animatedPiecesDict.ContainsKey(key))
                    {
                        fullPieceID = animatedPiecesDict[key];
                    }
                }
                else
                {
                    var sprite = pieceInstance.GetComponentInChildren<SpriteRenderer>(true);
                    var key = (sprite.sprite.name, sprite.color, pieceInstance.level);
                    if (staticPieces.ContainsKey(key))
                    {
                        fullPieceID = staticPieces[key];
                        if (duplicatePieces.Contains(key))
                        {
                            log.AppendLine($"Check tile {tile._gridPosition} for proper piece, two or more pieces with the same key found");
                        }
                    }
                }

                if (!string.IsNullOrEmpty(fullPieceID))
                {
                    UpdatePieceID(fullPieceID, pieceInstance, tile, log);
                }
                else
                {
                    error = true;
                    log.AppendLine(
                        $"ERROR: Cant find addressable asset for piece '{pieceInstance}'/'{tile.gameObject.name}/{tile.GetInstanceID()}' with texture '{pieceInstance.GetComponentInChildren<SpriteRenderer>()?.sprite?.name}'");
                }
                
            }
            
            if (allTiles.ContainsKey(tile.spriteRenderer.sprite.name))
            {
                var tileId = allTiles[tile.spriteRenderer.sprite.name];
                if (tile.id != tileId)
                {
                    log.AppendLine($"Changing tile id from '{tile.id}' to '{tileId}' for tile {tile._gridPosition}");
                    tile.id = tileId;
                }
                else
                {
                    log.AppendLine($"Tile with id '{tile.id}' at {tile._gridPosition} had correct id and texture");
                }
                if (duplicateTiles.Contains(tile.spriteRenderer.sprite.name))
                {
                    log.AppendLine($"Check tile {tile._gridPosition} for proper tile, two or more tiles with the same key found");
                }
            }
            else
            {
                error = true;
                log.AppendLine($"ERROR: Cant find addressable asset for tile '{tile.gameObject.name}/{tile.GetInstanceID()}' with texture '{tile.spriteRenderer.sprite.name}'");
            }

            foreach (var overlay in tile.overlays)
            {
                var sprite = overlay.GetComponentInChildren<SpriteRenderer>();
                var key = (sprite.sprite.name, sprite.color);
                if (allOverlays.ContainsKey(key))
                {
                    var overlayId = allOverlays[key];
                    if (overlay.id != overlayId)
                    {
                        log.AppendLine($"Changing overlay id from '{overlay.id}' to '{overlayId}' for tile {tile._gridPosition}");
                        overlay.id = overlayId;
                    }
                    else
                    {
                        log.AppendLine($"Overlay with id '{overlay.id}' at {tile._gridPosition} had correct id and texture");
                    }
                    
                    if (duplicateOverlays.Contains(key))
                    {
                        log.AppendLine($"Check tile {tile._gridPosition} for proper overlay, two or more overlays with the same key found");
                    }
                }
                else
                {
                    error = true;
                    log.AppendLine($"ERROR: Cant find addressable asset for overlay '{overlay.gameObject.name}/{tile.GetInstanceID()}' with texture '{overlay.GetComponentInChildren<SpriteRenderer>()?.sprite?.name}'");
                }  
            }
        }
        
        log.AppendLine($"Finished updating {tiles.Count} tiles");
        if (error)
        {
            Debug.LogError(log.ToString());
        }
        else
        {
            Debug.Log(log.ToString());
        }
    }

    private static void UpdatePieceID(string fillPieceId, Piece pieceInstance, BoardTile tile, StringBuilder log)
    {
        var pieceId = fillPieceId.Substring(0, fillPieceId.IndexOf('{'));
        if (pieceInstance.GetFullId() != fillPieceId)
        {
            log.AppendLine($"Changing piece id from '{pieceInstance.id}' to '{pieceId}' for tile {tile._gridPosition}");
            pieceInstance.id = pieceId;
        }
        else
        {
            log.AppendLine($"Piece with id '{pieceInstance.GetFullId()}' at {tile._gridPosition} had correct id and texture");
        }
    }


    [Button]
    public void UpdateTileData()
    {
        boardData = new BoardData();
        boardData.boardPos = new Vector3FloatStripped(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
        boardData.boardScale = new Vector3FloatStripped(transform.localScale.x, transform.localScale.y, transform.localScale.z);
        foreach (var tile in tiles)
        {
            var hasPiece = tile.pieceController != null;
            var pieceAnims = hasPiece ? GetAnimationForObject(tile.pieceController.pieceInstance.transform.parent.gameObject):
                    new List<TileData.AnimationData>();
            var data = new TileData()
            {
                isGameplayTile = tile?.isGameplayTile ?? false,
                gridPosition = new Vector3IntStripped(tile._gridPosition.x, tile._gridPosition.y, tile._gridPosition.z),
                pieceId = tile?.pieceController?.pieceInstance?.GetFullId(),
                tileId = tile.id,
                tileAnimations = GetAnimationForObject(tile.gameObject),
                pieceAnimations = pieceAnims,
                overlayIds = tile.overlays.Select(x => x.id).ToList()
            };
            boardData.tileData.Add(data);
        }
    }
    
#endif


#if UNITY_EDITOR
    [Button]
#endif
    public async void SpawnLevel()
    {
        var startTime = DateTime.Now;
        await LoadLevelFromData();
        Debug.Log($"Spawned level in {(DateTime.Now - startTime).TotalMilliseconds} milliseconds");
    }

    #region Load
#if UNITY_EDITOR
    [Button]
#endif
    private async Task<bool> LoadAssets(BoardData data)
    {
        bool error = false;
        var assetManager = AddressableManager.Instance;
        var tileIds = data.tileData.Select(x => x.tileId).Distinct().ToList();
        var pieceIds = data.tileData.Select(x => x.pieceId).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
        var overlayIds = data.tileData.SelectMany(x => x.overlayIds).Distinct().ToList();
        var tileAssets = await assetManager.LoadAssets<GameObject>(tileIds, this);
        if (!tileAssets.Item1)
        {
            error = true;
            Debug.LogError("Tile assets could not load");
        }
        var pieceAssets = await assetManager.LoadAssets<GameObject>(pieceIds, this);
        if (!pieceAssets.Item1)
        {
            error = true;
            Debug.LogError("Piece assets could not load");
        }
        var overlayAssets = await assetManager.LoadAssets<GameObject>(overlayIds, this);
        if (!overlayAssets.Item1)
        {
            error = true;
            Debug.LogError("Overlay assets could not load");
        }

        return !error;
    }
    
#if UNITY_EDITOR
    [Button]
#endif
    private async Task<bool> LoadLevelFromData()
    {
        var assetManager = AddressableManager.Instance;
        transform.localPosition = new Vector3(boardData.boardPos.x, boardData.boardPos.y, boardData.boardPos.z);
        transform.localScale = new Vector3(boardData.boardScale.x, boardData.boardScale.y, boardData.boardScale.z);
        foreach (var data in boardData.tileData)
        {
            var gridPos = new Vector3Int(data.gridPosition.x, data.gridPosition.y, data.gridPosition.z);
            var tileToSpawn = assetManager.GetAsset<GameObject>(data.tileId);
            if (tileToSpawn != null)
            {
                var newTile = Instantiate(tileToSpawn, tilesParent.transform);
                newTile.transform.position = Grid.GetCellCenterWorld(gridPos);
                var tile = newTile.GetComponent<BoardTile>();
                tile.isGameplayTile = data.isGameplayTile;
                tile.SetGridPosition(gridPos);
                tile.SetGrid(Grid);
                tile.SetBoard(this);
                tiles.Add(tile);

                if (data.tileAnimations != null && data.tileAnimations.Count > 0)
                {
                    SetAnimationForObject(newTile, data.tileAnimations);
                }


                if (!string.IsNullOrEmpty(data.pieceId))
                {
                    var pieceToSpawn = assetManager.GetAsset<GameObject>(data.pieceId);
                    if (pieceToSpawn != null)
                    {
                        tile.SetPiece(pieceToSpawn.GetComponent<Piece>());
                        if (tile.pieceController.pieceInstance != null && data.pieceAnimations != null && data.pieceAnimations.Count > 0)
                        {
                            SetAnimationForObject(tile.pieceController.pieceInstance.transform.parent.gameObject, data.pieceAnimations);
                        }
                    }
                    else
                    {
                        Debug.LogError($"Couldn't spawn piece with id '{data.pieceId}'");
                    }
                }
                
                if (data.overlayIds != null && data.overlayIds.Count > 0)
                {
                    foreach (var overlayId in data.overlayIds)
                    {
                        var overlayToSpawn = assetManager.GetAsset<GameObject>(overlayId);
                        if (overlayToSpawn != null)
                        {
                            tile.AddNewOverlay(overlayToSpawn.GetComponent<Overlay>());
                        }
                        else
                        {
                            Debug.LogError($"Couldn't spawn overlay with id '{overlayId}'");
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"Couldn't spawn tile with id '{data.tileId}'");
            }

        }

        tiles.RemoveAll(x => x == null);
        var pieceBehaviours = this.GetAllBehaviours(true);
        pieceBehaviours
            .Where(b => b.triggerPoints.Contains(BehaviourTriggerPoint.OnAfterPlace))
            .ToList()
            .ForEach(b => b.ApplyEffect(this, b.GetComponentInParent<BoardTile>()));
        BoardSetTileNeighbours();
        return true;
    }

#if UNITY_EDITOR
    [Button]
#endif
    private void UnloadAssets()
    {
        var assetManager = AddressableManager.Instance;
        var tileIds = boardData.tileData.Select(x => x.tileId).Distinct().ToList();
        var pieceIds = boardData.tileData.Select(x => x.pieceId).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
        var overlayIds = boardData.tileData.SelectMany(x => x.overlayIds).Distinct().ToList();
        
        assetManager.UnloadAssets(tileIds, this);
        assetManager.UnloadAssets(pieceIds, this);
        assetManager.UnloadAssets(overlayIds, this);
    }
    
    #endregion

    #region BoardUpdate

    private void BoardSetTileNeighbours()
    {
        foreach (var tile in tiles)
        {
            var foundNeighbours = tiles
                .Where(bt => Utilities.HexGrid.GetNeighbourPositions(tile.GridPosition).Contains(bt.GridPosition))
                .ToList();
            tile.SetNeighbours(foundNeighbours);
        }
    }
    
    #endregion
}