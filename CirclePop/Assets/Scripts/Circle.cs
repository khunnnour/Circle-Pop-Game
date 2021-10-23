using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    const float START_DELAY = 0.1f;
    const float TIME_TO_FALL = 0.2f;

    private SpriteRenderer _renderer;
    private Vector3 _defaultPos;
    private int _colIndex = -1;
    private int _index;

    public int ColIndex => _colIndex;
    public int CellIndex => _index;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void Init(int index, int colInd, Color col)
    {
        _index = index;
        SetColor(colInd, col);
        _defaultPos = transform.localPosition;
    }

    public void SetColor(int index, Color col)
    {
        _colIndex = index;
        _renderer.color = col;
    }

    public void Drop(float newY)
    {
        var position = transform.localPosition;
        position = new Vector3(position.x,newY,0);
        transform.localPosition = position;
        StartCoroutine("DropToPosition", position);
    }

    IEnumerator DropToPosition(Vector3 start)
    {
        // delayed start
        yield return new WaitForSeconds(START_DELAY);

        float t = 0.0f;
        while (t < TIME_TO_FALL)
        {
            transform.localPosition = Vector3.Lerp(start, _defaultPos, t / TIME_TO_FALL);
            t += Time.deltaTime;
            yield return null;
        }
    }
}
