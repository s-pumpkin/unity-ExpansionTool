using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace yo.ExpansionTool
{
    /// <summary>
    /// 3D Grid Layout Group
    ///
    /// 類似 Unity UI GridLayoutGroup，
    /// 但作用於一般 3D Transform。
    ///
    /// 支援：
    /// 1. Grid
    /// 2. Horizontal
    /// 3. Vertical
    ///
    /// 並支援：
    /// TopLeft / TopCenter / TopRight
    /// MiddleLeft / Center / MiddleRight
    /// BottomLeft / BottomCenter / BottomRight
    /// </summary>
    [ExecuteAlways]
    public class LayoutGroup3D : MonoBehaviour
    {
        public enum LayoutMode
        {
            Grid,
            Horizontal,
            Vertical
        }

        public enum Alignment
        {
            TopLeft,
            TopCenter,
            TopRight,

            MiddleLeft,
            Center,
            MiddleRight,

            BottomLeft,
            BottomCenter,
            BottomRight
        }

        public enum Axis
        {
            XZ,
            XY,
            YZ
        }

        [Header("Layout")]

        [Tooltip("Grid：依 Columns / Rows 排列\n" +
                 "Horizontal：全部水平排列\n" +
                 "Vertical：全部垂直排列")]
        public LayoutMode layoutMode = LayoutMode.Grid;

        [Header("Grid 設定")]

        [Min(1)]
        [Tooltip("Grid 每列最多幾個物件")]
        public int columns = 4;

        [Min(1)]
        [Tooltip("Grid 最多幾列")]
        public int rows = 1;

        [Tooltip("每個 Cell 的大小")]
        public Vector3 cellSize = Vector3.one;

        [Tooltip("Cell 之間的間距")]
        public Vector3 spacing = Vector3.zero;

        [Header("排列方向")]

        [Tooltip("Grid 使用哪一個 3D 平面")]
        public Axis axis = Axis.XZ;

        [Tooltip("排列相對於父物件的位置")]
        public Alignment alignment = Alignment.Center;

        [Header("更新設定")]

        [Tooltip("是否包含未啟用的子物件")]
        public bool includeInactive = true;

        [Tooltip("Inspector 修改參數時自動重新排列")]
        public bool autoUpdate = true;


        #region Public
        /// <summary>
        /// 重新排列所有子物件。
        /// </summary>
        [ContextMenu("Refresh Layout")]
        public void RefreshLayout()
        {
            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);

            switch (layoutMode)
            {
                case LayoutMode.Grid:
                    RefreshGrid();
                    break;

                case LayoutMode.Horizontal:
                    RefreshHorizontal();
                    break;

                case LayoutMode.Vertical:
                    RefreshVertical();
                    break;
            }
        }
        #endregion

        #region Grid
        private void RefreshGrid()
        {
            int index = 0;

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                if (!ShouldLayout(child))
                    continue;

                int row = index / columns;
                int column = index % columns;

                if (row >= rows)
                    break;

                child.localPosition = CalculateGridPosition(
                    row,
                    column
                );

                index++;
            }
        }

        private Vector3 CalculateGridPosition(int row, int column)
        {
            float stepX = cellSize.x + spacing.x;
            float stepY = cellSize.y + spacing.y;
            float stepZ = cellSize.z + spacing.z;

            Vector3 position = Vector3.zero;

            switch (axis)
            {
                case Axis.XZ:
                    {
                        position = new Vector3(
                            column * stepX,
                            0f,
                            -row * stepZ
                        );

                        break;
                    }

                case Axis.XY:
                    {
                        position = new Vector3(
                            column * stepX,
                            -row * stepY,
                            0f
                        );

                        break;
                    }

                case Axis.YZ:
                    {
                        position = new Vector3(
                            0f,
                            column * stepY,
                            -row * stepZ
                        );

                        break;
                    }
            }

            position += GetGridAlignmentOffset();

            return position;
        }
        #endregion

        #region Horizontal
        private void RefreshHorizontal()
        {
            int count = GetLayoutChildCount();
            if (count <= 0)
                return;

            float totalLength = GetHorizontalTotalLength(count);

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                if (!ShouldLayout(child))
                    continue;

                float position = GetHorizontalPosition(
                    i,
                    count,
                    totalLength
                );

                child.localPosition =
                    GetHorizontalVector(position);
            }
        }

        private float GetHorizontalTotalLength(int count)
        {
            switch (axis)
            {
                case Axis.XZ:
                case Axis.XY:
                    return count * cellSize.x +
                           Mathf.Max(0, count - 1) * spacing.x;

                case Axis.YZ:
                    return count * cellSize.y +
                           Mathf.Max(0, count - 1) * spacing.y;
            }

            return 0f;
        }

        private float GetHorizontalPosition(int index, int count, float totalLength)
        {
            float start = 0f;

            switch (alignment)
            {
                case Alignment.TopLeft:
                case Alignment.MiddleLeft:
                case Alignment.BottomLeft:

                    start = 0f;
                    break;

                case Alignment.TopCenter:
                case Alignment.Center:
                case Alignment.BottomCenter:

                    start = -totalLength * 0.5f;
                    break;

                case Alignment.TopRight:
                case Alignment.MiddleRight:
                case Alignment.BottomRight:

                    start = -totalLength;
                    break;
            }

            return start +
                   index * GetHorizontalStep();
        }

        private float GetHorizontalStep()
        {
            switch (axis)
            {
                case Axis.XZ:
                case Axis.XY:
                    return cellSize.x + spacing.x;

                case Axis.YZ:
                    return cellSize.y + spacing.y;
            }

            return 0f;
        }

        private Vector3 GetHorizontalVector(float position)
        {
            switch (axis)
            {
                case Axis.XZ:
                case Axis.XY:
                    return new Vector3(
                        position,
                        0f,
                        0f
                    );

                case Axis.YZ:
                    return new Vector3(
                        0f,
                        position,
                        0f
                    );
            }

            return Vector3.zero;
        }
        #endregion

        #region Vertical
        private void RefreshVertical()
        {
            int count = GetLayoutChildCount();

            if (count <= 0)
                return;

            float totalLength = GetVerticalTotalLength(count);

            int index = 0;

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                if (!ShouldLayout(child))
                    continue;

                float position = GetVerticalPosition(
                    index,
                    count,
                    totalLength
                );

                child.localPosition =
                    GetVerticalVector(position);

                index++;
            }
        }

        private float GetVerticalTotalLength(int count)
        {
            switch (axis)
            {
                case Axis.XZ:
                    return count * cellSize.z +
                           Mathf.Max(0, count - 1) * spacing.z;

                case Axis.XY:
                    return count * cellSize.y +
                           Mathf.Max(0, count - 1) * spacing.y;

                case Axis.YZ:
                    return count * cellSize.z +
                           Mathf.Max(0, count - 1) * spacing.z;
            }

            return 0f;
        }

        private float GetVerticalPosition(
            int index,
            int count,
            float totalLength)
        {
            float start = 0f;

            switch (alignment)
            {
                case Alignment.TopLeft:
                case Alignment.TopCenter:
                case Alignment.TopRight:

                    start = 0f;
                    break;

                case Alignment.MiddleLeft:
                case Alignment.Center:
                case Alignment.MiddleRight:

                    start = totalLength * 0.5f;
                    break;

                case Alignment.BottomLeft:
                case Alignment.BottomCenter:
                case Alignment.BottomRight:

                    start = totalLength;
                    break;
            }

            return start -
                   index * GetVerticalStep();
        }

        private float GetVerticalStep()
        {
            switch (axis)
            {
                case Axis.XZ:
                    return cellSize.z + spacing.z;

                case Axis.XY:
                    return cellSize.y + spacing.y;

                case Axis.YZ:
                    return cellSize.z + spacing.z;
            }

            return 0f;
        }

        private Vector3 GetVerticalVector(float position)
        {
            switch (axis)
            {
                case Axis.XZ:
                    return new Vector3(
                        0f,
                        0f,
                        position
                    );

                case Axis.XY:
                    return new Vector3(
                        0f,
                        position,
                        0f
                    );

                case Axis.YZ:
                    return new Vector3(
                        0f,
                        0f,
                        position
                    );
            }

            return Vector3.zero;
        }
        #endregion

        #region Grid Alignment
        private Vector3 GetGridAlignmentOffset()
        {
            float width;
            float height;

            switch (axis)
            {
                case Axis.XZ:

                    width =
                        columns * cellSize.x +
                        Mathf.Max(0, columns - 1) * spacing.x;

                    height =
                        rows * cellSize.z +
                        Mathf.Max(0, rows - 1) * spacing.z;

                    break;

                case Axis.XY:

                    width =
                        columns * cellSize.x +
                        Mathf.Max(0, columns - 1) * spacing.x;

                    height =
                        rows * cellSize.y +
                        Mathf.Max(0, rows - 1) * spacing.y;

                    break;

                case Axis.YZ:

                    width =
                        columns * cellSize.y +
                        Mathf.Max(0, columns - 1) * spacing.y;

                    height =
                        rows * cellSize.z +
                        Mathf.Max(0, rows - 1) * spacing.z;

                    break;

                default:

                    width = 0f;
                    height = 0f;
                    break;
            }

            float offsetX = 0f;
            float offsetY = 0f;

            switch (alignment)
            {
                case Alignment.TopLeft:

                    offsetX = 0f;
                    offsetY = 0f;
                    break;

                case Alignment.TopCenter:

                    offsetX = -width * 0.5f;
                    offsetY = 0f;
                    break;

                case Alignment.TopRight:

                    offsetX = -width;
                    offsetY = 0f;
                    break;

                case Alignment.MiddleLeft:

                    offsetX = 0f;
                    offsetY = height * 0.5f;
                    break;

                case Alignment.Center:

                    offsetX = -width * 0.5f;
                    offsetY = height * 0.5f;
                    break;

                case Alignment.MiddleRight:

                    offsetX = -width;
                    offsetY = height * 0.5f;
                    break;

                case Alignment.BottomLeft:

                    offsetX = 0f;
                    offsetY = height;
                    break;

                case Alignment.BottomCenter:

                    offsetX = -width * 0.5f;
                    offsetY = height;
                    break;

                case Alignment.BottomRight:

                    offsetX = -width;
                    offsetY = height;
                    break;
            }

            switch (axis)
            {
                case Axis.XZ:

                    return new Vector3(
                        offsetX,
                        0f,
                        offsetY
                    );

                case Axis.XY:

                    return new Vector3(
                        offsetX,
                        -offsetY,
                        0f
                    );

                case Axis.YZ:

                    return new Vector3(
                        0f,
                        offsetX,
                        offsetY
                    );
            }

            return Vector3.zero;
        }
        #endregion

        #region Helper
        private bool ShouldLayout(Transform child)
        {
            if (includeInactive)
                return true;

            return child.gameObject.activeSelf;
        }

        private int GetLayoutChildCount()
        {
            int count = 0;

            for (int i = 0; i < transform.childCount; i++)
            {
                if (ShouldLayout(transform.GetChild(i)))
                    count++;
            }

            return count;
        }
        #endregion

        // =========================================================
        // Editor
        // =========================================================

#if UNITY_EDITOR

        private void OnValidate()
        {
            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);

            if (!autoUpdate)
                return;

            EditorApplication.delayCall -= DelayedRefresh;
            EditorApplication.delayCall += DelayedRefresh;
        }

        private void DelayedRefresh()
        {
            if (this == null)
                return;

            if (Application.isPlaying)
                return;

            RefreshLayout();
        }

#endif
    }
}
