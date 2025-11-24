using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBase : MonoBehaviour
{
    private List<TileItem> OccupiedItems = new List<TileItem>();

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
