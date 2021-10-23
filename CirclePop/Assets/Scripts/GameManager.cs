using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public GameObject startPanel;
    public TMP_Dropdown colDrop;

    private Vector2 _playAreaSize;
    private int _score;
    private int _moves;
    private bool _playing;
    private float _bonusScale;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get dimensions of play area
        Vector2 bottomLeft = mainCam.ScreenToWorldPoint(new Vector3(0,100,0));
        Vector2 topRight = mainCam.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,0));
        Debug.Log(bottomLeft+"  &  "+topRight);
        _playAreaSize = topRight - bottomLeft;
            
        // place board
        BoardManager.Instance.transform.position= bottomLeft;
        // init board
        //BoardManager.Instance.Init(boardSize,topRight-bottomLeft);

        // set score up
        _score = 0;
        // set moves up
        _moves = startingMoves;
        
        UpdateUI();
    }

    private void Update()
    {
        // on left click
        if (_playing&&Input.GetMouseButtonDown(0))
        {
            int val = BoardManager.Instance.MakeMove(mainCam.ScreenToWorldPoint(Input.mousePosition));
            Debug.Log("MakeMove returned "+val);
            if (val > 0)
            {
                _score += Mathf.RoundToInt(0.075f*_bonusScale * Mathf.Pow(val, 1.4f)) + val;

                if (val >= extraMoveAmt)
                    ++_moves;
                else
                    --_moves;
                
                if(_moves<=0) EndGame();
                
                UpdateUI();
            }
        }
    }

    private void UpdateUI()
    {
        scoreText.text = _score.ToString();
        movesText.text = _moves.ToString();
    }

    public void StartGame()
    {
        startPanel.SetActive(false);
        
        int numCol= int.Parse(colDrop.options[colDrop.value].text);
        BoardManager.Instance.Init(boardSize,_playAreaSize,numCol);
        
        extraMoveAmt = 21 - Mathf.RoundToInt((float)numCol*2.5f);

        _bonusScale = numCol * 0.25f;
        
        // set score up
        _score = 0;
        // set moves up
        _moves = startingMoves;
        
        _playing = true;
        UpdateUI();
    }

    private void EndGame()
    {
        BoardManager.Instance.ClearBoard();
        startPanel.SetActive(true);
        _playing = false;
    }
}
