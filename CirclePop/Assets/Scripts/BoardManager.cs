using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


/*
 * Board Manager Class
 * Should be attached to empty game object to be used as container for circles
 */
public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance = null;

    public Color[] colors;

    private Vector2Int _boardSize;
    private Vector2 _cellDimensions;
    private int _numCells;
    private Circle[] _cells;
    private bool _clickable; // if board is currently intractable

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Debug.LogWarning("Multiple board managers found!");
    }

    /// <summary>
    /// Initializes the board manager
    /// </summary>
    /// <param name="bS">board size (in circles)</param>
    /// <param name="sS">screen size (in world units)</param>
    public void Init(Vector2Int bS, Vector2 sS)
    {
        _boardSize = bS;
        // turn into dimensions of each cell
        _cellDimensions = new Vector2(
            (sS.x) / _boardSize.x,
            (sS.y) / _boardSize.y
        );
        Debug.Log(_cellDimensions.ToString("F3"));

        // populate the board ---
        // get prefab
        GameObject circlePref = Resources.Load<GameObject>("Prefabs/Circle");
        // create circles for each cell
        _numCells = _boardSize.x * _boardSize.y;
        _cells = new Circle[_numCells];
        for (int i = 0; i < _numCells; i++)
        {
            GameObject newCell = Instantiate(circlePref, transform);
            newCell.transform.localPosition = MapCoordToWorldPos(IndexToMapCoord(i));
            int colIn = Random.Range(0, colors.Length);
            _cells[i] = newCell.GetComponent<Circle>();
            _cells[i].Init(i,colIn, colors[colIn]);
        }

        _clickable = true;
    }

    /// <summary>
    /// Attempt to click circles.
    /// </summary>
    /// <param name="clickPos">Screen position of click</param>
    /// <returns>Returns number of circles popped, or -1 if invalid move</returns>
    public int MakeMove(Vector3 clickPos)
    {
        float startTime = Time.time;
        // == Initial checks == //
        // if not clickable then return invalid
        if (!_clickable)
        {
            Debug.Log("Not clickable");
            Debug.Log("Took " + ((Time.time - startTime) * 1000f).ToString("F3") + "ms");
            return -1;
        }

        // convert click position into a map coordinate
        Vector2 moveOrigin = WorldPosToMapCoord(clickPos);
        Debug.Log("Trying to make a move at " + moveOrigin);

        // if click is out of play area then return invalid 
        if (moveOrigin.x < 0 || moveOrigin.x >= _boardSize.x ||
            moveOrigin.y < 0 || moveOrigin.y >= _boardSize.y)
        {
            Debug.Log("Not in play area");
            Debug.Log("Took " + ((Time.time - startTime) * 1000f).ToString("F3") + "ms");
            return -1;
        }

        // == Start finding correct neighbors == //
        // set clickable to false while updating
        _clickable = false;

        List<Circle> openList = new List<Circle>();
        List<Circle> closedList = new List<Circle>() {_cells[MapCoordToIndex(moveOrigin)]};
        List<Circle> poppedList = new List<Circle>() {_cells[MapCoordToIndex(moveOrigin)]};
        // init the popped list with the clicked circle
        int numPopped = 1;
        int targetCol = _cells[MapCoordToIndex(moveOrigin)].ColIndex;

        // populate open list with valid neighbors
        AddNeighbors(ref openList, ref closedList, moveOrigin);

        // keep going until done
        while (openList.Count > 0)
        {
            // get first in list
            Circle curr = openList[0];

            // remove from open and add to closed
            openList.RemoveAt(0);
            closedList.Add(curr);

            // check if target color
            if (curr.ColIndex == targetCol)
            {
                //Debug.Log("Found match at "+IndexToMapCoord(curr.CellIndex));
                poppedList.Add(curr);
                ++numPopped;
                // add neighbors if proper color
                AddNeighbors(ref openList, ref closedList, IndexToMapCoord(curr.CellIndex));
            }
        }

        if (numPopped < GameManager.Instance.minPopAmt)
        {
            Debug.Log("Insufficient Pops (" + numPopped + ")");
            _clickable = true;
            Debug.Log("Took " + ((Time.time - startTime) * 1000f).ToString("F3") + "ms");
            return -1;
        }
        
        _clickable = true;
        UpdateBoard(poppedList);
        Debug.Log("Took " + ((Time.time - startTime) * 1000f).ToString("F3") + "ms");
        return numPopped;
    }

    private void UpdateBoard(List<Circle> poppedCircles)
    {
        // post pop
        // get starting points for updating board 
        int[] colOffsets = new int[_boardSize.x]; // number of circles popped in the column
        int[] rowStart = new int[_boardSize.x]; // the lowest popped circle
        for (int i = 0; i < _boardSize.x; i++)
            rowStart[i] = _boardSize.y;

        foreach (Circle circle in poppedCircles)
        {
            // get map coord of popped circle
            Vector2 mapCoord = IndexToMapCoord(circle.CellIndex);
            // log column popped is in
            ++colOffsets[(int) mapCoord.x];
            // see if its the lowest one
            if (mapCoord.y < rowStart[(int) mapCoord.x])
                rowStart[(int) mapCoord.x] = (int) mapCoord.y;
        }

        // go through every column
        for (int col = 0; col < _boardSize.x; col++)
        {
            //Debug.Log("Col " + col + ": " + colOffsets[col] + " popped, lowest = " + rowStart[col]);
            // skip if no popped circles
            if (colOffsets[col] <= 0) continue;

            // got to top of board from lowest point
            for (int row = rowStart[col]; row < _boardSize.y; row++)
            {
                // find new map coord for the circle to fall from
                Vector2 newMapCoord = new Vector2(col, row + colOffsets[col]);
                //Debug.Log("(" + col + ", " + row + ") => " + newMapCoord.ToString("F3"));

                int colIndex;
                // if map coord is in the board: steal color from circle in drop location
                if (newMapCoord.y < _boardSize.y)
                {
                    colIndex = _cells[MapCoordToIndex(newMapCoord)].ColIndex;
                }
                else // otherwise: generate new random color
                {
                    colIndex = Random.Range(0, colors.Length);
                }

                // get the circle you are actually updating
                Circle curr = _cells[MapCoordToIndex(new Vector2(col, row))];
                // set the color
                curr.SetColor(colIndex, colors[colIndex]);
                // drop it
                curr.Drop(MapCoordToWorldPos(newMapCoord).y);
            }
        }
    }

    /* == HELPERS == */
    // add neighbors to list
    private void AddNeighbors(ref List<Circle> open, ref List<Circle> closed, Vector2 or)
    {
        Vector2[] dirs = {Vector2.up, Vector2.down, Vector2.left, Vector2.right};
        foreach (var t in dirs)
        {
            Vector2 testCoord = or + t;
            // skip if out of bounds
            if (testCoord.x < 0 || testCoord.x >= _boardSize.x ||
                testCoord.y < 0 || testCoord.y >= _boardSize.y)
                continue;

            // check if valid and not already in either list
            Circle cell = _cells[MapCoordToIndex(testCoord)];
            if (!closed.Contains(cell) && !open.Contains(cell))
            {
                open.Add(cell);
            }
        }
    }

    /* == CONVERTERS == */
    // converts int index into Vector2 map coord (w/in board dimensions)
    Vector2 IndexToMapCoord(int index)
    {
        return new Vector2(
            index % _boardSize.x,
            Mathf.FloorToInt((float)index / (float)_boardSize.x)
        );
    }

    // converts map coord to world position
    Vector3 MapCoordToWorldPos(Vector2 mapCoord)
    {
        return new Vector3(
            _cellDimensions.x * (mapCoord.x + 0.5f),
            _cellDimensions.y * (mapCoord.y + 0.5f),
            0f
        );
    }

    // converts map coord into an index
    int MapCoordToIndex(Vector2 mapCoord)
    {
        return (int) (mapCoord.x + mapCoord.y * _boardSize.x);
    }

    // converts world position into map coordinate
    Vector2 WorldPosToMapCoord(Vector3 worldPos)
    {
        return new Vector2(
            Mathf.Floor((worldPos.x - transform.position.x) / _cellDimensions.x),
            Mathf.Floor((worldPos.y - transform.position.y) / _cellDimensions.y)
        );
    }
}
