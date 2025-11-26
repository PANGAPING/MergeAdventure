using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class TileBase : MonoBehaviour
{
    private List<TileItem> OccupiedItems = new List<TileItem>();

    private TileState state = TileState.WHITE;

    private Vector2Int pos = Vector2Int.zero;

    public void Refresh() { 
                 
    }

    public void Mount(Vector2Int pos) { 
        this.pos = pos;
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
}

public enum TileState { 
    WHITE,
    GRAY,
    HIDE
}
