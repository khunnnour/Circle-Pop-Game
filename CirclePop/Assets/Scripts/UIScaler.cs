using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScaler : MonoBehaviour
{
	[Range(0f,1f)]
	public float startPanelWidth=0.75f;
	public RectTransform startPanel;
	public RectTransform gameUIPanel;

	// Start is called before the first frame update
	void Awake()
	{
		// set the panel size
		startPanel.sizeDelta = new Vector2(
			Screen.width * startPanelWidth,
			Screen.width * startPanelWidth * 0.525f
			);
		float off = startPanel.sizeDelta.y * 0.45f * 0.35f;
		// set the button parameters
		startPanel.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(0,-off);
		startPanel.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(
			startPanel.sizeDelta.y * 0.30f * 4.2f,
			startPanel.sizeDelta.y * 0.30f
			);
		// set dropdown size
		startPanel.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(
			startPanel.sizeDelta.y * 0.25f * 1.42f,
			startPanel.sizeDelta.y * 0.25f
			);
		startPanel.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(10f, off);
		Debug.Log(startPanel.sizeDelta);
		// dropdown label
		startPanel.GetChild(2).GetComponent<RectTransform>().sizeDelta = new Vector2(
			100f,
			startPanel.sizeDelta.y * 0.25f
			);
		startPanel.GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector2(-10f, off);
		Debug.Log(startPanel.sizeDelta);
	}
}
