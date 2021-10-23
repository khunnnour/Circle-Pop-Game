using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;
    
    [Header("Parameters")] 
    public Vector2Int boardSize = new Vector2Int(8, 10);
    public int startingMoves = 20;
    public int extraMoveAmt = 7;
    public int minPopAmt = 3;

    [Header("Object References")] 
    public Camera mainCam;
    public Text scoreText;
    public Text movesText;

    private int _score;
    private int _moves;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get dimensions of play area
        Vector2 bottomLeft = mainCam.ScreenToWorldPoint(new Vector3(0,50,0));
        Vector2 topRight = mainCam.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,0));
        Debug.Log(bottomLeft+"  &  "+topRight);
        
        // place board
        BoardManager.Instance.transform.position= bottomLeft;
        // init board
        BoardManager.Instance.Init(boardSize,topRight-bottomLeft);

        // set score up
        _score = 0;
        // set moves up
        _moves = startingMoves;
        
        UpdateUI();
    }

    private void Update()
    {
        // on left click
        if (Input.GetMouseButtonDown(0))
        {
            int val = BoardManager.Instance.MakeMove(mainCam.ScreenToWorldPoint(Input.mousePosition));
            Debug.Log("MakeMove returned "+val);
            if (val > 0)
            {
                _score += Mathf.RoundToInt(0.06f * Mathf.Pow(val, 1.4f)) + val;

                if (val >= extraMoveAmt)
                    ++_moves;
                else
                    --_moves;
                
                UpdateUI();
            }
        }
    }

    private void UpdateUI()
    {
        scoreText.text = _score.ToString();
        movesText.text = _moves.ToString();
    }
}
