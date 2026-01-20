using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GridBorderRenderer : MonoBehaviour
{
    [Header("References (optional, will auto-find)")]
    public GridLayoutGroup grid;
    public RectTransform gridRect;
    public UISegmentsBorderGraphic borderGraphic;

    [Header("Visible Cell Rule")]
    public bool includeInactive = false;
    [Range(0f, 1f)] public float alphaThreshold = 0.01f;

    [Header("Border Options")]
    public float borderThickness = 6f;

    [Tooltip("为了确保不被格子压住，向外额外偏移一点点（单位：像素）")]
    public float outwardEpsilon = 0.5f;

    private readonly HashSet<Vector2Int> _occupied = new HashSet<Vector2Int>();
    private readonly List<UISegmentsBorderGraphic.Segment> _segments = new List<UISegmentsBorderGraphic.Segment>(512);
    private Coroutine _refreshCo;

    private void Awake()
    {
        AutoBindIfNeeded();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        AutoBindIfNeeded();
        if (borderGraphic) borderGraphic.Thickness = borderThickness;
    }
#endif

    /// <summary>外部触发刷新（立即）</summary>
    public void Refresh()
    {
        AutoBindIfNeeded();
        if (!grid || !gridRect || !borderGraphic) return;

        borderGraphic.Thickness = borderThickness;

        BuildOccupiedCells(_occupied);

        _segments.Clear();
        if (_occupied.Count == 0)
        {
            borderGraphic.SetSegments(_segments);
            return;
        }

        // 暴露边 => segment（多连通块/多个闭合区域天然支持）
        foreach (var c in _occupied)
        {
            var left  = new Vector2Int(c.x - 1, c.y);
            var right = new Vector2Int(c.x + 1, c.y);
            var down  = new Vector2Int(c.x, c.y - 1);
            var up    = new Vector2Int(c.x, c.y + 1);

            Vector2Int bl = new Vector2Int(c.x,     c.y);
            Vector2Int tl = new Vector2Int(c.x,     c.y + 1);
            Vector2Int br = new Vector2Int(c.x + 1, c.y);
            Vector2Int tr = new Vector2Int(c.x + 1, c.y + 1);

            if (!_occupied.Contains(left))  AddSegment(bl, tl, Vector2.left);
            if (!_occupied.Contains(right)) AddSegment(br, tr, Vector2.right);
            if (!_occupied.Contains(down))  AddSegment(bl, br, Vector2.down);
            if (!_occupied.Contains(up))    AddSegment(tl, tr, Vector2.up);
        }

        borderGraphic.SetSegments(_segments);
    }

    /// <summary>
    /// 外部触发刷新（下一帧）：适合你先 SetActive / 改内容，再让 LayoutGroup 结算完再算边界
    /// </summary>
    public void RefreshNextFrame()
    {
        if (_refreshCo != null) StopCoroutine(_refreshCo);
        _refreshCo = StartCoroutine(RefreshNextFrameCo());
    }

    private IEnumerator RefreshNextFrameCo()
    {
        yield return null; // 等一帧：LayoutGroup/ContentSizeFitter 等结算
        _refreshCo = null;
        Refresh();
    }

    private void AddSegment(Vector2Int cornerA, Vector2Int cornerB, Vector2 outward)
    {
        Vector2 a = GridCornerToLocal(cornerA);
        Vector2 b = GridCornerToLocal(cornerB);

        // 额外向外推一点点，避免“刚好压线”被覆盖（视觉上更稳）
        Vector2 eps = outward.normalized * outwardEpsilon;
        a += eps;
        b += eps;

        _segments.Add(new UISegmentsBorderGraphic.Segment(a, b, outward));
    }

    private void AutoBindIfNeeded()
    {
        if (!grid) grid = GetComponent<GridLayoutGroup>() ?? GetComponentInParent<GridLayoutGroup>();
        if (!gridRect && grid) gridRect = grid.GetComponent<RectTransform>();
        if (!borderGraphic)
            borderGraphic = GetComponent<UISegmentsBorderGraphic>()
                         ?? GetComponentInChildren<UISegmentsBorderGraphic>(true)
                         ?? GetComponentInParent<UISegmentsBorderGraphic>();
    }

    private void BuildOccupiedCells(HashSet<Vector2Int> occupied)
    {
        occupied.Clear();
        int childCount = gridRect.childCount;

        int cols, rows;
        CalcGridSize(childCount, out cols, out rows);

        for (int i = 0; i < childCount; i++)
        {
            Transform t = gridRect.GetChild(i);
            if (!IsCellVisible(t)) continue;

            Vector2Int cell = IndexToCell(i, cols, rows);
            occupied.Add(cell);
        }
    }

    private bool IsCellVisible(Transform t)
    {
        var go = t.gameObject;
        if (!includeInactive && !go.activeInHierarchy) return false;

        var g = go.GetComponent<Graphic>();
        if (g != null)
        {
            if (!g.enabled) return false;
            if (g.color.a <= alphaThreshold) return false;

            var cr = go.GetComponent<CanvasRenderer>();
            if (cr != null && cr.GetAlpha() <= alphaThreshold) return false;
        }

        var b = go.GetComponent<TileBase>();
        if (b != null) {
            return !b.IsDieTile();
        }
        return true;
    }

    private void CalcGridSize(int childCount, out int cols, out int rows)
    {
        int cCount = grid.constraintCount;

        if (grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
        {
            cols = Mathf.Max(1, cCount);
            rows = Mathf.CeilToInt(childCount / (float)cols);
        }
        else if (grid.constraint == GridLayoutGroup.Constraint.FixedRowCount)
        {
            rows = Mathf.Max(1, cCount);
            cols = Mathf.CeilToInt(childCount / (float)rows);
        }
        else
        {
            Vector2 step = grid.cellSize + grid.spacing;
            float width = gridRect.rect.width - grid.padding.left - grid.padding.right;
            cols = Mathf.Max(1, Mathf.FloorToInt((width + grid.spacing.x + 0.0001f) / Mathf.Max(1f, step.x)));
            rows = Mathf.CeilToInt(childCount / (float)cols);
        }
    }

    private Vector2Int IndexToCell(int index, int cols, int rows)
    {
        int x, y;
        if (grid.startAxis == GridLayoutGroup.Axis.Horizontal)
        {
            x = index % cols;
            y = index / cols;
        }
        else
        {
            y = index % rows;
            x = index / rows;
        }

        bool upper = grid.startCorner == GridLayoutGroup.Corner.UpperLeft || grid.startCorner == GridLayoutGroup.Corner.UpperRight;
        bool right = grid.startCorner == GridLayoutGroup.Corner.UpperRight || grid.startCorner == GridLayoutGroup.Corner.LowerRight;

        int cx = right ? (cols - 1 - x) : x;
        int rowFromTop = y;
        int cy = upper ? (rows - 1 - rowFromTop) : rowFromTop;

        return new Vector2Int(cx, cy); // (0,0)=左下
    }

    private Vector2 GridCornerToLocal(Vector2Int corner)
    {
        Vector2 step = grid.cellSize + grid.spacing;
        Rect r = gridRect.rect;

        float originX = r.xMin + grid.padding.left;
        float originY = r.yMin + grid.padding.bottom;

        return new Vector2(originX + corner.x * step.x, originY + corner.y * step.y);
    }
}
