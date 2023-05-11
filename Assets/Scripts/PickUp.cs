using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PickUp : MonoBehaviour
{
    public int x, y;

    // Calculate #1 Clicked on that game object
    private void OnMouseDown()
    {
        Debug.Log($"Clicked Cube Coordinates: ({x}, {y})");
        Debug.Log($"Cube Dictionary Value: {GameManager.Item[new Tuple<int, int>(x, y)]}");
        if (!GameManager.IsClick)
        {
            if (GetComponent<IDName>().IsSpecialCube) // True ==> Special cube
            {
                // TODO: Blank ==> Next time
                GameManager.IsClick = true;
                var i = GetComponent<IDName>();
                //Check Disco Ball Cube
                if(i.IsDiscoBall && !i.IsBomb && !i.IsSwitchHorizontal && !i.IsSwitchVertical)
                {
                    FindObjectOfType<GameManager>().CalculateCubeForDiscoBall(i);
                }
                //Check Bomb Cube
                if (!i.IsDiscoBall && i.IsBomb && !i.IsSwitchHorizontal && !i.IsSwitchVertical)
                {
                    FindObjectOfType<GameManager>().CalculateCubeForBomb(i);
                }
                //Check Switch Horizontal Cube
                if (!i.IsDiscoBall && !i.IsBomb && i.IsSwitchHorizontal && !i.IsSwitchVertical)
                {
                    FindObjectOfType<GameManager>().CalculateCubeForSwitchHorizontal(i);
                }
                //Check Switch Vertical Cube
                if (!i.IsDiscoBall && !i.IsBomb && !i.IsSwitchHorizontal && i.IsSwitchVertical)
                {
                    FindObjectOfType<GameManager>().CalculateCubeForSwitchVertical(i);
                }
            }
            else
            {
                GameManager.Calculate_CubeCallBack(GetComponent<PickUp>(), GetComponent<IDName>());
                FindObjectOfType<GameManager>().Delete_Callback();
                GameManager.IsClick = true;
            }
        }
    }

    // Calculate #2
    public void Continue_CalculateCallBack()
    {
        GameManager.Calculate_CubeCallBack(GetComponent<PickUp>(), GetComponent<IDName>());
    }
    // Calculate #3 // ChangeSprites
    public void Continue_CalculateCallBack(Tuple<int, int> id)
    {
        GameManager.Calculate_CubeCallBack(GetComponent<PickUp>(), GetComponent<IDName>(), id);
    }
}
