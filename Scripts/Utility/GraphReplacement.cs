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
    private const int RequiredMiniBosses = 3;
    private const float TreasureRoomChance = 0.15f;
    private const float PuzzleRoomChance = 0.10f;

    private Random rng;

    private const int MinTreasureRooms = 3;
    private const int MinPuzzleRooms = 2;

    private const int minPathLengthOfCriticalPath = 15; // Added a minimum path length requirement for the critical path to ensure more interesting dungeons, can be adjusted as needed.

    public bool Replace(List<RandomWalkRoom> rooms, List<RandomWalkHallway> hallways, int seed)
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

        // 4. Assign start room
        startRoom.RoomType = RoomType.Start;
        startRoom.IsCleared = true;
        startRoom.Depth = 0f;
        startRoom.IsDiscovered = true;

        // 5. Trace critical path from start to boss
        var (distancesFromStart, parentsFromStart) = BFS(startRoom, adjacency);
        var criticalPath = TracePath(bossRoom, startRoom, parentsFromStart);

        if (criticalPath.Count < minPathLengthOfCriticalPath)
        {
           return false;
        }

        // 6. Assign depth along critical path (0 at start, 1 at boss)
        var criticalSet = new HashSet<RandomWalkRoom>(criticalPath);
        int pathLength = criticalPath.Count;
        for (int i = 0; i < pathLength; i++)
        {
            criticalPath[i].Depth = pathLength > 1 ? (float)i / (pathLength - 1) : 0f;
        }

        // 7. Propagate depth to branch rooms (BFS from each critical path room into branches)
        var assigned = new HashSet<RandomWalkRoom>(criticalSet);
        var queue = new Queue<RandomWalkRoom>();

        foreach (var room in criticalPath)
            queue.Enqueue(room);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!adjacency.ContainsKey(current)) continue;
            foreach (var neighbor in adjacency[current])
            {
                if (assigned.Contains(neighbor)) continue;
                neighbor.Depth = current.Depth; // inherit parent's depth
                assigned.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        // 8. Place mini bosses along critical path
        PlaceMiniBosses(criticalPath, RequiredMiniBosses);

        // 8b NEW Place at least one treasure room on critical path ( no longer needed )
        // PlaceMandatoryTreasureRoom(criticalPath);

        // 9. Assign remaining room types
        foreach (var room in rooms)
        {
            if (room.RoomType != RoomType.EnemyRoom) continue;
            if (criticalSet.Contains(room)) continue;

            // double roll = rng.NextDouble();
            // if (roll < TreasureRoomChance)
            // {
            //     room.RoomType = RoomType.TreasureRoom;
            //     room.IsCleared = true;
            // }

            // NOT ENOUGH TIME TO IMPLEMENT PUZZLE ROOMS, SAVE FOR LATER

            // else if (roll < TreasureRoomChance + PuzzleRoomChance)
            // {
            //     room.RoomType = RoomType.PuzzleRoom;
            // }
        }

        // PlaceTreasureRooms(rooms, criticalSet, MinTreasureRooms);

        // 10. Ensure minimum treasure rooms (dont need this anymore, replaced by placetreasurerooms function)
        //EnsureMinimum(rooms, criticalSet, RoomType.TreasureRoom, MinTreasureRooms);

        // NOT ENOUGH TIME TO IMPLEMENT PUZZLE ROOMS, SAVE FOR LATER

        //EnsureMinimum(rooms, criticalSet, RoomType.PuzzleRoom, MinPuzzleRooms);

        PlaceBranchTreasures(rooms, criticalSet, adjacency);

    return true;
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

        //0.5 Place final treasure explicitly on the room right before the boss
        var finalTreasure = criticalPath[criticalPath.Count - 2];
        if (finalTreasure.RoomType == RoomType.EnemyRoom)
        {
            finalTreasure.RoomType = RoomType.TreasureRoom;
        }

        
        // 1. Create a list of eligible rooms for minibosses
        var eligible = new List<RandomWalkRoom>();
        for (int i = 0; i<criticalPath.Count; i++)
        {
            if (i==0) continue; // skip the start room
            if (i==1) continue; // skip the first room after start
            if (i == criticalPath.Count - 1) continue; // skip the boss room
            if (i >= criticalPath.Count - 3) continue; // leave at least 2 rooms before boss
            eligible.Add(criticalPath[i]);

        }



        if (eligible.Count < count * 2) return;


        // Now work with the remaining eligible rooms (exclude the final treasure)

        float spacing = (float)eligible.Count / count;

        for (int i = 0; i < count; i++)
        {
            int segmentStart = (int)(spacing * i);
            int segmentEnd = (int)(spacing * (i + 1)) - 1;

            // Find first available enemy room in segment for miniboss
            int mIndex = -1;
            for (int j = segmentStart; j <= segmentEnd; j++)
            {
                if (eligible[j].RoomType == RoomType.EnemyRoom)
                {
                    mIndex = j;
                    break;
                }
            }
            if (mIndex == -1) continue;
            eligible[mIndex].RoomType = RoomType.MiniBoss;

            // Treasure goes in next available enemy room after miniboss in segment
            for (int j = mIndex + 1; j <= segmentEnd && j < eligible.Count; j++)
            {
                if (eligible[j].RoomType == RoomType.EnemyRoom)
                {
                    eligible[j].RoomType = RoomType.TreasureRoom;
                    break;
                }
            }
        }
    }

    // private void EnsureMinimum(List<RandomWalkRoom> rooms, HashSet<RandomWalkRoom> criticalSet, RoomType type, int minimum)
    // {
    //     int current = rooms.Count(r => r.RoomType == type);
    //     int needed = minimum - current;
    //     if (needed <=0) return;

    //     var candidates = rooms
    //         .Where(r => r.RoomType == RoomType.EnemyRoom && !criticalSet.Contains(r))
    //         .OrderBy(_ => rng.Next()) // Shuffle candidates
    //         .Take(needed)
    //         .ToList();

    //     foreach (var room in candidates)
    //     {
    //         room.RoomType = type;
    //         if (type == RoomType.TreasureRoom)
    //         {
    //             room.IsCleared = true; // TREASURE ROOMS AUTO CLEAR
    //         }
    //     }
    //     if (candidates.Count < needed)
    //     {
    //         GD.PrintErr($"Warning: Not enough rooms to assign {type}. Needed {needed}, but only found {candidates.Count} candidates.");
    //     }
    // }

    // private void PlaceMandatoryTreasureRoom(List<RandomWalkRoom> criticalPath)
    // {
        
    //     // Skip start (index 0) and boss (last index), and rooms already assigned
    //     var eligible = criticalPath
    //         .Skip(1)
    //         .Take(criticalPath.Count - 2)
    //         .Where(r => r.RoomType == RoomType.EnemyRoom)
    //         .ToList();

    //     if (eligible.Count == 0) return;

    //     // Pick a random eligible room from the first half of the critical path
    //     var candidate = eligible[rng.Next(eligible.Count / 2)];
    //     candidate.RoomType = RoomType.TreasureRoom;
    //     candidate.IsCleared = true; // treasure rooms auto clear
    // }

    // private void PlaceTreasureRooms(List<RandomWalkRoom> rooms, HashSet<RandomWalkRoom> criticalSet, int count)
    // {
    //     // Get all eligible branch rooms sorted by depth
    //     var candidates = rooms
    //         .Where(r => r.RoomType == RoomType.EnemyRoom && !criticalSet.Contains(r))
    //         .OrderBy(r => r.Depth)
    //         .ToList();

    //     if (candidates.Count == 0) return;

    //     // Pick evenly spaced rooms across the sorted list
    //     float spacing = (float)candidates.Count / count;
    //     for (int i = 0; i < count && i < candidates.Count; i++)
    //     {
    //         int index = (int)(spacing * i + spacing / 2f);
    //         index = Math.Min(index, candidates.Count - 1);
    //         candidates[index].RoomType = RoomType.TreasureRoom;
    //         candidates[index].IsCleared = true;
    //     }
    // }

    private void PlaceBranchTreasures(List<RandomWalkRoom> rooms, HashSet<RandomWalkRoom> criticalSet, Dictionary<RandomWalkRoom, List<RandomWalkRoom>> adjacency)
    {
        var visited = new HashSet<RandomWalkRoom>(criticalSet);

        // For each room on the critical path, explore its branches
        foreach (var critRoom in criticalSet)
        {
            if (!adjacency.ContainsKey(critRoom)) continue;

            foreach (var neighbor in adjacency[critRoom])
            {
                if (criticalSet.Contains(neighbor)) continue; // already on critical path
                if (visited.Contains(neighbor)) continue; // already part of another branch

                // BFS into this branch to find its furthest point
                var branchQueue = new Queue<RandomWalkRoom>();
                var branchVisited = new HashSet<RandomWalkRoom>();
                RandomWalkRoom furthest = neighbor;

                branchQueue.Enqueue(neighbor);
                branchVisited.Add(neighbor);
                visited.Add(neighbor);

                while (branchQueue.Count > 0)
                {
                    var current = branchQueue.Dequeue();
                    furthest = current; // last dequeued in BFS is furthest

                    foreach (var next in adjacency[current])
                    {
                        if (branchVisited.Contains(next) || criticalSet.Contains(next)) continue;
                        if (visited.Contains(next)) continue;
                        branchVisited.Add(next);
                        visited.Add(next);
                        branchQueue.Enqueue(next);
                    }
                }
                // Place treasure at furthest point only if branch is long enough
                if (furthest.RoomType == RoomType.EnemyRoom && branchVisited.Count >= 3)
                {
                    furthest.RoomType = RoomType.TreasureRoom;
                }
            }
        }
    }
}