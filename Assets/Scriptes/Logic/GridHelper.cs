using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GridHelper
{
    public GridLayoutGroup gridLayout;

    public List<TileBase> _tileBases;

    public int _sizeX;

    public int _sizeY;

    public Dictionary<Vector2Int, TileState> _tileStateMap;

    public GridHelper(GridLayoutGroup gridLayout)
    {
        this.gridLayout = gridLayout;
        _tileBases = gridLayout.transform.GetComponentsInChildren<TileBase>().ToList(); ;

        for (int i = 0; i < _sizeX; i++) {
            for (int j = 0; j < _sizeY; i++) {
                Vector2Int pos = new Vector2Int(i, j);
                TileBase tileBase = GetTileBase(pos);
                tileBase.Mount(pos);
                _tileBases.Add(tileBase);
            }
        }
    }

    public List<TileBase> GetTileBases() { 
        return _tileBases;
    }

    public Vector2Int GetGridSize() { 
        return new Vector2Int( _sizeX, _sizeY );
    }


    public void RefreshTilesState(Vector2Int startPos) {

        




        foreach (TileBase tileBase in _tileBases)
        {
            tileBase.SetState(_tileStateMap[tileBase.GetPos()]);
        }
    }
    public void RefreshTiles() {
        foreach (TileBase tileBase in _tileBases) {
            tileBase.Refresh();
        }
    }
    public void PutObjectOnTile(GameObject gameObject, Vector2Int tilePos, float zOffset = 0)
    {
        gameObject.transform.position = GetCellWorldPosition(tilePos);
        gameObject.transform.position += Vector3.forward * zOffset;
    }

    public void PutObjectOnTile(GameObject gameObject, Vector2Int[] tilePoses, float zOffset = 0)
    {
        gameObject.transform.position = GetCellWorldPosition(tilePoses);
        gameObject.transform.position += Vector3.forward * zOffset;
    }


    public Vector2Int GetGridPosition(Vector2 mouseScreenPosition)
    {
        RectTransform rectTransform = gridLayout.GetComponent<RectTransform>();

        // 将屏幕坐标转换为RectTransform的本地坐标系（基于父容器的左下角）
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            mouseScreenPosition,
            null, // 适用于Screen Space - Overlay模式
            out Vector2 localPoint);

        // 修正坐标系为左下原点系统
        // 原理：原本的Y坐标从下到上递增（0在最下端），现在需要转换为从下到上递增的正值
        float containerHeight = rectTransform.rect.height;
        float containerWidth = rectTransform.rect.width;

        // 将原点从父容器的中心点左下角调整到真正的左下角
        Vector2 bottomLeftOrigin = new Vector2(
            -containerWidth / 2 + gridLayout.padding.left,  // 左padding边界
            -containerHeight / 2 + gridLayout.padding.bottom  // 下padding边界
        );

        // 有效坐标范围（网格区域）
        Vector2 effectivePoint = localPoint - bottomLeftOrigin;

        // 计算单元格索引（注意Y轴方向转换）
        float cellWidth = gridLayout.cellSize.x + gridLayout.spacing.x;
        float cellHeight = gridLayout.cellSize.y + gridLayout.spacing.y;

        // 列计算（从左到右）
        int column = Mathf.FloorToInt(effectivePoint.x / cellWidth);

        // 行计算（从下到上）
        int row = Mathf.FloorToInt(effectivePoint.y / cellHeight);

        return new Vector2Int(column, row);
    }

    public Vector2 GetCellWorldPosition(Vector2Int gridPosition)
    {
        Vector2 localPos = GetCellCenterPosition(gridPosition);
        RectTransform rectTransform = gridLayout.GetComponent<RectTransform>();
        return rectTransform.TransformPoint(localPos);
    }

    public Vector2 GetCellWorldPosition(Vector2Int[] gridPositions) {
        Vector2 worldPosition = Vector2.zero;

        foreach (Vector2Int gridPosition in gridPositions) {
            worldPosition += GetCellWorldPosition(gridPosition);
        }
        return worldPosition/gridPositions.Length;
    }


    public TileBase GetTileBase(Vector2Int gridPos) {
        return GetCellAtPosition(gridPos).GetComponent<TileBase>(); 
    }
 
    public GameObject GetCellAtPosition(Vector2Int gridPos)
    {
        // 获取布局参数
        int constraintCount = gridLayout.constraintCount;
        int childCount = gridLayout.transform.childCount;

        // 校验约束类型
        if (gridLayout.constraint != GridLayoutGroup.Constraint.FixedColumnCount)
        {
            Debug.LogError("当前只支持FixedColumnCount约束模式");
            return null;
        }

        // 计算最大行列数
        int maxColumn = Mathf.Min(constraintCount, childCount);
        int maxRow = Mathf.CeilToInt(childCount / (float)constraintCount);

        // 边界检查
        if (gridPos.x < 0 || gridPos.x >= maxColumn)
        {
            return null;
        }
        if (gridPos.y < 0 || gridPos.y >= maxRow)
        {
            return null;
        }

        // 计算线性索引
        int index = gridPos.y * constraintCount + gridPos.x;

        // 二次安全校验
        if (index >= childCount)
        {
            return null;
        }

        return gridLayout.transform.GetChild(index).gameObject;
    }

    public Vector2 GetCellCenterPosition(Vector2Int gridPosition)
    {
        RectTransform rectTransform = gridLayout.GetComponent<RectTransform>();
        Canvas canvas = rectTransform.GetComponentInParent<Canvas>();

        // 基础参数
        Vector2 cellSize = gridLayout.cellSize;
        Vector2 spacing = gridLayout.spacing;
        RectOffset padding = gridLayout.padding;

        // 动态获取行列数
        int totalColumns = gridLayout.constraintCount;
        int totalRows = Mathf.CeilToInt(rectTransform.childCount / (float)totalColumns);

        // 计算总内容区尺寸
        float totalWidth = padding.left + padding.right +
                           totalColumns * cellSize.x +
                           Mathf.Max(0, totalColumns - 1) * spacing.x;

        float totalHeight = padding.top + padding.bottom +
                            totalRows * cellSize.y +
                            Mathf.Max(0, totalRows - 1) * spacing.y;

        // 对齐偏移计算（MiddleCenter模式）
        Vector2 alignmentOffset = new Vector2(
            (rectTransform.rect.width - totalWidth) * 0.5f,
            (rectTransform.rect.height - totalHeight) * -0.5f // Unity Y轴向下为负
        );

        // 初始起点（考虑对齐偏移）
        Vector2 startPos = new Vector2(
            rectTransform.rect.xMin + padding.left + alignmentOffset.x,
            rectTransform.rect.yMin + padding.bottom + alignmentOffset.y
        );

        // 单元格精确坐标
        float posX = startPos.x +
                     gridPosition.x * (cellSize.x + spacing.x) +
                     cellSize.x / 2;

        float posY = startPos.y +
                     gridPosition.y * (cellSize.y + spacing.y) +
                     cellSize.y / 2;
        return new Vector3(posX, posY, 0f);
    }

}
