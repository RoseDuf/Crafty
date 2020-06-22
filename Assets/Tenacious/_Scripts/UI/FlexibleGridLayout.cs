using UnityEngine;
using UnityEngine.UI;

namespace Tenacious.UI
{
    [ExecuteInEditMode]
    public class FlexibleGridLayout : LayoutGroup
    {
        public enum EFitType { Uniform, Width, Height, FixedRows, FixedColumns }

        private Vector2 cellSize;

        [SerializeField] private Vector2 spacing;
        [SerializeField] private EFitType fitType;
        [SerializeField] private int rows;
        [SerializeField] private int columns;
        [SerializeField] private bool fitX;
        [SerializeField] private bool fitY;

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            if (fitType == EFitType.Width || fitType == EFitType.Height || fitType == EFitType.Uniform)
            {
                fitX = fitY = true;

                float sqrt = Mathf.Sqrt(transform.childCount);
                rows = Mathf.CeilToInt(sqrt);
                columns = Mathf.CeilToInt(sqrt);
            }

            if (fitType == EFitType.Width || fitType == EFitType.FixedColumns)
                rows = Mathf.CeilToInt(transform.childCount / (float)columns);
            if (fitType == EFitType.Height || fitType == EFitType.FixedRows)
                columns = Mathf.CeilToInt(transform.childCount / (float)rows);

            float parentWidth = rectTransform.rect.width;
            float parentHeight = rectTransform.rect.height;

            float cellWidth = (parentWidth - (spacing.x * (columns - 1)) - padding.left - padding.right) / columns;
            float cellHeight = (parentHeight - (spacing.y * (rows - 1)) - padding.top - padding.bottom) / rows;

            cellSize.x = fitX ? cellWidth : cellSize.x;
            cellSize.y = fitY ? cellHeight : cellSize.y;

            int columnCount, rowCount = 0;
            for (int i = 0; i < rectChildren.Count; ++i)
            {
                rowCount = i / columns;
                columnCount = i % columns;

                float columnShift = 0;
                if (childAlignment == TextAnchor.UpperCenter || childAlignment == TextAnchor.MiddleCenter || childAlignment == TextAnchor.LowerCenter)
                {
                    int remainingCells = rectChildren.Count - i;
                    if ((columnCount + remainingCells) < columns)
                        columnShift = (float)(columns - (columnCount + remainingCells)) / 2f;
                }

                float xPos = cellSize.x * ((float)columnCount + columnShift) + (spacing.x * ((float)columnCount + columnShift)) + padding.left;
                float yPos = cellSize.y * rowCount + (spacing.y * rowCount) + padding.top;

                SetChildAlongAxis(rectChildren[i], 0, xPos, cellSize.x);
                SetChildAlongAxis(rectChildren[i], 1, yPos, cellSize.y);

                if (rectChildren[i].GetComponent<FlexibleGridLayout>() != null)
                    rectChildren[i].GetComponent<FlexibleGridLayout>().CalculateLayoutInputHorizontal();
            }
        }

        public override void CalculateLayoutInputVertical()
        {
            //
        }

        public override void SetLayoutHorizontal()
        {
            //
        }

        public override void SetLayoutVertical()
        {
            //
        }
    }
}