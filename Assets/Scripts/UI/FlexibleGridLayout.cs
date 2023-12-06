using UnityEngine;
using UnityEngine.UI;

public class FlexibleGridLayout : LayoutGroup
{
    [SerializeField] private int rows;
    [SerializeField] private int columns;
    [SerializeField] private Vector2 cellSize;
    [SerializeField] private Vector2 spacing;
    [SerializeField] private FitType fitType;
    [SerializeField] private bool fitX;
    [SerializeField] private bool fitY;

    private enum FitType
    {
        Uniform,
        Width,
        Height,
        FixedRows,
        FixedColumns
    }
    
    public override void SetLayoutHorizontal()
    {
        if (fitType is FitType.Width or FitType.Height or FitType.Uniform)
        {
            fitX = true;
            fitY = true;

            var sqrt = Mathf.Sqrt(transform.childCount);
            rows = Mathf.CeilToInt(sqrt);
            columns = Mathf.CeilToInt(sqrt);
        }
        
        if (fitType is FitType.Width or FitType.FixedColumns)
            rows = Mathf.CeilToInt(transform.childCount / (float) columns);
        else if (fitType is FitType.Height or FitType.FixedRows)
            columns = Mathf.CeilToInt(transform.childCount / (float) rows);
        
        var parentWidth = rectTransform.rect.width;
        var parentHeight = rectTransform.rect.height;
        
        var cellWidth = (parentWidth - spacing.x * 2 - padding.left - padding.right) / columns;
        var cellHeight = (parentHeight - spacing.y * 2 - padding.top - padding.bottom) / rows;
        
        cellSize.x = fitX ? cellWidth : cellSize.x;
        cellSize.y = fitY ? cellHeight : cellSize.y;
        
        for (var i = 0; i < rectChildren.Count; i++)
        {
            var rowCount = i / columns;
            var columnCount = i % columns;
            
            var item = rectChildren[i];
            
            var xPos = (cellSize.x * columnCount) + (spacing.x * columnCount) + padding.left;
            var yPos = (cellSize.y * rowCount) + (spacing.y * rowCount) + padding.top;
            
            SetChildAlongAxis(item, 0, xPos, cellSize.x);
            SetChildAlongAxis(item, 1, yPos, cellSize.y);
        }
    }
    
    public override void CalculateLayoutInputVertical()
    {
    }
    
    public override void SetLayoutVertical()
    {
    }
}
