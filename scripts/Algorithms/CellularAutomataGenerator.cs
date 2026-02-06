using System;

public static class CellularAutomataGenerator
{
	public static bool[,] Generate(
		int width, // number of columns
		int height, // number of rows
		float fillProbability,
		int iterations, 
		int birthThreshold, // empty cell becomes wall if it has at least this many wall neighbours
		int survivalThreshold, // wall cell stays wall if it has at least this many wall neighbours; otherwise becomes floor
		int? seed = null) // optional seed for reproducible maps; null for random seed
	{
		// Check if dimensions are valid
		if (width <= 0 || height <= 0)
			return new bool[0, 0];

		var rng = seed.HasValue ? new Random(seed.Value) : new Random();
		bool[,] grid = new bool[width, height];

		// Initial random fill: true = wall, false = floor
		for (int x = 0; x < width; x++)
			for (int y = 0; y < height; y++)
				grid[x, y] = rng.NextDouble() < fillProbability;

		// CA iterations with double buffer
		bool[,] next = new bool[width, height];
		for (int iteration = 0; iteration < iterations; iteration++)
		{
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					int wallNeighbours = CountWallNeighbours(grid, width, height, x, y);
					if (grid[x, y])
						next[x, y] = wallNeighbours >= survivalThreshold;
					else
						next[x, y] = wallNeighbours >= birthThreshold;
				}
			}
			(grid, next) = (next, grid);
		}

		return grid; 
	}

	/// Counts wall neighbours in the cells around the given cell. Out-of-bounds cells count as wall.
	private static int CountWallNeighbours(bool[,] grid, int width, int height, int cellX, int cellY)
	{
		int count = 0;
		for (int offsetX = -1; offsetX <= 1; offsetX++)
			for (int offsetY = -1; offsetY <= 1; offsetY++)
			{
				if (offsetX == 0 && offsetY == 0) continue;
				int neighborX = cellX + offsetX;
				int neighborY = cellY + offsetY;
				if (neighborX < 0 || neighborX >= width || neighborY < 0 || neighborY >= height)
					count++;
				else if (grid[neighborX, neighborY])
					count++;
			}
		return count;
	}
}
