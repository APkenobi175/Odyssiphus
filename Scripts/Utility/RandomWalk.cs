using Godot;
using System;
using System.Collections.Generic;

// Comment from Ammon Phipps to Team 3/20/26
// This is the code I used for the random walk assignment

public class RandomWalk
{
    private int minSteps;
    private int maxSteps;
    private float stepChance;
    private float branchChance;
    private bool allowLoops;
    private bool allowBranches;
    private bool allowBranchesToConnect;
    private int seed;
    private Random rng;
    private HashSet<Vector2I> visited; // to track which grid cells have rooms
    private List<RandomWalkRoom> rooms;
    private List<RandomWalkHallway> hallways;
    private int maxRooms = 1_000_000; // safety limit to prevent infinite loops!!!! (i keep crashing the program....)
    private bool maxRoomsHit = false; // flag to indicate if we hit the max room limit during generation
    private int totalCalls;
    private int maxCalls = 100_000;

    // Instead of doing this I'll actually make them classes in a seperate file so it matches BspTypes

    // //1. Define a room struct
    // public struct Room
    // {
    //     // Grid position of the room
    //     public Vector2I Position; 
    //     // Constructor
    //     public Room(Vector2I pos) { Position = pos; }
    // }

    // //2. Define a hallway struct
    // public struct Hallway
    // {
    //     // Position of hallway start and end points
    //     public Vector2I From;
    //     public Vector2I To;
    //     // Constructor
    //     public Hallway(Vector2I from, Vector2I to) { From = from; To = to; }
    // }

    //3. Define a result struct to hold the generated rooms and hallways
    public struct Result
    {
        public List<RandomWalkRoom> Rooms;
        public List<RandomWalkHallway> Hallways;
        public bool maxRoomsHit; // indicates if the generation hit the max room limit (to stop crashes and almost infinite loops)
    }

    // These are the four directions we can walk in (N, E, S, W)
    private static readonly Vector2I[] Directions = new Vector2I[]
    {
        new Vector2I(0, -1), // N
        new Vector2I(1,  0), // E
        new Vector2I(0,  1), // S
        new Vector2I(-1, 0), // W
    };

    // 4. Generate a random walk dungeon with the parameters taken from the UI
    public Result Generate(
        // The users will pass these parameters in
        int minSteps,
        int maxSteps,
        float stepChance,   // chance to keep walking at each step (0-1)
        float branchChance, // chance to start a branch from current room
        bool allowLoops,
        bool allowBranches,
        bool allowBranchesToConnect,
        int seed
    )
    {
        totalCalls = 0;
        // If user specifies a seed of 0, use a random seed
        if (seed == 0)
        {
            this.seed = new Random().Next();
        }
        // Otherwise use the provided seed
        else
        {
            this.seed = seed;
        }
        // Store parameters in the instance variables
        this.minSteps = minSteps;
        this.maxSteps = maxSteps;
        this.stepChance = stepChance;
        this.branchChance = branchChance;
        this.allowLoops = allowLoops;
        this.allowBranches = allowBranches;
        this.allowBranchesToConnect = allowBranchesToConnect;
        rng = new Random(this.seed);
        rooms = new List<RandomWalkRoom>();
        hallways = new List<RandomWalkHallway>();
        visited = new HashSet<Vector2I>(); // grid cells that already have a room

        // Start room at origin, and add it to our visited and rooms list
        Vector2I startPos = Vector2I.Zero;
        rooms.Add(new RandomWalkRoom(startPos));
        visited.Add(startPos);

        // Start doing the recursive random walk
        maxRoomsHit = RandomWalkRecursive(startPos, 0, false);
        maxRoomsHit = totalCalls >= maxCalls || rooms.Count >= maxRooms;
        GD.Print("Random Walk Generation Complete. Rooms: " + rooms.Count + ", Hallways: " + hallways.Count + ", Max Rooms Hit: " + maxRoomsHit);

        return new Result { Rooms = rooms, Hallways = hallways, maxRoomsHit = maxRoomsHit };
    }

    private bool RandomWalkRecursive(Vector2I currentRoom, int stepCount, bool isBranch)
    {
        // 1. Define the base case
        // Base case: if we have taken at least minSteps and we hit max steps
        // OR the stepchance says we stop
        // OR WE HIT THE MAX ROOM LIMIT (STOP CRASHING)
        // OR WE HIT THE MAX CALL LIMIT (STOP CRASHING)

        if (totalCalls >= maxCalls)
        {
            return false; // stop generation if we hit the max call limit
        }
        totalCalls++;

        if (rooms.Count >= maxRooms)
        {
            return false;
        }


        if (stepCount >= minSteps)
        {
            if (stepCount >= maxSteps)
                return true;

            if (rng.NextDouble() > stepChance)
                return true;
        }

        // 2. Choose a random valid direction from currentRoom

        var dirs = new List<Vector2I>(Directions);
        ShuffleArray(dirs, rng);

        //3. Define variables to track next room and whether we found a valid step
        Vector2I nextRoom = Vector2I.Zero;
        bool foundStep = false;

        // 4. For each direction, check if we can walk there
        foreach (var dir in dirs)
        {
            Vector2I candidate = currentRoom + dir;
            bool isVisited = visited.Contains(candidate);

            bool canStepIntoVisited = allowLoops || (allowBranchesToConnect && isBranch);

            // 5. Check for if we allow loops
            if (isVisited && canStepIntoVisited)
            {
                nextRoom = candidate;
                foundStep = true;
                hallways.Add(new RandomWalkHallway(currentRoom, nextRoom));
                break;
            // 6. If we don't allow loops, only step into unvisited rooms
            }else if (!isVisited)
            {
                nextRoom = candidate;
                foundStep = true;

                // Create the new room and hallway, and add them to the lists
                rooms.Add(new RandomWalkRoom(nextRoom));
                hallways.Add(new RandomWalkHallway(currentRoom, nextRoom));
                visited.Add(nextRoom);
                break;
            }
        }

        if (!foundStep)
        {
            return true; // No valid steps from this room
        }

        // 7. Call the function recursively with the new room and incremented step count
        bool result = RandomWalkRecursive(nextRoom, stepCount + 1, isBranch);

        // 8. Branch chase: If we branch, start a new room

        if (allowBranches && rng.NextDouble() < branchChance)
        {
            // Start a new branch from the current room
            RandomWalkRecursive(currentRoom, stepCount, true); // isBranch goes to true here because we want to allow branch connections from this branch
        }

        return result;

    

    }

    public static void ShuffleArray<T>(List<T> list, Random rng)
    {
        for (int i = list.Count - 1; i>0; i --)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }


}
