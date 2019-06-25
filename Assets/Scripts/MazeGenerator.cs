using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int mazeRows;
    public int mazeColumns;
    [SerializeField]
    private GameObject cellPrefab;
    public bool disableCellSprite;


    private Dictionary<Vector2, Cell> allCells = new Dictionary<Vector2, Cell>();
    private List<Cell> unvisited = new List<Cell>();
    private List<Cell> stack = new List<Cell>();

    private int centreSize = 2;
    private Cell[] centreCells = new Cell[4];

    private Cell currentCell;
    private Cell checkCell;

    private Vector2[] neighbourPositions = new Vector2[] { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, -1) };

    private float cellSize;

    private GameObject mazeParent;

    private void Start()
    {
        GenerateMaze(mazeRows, mazeColumns);
    }

    private void GenerateMaze(int rows, int columns)
    {
        if (mazeParent != null) DeleteMaze();

        mazeRows = rows;
        mazeColumns = columns;
        CreateLayout();
    }

    public void CreateLayout()
    {
        InitValues();

        // Set starting point, set spawn point to start.
        Vector2 startPos = new Vector2(-(cellSize * (mazeColumns / 2)) + (cellSize / 2), -(cellSize * (mazeRows / 2)) + (cellSize / 2));
        Vector2 spawnPos = startPos;

        for (int x = 1; x <= mazeColumns; x++)
        {
            for (int y = 1; y <= mazeRows; y++)
            {
                // Create new cell
                GenerateCell(spawnPos, new Vector2(x, y));

                // Increase spawnPos y.
                spawnPos.y += cellSize;
            }

            // Reset spawnPos y and increase spawnPos x.
            spawnPos.y = startPos.y;
            spawnPos.x += cellSize;
        }

        CreateCentre();
        RunAlgorithm();
        MakeExit();
    }

    public void RunAlgorithm()
    {
        // Get start cell, make it visited (i.e. remove from unvisited list).
        unvisited.Remove(currentCell);

        // While we have unvisited cells.
        while (unvisited.Count > 0)
        {
            List<Cell> unvisitedNeighbours = GetUnvisitedNeighbours(currentCell);
            if (unvisitedNeighbours.Count > 0)
            {
                // Get a random unvisited neighbour.
                checkCell = unvisitedNeighbours[Random.Range(0, unvisitedNeighbours.Count)];
                // Add current cell to stack.
                stack.Add(currentCell);
                // Compare and remove walls.
                CompareWalls(currentCell, checkCell);
                // Make currentCell the neighbour cell.
                currentCell = checkCell;
                // Mark new current cell as visited.
                unvisited.Remove(currentCell);
            }
            else if (stack.Count > 0)
            {
                // Make current cell the most recently added Cell from the stack.
                currentCell = stack[stack.Count - 1];
                // Remove it from stack.
                stack.Remove(currentCell);
            }
        }
    }

    public void MakeExit()
    {
        // Create and populate list of all possible edge cells.
        List<Cell> edgeCells = new List<Cell>();

        foreach (KeyValuePair<Vector2, Cell> cell in allCells)
        {
            if (cell.Key.x == 0 || cell.Key.x == mazeColumns || cell.Key.y == 0 || cell.Key.y == mazeRows)
            {
                edgeCells.Add(cell.Value);
            }
        }

        // Get edge cell randomly from list.
        Cell newCell = edgeCells[Random.Range(0, edgeCells.Count)];
        newCell.cScript.end = true;
        newCell.end = true;
        endcell = newCell;

        // Remove appropriate wall for chosen edge cell.
        if (newCell.gridPos.x == 0) RemoveWall(newCell.cScript, 1);
        else if (newCell.gridPos.x == mazeColumns) RemoveWall(newCell.cScript, 2);
        else if (newCell.gridPos.y == mazeRows) RemoveWall(newCell.cScript, 3);
        else RemoveWall(newCell.cScript, 4);
    }

    public List<Cell> GetUnvisitedNeighbours(Cell curCell)
    {
        // Create a list to return.
        List<Cell> neighbours = new List<Cell>();
        // Create a Cell object.
        Cell nCell = curCell;
        // Store current cell grid pos.
        Vector2 cPos = curCell.gridPos;

        foreach (Vector2 p in neighbourPositions)
        {
            // Find position of neighbour on grid, relative to current.
            Vector2 nPos = cPos + p;
            // If cell exists.
            if (allCells.ContainsKey(nPos)) nCell = allCells[nPos];
            // If cell is unvisited.
            if (unvisited.Contains(nCell)) neighbours.Add(nCell);
        }

        return neighbours;
    }

    // Compare neighbour with current and remove appropriate walls.
    public void CompareWalls(Cell cCell, Cell nCell)
    {
        // If neighbour is left of current.
        if (nCell.gridPos.x < cCell.gridPos.x)
        {
            RemoveWall(nCell.cScript, 2);
            RemoveWall(cCell.cScript, 1);
        }
        // Else if neighbour is right of current.
        else if (nCell.gridPos.x > cCell.gridPos.x)
        {
            RemoveWall(nCell.cScript, 1);
            RemoveWall(cCell.cScript, 2);
        }
        // Else if neighbour is above current.
        else if (nCell.gridPos.y > cCell.gridPos.y)
        {
            RemoveWall(nCell.cScript, 4);
            RemoveWall(cCell.cScript, 3);
        }
        // Else if neighbour is below current.
        else if (nCell.gridPos.y < cCell.gridPos.y)
        {
            RemoveWall(nCell.cScript, 3);
            RemoveWall(cCell.cScript, 4);
        }
    }

    // Function disables wall of your choosing, pass it the script attached to the desired cell
    // and an 'ID', where the ID = the wall. 1 = left, 2 = right, 3 = up, 4 = down.
    public void RemoveWall(CellScript cScript, int wallID)
    {
        if (wallID == 1) cScript.wallL.SetActive(false);
        else if (wallID == 2) cScript.wallR.SetActive(false);
        else if (wallID == 3) cScript.wallU.SetActive(false);
        else if (wallID == 4) cScript.wallD.SetActive(false);
    }

    public void CreateCentre()
    {
        // Get the 4 centre cells using the rows and columns variables.
        // Remove the required walls for each.
        centreCells[0] = allCells[new Vector2((mazeColumns / 2), (mazeRows / 2) + 1)];
        RemoveWall(centreCells[0].cScript, 4);
        RemoveWall(centreCells[0].cScript, 2);
        centreCells[1] = allCells[new Vector2((mazeColumns / 2) + 1, (mazeRows / 2) + 1)];
        RemoveWall(centreCells[1].cScript, 4);
        RemoveWall(centreCells[1].cScript, 1);
        centreCells[2] = allCells[new Vector2((mazeColumns / 2), (mazeRows / 2))];
        RemoveWall(centreCells[2].cScript, 3);
        RemoveWall(centreCells[2].cScript, 2);
        centreCells[3] = allCells[new Vector2((mazeColumns / 2) + 1, (mazeRows / 2))];
        RemoveWall(centreCells[3].cScript, 3);
        RemoveWall(centreCells[3].cScript, 1);

        // Create a List of ints, using this, select one at random and remove it.
        // We then use the remaining 3 ints to remove 3 of the centre cells from the 'unvisited' list.
        // This ensures that one of the centre cells will connect to the maze but the other three won't.
        // This way, the centre room will only have 1 entry / exit point.
        List<int> rndList = new List<int> { 0, 1, 2, 3 };
        int startCell = rndList[Random.Range(0, rndList.Count)];
        rndList.Remove(startCell);
        currentCell = centreCells[startCell];
        foreach (int c in rndList)
        {
            unvisited.Remove(centreCells[c]);
        }
        var pos = new Vector2(currentCell.cellObject.transform.position.x, currentCell.cellObject.transform.position.y);
        currentCell.cScript.spawn = true;
        currentCell.spawn = true;
        Init(pos, currentCell.gridPos);
    }

    public void GenerateCell(Vector2 pos, Vector2 keyPos)
    {
        // Create new Cell object.
        Cell newCell = new Cell();

        // Store reference to position in grid.
        newCell.gridPos = keyPos;
        // Set and instantiate cell GameObject.
        newCell.cellObject = Instantiate(cellPrefab, pos, cellPrefab.transform.rotation);
        // Child new cell to parent.
        if (mazeParent != null) newCell.cellObject.transform.parent = mazeParent.transform;
        // Set name of cellObject.
        newCell.cellObject.name = "Cell - X:" + keyPos.x + " Y:" + keyPos.y;
        // Get reference to attached CellScript.
        newCell.cScript = newCell.cellObject.GetComponent<CellScript>();
        // Disable Cell sprite, if applicable.
        if (disableCellSprite) newCell.cellObject.GetComponent<SpriteRenderer>().enabled = false;

        // Add to Lists.
        allCells[keyPos] = newCell;
        unvisited.Add(newCell);
    }

    public void DeleteMaze()
    {
        if (mazeParent != null) Destroy(mazeParent);
    }

    public void InitValues()
    {
        // Check generation values to prevent generation failing.
        if (IsOdd(mazeRows)) mazeRows--;
        if (IsOdd(mazeColumns)) mazeColumns--;

        if (mazeRows <= 3) mazeRows = 4;
        if (mazeColumns <= 3) mazeColumns = 4;

        // Determine size of cell using localScale.
        cellSize = cellPrefab.transform.localScale.x;

        // Create an empty parent object to hold the maze in the scene.
        mazeParent = new GameObject();
        mazeParent.transform.position = Vector2.zero;
        mazeParent.name = "Maze";
    }

    public GameObject prefab;
    public double interval = 0.5;
    double nextTime = 0;
    bool started;
    Cell endcell;
    public int populationsize = 100;
    public int maxmoves = 10;
    public double mutationchance = 0.2;
    List<Summon> population = new List<Summon>();
    int test;

    public void Init(Vector2 pos, Vector2 keypos)
    {
        for (int i = 0; i < populationsize; i++)
        {
            InitSummon(pos, keypos, i);
        }
    }

    public void CalculateFiness(Vector2 lastpos, Summon summon)
    {
        if (endcell != null)
        {
            // Calculate penalty for touching walls
            float penalty = summon.penalty / 5;
            // The higher the score the better
            float score = Mathf.Abs((lastpos.x - endcell.gridPos.x) + Mathf.Abs(lastpos.y - endcell.gridPos.y)) - penalty;
            // Set score on the summon
            summon.score = score;
        }
    }

    public void Crossover()
    {
        var newPopulation = new List<Summon>();
        Summon dad = null;
        Summon mom = null;
        Summon temp = null;

        float highestScore = int.MinValue;

        for(int i = 0; i < population.Count; i++)
        {
            if (population[i].score > highestScore)
            {
                // Dad is highest
                dad = population[i];
                if(temp != null)
                {
                    // Mom is old highest
                    mom = temp;
                }
                else
                {
                    // If first score is highest set mom to dad
                    mom = dad;
                }
                // Set temp to new highest
                temp = population[i];
                highestScore = population[i].score;
            }
        }

    }

    public void InitSummon(Vector2 pos, Vector2 keyPos, int i)
    {
        // Create new Summon object.
        Summon summon = new Summon();

        // Store reference to position in grid.
        summon.gridPos = keyPos;
        // Set and instantiate cell GameObject.
        summon.summonObject = Instantiate(prefab, pos, prefab.transform.rotation);
        // Child new cell to parent.
        if (mazeParent != null) summon.summonObject.transform.parent = mazeParent.transform;
        // Set name of cellObject.
        summon.summonObject.name = "Summoned on - X:" + pos.x + " Y:" + pos.y + " i " + i;
        // Get reference to attached CellScript.
        summon.sScript = summon.summonObject.GetComponent<SummonScript>();

        summon.moves = new Queue<int>();
        population.Add(summon);
        started = true;
    }

    public void Update()
    {
        if (started)
        {
            foreach (var summon in population)
            {
                if (summon.movedtimes < maxmoves)
                {
                    test = summon.movedtimes + 1;
                    if (Time.time >= nextTime)
                    {
                        summon.movedtimes++;

                        // Get cell from maze
                        Cell cell = allCells[summon.gridPos];

                        // If it is the end cell and the summon gridpos is the same as the cell pos
                        if (cell.end && cell.gridPos == summon.gridPos)
                        {
                            Debug.Log("Found end");
                            started = false;
                            return;
                        }

                        // Default
                        int direction = 0;

                        if (summon.moves.Count == maxmoves)
                        {
                            // Get direction from summon
                            direction = summon.moves.Dequeue();
                        }
                        else
                        {
                            // Random direction
                            direction = Random.Range(0, 4);
                        }

                        // If direction has no wall move to that direction otherwise give penalty
                        switch (direction)
                        {
                            case 0:
                                if (!cell.cScript.wallU.activeSelf) Move(summon, direction);
                                else summon.penalty++;
                                break;
                            case 1:
                                if (!cell.cScript.wallD.activeSelf) Move(summon, direction); 
                                else summon.penalty++;
                                break;
                            case 2:
                                if (!cell.cScript.wallL.activeSelf) Move(summon, direction);
                                else summon.penalty++;
                                break;
                            case 3:
                                if (!cell.cScript.wallR.activeSelf) Move(summon, direction);
                                else summon.penalty++;
                                break;
                        }
                        summon.moves.Enqueue(direction);
                        nextTime += interval;
                    }
                }
                CalculateFiness(summon.gridPos, summon);
            }
            if (test == maxmoves)
            {
                started = false;
                Crossover();
            }
        }
    }

    public void Move(Summon summon, int direction)
    {
        Vector2 temp = summon.summonObject.transform.position;
        switch (direction)
        {
            // Move up and add move to list
            case 0:
                summon.gridPos.y++;
                temp.y++;
                break;
            // Move down and add move to list
            case 1:
                summon.gridPos.y--;
                temp.y--;
                break;
            // Move right and add move to list
            case 2:
                summon.gridPos.x--;
                temp.x--;
                break;
            // Move left and add move to list
            case 3:
                summon.gridPos.x++;
                temp.x++;
                break;
        }
        summon.summonObject.transform.position = temp;
    }

    public bool IsOdd(int value)
    {
        return value % 2 != 0;
    }
}


