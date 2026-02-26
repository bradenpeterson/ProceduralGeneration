using System;
using System.Collections.Generic;

public static class RandomWalkGenerator
{
    /// Result of random walk: which cells are rooms and which room pairs are connected by a hallway.
    public struct RandomWalkResult
    {
        public bool[,] Grid;
        public List<(int FromX, int FromY, int ToX, int ToY)> Edges;
    }

    private static readonly (int directionX, int directionY)[] Directions = new (int, int)[]
    {
        (1, 0), // Right
        (0, 1), // Down
        (-1, 0), // Left
        (0, -1)  // Up
    };

	public static RandomWalkResult Generate(
		int minSteps,
		int maxSteps,
		float stepChance,
		bool allowLoops,
		bool allowBranching,
		bool allowConnectingBranches,
		float branchingChance,
		int? seed = null)
	{
        if (minSteps <= 0 || maxSteps <= 0 || minSteps > maxSteps)
            return new RandomWalkResult { Grid = new bool[0, 0], Edges = new List<(int, int, int, int)>() };

        // Clamp probabilities into [0,1] so bad UI input doesn't kill generation.
        stepChance = Math.Clamp(stepChance, 0f, 1f);
        branchingChance = Math.Clamp(branchingChance, 0f, 1f);

		var rng = seed.HasValue ? new Random(seed.Value) : new Random();
		var grid = new bool[maxSteps, maxSteps];
		var edges = new List<(int FromX, int FromY, int ToX, int ToY)>();
		var branchOwners = new int[maxSteps, maxSteps]; // which branch first carved each cell

        // Cap total edges so branching doesn't cause exponential blow-up
        int maxTotalEdges = maxSteps * maxSteps * 2;

        // Start at the center of the grid
        int currentX = maxSteps / 2;
        int currentY = maxSteps / 2;

        grid[currentX, currentY] = true;

        // Branch 0 is the initial walk starting from the center.
        int currentBranchId = 0;
        branchOwners[currentX, currentY] = currentBranchId;
        int nextBranchId = 1;

        Walk(grid, branchOwners, edges, maxTotalEdges,
             currentX, currentY, currentBranchId, ref nextBranchId,
             0, minSteps, maxSteps, stepChance, branchingChance,
             allowLoops, allowConnectingBranches, allowBranching, rng);

        return new RandomWalkResult { Grid = grid, Edges = edges };
    }

    /// Checks if the given coordinates are within the grid boundaries.
    private static bool InBounds(bool[,] grid, int x, int y)
    {
        return x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1);
    }

    /// Checks if the given cell has a valid move for the given branch.
    private static bool HasValidMove(
        bool[,] grid,
        int[,] branchOwners,
        int x,
        int y,
        int currentBranchId,
        bool allowLoops,
        bool allowConnectingBranches)
    {
        foreach (var (directionX, directionY) in Directions)
        {
            int newX = x + directionX;
            int newY = y + directionY;

            if (!InBounds(grid, newX, newY))
                continue;

            if (!grid[newX, newY])
                return true;

            if (!allowLoops)
                continue;

            // Existing room: allowed if it belongs to this branch, or if
            // cross-branch connections are enabled.
            int owner = branchOwners[newX, newY];
            if (owner == currentBranchId || allowConnectingBranches)
                return true;
        }

        return false;
    }

    /// Tries to get a random valid direction from the given cell.
    private static bool TryGetRandomDirection(
        bool[,] grid,
        int[,] branchOwners,
        int x,
        int y,
        int currentBranchId,
        bool allowLoops,
        bool allowConnectingBranches,
        Random rng,
        out int dx,
        out int dy)
    {
        dx = 0;
        dy = 0;

        int validDirections = 0;
        foreach (var (directionX, directionY) in Directions)
        {
            int newX = x + directionX;
            int newY = y + directionY;

            if (!InBounds(grid, newX, newY))
                continue;

            if (!grid[newX, newY])
            {
                validDirections++;
                continue;
            }

            if (!allowLoops)
                continue;

            int owner = branchOwners[newX, newY];
            if (owner == currentBranchId || allowConnectingBranches)
                validDirections++;
        }

        if (validDirections == 0)
            return false;

        int randomIndex = rng.Next(validDirections);
        int currentIndex = 0;

        foreach (var (directionX, directionY) in Directions)
        {
            int newX = x + directionX;
            int newY = y + directionY;

            if (!InBounds(grid, newX, newY))
                continue;

            if (!grid[newX, newY])
            {
                if (currentIndex == randomIndex)
                {
                    dx = directionX;
                    dy = directionY;
                    return true;
                }
                
                currentIndex++;
                continue;
            }

            if (!allowLoops)
                continue;

            int owner = branchOwners[newX, newY];
            if (owner == currentBranchId || allowConnectingBranches)
            {
                if (currentIndex == randomIndex)
                {
                    dx = directionX;
                    dy = directionY;
                    return true;
                }

                currentIndex++;
            }
        }

        return false;
    }

    /// Recursively produces a random walk on the grid from the given cell.
    private static void Walk(
        bool[,] grid,
        int[,] branchOwners,
        List<(int FromX, int FromY, int ToX, int ToY)> edges,
        int maxTotalEdges,
        int currentX,
        int currentY,
        int currentBranchId,
        ref int nextBranchId,
        int stepCount,
        int minSteps,
        int maxSteps,
        float stepChance,
        float branchingChance,
        bool allowLoops,
        bool allowConnectingBranches,
        bool allowBranching,
        Random rng)
    {
        if (edges.Count >= maxTotalEdges)
            return;

        bool canStop = stepCount >= minSteps &&
                       (stepCount >= maxSteps || !HasValidMove(grid, branchOwners, currentX, currentY, currentBranchId, allowLoops, allowConnectingBranches));
        if (canStop)
            return;

        // Once we've taken at least minSteps, stepChance is the probability to CONTINUE the branch.
        if (stepCount >= minSteps && rng.NextDouble() > stepChance)
            return;

        if (!TryGetRandomDirection(grid, branchOwners, currentX, currentY, currentBranchId, allowLoops, allowConnectingBranches, rng, out int dx, out int dy))
            return;

        int newX = currentX + dx;
        int newY = currentY + dy;

        // Record this step as a hallway between current room and next (same branch).
        edges.Add((currentX, currentY, newX, newY));

        if (!grid[newX, newY])
        {
            grid[newX, newY] = true;
            branchOwners[newX, newY] = currentBranchId;
        }

        // Continue this branch forward.
        Walk(grid, branchOwners, edges, maxTotalEdges,
             newX, newY, currentBranchId, ref nextBranchId,
             stepCount + 1, minSteps, maxSteps, stepChance, branchingChance,
             allowLoops, allowConnectingBranches, allowBranching, rng);

        // Optionally start a new branch from the current room.
        if (edges.Count < maxTotalEdges && allowBranching && rng.NextDouble() < branchingChance)
        {
            int newBranchId = nextBranchId++;
            Walk(grid, branchOwners, edges, maxTotalEdges,
                 currentX, currentY, newBranchId, ref nextBranchId,
                 stepCount, minSteps, maxSteps, stepChance, branchingChance,
                 allowLoops, allowConnectingBranches, allowBranching, rng);
        }
    }

}