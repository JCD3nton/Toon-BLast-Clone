using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public int Row, Column;
    public int PaddingTop;
    public int RandomColorCount;
    [SerializeField] private GameObject[] NormalCubePrefabs;
    [SerializeField] private Transform SetBg, SetItem;
    public static Dictionary<Tuple<int, int>, PickUp> Item = new Dictionary<Tuple<int, int>, PickUp>();
    private static List<GameObject> DeleteObject = new List<GameObject>();
    public static Dictionary<Tuple<int, int>, List<GameObject>> Square = new Dictionary<Tuple<int, int>, List<GameObject>>();

    // Setup time to click on cube
    [SerializeField] private float TimeToClick;
    private float TimeToClickSet;
    public static bool IsClick;

    //Setup sound effects
    public AudioClip BreakCubeSound, DiscoBallSound, SwitchSound, BombSound;
    public AudioSource AsPlayer;

    private void Awake()
    {
        TimeToClickSet = TimeToClick;

        Spawn_FillGrid();
        
        // #1 Start Game
        StartCoroutine(Wait(0.1f, () =>
        {
            Start_Square();
        }));
    }

    private void Spawn_FillGrid()
    {
        // Setup Cube Item # 1
        for(int x = 0; x < Row; x++)//Generate Horizontal  Vector 2 Axis
        {
            for(int y = 0; y < Column; y++)//Generate Vertical  Vector 2 Axis
            {
                var clone = Instantiate(NormalCubePrefabs[UnityEngine.Random.Range(0, RandomColorCount)], new Vector2(x, y), Quaternion.identity);
                clone.transform.SetParent(SetItem);
                clone.AddComponent<CapsuleCollider2D>(); 
                clone.tag = "Item";
                clone.AddComponent<PickUp>();
                clone.name = x.ToString() + "." + y.ToString();
                clone.GetComponent<PickUp>().x = x;
                clone.GetComponent<PickUp>().y = y;
                Item.Add(new Tuple<int, int>(x, y), clone.GetComponent<PickUp>());
                clone.GetComponent<IDName>().IsRunChangeSprites = true;
            }
        }
        // Setup Bg # 2
        for (int x = 0; x < Row; x++)
        {
            for (int y = 0; y < Column; y++)
            {
                var clone = Instantiate(NormalCubePrefabs[UnityEngine.Random.Range(0, NormalCubePrefabs.Length)], new Vector2(x, y), Quaternion.identity);
                clone.transform.SetParent(SetBg);
                Destroy(clone.GetComponent<SpriteRenderer>());
                clone.AddComponent<BoxCollider2D>();
                clone.GetComponent<BoxCollider2D>().isTrigger = true;
                clone.AddComponent<Change>();
                clone.name = x.ToString() + "." + y.ToString();
                clone.GetComponent<Change>().x = x;
                clone.GetComponent<Change>().y = y;
                clone.GetComponent<Rigidbody2D>().gravityScale = 0f;
                clone.GetComponent<BoxCollider2D>().size = new Vector2(0.3f, 0.3f);
                clone.GetComponent<BoxCollider2D>().enabled = false;
            }
        }
        // Setup BG pos
        Invoke("Change_BgPos", 1f);
    }

    private void Change_BgPos()
    {
        for(int i = 0; i < SetBg.childCount; i++)
        {
            var _change = SetBg.GetChild(i).GetComponent<Change>();
            SetBg.GetChild(i).transform.position = Item[new Tuple<int, int>(_change.x, _change.y)].transform.position;
        }
    }

    private void Start_Square()
    {
        foreach(var item in Item.Values)
        {
            if (!item.GetComponent<IDName>().IsSpecialCube) // False
            {
                Square.Add(new Tuple<int, int>(item.x, item.y), new List<GameObject>());
                Calculate_CubeCallBack(Item[new Tuple<int, int>(item.x, item.y)],
                    Item[new Tuple<int, int>(item.x, item.y)].GetComponent<IDName>(),
                    new Tuple<int, int>(item.x, item.y));
            }
        }
        // Change Calculate Change Sprites
        foreach (var item in Square.Values)
        {
            if(item.Count == 1 || item.Count == 2 || item.Count == 3 || item.Count == 4)
            {
                for(int i = 0; i < item.Count; i++)
                {
                    item[i].GetComponent<IDName>().TypeOfCube = IDName.CubeType.Normal;
                }
            }
            else if (item.Count == 5 || item.Count == 6 || item.Count == 7 )
            {
                for (int i = 0; i < item.Count; i++)
                {
                    item[i].GetComponent<IDName>().TypeOfCube = IDName.CubeType.Switch;
                }
            }
            else if (item.Count == 8 || item.Count == 9 || item.Count == 10)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    item[i].GetComponent<IDName>().TypeOfCube = IDName.CubeType.Bomb;
                }
            }
            else if (item.Count > 10)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    item[i].GetComponent<IDName>().TypeOfCube = IDName.CubeType.DiscoBall;
                }
            }

        }
        // Clear Square
        Square.Clear();
    }

    private IEnumerator Wait(float time, Action Call) // Wait for time
    {
        yield return new WaitForSeconds(time);
        if(Call != null)
        {
            Call.Invoke();
        }
    }

    public static void Calculate_CubeCallBack(PickUp p, IDName i, Tuple<int, int> id)
    {
        var Top = new Tuple<int, int>(p.x, p.y + 1);
        var Down = new Tuple<int, int>(p.x, p.y - 1);
        var Right = new Tuple<int, int>(p.x + 1, p.y);
        var Left = new Tuple<int, int>(p.x - 1, p.y);
        //Break Top
        if (Item.ContainsKey(Top))
        {
            if (i.ID == Item[Top].GetComponent<IDName>().ID)
            {
                if (!Square[id].Contains(i.gameObject) && !i.IsSpecialCube)
                    Square[id].Add(i.gameObject);
                if (!Square[id].Contains(Item[Top].gameObject) && !Item[Top].GetComponent<IDName>().IsSpecialCube)
                {
                    Square[id].Add(Item[Top].gameObject);
                    Item[Top].Continue_CalculateCallBack(id);
                }

            }
        }
        //Break Down
        if (Item.ContainsKey(Down))
        {
            if (i.ID == Item[Down].GetComponent<IDName>().ID)
            {
                if (!Square[id].Contains(i.gameObject) && !i.IsSpecialCube)
                    Square[id].Add(i.gameObject);
                if (!Square[id].Contains(Item[Down].gameObject) && !Item[Down].GetComponent<IDName>().IsSpecialCube)
                {
                    Square[id].Add(Item[Down].gameObject);
                    Item[Down].Continue_CalculateCallBack(id);
                }
            }
        }
        //Break Right
        if (Item.ContainsKey(Right))
        {
            if (i.ID == Item[Right].GetComponent<IDName>().ID)
            {
                if (!Square[id].Contains(i.gameObject) && !i.IsSpecialCube)
                    Square[id].Add(i.gameObject);
                if (!Square[id].Contains(Item[Right].gameObject) && !Item[Right].GetComponent<IDName>().IsSpecialCube)
                {
                    Square[id].Add(Item[Right].gameObject);
                    Item[Right].Continue_CalculateCallBack(id);
                }
            }
        }
        //Break Left
        if (Item.ContainsKey(Left))
        {
            if (i.ID == Item[Left].GetComponent<IDName>().ID)
            {
                if (!Square[id].Contains(i.gameObject) && !i.IsSpecialCube)
                    Square[id].Add(i.gameObject);
                if (!Square[id].Contains(Item[Left].gameObject) && !Item[Left].GetComponent<IDName>().IsSpecialCube)
                {
                    Square[id].Add(Item[Left].gameObject);
                    Item[Left].Continue_CalculateCallBack(id);
                }
            }

        }
    }
    public static void Calculate_CubeCallBack(PickUp p, IDName i)
    {
        var Top = new Tuple<int, int>(p.x, p.y + 1);
        var Down = new Tuple<int, int>(p.x, p.y - 1);
        var Right = new Tuple<int, int>(p.x + 1, p.y);
        var Left = new Tuple<int, int>(p.x - 1, p.y);
        //Break Top
        if (Item.ContainsKey(Top))
        {
            if(!Item[Top].GetComponent<IDName>().IsSpecialCube)

            if(i.ID == Item[Top].GetComponent<IDName>().ID)
            {
                if (!DeleteObject.Contains(i.gameObject))
                    DeleteObject.Add(i.gameObject);
                if (!DeleteObject.Contains(Item[Top].gameObject))
                {
                    DeleteObject.Add(Item[Top].gameObject);
                    Item[Top].Continue_CalculateCallBack();
                }
                    
            }
        }
        //Break Down
        if (Item.ContainsKey(Down))
        {
                if (!Item[Down].GetComponent<IDName>().IsSpecialCube)

                if (i.ID == Item[Down].GetComponent<IDName>().ID)
            {
                if (!DeleteObject.Contains(i.gameObject))
                    DeleteObject.Add(i.gameObject);
                if (!DeleteObject.Contains(Item[Down].gameObject))
                {
                    DeleteObject.Add(Item[Down].gameObject);
                    Item[Down].Continue_CalculateCallBack();
                }
            }
        }
        //Break Right
        if (Item.ContainsKey(Right))
        {
            if (!Item[Right].GetComponent<IDName>().IsSpecialCube)

                if (i.ID == Item[Right].GetComponent<IDName>().ID)
            {
                if (!DeleteObject.Contains(i.gameObject))
                    DeleteObject.Add(i.gameObject);
                if (!DeleteObject.Contains(Item[Right].gameObject))
                {
                    DeleteObject.Add(Item[Right].gameObject);
                    Item[Right].Continue_CalculateCallBack();
                }
            }
        }
        //Break Left
        if (Item.ContainsKey(Left))
        {
            if (!Item[Left].GetComponent<IDName>().IsSpecialCube)

                if (i.ID == Item[Left].GetComponent<IDName>().ID)
            {
                if (!DeleteObject.Contains(i.gameObject))
                    DeleteObject.Add(i.gameObject);
                if (!DeleteObject.Contains(Item[Left].gameObject))
                {
                    DeleteObject.Add(Item[Left].gameObject);
                    Item[Left].Continue_CalculateCallBack();
                }
            }

        }
    }
    public static bool Calculate_CubeCallBack(PickUp p)
    {
        var Top = new Tuple<int, int>(p.x, p.y + 1);
        var Down = new Tuple<int, int>(p.x, p.y - 1);
        var Right = new Tuple<int, int>(p.x + 1, p.y);
        var Left = new Tuple<int, int>(p.x - 1, p.y);
        
        //Break Top
        if (Item.ContainsKey(Top))
        {
            if (!Item[Top].GetComponent<IDName>().IsSpecialCube)

                if (p.GetComponent<IDName>().ID == Item[Top].GetComponent<IDName>().ID)
                {
                    return true;
                }
        }
        //Break Down
        if (Item.ContainsKey(Down))
        {
            if (!Item[Down].GetComponent<IDName>().IsSpecialCube)

                if (p.GetComponent<IDName>().ID == Item[Down].GetComponent<IDName>().ID)
                {
                    return true;
                }
        }
        //Break Right
        if (Item.ContainsKey(Right))
        {
            if (!Item[Right].GetComponent<IDName>().IsSpecialCube)

                if (p.GetComponent<IDName>().ID == Item[Right].GetComponent<IDName>().ID)
                {
                    return true;
                }
        }
        //Break Left
        if (Item.ContainsKey(Left))
        {
            if (!Item[Left].GetComponent<IDName>().IsSpecialCube)

                if (p.GetComponent<IDName>().ID == Item[Left].GetComponent<IDName>().ID)
                {
                    return true;
                }
        }
        return false;
    }
    public void Delete_Callback()
    {
        Invoke("Delete_Cubes", 0.1f);
        //Setup new cubes
        Invoke("EnableBoxCollider2DCallBack", 1f);
     }
    private void Delete_Cubes()
    {
        for (int i = 0; i < DeleteObject.Count; i++)
        {
            if (i == 0) // First element  or pos when we clicked on it
            {
                // Works only for DiscoBall cube
                if (DeleteObject[0].GetComponent<IDName>().TypeOfCube == IDName.CubeType.DiscoBall)
                {
                    DeleteObject[0].GetComponent<IDName>().IsRunChangeSprites = false;
                    DeleteObject[0].GetComponent<SpriteRenderer>().sprite =
                    DeleteObject[0].GetComponent<IDName>().DiscoBall;
                    DeleteObject[0].GetComponent<IDName>().IsSpecialCube = true; ;
                    DeleteObject[0].GetComponent<IDName>().IsDiscoBall = true;

                    // Sound
                    AsPlayer.clip = DiscoBallSound;
                    AsPlayer.Play();
                }
                // Works only for Switch cube
                else if (DeleteObject[0].GetComponent<IDName>().TypeOfCube == IDName.CubeType.Switch)
                {
                    DeleteObject[0].GetComponent<IDName>().IsRunChangeSprites = false;
                    int ran = UnityEngine.Random.Range(0, DeleteObject[0].GetComponent<IDName>().Switch.Length);

                    if (ran == 0)
                        DeleteObject[0].GetComponent<IDName>().IsSwitchVertical = true;
                    if (ran == 1)
                        DeleteObject[0].GetComponent<IDName>().IsSwitchHorizontal = true;

                    DeleteObject[0].GetComponent<SpriteRenderer>().sprite =
                    DeleteObject[0].GetComponent<IDName>().Switch[ran];
                    DeleteObject[0].GetComponent<IDName>().IsSpecialCube = true; ;

                    // Sound
                    AsPlayer.clip = SwitchSound;
                    AsPlayer.Play();
                }
                // Works only for Bomb cube
                else if (DeleteObject[0].GetComponent<IDName>().TypeOfCube == IDName.CubeType.Bomb)
                {
                    DeleteObject[0].GetComponent<IDName>().IsRunChangeSprites = false;
                    DeleteObject[0].GetComponent<SpriteRenderer>().sprite =
                    DeleteObject[0].GetComponent<IDName>().Bomb;
                    DeleteObject[0].GetComponent<IDName>().IsSpecialCube = true; ;
                    DeleteObject[0].GetComponent<IDName>().IsBomb = true;

                    // Sound
                    AsPlayer.clip = BombSound;
                    AsPlayer.Play();
                }
                else
                {
                    SpawnBack(DeleteObject[i].GetComponent<PickUp>());
                    Destroy(DeleteObject[i]);

                    // Sound
                    AsPlayer.clip = BreakCubeSound;
                    AsPlayer.Play();
                }
            }
            else
            {
                SpawnBack(DeleteObject[i].GetComponent<PickUp>());
                Destroy(DeleteObject[i]);
            }
        }
        Item.Clear();
        DeleteObject.Clear();
    }
    public void Delete_Invoke()
    {
        Invoke("Delete_CubesInvoke", 0.1f);
        //Setup new cubes
        Invoke("EnableBoxCollider2DCallBack", 1f);
    }
    private void Delete_CubesInvoke()
    {
        for (int i = 0; i < DeleteObject.Count; i++)
        {
            SpawnBack(DeleteObject[i].GetComponent<PickUp>());
            Destroy(DeleteObject[i]);
        }
        Item.Clear();
        DeleteObject.Clear();
    }
    private void SpawnBack(PickUp p)
    {
        var clone = Instantiate(NormalCubePrefabs[UnityEngine.Random.Range(0, RandomColorCount)], new Vector2(p.x, p.y + PaddingTop), Quaternion.identity);
        clone.transform.SetParent(SetItem);
        clone.AddComponent<CapsuleCollider2D>();
        clone.tag = "Item";
        clone.AddComponent<PickUp>();
        clone.GetComponent<IDName>().IsRunChangeSprites = true;
    }
    private void EnableBoxCollider2DCallBack()
    {
        for (int i = 0; i < SetBg.childCount; i++)
        {
            SetBg.GetChild(i).GetComponent<BoxCollider2D>().enabled = true;
        }
        Invoke("DisableBoxCollider2DCallback", 0.1f);
    }
    private void DisableBoxCollider2DCallback()
    {
        for (int i = 0; i < SetBg.childCount; i++)
        {
            SetBg.GetChild(i).GetComponent<BoxCollider2D>().enabled = false;
        }

        // #1 Start Game
        StartCoroutine(Wait(0.1f, () =>
        {
            Start_Square();
            foreach(var  i in Item.Values)
            {
                if (!Calculate_CubeCallBack(i))
                {
                    i.GetComponent<IDName>().TypeOfCube = IDName.CubeType.Normal;
                }
            }
        }));
    }

    //Check
    public void CalculateCubeForDiscoBall(IDName i)
    {
        foreach (var item in Item.Values)
            if(item.GetComponent<IDName>().ID == i.ID)
                    DeleteObject.Add(item.gameObject);
        Delete_Invoke();
    }
     public void CalculateCubeForDiscoBall_Callback(IDName i)
    {
        foreach (var item in Item.Values)
            if(item.GetComponent<IDName>().ID == i.ID)
                if(!DeleteObject.Contains(item.gameObject))
                    DeleteObject.Add(item.gameObject);
    }
    public void CalculateCubeForBomb(IDName i)
    {
        var p = i.GetComponent<PickUp>();
        var TopLeft = new Tuple<int, int>(p.x - 1, p.y + 1);
        var Left = new Tuple<int, int>(p.x - 1, p.y);
        var DownLeft = new Tuple<int, int>(p.x - 1, p.y - 1);
        var Top = new Tuple<int, int>(p.x, p.y + 1);
        var Down = new Tuple<int, int>(p.x - 1, p.y - 1);
        var TopRight = new Tuple<int, int>(p.x + 1, p.y + 1);
        var Right = new Tuple<int, int>(p.x + 1, p.y);
        var DownRight = new Tuple<int, int>(p.x + 1, p.y - 1);

        i.TypeOfCube = IDName.CubeType.Normal;
        DeleteObject.Add(i.gameObject);

        if (Item.ContainsKey(TopLeft))
        {
            if (!DeleteObject.Contains(Item[TopLeft].gameObject))
            {
                DeleteObject.Add(Item[TopLeft].gameObject);
                CheckContinueCube_Callback(Item[TopLeft].GetComponent<IDName>());
            }
        }
        if (Item.ContainsKey(Left))
        {
            if (!DeleteObject.Contains(Item[Left].gameObject))
            {
                DeleteObject.Add(Item[Left].gameObject);
                CheckContinueCube_Callback(Item[Left].GetComponent<IDName>());
            }
        }
        if (Item.ContainsKey(DownLeft))
        {
            if (!DeleteObject.Contains(Item[DownLeft].gameObject))
            {
                DeleteObject.Add(Item[DownLeft].gameObject);
                CheckContinueCube_Callback(Item[DownLeft].GetComponent<IDName>());
            }
        }
        if (Item.ContainsKey(Top))
        {
            if (!DeleteObject.Contains(Item[Top].gameObject))
            {
                DeleteObject.Add(Item[Top].gameObject);
                CheckContinueCube_Callback(Item[Top].GetComponent<IDName>());
            }
        }
        if (Item.ContainsKey(Down))
        {
            if (!DeleteObject.Contains(Item[Down].gameObject))
            {
                DeleteObject.Add(Item[Down].gameObject);
                CheckContinueCube_Callback(Item[Down].GetComponent<IDName>());
            }
        }
        if (Item.ContainsKey(TopRight))
        {
            if (!DeleteObject.Contains(Item[TopRight].gameObject))
            {
                DeleteObject.Add(Item[TopRight].gameObject);
                CheckContinueCube_Callback(Item[TopRight].GetComponent<IDName>());
            }
        }
        if (Item.ContainsKey(Right))
        {
            if (!DeleteObject.Contains(Item[Right].gameObject))
            {
                DeleteObject.Add(Item[Right].gameObject);
                CheckContinueCube_Callback(Item[Right].GetComponent<IDName>());
            }
        }
        if (Item.ContainsKey(DownRight))
        {
            if (!DeleteObject.Contains(Item[DownRight].gameObject))
            {
                DeleteObject.Add(Item[DownRight].gameObject);
                CheckContinueCube_Callback(Item[DownRight].GetComponent<IDName>());
            }
        }
        Delete_Invoke();
    }
    public void CalculateCubeForBomb_CallBack(IDName i)
    {
        var p = i.GetComponent<PickUp>();
        var TopLeft = new Tuple<int, int>(p.x - 1, p.y + 1);
        var Left = new Tuple<int, int>(p.x - 1, p.y);
        var DownLeft = new Tuple<int, int>(p.x - 1, p.y - 1);
        var Top = new Tuple<int, int>(p.x, p.y + 1);
        var Down = new Tuple<int, int>(p.x - 1, p.y - 1);
        var TopRight = new Tuple<int, int>(p.x + 1, p.y + 1);
        var Right = new Tuple<int, int>(p.x + 1, p.y);
        var DownRight = new Tuple<int, int>(p.x + 1, p.y - 1);

        i.TypeOfCube = IDName.CubeType.Normal;
        DeleteObject.Add(i.gameObject);

        if (Item.ContainsKey(TopLeft))
        {
            if (!DeleteObject.Contains(Item[TopLeft].gameObject))
            {
                DeleteObject.Add(Item[TopLeft].gameObject);
                CheckContinueCube_Callback(Item[TopLeft].GetComponent<IDName>());
            }
        }
        if (Item.ContainsKey(Left))
        {
            if (!DeleteObject.Contains(Item[Left].gameObject))
            {
                DeleteObject.Add(Item[Left].gameObject);
                CheckContinueCube_Callback(Item[Left].GetComponent<IDName>());
            }
        }
        if (Item.ContainsKey(DownLeft))
        {
            if (!DeleteObject.Contains(Item[DownLeft].gameObject))
            {
                DeleteObject.Add(Item[DownLeft].gameObject);
                CheckContinueCube_Callback(Item[DownLeft].GetComponent<IDName>());
            }
        }
        if (Item.ContainsKey(Top))
        {
            if (!DeleteObject.Contains(Item[Top].gameObject))
            {
                DeleteObject.Add(Item[Top].gameObject);
                CheckContinueCube_Callback(Item[Top].GetComponent<IDName>());
            }
        }
        if (Item.ContainsKey(Down))
        {
            if (!DeleteObject.Contains(Item[Down].gameObject))
            {
                DeleteObject.Add(Item[Down].gameObject);
                CheckContinueCube_Callback(Item[Down].GetComponent<IDName>());
            }
        }
        if (Item.ContainsKey(TopRight))
        {
            if (!DeleteObject.Contains(Item[TopRight].gameObject))
            {
                DeleteObject.Add(Item[TopRight].gameObject);
                CheckContinueCube_Callback(Item[TopRight].GetComponent<IDName>());
            }
        }
        if (Item.ContainsKey(Right))
        {
            if (!DeleteObject.Contains(Item[Right].gameObject))
            {
                DeleteObject.Add(Item[Right].gameObject);
                CheckContinueCube_Callback(Item[Right].GetComponent<IDName>());
            }
        }
        if (Item.ContainsKey(DownRight))
        {
            if (!DeleteObject.Contains(Item[DownRight].gameObject))
            {
                DeleteObject.Add(Item[DownRight].gameObject);
                CheckContinueCube_Callback(Item[DownRight].GetComponent<IDName>());
            }
        }
    }
    public void CalculateCubeForSwitchHorizontal(IDName i)
    {
        var p = i.GetComponent<PickUp>();
        for(int b = 0; b < Row; b++)
        {
            if (!DeleteObject.Contains(Item[new Tuple<int, int>(b, p.y)].gameObject))
            {
                DeleteObject.Add(Item[new Tuple<int, int>(b, p.y)].gameObject);
                CheckContinueCube_Callback(Item[new Tuple<int, int>(b, p.y)].GetComponent<IDName>());
            }
            Delete_Invoke();
        }
    }
    public void CalculateCubeForSwitchHorizontal_CallBack(IDName i)
    {
        var p = i.GetComponent<PickUp>();
        for (int b = 0; b < Row; b++)
        {
            if (!DeleteObject.Contains(Item[new Tuple<int, int>(b, p.y)].gameObject))
                DeleteObject.Add(Item[new Tuple<int, int>(b, p.y)].gameObject);
        }
    }
   public void CalculateCubeForSwitchVertical(IDName i)
    {
        var p = i.GetComponent<PickUp>();
        Debug.Log($"CalculateCubeForSwitchHorizontal Coordinates: ({p.x}, {p.y})");
        for (int b = 0; b < Column; b++)
        {
            if (!DeleteObject.Contains(Item[new Tuple<int, int>(p.x, b)].gameObject))
            {
                DeleteObject.Add(Item[new Tuple<int, int>(p.x, b)].gameObject);
                CalculateCubeForSwitchVertical(Item[new Tuple<int, int>(p.x, b)].GetComponent<IDName>());
            }
        Delete_Invoke();
        }
    }
    public void CalculateCubeForSwitchVertical_CallBack(IDName i)
    {
        var p = i.GetComponent<PickUp>();
        for (int b = 0; b < Row; b++)
        {
            if (!DeleteObject.Contains(Item[new Tuple<int, int>(b, p.y)].gameObject))
                DeleteObject.Add(Item[new Tuple<int, int>(b, p.y)].gameObject);
        }
    }

    //Check continue cube
    public void CheckContinueCube_Callback(IDName i)
    {
        //Check Disco Ball Cube
        if (i.IsDiscoBall && !i.IsBomb && !i.IsSwitchHorizontal && !i.IsSwitchVertical)
        {
            CalculateCubeForDiscoBall_Callback(i);
        }
        //Check Bomb Cube
        if (!i.IsDiscoBall && i.IsBomb && !i.IsSwitchHorizontal && !i.IsSwitchVertical)
        {
            CalculateCubeForBomb_CallBack(i);
        }
        //Check Switch Horizontal Cube
        if (!i.IsDiscoBall && !i.IsBomb && i.IsSwitchHorizontal && !i.IsSwitchVertical)
        {
            CalculateCubeForSwitchHorizontal_CallBack(i);
        }
        //Check Switch Vertical Cube
        if (!i.IsDiscoBall && !i.IsBomb && !i.IsSwitchHorizontal && i.IsSwitchVertical)
        {
            CalculateCubeForSwitchVertical_CallBack(i);
        }
    }

    private void Update()
    {
        if (IsClick) // True
        {
            TimeToClick -= Time.deltaTime;
            if(TimeToClick <= 0)
            {
                TimeToClick = TimeToClickSet;
                IsClick = false;
            }
        }
    }
}
