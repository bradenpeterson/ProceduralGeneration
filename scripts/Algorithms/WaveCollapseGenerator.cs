using System;

public static class WaveCollapseGenerator
{
	public static int[,] Generate(
		int width,
		int height,
		int tileType,
		int? seed = null)
	{
        // Check if dimensions are valid
		if (width <= 0 || height <= 0)
			return new int[0, 0];

		var rng = seed.HasValue ? new Random(seed.Value) : new Random();
		int[,] grid = new int[width, height];

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
                // TODO: Implement wave collapse algorithm
			}
		}
		return grid;
	}
}