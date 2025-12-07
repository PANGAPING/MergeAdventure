using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class TileBase : MonoBehaviour
{
    protected List<TileItem> OccupiedItems = new List<TileItem>();

    public TileState state = TileState.WHITE;

    protected Vector2Int pos = Vector2Int.zero;

    protected TileItem topTileItem;

    protected Image _tileImage;

    protected static Color _whiteColor = new Color(142f/255,184f/255,241f/255);

    protected static Color _grayColor = new Color(113f/255,113f/255,160f/255);

    protected static Color _hideColor = new Color(50f/255,50f/255,50f/255);

    protected bool dirty = false;

    protected void Awake()
    {
        InitSelf(); 
    }

    protected void Start()
    {
        Init(); 
    }

    protected void InitSelf() { 
        
    }

    protected void Init() { 
    
    
    }

    public virtual void Refresh() {
        SortItemsLayer();
        TileItem nowTopItem = GetLayerTopItem();
        if (dirty) {
            if (nowTopItem != null && nowTopItem != topTileItem) { 
                nowTopItem.AppearAnimation();
            }
            dirty = false;
        }

        if (state == TileState.WHITE)
        {
            _tileImage.color = _whiteColor;
            if (nowTopItem != null)
            {
                nowTopItem.SetWhiteColor();
            }
        }
        else if (state == TileState.GRAY)
        {
            _tileImage.color = _grayColor;
            if (nowTopItem != null)
                nowTopItem.SetGrayColor();
        }
        else if (state == TileState.HIDE) {
            _tileImage.color = _hideColor;
        }

        topTileItem = nowTopItem;
    }

    public void SetDirty() { 
        dirty = true;
    }

    protected void SortItemsLayer() {
        OccupiedItems.Sort((x, y) => y.GetLayer().CompareTo(x.GetLayer()));
        if (OccupiedItems.Count == 0) {
            return;
        }

        OccupiedItems[0].Show();

        for (int i = 1; i < OccupiedItems.Count; i++) {
            OccupiedItems[i].Hide();
        }
    }

    public void Mount(Vector2Int pos) { 
        this.pos = pos;
        _tileImage = GetComponent<Image>();
    }

    public Vector2Int GetPos() { 
        return pos; 
    }

    public TileState GetState() {
        return state;
    }

    public void SetState(TileState state) { 
        this.state = state;
    }

    public TileItem GetLayerTopItem() {
        if (OccupiedItems == null || OccupiedItems.Count == 0) {
            return null;
        }
        return OccupiedItems[0];
    }

    public virtual void OccupyItem(TileItem item) { 
        OccupiedItems.Add(item);
    }

    public virtual void RemoveOccupyItem(TileItem item) {
        if (OccupiedItems.Contains(item)) { 
            OccupiedItems.Remove(item);
        }
    }

    public bool IsEmpty() {
        return OccupiedItems == null || OccupiedItems.Count == 0;
    }

    public bool ExistItemOfType(ItemType itemType) {
        return OccupiedItems.Exists(x => x.GetItemType() == itemType);
    }

    public bool ExistItemOfLayer(int layer) { 
        return OccupiedItems.Exists(x => x.GetLayer() == layer);
    }

    public TileItem GetItemOfType(ItemType itemType) {
        return OccupiedItems.Find(x => x.GetItemType() == itemType); 
    }

    public bool IsObstacleTile() {
        return !IsCloudTile() && OccupiedItems.Exists(x => x.Model.GetItemConfig().Obstacle == 1);
    }

    public bool IsCloudTile() {
        return OccupiedItems.Exists(x => x.GetItemType() == ItemType.ELFCLOUD);
    }

    public bool IsDragable() {
        TileItem tileItem = GetLayerTopItem();
        return state == TileState.WHITE && tileItem != null && tileItem.IsMovable(); ;
    }
}

public enum TileState { 
    //连通格子
    WHITE,
    //灰色格子
    GRAY,
    //遮盖格子
    HIDE
}
