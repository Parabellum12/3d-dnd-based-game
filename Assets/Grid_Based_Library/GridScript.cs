using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridClass<TGridObject>
{

    public event EventHandler<OnGridValueChangedEventArgs> onGridValueChanged;
    public class OnGridValueChangedEventArgs : EventArgs
    {
        public int x, y;
    }
    private static bool showDebug = false;

    public int width;
    public int height;
    private float cellSize;
    private Vector3 originPos;
    public TGridObject[,] gridArray;
    private TextMeshPro[,] debugGridArray;
    GameObject gameobject;
    public GridClass(Transform parent, int Width, int Height, float cellSize, Vector3 originPos, System.Func<int, int, TGridObject> createGridObject)
    {
        this.width = Width;
        this.height = Height;
        this.cellSize = cellSize;
        this.originPos = originPos;
        gridArray = new TGridObject[width, height];
        debugGridArray = new TextMeshPro[width, height];


        for (int i = 0; i < gridArray.GetLength(0); i++)
        {
            for (int j = 0; j < gridArray.GetLength(1); j++)
            {
                gridArray[i, j] = createGridObject(i, j);
            }
        }

        if (showDebug)
        {
            Debug.Log(width + " " + height);
            gameobject = new GameObject("GridHolder");
            gameobject.transform.parent = parent;
            for (int i = 0; i < gridArray.GetLength(0); i++)
            {
                for (int j = 0; j < gridArray.GetLength(1); j++)
                {
                    debugGridArray[i, j] = UtilClass.createWorldText(gridArray[i, j]?.ToString(), parent, getWorldPosition(i, j) + new Vector3(cellSize, cellSize) * 0.5f, 30, Color.white, TMPro.TextContainerAnchors.Middle);
                    Debug.DrawLine(getWorldPosition(i, j), getWorldPosition(i, j + 1), Color.white, 10000f);
                    Debug.DrawLine(getWorldPosition(i, j), getWorldPosition(i + 1, j), Color.white, 10000f);
                }
            }

            Debug.DrawLine(getWorldPosition(0, height), getWorldPosition(width, height), Color.white, 10000f);
            Debug.DrawLine(getWorldPosition(width, 0), getWorldPosition(width, height), Color.white, 10000f);
            onGridValueChanged += (object sender, OnGridValueChangedEventArgs eventArgs) =>
            {
                debugGridArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
            };
        }

    }

    public Vector3 getWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPos;
    }

    public void GetXY(Vector3 worldPos, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPos - originPos).x / cellSize);
        y = Mathf.FloorToInt((worldPos - originPos).y / cellSize);
    }



    public void triggerGridObjectChanged(int x, int y)
    {
        if (onGridValueChanged != null)
        {
            onGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, y = y });
        }
    }

    public void setGridObject(Vector3 worldPos, TGridObject value)
    {
        int x, y;
        GetXY(worldPos, out x, out y);
        setGridObject(x, y, value);
    }
    public void setGridObject(int x, int y, TGridObject value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            if (onGridValueChanged != null)
            {
                onGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, y = y });
            }
        }
    }

    public TGridObject getGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(TGridObject);
        }
    }

    public TGridObject getGridObject(Vector3 worldPos)
    {
        int x, y;
        GetXY(worldPos, out x, out y);
        return getGridObject(x, y);
    }

    public int getWidth()
    {
        return width;
    }

    public int getHeight()
    {
        return height;
    }

    public float getCellSize()
    {
        return cellSize;
    }

    public bool inBounds(int x, int y)
    {
        if (x <= width - 1 && y <= height - 1 && x >= 0 && y >= 0)
        {
            return true;
        }
        return false;
    }
}



