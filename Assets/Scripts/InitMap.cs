using UnityEngine;
using System.Collections;

public class InitMap : MonoBehaviour
{
    Dungeon d;
    public GameObject cube;
    public GameObject player;
    Vector2 startPos;
    bool have_empty = false;
	// Use this for initialization
	void Start ()
    {
        d = new Dungeon(new System.Random());
        //d.Initialize();
        d.CreateDungeon(128, 128, 256);

        for(int y=0;y<128;y++)
        {
            for(int x=0;x<128;x++)
            {
                int cell = d.GetCellType(x, y);
                if (cell != 0 && cell != 2 && cell != 5)
                    Instantiate(cube, new Vector3(x * 3, 1.5f, y * 3),Quaternion.identity);
                else
                {
                    Instantiate(cube, new Vector3(x * 3, 4.5f, y * 3), Quaternion.identity);
                }

                if(!have_empty && cell == 5)
                {
                    startPos = new Vector2(x, y);
                }
            }
        }

        player.transform.position = new Vector3(startPos.x * 3, 1, startPos.y * 3);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
