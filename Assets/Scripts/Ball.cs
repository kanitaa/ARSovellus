using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    int index;

    private void OnMouseDown()
    {
        GameManager.instance.SetListIndex(index);
        GameManager.instance.cat = false;
    }
    public void SetIndex(int _index)
    {
        index = _index;
    }
}
