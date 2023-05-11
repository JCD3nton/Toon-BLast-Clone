using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDName : MonoBehaviour
{
    public int ID;

    public enum CubeType { Normal, Switch, Bomb, DiscoBall}
    public CubeType TypeOfCube;
    public Sprite[] ChangeSprites;
    public Sprite Bomb, DiscoBall;
    public Sprite[] Switch;
   [HideInInspector] public bool IsDiscoBall;
   [HideInInspector] public bool IsSwitchHorizontal, IsSwitchVertical;
   [HideInInspector] public bool IsBomb;
   [HideInInspector] public bool IsRunChangeSprites;
   [HideInInspector] public bool IsSpecialCube = false;

    private void Update()
    {
        if (IsRunChangeSprites)
        {
            switch (TypeOfCube)
            {
                case CubeType.Normal:
                    GetComponent<SpriteRenderer>().sprite = ChangeSprites[0];
                    break;
                case CubeType.Switch:
                    GetComponent<SpriteRenderer>().sprite = ChangeSprites[1];
                    break;
                case CubeType.Bomb:
                    GetComponent<SpriteRenderer>().sprite = ChangeSprites[2];
                    break;
                case CubeType.DiscoBall:
                    GetComponent<SpriteRenderer>().sprite = ChangeSprites[3];
                    break;
                default:
                    break;
            }
        }
        
    }
}
