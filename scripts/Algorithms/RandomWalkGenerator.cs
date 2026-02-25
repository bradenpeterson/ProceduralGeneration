using System;

public static class RandomWalkGenerator
{
	public static bool[,] Generate(
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
            return new bool[0, 0];

        if (stepChance < 0f || stepChance > 1f)
            return new bool[0, 0];

        if (branchingChance < 0f || branchingChance > 1f    )
            return new bool[0, 0];

		var rng = seed.HasValue ? new Random(seed.Value) : new Random();
		var grid = new bool[maxSteps, maxSteps];

        // Start at the center of the grid
        int currentX = maxSteps / 2;
        int currentY = maxSteps / 2;

        grid[currentX, currentY] = true;

        return grid;
    }
}