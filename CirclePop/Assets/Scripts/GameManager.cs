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
	public TMP_Text scoreText;
	public TMP_Text movesText;
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
		RectTransform sPRect = startPanel.GetComponent<RectTransform>();
		Vector2 dim = sPRect.rect.max - sPRect.rect.min;
		Vector2 cen = sPRect.anchoredPosition;
		Vector2 bottomLeft	= mainCam.ScreenToWorldPoint(new Vector3(0,		cen.y + 0.5f * (Screen.height - dim.y), 0));
		Vector2 topRight	= mainCam.ScreenToWorldPoint(new Vector3(dim.x, cen.y + 0.5f * (Screen.height + dim.y), 0));
		//Debug.Log(bottomLeft.ToString("F3") + "  &  " + topRight.ToString("F3"));

		_playAreaSize = topRight - bottomLeft;

		// place board
		BoardManager.Instance.transform.position = bottomLeft;
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
		if (_playing && Input.GetMouseButtonDown(0))
		{
			int val = BoardManager.Instance.MakeMove(mainCam.ScreenToWorldPoint(Input.mousePosition));
			//Debug.Log("MakeMove returned " + val);
			if (val > 0)
			{
				_score += Mathf.RoundToInt(0.08f * _bonusScale * Mathf.Pow(val, 1.5f)) + val;

				if (val >= extraMoveAmt)
					++_moves;
				else
					--_moves;

				if (_moves <= 0) EndGame();

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
		//startPanel.SetActive(false);
		StartCoroutine(HideStartPanel());
		
		// clear out the board
		BoardManager.Instance.ClearBoard();

		int numCol = int.Parse(colDrop.options[colDrop.value].text);
		BoardManager.Instance.Init(boardSize, _playAreaSize, numCol);

		switch (numCol)
		{
			case 3:
				extraMoveAmt = 20;
				_bonusScale = 0.3f;
				_moves = 25;
				break;
			case 4:
				extraMoveAmt = 15;
				_bonusScale = 1.55f;
				_moves = 30;
				break;
			case 5:
				extraMoveAmt = 10;
				_bonusScale = 2.05f;
				_moves = 35;
				break;
		}

		//_bonusScale = numCol *numCol * 0.075f;

		// set score up
		_score = 0;
		// set moves up
		//_moves = startingMoves+numCol;

		UpdateUI();
	}

	private void EndGame()
	{
		StartCoroutine(ShowStartPanel());
		//startPanel.SetActive(true);
	}

	IEnumerator ShowStartPanel()
	{
		yield return new WaitForSeconds(0.05f);

		float fadeInTime = 0.75f;
		float t = 0;

		RectTransform buttonBox = startPanel.transform.GetChild(0).GetComponent<RectTransform>();
		while (t < fadeInTime)
		{
			// increment time
			t += Time.deltaTime;

			// fade in overlay
			Color newCol = startPanel.GetComponent<Image>().color;
			newCol.a = 0.6f * (t / fadeInTime);
			startPanel.GetComponent<Image>().color = newCol;

			// transition start box
			Vector2 newPos = buttonBox.anchoredPosition;
			newPos.y = 700 + (0 - 700) * t / fadeInTime;
			buttonBox.anchoredPosition = newPos;

			//Vector2 newSize = buttonBox.sizeDelta;
			//newSize.x = 200 + (350 - 200) * t / fadeInTime;
			//newSize.y = 100 + (175 - 100) * t / fadeInTime;
			//buttonBox.sizeDelta = newSize;
		}

		_playing = false;
	}
	IEnumerator HideStartPanel()
	{
		yield return new WaitForSeconds(0.05f);

		float fadeOutTime = 0.75f;
		float t = 0;

		RectTransform buttonBox = startPanel.transform.GetChild(0).GetComponent<RectTransform>();
		while (t < fadeOutTime)
		{
			// increment time
			t += Time.deltaTime;

			// fade out overlay
			Color newCol = startPanel.GetComponent<Image>().color;
			newCol.a = 0.6f + (0f - 0.6f) * t / fadeOutTime;
			startPanel.GetComponent<Image>().color = newCol;

			// transition start box
			Vector2 newPos = buttonBox.anchoredPosition;
			newPos.y = 700 + (700 - 0) * t / fadeOutTime;
			buttonBox.anchoredPosition = newPos;
			
			//Vector2 newSize = buttonBox.sizeDelta;
			//newSize.x = 350 + (200 - 350) * t / fadeOutTime;
			//newSize.y = 175 + (100 - 175) * t / fadeOutTime;
			//buttonBox.sizeDelta = newSize;
		}

		_playing = true;
	}
}
