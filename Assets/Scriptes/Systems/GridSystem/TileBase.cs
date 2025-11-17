using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBase : MonoBehaviour
{
    private List<Item> OccupiedItems = new List<Item>();

    public void OccupyItem(Item item) { 
        OccupiedItems.Add(item);
    }

    public void RemoveOccupyItem(Item item) { 
        OccupiedItems.Remove(item);
    }
}
