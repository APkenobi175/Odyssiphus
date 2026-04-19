using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// RESOURCES:
// https://www.youtube.com/watch?v=MxeJh2Asigg
// https://www.godotengine.org/qa/12286/how-to-generate-a-dungeon-based-on-a-graph
// https://journal.stuffwithstuff.com/2014/12/21/rooms-and-mazes/
// https://www.youtube.com/watch?v=eMiMMwNEPVs

// BFS in C# https://www.youtube.com/watch?v=1ETLBFMa4Y4


public class DungeonGraphReplacement
{
    private const int RequiredMiniBosses = 4;
    private const float TreasureRoomChance = 0.15f;
    private const float PuzzleRoomChance = 0.10f;

    private Random rng;

    private const int MinTreasureRooms = 3;
    private const int MinPuzzleRooms = 2;

public void Replace(List<RandomWalkRoom> rooms, List<RandomWalkHallway> hallways, int seed)
{
    rng = new Random(seed);

    // 1. Build Adjacency Map
    var adjacency = BuildAdjacency(rooms, hallways);

    // 2. BFS from origin (0,0)
    var startRoom = rooms.Find(r => r.Position == Vector2I.Zero);
    var (distances, parents) = BFS(startRoom, adjacency);

    // 3. Boss is farthest from start
    var bossRoom = distances.OrderByDescending(kvp => kvp.Value).First().Key;
    bossRoom.RoomType = RoomType.BossRoom;

    // 4. BFS from boss for depth assignment
    var (distancesFromBoss, _) = BFS(bossRoom, adjacency);

    int maxDistFromBoss = distancesFromBoss.Values.Max();
    foreach (var kvp in distancesFromBoss)
    {
        kvp.Key.Depth = maxDistFromBoss > 0
            ? 1f - ((float)kvp.Value / maxDistFromBoss)
            : 0f;
    }

    // 5. Assign start room
    startRoom.RoomType = RoomType.Start;
    startRoom.IsCleared = true;
    startRoom.Depth = 0f; // force exactly 0
    startRoom.IsDiscovered = true; // Start room is always discovered

    // 6. Trace critical path from start to boss
    var (distancesFromStart, parentsFromStart) = BFS(startRoom, adjacency);
    var criticalPath = TracePath(bossRoom, startRoom, parentsFromStart);

    // 8. Place mini bosses along critical path
    PlaceMiniBosses(criticalPath, RequiredMiniBosses);

    // 9. Assign remaining room types
    var criticalSet = new HashSet<RandomWalkRoom>(criticalPath);

    foreach (var room in rooms)
    {
        if (room.RoomType != RoomType.EnemyRoom) continue;
        if (criticalSet.Contains(room)) continue;

        double roll = rng.NextDouble();
        if (roll < TreasureRoomChance)
        {
            room.RoomType = RoomType.TreasureRoom;
            room.IsCleared = true; // TREASURE ROOMS AUTO CLEAR
        }
        else if (roll < TreasureRoomChance + PuzzleRoomChance)
        {
            room.RoomType = RoomType.PuzzleRoom;
        }
    }

    // Ensure we have the minimum number of room types
    EnsureMinimum(rooms, criticalSet, RoomType.TreasureRoom, MinTreasureRooms);
    EnsureMinimum(rooms, criticalSet, RoomType.PuzzleRoom, MinPuzzleRooms);
}


    private (Dictionary<RandomWalkRoom, int>, Dictionary<RandomWalkRoom, RandomWalkRoom>) BFS(RandomWalkRoom start, Dictionary<RandomWalkRoom, List<RandomWalkRoom>> adjacency)
    {
        // 1. Initialize distances and parents
        var distances = new Dictionary<RandomWalkRoom, int>();
        var parents = new Dictionary<RandomWalkRoom, RandomWalkRoom>();
        var queue = new Queue<RandomWalkRoom>();

        // 2. Set initial distances to infinity and parents to null

        distances[start] = 0;
        parents[start] = null;
        queue.Enqueue(start);

        // 3. BFS loop

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!adjacency.ContainsKey(current)) continue;

            foreach(var neighbor in adjacency[current])
            {
                if (!distances.ContainsKey(neighbor))
                {
                    distances[neighbor] = distances[current] + 1;
                    parents[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }
        return (distances, parents);
    }
    private Dictionary<RandomWalkRoom, List<RandomWalkRoom>> BuildAdjacency(List<RandomWalkRoom> rooms, List<RandomWalkHallway> hallways)
    {
        // 1. Build a mapping of room positions to rooms for quick lookup
        var roomByPos = rooms.ToDictionary(r => r.Position);
        var adjacency = rooms.ToDictionary(r => r, r => new List<RandomWalkRoom>());

        // 2. For each hallway, connect the corresponding rooms in the adjacency list
        foreach (var hallway in hallways)
        {
            if (roomByPos.TryGetValue(hallway.From, out var fromRoom) &&
                roomByPos.TryGetValue(hallway.To, out var toRoom))
            {
                if (!adjacency[fromRoom].Contains(toRoom))
                    adjacency[fromRoom].Add(toRoom);
                if (!adjacency[toRoom].Contains(fromRoom))
                    adjacency[toRoom].Add(fromRoom);
            }
        }
        return adjacency;
    }

    private List<RandomWalkRoom> TracePath(RandomWalkRoom end, RandomWalkRoom start, Dictionary<RandomWalkRoom, RandomWalkRoom> parents)
    {

        // 1. Trace back from the end room to the start room using the parents dictionary
        var path = new List<RandomWalkRoom>();
        var current = end;
        while (current != null)
        {
            path.Add(current);
            parents.TryGetValue(current, out current);
        }
        // 2. Reverse the path to get it from start to end
        path.Reverse();
        return path;
    }

    private void PlaceMiniBosses(List<RandomWalkRoom> criticalPath, int count)
    {
        // Skip start (index 0) and boss room (last index)
        var eligible = criticalPath.Skip(1).Take(criticalPath.Count - 2).ToList();
        if (eligible.Count == 0) return; // No rooms to place mini bosses in

        count = Math.Min(count, eligible.Count);
        float spacing = (float)eligible.Count / count;

        for (int i = 0; i < count; i++)
        {
            int index = (int)(spacing * i + spacing / 2); // Place mini boss in the middle of each segment
            eligible[index].RoomType = RoomType.MiniBoss;
        }

    }

    private void EnsureMinimum(List<RandomWalkRoom> rooms, HashSet<RandomWalkRoom> criticalSet, RoomType type, int minimum)
    {
        int current = rooms.Count(r => r.RoomType == type);
        int needed = minimum - current;
        if (needed <=0) return;

        var candidates = rooms
            .Where(r => r.RoomType == RoomType.EnemyRoom && !criticalSet.Contains(r))
            .OrderBy(_ => rng.Next()) // Shuffle candidates
            .Take(needed)
            .ToList();

        foreach (var room in candidates)
        {
            room.RoomType = type;
            if (type == RoomType.TreasureRoom)
            {
                room.IsCleared = true; // TREASURE ROOMS AUTO CLEAR
            }
        }
        if (candidates.Count < needed)
        {
            GD.PrintErr($"Warning: Not enough rooms to assign {type}. Needed {needed}, but only found {candidates.Count} candidates.");
        }
    }
            
}