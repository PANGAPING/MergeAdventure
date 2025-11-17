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

        // 将屏幕坐标转换为Grid的本地坐标（左下原点）
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            mouseScreenPosition,
            null, // 不使用相机（对于Canvas的Screen Space模式）
            out Vector2 localPoint);

        // 修正坐标系统：容器左下角为(0,0)
        Vector2 containerSize = rectTransform.rect.size;
        float gridBottom = -containerSize.y / 2; // 修正后的下边界

        // 计算有效区域（扣除padding）
        RectOffset padding = gridLayout.padding;
        float startX = padding.left - containerSize.x / 2;
        float startY = padding.bottom - containerSize.y / 2;

        // 获取布局参数
        Vector2 cellSize = gridLayout.cellSize;
        Vector2 spacing = gridLayout.spacing;

        // 计算单元格索引
        int column = Mathf.FloorToInt((localPoint.x - startX) / (cellSize.x + spacing.x));
        int row = Mathf.FloorToInt((-localPoint.y - startY) / (cellSize.y + spacing.y));

        // 边界检测
        if (column < 0 || row < 0 ||
            localPoint.x > (startX + (cellSize.x + spacing.x) * gridLayout.constraintCount) ||
            -localPoint.y > (startY + (cellSize.y + spacing.y) * (rectTransform.rect.height / cellSize.y)))
        {
            return new Vector2Int(-1, -1);
        }

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

    public GameObject GetCellAtPosition(Vector2Int targetPos)
    {
        Vector2 targetCenter = GetCellCenterPosition(targetPos);
        const float precision = 2f; // 允许的坐标误差范围

        foreach (Transform child in gridLayout.transform)
        {
            if (Vector2.Distance(child.localPosition, targetCenter) < precision)
            {
                return child.gameObject;
            }
        }
        return null;
    }

    public Vector2 GetCellCenterPosition(Vector2Int gridPosition)
    {
        RectTransform rectTransform = gridLayout.GetComponent<RectTransform>();
        Vector2 containerSize = rectTransform.rect.size;
        RectOffset padding = gridLayout.padding;

        // 计算实际可用区域的起始位置（左下原点）
        Vector2 startPos = new Vector2(
            -containerSize.x / 2 + padding.left,    // X起始：左边缘+左padding
            -containerSize.y / 2 + padding.bottom   // Y起始：下边缘+下padding
        );

        // 获取布局参数
        Vector2 cellSize = gridLayout.cellSize;
        Vector2 spacing = gridLayout.spacing;

        // 计算坐标时考虑spacing的累计效应
        float posX = startPos.x +
                    gridPosition.x * (cellSize.x + spacing.x) +
                    cellSize.x / 2;

        float posY = startPos.y +
                    gridPosition.y * (cellSize.y + spacing.y) +
                    cellSize.y / 2;

        // 转换为RectTransform的本地坐标系坐标
        return new Vector2(posX, posY);
    }
}
