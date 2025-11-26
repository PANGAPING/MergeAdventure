using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class TileBase : MonoBehaviour
{
    private List<TileItem> OccupiedItems = new List<TileItem>();

    private TileState state = TileState.WHITE;

    private Vector2Int pos = Vector2Int.zero;

    private Image _tileImage;

    private static Color _whiteColor = new Color(142f/255,184f/255,241f/255);

    private static Color _grayColor = new Color(113f/255,113f/255,113f/255);

    private static Color _hideColor = new Color(50f/255,50f/255,50f/255);

    public void Refresh() {

        if (state == TileState.WHITE)
        {
            _tileImage.color = _whiteColor;
        }
        else if (state == TileState.GRAY)
        {
            _tileImage.color = _grayColor;
        }
        else if (state == TileState.HIDE) {
            _tileImage.color = _hideColor;
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

    public void OccupyItem(TileItem item) { 
        OccupiedItems.Add(item);
    }

    public void RemoveOccupyItem(TileItem item) { 
        OccupiedItems.Remove(item);
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
}

public enum TileState { 
    //连通格子
    WHITE,
    //灰色格子
    GRAY,
    //遮盖格子
    HIDE
}
