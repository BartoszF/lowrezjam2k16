using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using System.Collections;
using System;

public enum Direction
{
    North,
    South,
    East,
    West
}

public class Tuple
{
    public Vector2 vector;
    public int tile;
    public Direction dir;

    public Tuple(Vector2 v, Direction dir, int tile)
    {
        this.vector = v;
        this.dir = dir;
        this.tile = tile;
    }
}

public class Dungeon
{
	// misc. messages to print
	const string MsgXSize = "X size of dungeon: \t";

	const string MsgYSize = "Y size of dungeon: \t";

	const string MsgMaxObjects = "max # of objects: \t";

	const string MsgNumObjects = "# of objects made: \t";

	// max size of the map
	int xmax = 256; //columns
	int ymax = 256; //rows

	// size of the map
	int _xsize;
	int _ysize;

	// number of "objects" to generate on the map
	int _objects;

	// define the %chance to generate either a room or a corridor on the map
	// BTW, rooms are 1st priority so actually it's enough to just define the chance
	// of generating a room
	const int ChanceRoom = 75;

	// our map
	int[] _dungeonMap = { };

	readonly System.Random _rnd;

	public Dungeon(System.Random rnd)
	{
	    _rnd = rnd;
	}

	public int Corridors
	{
	    get;
	    private set;
	}

	public static bool IsWall(int x, int y, int xlen, int ylen, int xt, int yt, Direction d)
	{
	    Func<int, int, int> a = GetFeatureLowerBound;

	    Func<int, int, int> b = IsFeatureWallBound;
	    switch (d)
	    {
		    case Direction.North:
			    return xt == a(x, xlen) || xt == b(x, xlen) || yt == y || yt == y - ylen + 1;
		    case Direction.East:
			    return xt == x || xt == x + xlen - 1 || yt == a(y, ylen) || yt == b(y, ylen);
		    case Direction.South:
			    return xt == a(x, xlen) || xt == b(x, xlen) || yt == y || yt == y + ylen - 1;
		    case Direction.West:
			    return xt == x || xt == x - xlen + 1 || yt == a(y, ylen) || yt == b(y, ylen);
	    }

	    throw new InvalidOperationException();
	}

	public static int GetFeatureLowerBound(int c, int len)
	{
	    return c - len / 2;
	}

	public static int IsFeatureWallBound(int c, int len)
	{
	    return c + (len - 1) / 2;
	}

	public static int GetFeatureUpperBound(int c, int len)
	{
	    return c + (len + 1) / 2;
	}

	public static IEnumerable<Vector2> GetRoomPoints(int x, int y, int xlen, int ylen, Direction d)
	{
	    // north and south share the same x strategy
	    // east and west share the same y strategy
	    Func<int, int, int> a = GetFeatureLowerBound;
	    Func<int, int, int> b = GetFeatureUpperBound;

	    switch (d)
	    {
		    case Direction.North:
			    for (var xt = a(x, xlen); xt < b(x, xlen); xt++) for (var yt = y; yt > y - ylen; yt--) yield return new Vector2 { x = xt, y = yt };
			    break;
		    case Direction.East:
			    for (var xt = x; xt < x + xlen; xt++) for (var yt = a(y, ylen); yt < b(y, ylen); yt++) yield return new Vector2 { x = xt, y = yt };
			    break;
		    case Direction.South:
			    for (var xt = a(x, xlen); xt < b(x, xlen); xt++) for (var yt = y; yt < y + ylen; yt++) yield return new Vector2 { x = xt, y = yt };
			    break;
		    case Direction.West:
			    for (var xt = x; xt > x - xlen; xt--) for (var yt = a(y, ylen); yt < b(y, ylen); yt++) yield return new Vector2 { x = xt, y = yt };
			    break;
		    default:
			    yield break;
	    }
	}

	public int GetCellType(int x, int y)
	{
	    try
	    {
		    return this._dungeonMap[x + this._xsize * y];
	    }
	    catch (IndexOutOfRangeException)
	    {
		    throw;
	    }
	}

	public int GetRand(int min, int max)
	{
	    return _rnd.Next(min, max);
	}

	public bool MakeCorridor(int x, int y, int length, Direction direction)
	{
	    // define the dimensions of the corridor (er.. only the width and height..)
	    int len = this.GetRand(2, length);

	    int xtemp;
	    int ytemp = 0;

	    switch (direction)
	    {
		    case Direction.North:
			    // north
			    // check if there's enough space for the corridor
			    // start with checking it's not out of the boundaries
			    if (x < 0 || x > this._xsize) return false;
			    xtemp = x;

			    // same thing here, to make sure it's not out of the boundaries
			    for (ytemp = y; ytemp > (y - len); ytemp--)
			    {
				    if (ytemp < 0 || ytemp > this._ysize) return false; // oh boho, it was!
				    if (GetCellType(xtemp, ytemp) != -1) return false;
			    }

			    // if we're still here, let's start building
			    Corridors++;
			    for (ytemp = y; ytemp > (y - len); ytemp--)
			    {
				    this.SetCell(xtemp, ytemp, 0);
			    }

			    break;

		    case Direction.East:
			    // east
			    if (y < 0 || y > this._ysize) return false;
			    ytemp = y;

			    for (xtemp = x; xtemp < (x + len); xtemp++)
			    {
				    if (xtemp < 0 || xtemp > this._xsize) return false;
				    if (GetCellType(xtemp, ytemp) != -1) return false;
			    }

			    Corridors++;
			    for (xtemp = x; xtemp < (x + len); xtemp++)
			    {
				    this.SetCell(xtemp, ytemp, 0);
			    }

			    break;

		    case Direction.South:
			    // south
			    if (x < 0 || x > this._xsize) return false;
			    xtemp = x;

			    for (ytemp = y; ytemp < (y + len); ytemp++)
			    {
				    if (ytemp < 0 || ytemp > this._ysize) return false;
				    if (GetCellType(xtemp, ytemp) != -1) return false;
			    }

			    Corridors++;
			    for (ytemp = y; ytemp < (y + len); ytemp++)
			    {
				    this.SetCell(xtemp, ytemp, 0);
			    }

			    break;
		    case Direction.West:
			    // west
			    if (ytemp < 0 || ytemp > this._ysize) return false;
			    ytemp = y;

			    for (xtemp = x; xtemp > (x - len); xtemp--)
			    {
				    if (xtemp < 0 || xtemp > this._xsize) return false;
				    if (GetCellType(xtemp, ytemp) != -1) return false;
			    }

			    Corridors++;
			    for (xtemp = x; xtemp > (x - len); xtemp--)
			    {
				    this.SetCell(xtemp, ytemp, 0);
			    }

			    break;
	    }

	    // woot, we're still here! let's tell the other guys we're done!!
	    return true;
	}

	public IEnumerable<KeyValuePair<Vector2, Direction>> GetSurroundingPoints(Vector2 v)
	{
	    var points = new[]
					 {
						 new KeyValuePair<Vector2, Direction>(new Vector2 { x = v.x, y = v.y + 1 }, Direction.North),
                         new KeyValuePair<Vector2, Direction>(new Vector2 { x = v.x - 1, y = v.y }, Direction.East),
                         new KeyValuePair<Vector2, Direction>(new Vector2 { x = v.x , y = v.y-1 }, Direction.South),
                         new KeyValuePair<Vector2, Direction>(new Vector2 { x = v.x +1, y = v.y  }, Direction.West),

					 };
	    return points.Where(p => InBounds(p.Key));
	}

	public IEnumerable<Tuple> GetSurroundings(Vector2 v)
	{
	    return
		    this.GetSurroundingPoints(v)
			    .Select(r => new Tuple(r.Key, r.Value, this.GetCellType((int)r.Key.x, (int)r.Key.y)));
	}

	public bool InBounds(int x, int y)
	{
	    return x > 0 && x < this.xmax && y > 0 && y < this.ymax;
	}

	public bool InBounds(Vector2 v)
	{
	    return this.InBounds((int)v.x, (int)v.y);
	}

	public bool MakeRoom(int x, int y, int xlength, int ylength, Direction direction)
	{
	    // define the dimensions of the room, it should be at least 4x4 tiles (2x2 for walking on, the rest is walls)
	    int xlen = this.GetRand(4, xlength);
	    int ylen = this.GetRand(4, ylength);

	    // the tile type it's going to be filled with
	    const int Floor = 0;

	    const int Wall = 1;
	    // choose the way it's pointing at

	    var points = GetRoomPoints(x, y, xlen, ylen, direction).ToArray();

	    // Check if there's enough space left for it
	    if (
		    points.Any(
			    s =>
			    s.y < 0 || s.y > this._ysize || s.x < 0 || s.x > this._xsize || this.GetCellType((int)s.x, (int)s.y) != 0)) return false;

	    foreach (var p in points)
	    {
		    this.SetCell((int)p.x, (int)p.y, IsWall(x, y, xlen, ylen, (int)p.x, (int)p.y, direction) ? Wall : Floor);
	    }

	    // yay, all done
	    return true;
	}

	public int[] GetDungeon()
    {
	    return this._dungeonMap;
	}

	public char GetCellTile(int x, int y)
	{
	    switch (GetCellType(x, y))
	    {
		    case 0:
			    return ' ';
		    case 1:
			    return '|';
		    case 2:
                return '_';
		    case 3:
			    return 'S';
		    case 4:
			    return '#';
		    case 5:
			    return 'D';
		    case 6:
			    return '+';
		    case 7:
			    return '-';
		    case 8:
			    return 'C';
		    default:
			    throw new ArgumentOutOfRangeException("x,y");
	    }
	}

	//used to print the map on the screen
	public void ShowDungeon()
	{
	for (int y = 0; y < this._ysize; y++)
	{
		for (int x = 0; x < this._xsize; x++)
		{
			Console.Write(GetCellTile(x, y));
		}

		//if (this._xsize <= xmax) Debug.Log();
	}
	}

	public Direction RandomDirection()
	{
	    int dir = this.GetRand(0, 4);
	    switch (dir)
	    {
		    case 0:
			    return Direction.North;
		    case 1:
			    return Direction.East;
		    case 2:
			    return Direction.South;
		    case 3:
			    return Direction.West;
		    default:
			    throw new InvalidOperationException();
	    }
	}

	//and here's the one generating the whole map
	public bool CreateDungeon(int inx, int iny, int inobj)
	{
	    this._objects = inobj < 1 ? 10 : inobj;

	    // adjust the size of the map, if it's smaller or bigger than the limits
	    if (inx < 3) this._xsize = 3;
	    else if (inx > xmax) this._xsize = xmax;
	    else this._xsize = inx;

	    if (iny < 3) this._ysize = 3;
	    else if (iny > ymax) this._ysize = ymax;
	    else this._ysize = iny;

	    Debug.Log(MsgXSize + this._xsize);
	    Debug.Log(MsgYSize + this._ysize);
	    Debug.Log(MsgMaxObjects + this._objects);

	    // redefine the map var, so it's adjusted to our new map size
	    this._dungeonMap = new int[this._xsize * this._ysize];

	    // start with making the "standard stuff" on the map
	    this.Initialize();

	    /*******************************************************************************
	    And now the code of the random-map-generation-algorithm begins!
	    *******************************************************************************/

	    // start with making a room in the middle, which we can start building upon
	    this.MakeRoom(this._xsize / 2, this._ysize / 2, 8, 6, RandomDirection()); // getrand saken f????r att slumpa fram riktning p?? rummet

	    // keep count of the number of "objects" we've made
	    int currentFeatures = 1; // +1 for the first room we just made

	    // then we sart the main loop
	    for (int countingTries = 0; countingTries < 1000; countingTries++)
	    {
		    // check if we've reached our quota
		    if (currentFeatures == this._objects)
		    {
			    break;
		    }

		    // start with a random wall
		    int newx = 0;
		    int xmod = 0;
		    int newy = 0;
		    int ymod = 0;
		    Direction? validTile = null;

		    // 1000 chances to find a suitable object (room or corridor)..
		    for (int testing = 0; testing < 1000; testing++)
		    {
			    newx = this.GetRand(1, this._xsize - 1);
			    newy = this.GetRand(1, this._ysize - 1);

			    if (GetCellType(newx, newy) == 1 || GetCellType(newx, newy) == 2)
			    {
				    var surroundings = this.GetSurroundings(new Vector2() { x = newx, y = newy });

				    // check if we can reach the place
				    var canReach =
					    surroundings.FirstOrDefault(s => s.tile == 2 || s.tile == 0);
				    if (canReach == null)
				    {
					    continue;
				    }
				    validTile = canReach.dir;
				    switch (canReach.dir)
				    {
					    case Direction.North:
						    xmod = 0;
						    ymod = -1;
						    break;
					    case Direction.East:
						    xmod = 1;
						    ymod = 0;
						    break;
					    case Direction.South:
						    xmod = 0;
						    ymod = 1;
						    break;
					    case Direction.West:
						    xmod = -1;
						    ymod = 0;
						    break;
					    default:
						    throw new InvalidOperationException();
				    }


				    // check that we haven't got another door nearby, so we won't get alot of openings besides
				    // each other

				    if (GetCellType(newx, newy + 1) == 5) // north
				    {
					    validTile = null;
				    }
				    else if (GetCellType(newx - 1, newy) == 5) // east
					    validTile = null;
				    else if (GetCellType(newx, newy - 1) == 5) // south
					    validTile = null;
				    else if (GetCellType(newx + 1, newy) == 5) // west
					    validTile = null;


				    // if we can, jump out of the loop and continue with the rest
				    if (validTile.HasValue) break;
			    }
		    }

		    if (validTile.HasValue)
		    {
			    // choose what to build now at our newly found place, and at what direction
			    int feature = this.GetRand(0, 100);
			    if (feature <= ChanceRoom)
			    { // a new room
				    if (this.MakeRoom(newx + xmod, newy + ymod, 8, 6, validTile.Value))
				    {
					    currentFeatures++; // add to our quota

					    // then we mark the wall opening with a door
					    this.SetCell(newx, newy, 5);

					    // clean up infront of the door so we can reach it
					    this.SetCell(newx + xmod, newy + ymod, 0);
				    }
			    }
			    else if (feature >= ChanceRoom)
			    { // new corridor
				    if (this.MakeCorridor(newx + xmod, newy + ymod, 6, validTile.Value))
				    {
					    // same thing here, add to the quota and a door
					    currentFeatures++;

					    this.SetCell(newx, newy, 5);
				    }
			    }
		    }
	    }

	    /*******************************************************************************
	    All done with the building, let's finish this one off
	    *******************************************************************************/
	    //AddSprinkles();

	    // all done with the map generation, tell the user about it and finish
	    Debug.Log(MsgNumObjects + currentFeatures);

	    return true;
	}

	void Initialize()
	{
	    for (int y = 0; y < this._ysize; y++)
	    {
		    for (int x = 0; x < this._xsize; x++)
		    {
			    // ie, making the borders of unwalkable walls
			    if (y == 0 || y == this._ysize - 1 || x == 0 || x == this._xsize - 1)
			    {
				    this.SetCell(x, y, 3);
			    }
			    else
			    {                        // and fill the rest with dirt
				    this.SetCell(x, y, 0);
			    }
		    }
	    }
	}

	// setting a tile's type
	void SetCell(int x, int y, int celltype)
	{
	    this._dungeonMap[x + this._xsize * y] = celltype;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void AddSprinkles()
	{
	    // sprinkle out the bonusstuff (stairs, chests etc.) over the map
	    int state = 0; // the state the loop is in, start with the stairs
	    while (state != 10)
	    {
		    for (int testing = 0; testing < 1000; testing++)
		    {
			    var newx = this.GetRand(1, this._xsize - 1);
			    int newy = this.GetRand(1, this._ysize - 2);

			    // Debug.Log("x: " + newx + "\ty: " + newy);
			    int ways = 4; // from how many directions we can reach the random spot from

			    // check if we can reach the spot
			    if (GetCellType(newx, newy + 1) == 0 || GetCellType(newx, newy + 1) == 2)
			    {
				    // north
				    if (GetCellType(newx, newy + 1) != 5)
					    ways--;
			    }

			    if (GetCellType(newx - 1, newy) == 0 || GetCellType(newx - 1, newy) == 2)
			    {
				    // east
				    if (GetCellType(newx - 1, newy) != 5)
					    ways--;
			    }

			    if (GetCellType(newx, newy - 1) == 0 || GetCellType(newx, newy - 1) == 2)
			    {
				    // south
				    if (GetCellType(newx, newy - 1) != 5)
					    ways--;
			    }

			    if (GetCellType(newx + 1, newy) == 0 || GetCellType(newx + 1, newy) == 2)
			    {
				    // west
				    if (GetCellType(newx + 1, newy) != 5)
					    ways--;
			    }

			    if (state == 0)
			    {
				    if (ways == 0)
				    {
					    // we're in state 0, let's place a "upstairs" thing
					    this.SetCell(newx, newy, 7);
					    state = 1;
					    break;
				    }
			    }
			    else if (state == 1)
			    {
				    if (ways == 0)
				    {
					    // state 1, place a "downstairs"
					    this.SetCell(newx, newy, 6);
					    state = 10;
					    break;
				    }
			    }
		    }
	    }
	}
}