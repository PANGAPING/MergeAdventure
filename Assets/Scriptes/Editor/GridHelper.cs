using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridHelper
{
    public GridLayoutGroup gridLayout;

    public GridHelper(GridLayoutGroup gridLayout)
    {
        this.gridLayout = gridLayout;
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
            Debug.LogWarning($"列索引{gridPos.x}超出范围[0-{maxColumn - 1}]");
            return null;
        }
        if (gridPos.y < 0 || gridPos.y >= maxRow)
        {
            Debug.LogWarning($"行索引{gridPos.y}超出范围[0-{maxRow - 1}]");
            return null;
        }

        // 计算线性索引
        int index = gridPos.y * constraintCount + gridPos.x;

        // 二次安全校验
        if (index >= childCount)
        {
            Debug.LogWarning($"计算索引{index}超出子物体数量{childCount}");
            return null;
        }

        return gridLayout.transform.GetChild(index).gameObject;
    }

    public Vector2 GetCellCenterPosition(Vector2Int gridPosition)
    {
        RectTransform rectTransform = gridLayout.GetComponent<RectTransform>();
        RectOffset padding = gridLayout.padding;

        // 正确获取左下角的起始位置（本地坐标系）
        Vector2 startPos = new Vector2(
            rectTransform.rect.xMin + padding.left,
            rectTransform.rect.yMin + padding.bottom
        );

        Vector2 cellSize = gridLayout.cellSize;
        Vector2 spacing = gridLayout.spacing;

        // 计算单元格中心位置（本地坐标系）
        float posX = startPos.x +
                    gridPosition.x * (cellSize.x + spacing.x) +
                    cellSize.x / 2;

        float posY = startPos.y +
                    gridPosition.y * (cellSize.y + spacing.y) +
                    cellSize.y / 2;

        return new Vector3(posX, posY, 0f);
    }

}
