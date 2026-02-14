using System;

public static class WaveCollapseGenerator
{
    private static int tileCount = 16;
	
	public static int[,] Generate(
		int width,
		int height,
		int tileType = 0,
		int? seed = null)
	{
        // Check if dimensions are valid
		if (width <= 0 || height <= 0)
			return new int[0, 0];

		var rng = seed.HasValue ? new Random(seed.Value) : new Random();

		float[] weights = GetWeights(tileType);
		var tiles = GetTiles(tileType);
		var waveFunction = new WaveFunctionCollapse(width, height, tiles, rng);

		if (waveFunction.Collapse())
		{
			return waveFunction.GetResult();
		}
		else
		{
			return new int[width, height];
		}
	}


	// Get the weights for the given tile type
	private static float[] GetWeights(int tileType)
	{
		return tileType switch
		{
			0 => CreateUniformWeights(),
            1 => CreateCornersOnlyWeights(),
            2 => CreateVerticalRoadsWeights(),
            3 => CreateIntersectionWeights(),
            _ => CreateUniformWeights()
		};
	}

	// Create uniform weights for all tiles
	private static float[] CreateUniformWeights()
    {
        float[] weights = new float[tileCount];
        for (int i = 0; i < tileCount; i++)
            weights[i] = 1.0f;
        return weights;
    }

	// Create weights for corners only
	private static float[] CreateCornersOnlyWeights()
    {
        float[] weights = new float[tileCount];
        weights[1] = 1.0f; // Top left corner
        weights[2] = 1.0f; // Top right corner
        weights[3] = 1.0f; // Bottom left corner
		weights[4] = 1.0f; // Bottom right corner
        return weights;
    }

	// Create weights for vertical roads
	private static float[] CreateVerticalRoadsWeights()
    {
        float[] weights = new float[tileCount];
        weights[6] = 3.0f;  // Vertical road
        weights[7] = 1.0f;  // T-junction
        weights[9] = 1.0f;  // T-junction
        weights[15] = 2.0f; // 4-way
        return weights;
    }

	// Create weights for intersections
	private static float[] CreateIntersectionWeights()
    {
        float[] weights = new float[tileCount];
        weights[15] = 5.0f; // 4-way intersection
        weights[5] = 1.0f;  // H-road
        weights[6] = 1.0f;  // V-road
        return weights;
    }


	// Tile class for the wave function collapse algorithm
	private class Tile 
	{
		public int Id { get; set; }
		public float Weight { get; set; }
		public ulong[] CompatibleNeighbors { get; set; } = new ulong[4]; // 0 = top, 1 = right, 2 = bottom, 3 = left
	}

	// Get the tiles for the given weights
	private static Tile[] GetTiles(float[] weights)
	{
		Tile[] tiles = new Tile[tileCount];
		for (int i = 0; i < tileCount; i++)
		{
			tiles[i] = new Tile { Id = i, Weight = weights[i] };
		}

		// Tile 0: Empty - connects to everything
        SetAllConnections(tiles[0]);

        // Tile 1: Corner TL (Top-Left) - connects South and East
        tiles[1].CompatibleNeighbors[2] = 0b0011111;  // South: 0,1,2,3,4
        tiles[1].CompatibleNeighbors[1] = 0b0011111;  // East: 0,1,2,3,4

        // Tile 2: Corner TR (Top-Right) - connects South and West
        tiles[2].CompatibleNeighbors[2] = 0b0011111;  // South: 0,1,2,3,4
        tiles[2].CompatibleNeighbors[3] = 0b0011111;  // West: 0,1,2,3,4

        // Tile 3: Corner BL (Bottom-Left) - connects North and East
        tiles[3].CompatibleNeighbors[0] = 0b0011111;  // North: 0,1,2,3,4
        tiles[3].CompatibleNeighbors[1] = 0b0011111;  // East: 0,1,2,3,4

        // Tile 4: Corner BR (Bottom-Right) - connects North and West
        tiles[4].CompatibleNeighbors[0] = 0b0011111;  // North: 0,1,2,3,4
        tiles[4].CompatibleNeighbors[3] = 0b0011111;  // West: 0,1,2,3,4

        // Tile 5: H-Road (Horizontal) - connects East and West only
        tiles[5].CompatibleNeighbors[1] = 0b100000111110;  // East: 0, 5, 7, 10, 13, 15
        tiles[5].CompatibleNeighbors[3] = 0b100000001110;  // West: 0, 5, 8, 11, 15

        // Tile 6: V-Road (Vertical) - connects North and South only
        tiles[6].CompatibleNeighbors[0] = 0b100000111100;  // North: 0, 6, 7, 8, 15
        tiles[6].CompatibleNeighbors[2] = 0b100000110100;  // South: 0, 6, 9, 10, 15

        // Tile 7: T-Junction (opens down) - connects South, East, West
        tiles[7].CompatibleNeighbors[1] = 0b100000111110;  // East
        tiles[7].CompatibleNeighbors[2] = 0b100000110100;  // South
        tiles[7].CompatibleNeighbors[3] = 0b100000001110;  // West

        // Tile 8: T-Junction (opens left) - connects North, South, West
        tiles[8].CompatibleNeighbors[0] = 0b100000111100;  // North
        tiles[8].CompatibleNeighbors[2] = 0b100000110100;  // South
        tiles[8].CompatibleNeighbors[3] = 0b100000001110;  // West

        // Tile 9: T-Junction (opens up) - connects North, East, West
        tiles[9].CompatibleNeighbors[0] = 0b100000111100;  // North
        tiles[9].CompatibleNeighbors[1] = 0b100000111110;  // East
        tiles[9].CompatibleNeighbors[3] = 0b100000001110;  // West

        // Tile 10: T-Junction (opens right) - connects North, South, East
        tiles[10].CompatibleNeighbors[0] = 0b100000111100;  // North
        tiles[10].CompatibleNeighbors[1] = 0b100000111110;  // East
        tiles[10].CompatibleNeighbors[2] = 0b100000110100;  // South

        // Tile 11: L-Bend (TL) - connects South and East
        tiles[11].CompatibleNeighbors[1] = 0b100000111110;  // East
        tiles[11].CompatibleNeighbors[2] = 0b100000110100;  // South

        // Tile 12: L-Bend (TR) - connects South and West
        tiles[12].CompatibleNeighbors[2] = 0b100000110100;  // South
        tiles[12].CompatibleNeighbors[3] = 0b100000001110;  // West

        // Tile 13: L-Bend (BR) - connects North and West
        tiles[13].CompatibleNeighbors[0] = 0b100000111100;  // North
        tiles[13].CompatibleNeighbors[3] = 0b100000001110;  // West

        // Tile 14: L-Bend (BL) - connects North and East
        tiles[14].CompatibleNeighbors[0] = 0b100000111100;  // North
        tiles[14].CompatibleNeighbors[1] = 0b100000111110;  // East

        // Tile 15: 4-Way Intersection - connects all directions
        SetAllConnections(tiles[15]);

		return tiles;
	}

	// Helper method to set all connections for a tile
	private static void SetAllConnections(Tile tile)
    {
        ulong allTiles = (1UL << tileCount) - 1;
        for (int dir = 0; dir < 4; dir++)
            tile.CompatibleNeighbors[dir] = allTiles;
    }


	// Wave function state class for the wave function collapse algorithm
	private class WaveFunctionState
    {
        private int width, height;
        private ulong[] superpositions;
        private int[] collapsed;
        private Tile[] tiles;
        private Random random;

        public WaveFunctionState(int w, int h, Tile[] tileSet, Random rng)
        {
            width = w;
            height = h;
            tiles = tileSet;
            superpositions = new ulong[w * h];
            collapsed = new int[w * h];
            random = rng;
        }

        public bool Collapse()
        {
			// Initialize allowed tiles
			ulong allowedTiles = 0;
			for (int i = 0; i < tiles.Length; i++)
				if (tiles[i].Weight > 0)
					allowedTiles |= 1UL << i;

			// Initialize superpositions and collapsed arrays
			for (int i = 0; i < superpositions.Length; i++)
			{
				superpositions[i] = allowedTiles;
				collapsed[i] = 0;
			}

			// Collapse the wave function
			while (true)
			{
				int cellIndex = FindMinEntropyCell();
				if (cellIndex == -1) return true; // Successfully collapsed the wave function
				if (collapsed[cellIndex] != 0) return false; // Cell already collapsed
				PropagateCollapse(cellIndex);
			}
        }

		private int FindMinEntropyCell()
		{
			int minEntropy = int.MaxValue;
			List<int> candidates = new List<int>();

			for (int i = 0; i < superpositions.Length; i++)
			{
				if (collapsed[i] != -1) continue;
				int entropy = PopCount(superpositions[i]);

				if (entropy < -2) continue; // Contradiction: no tiles compatible with neighbors
				if (entropy < minEntropy) // New minimum entropy
				{
					minEntropy = entropy;
					candidates.Clear();
					candidates.Add(i);
				}
				else if (entropy == minEntropy) // Equal entropy, add to candidates
				{
					candidates.Add(i);
			}

			return candidates.Count > 0 ? candidates[random.Next(candidates.Count)] : -1;
		}

		// Collapse a cell by choosing a random tile from the possible tiles
		private bool CollapseCell(int cellIndex)
		{
			ulong superposition = superpositions[cellIndex];
			List<int> possibleTiles = new List<int>();
			for (int i = 0; i < tiles.Length; i++)
				if ((superposition & (1UL << i)) != 0)
					possibleTiles.Add(i);

			if (possibleTiles.Count == 0) return false;

			int chosenTile = WeightedRandom(possibleTiles);
			collapsed[cellIndex] = chosenTile;
			superpositions[cellIndex] = 1UL << chosenTile;
			return true;
		}

		// Choose a random tile from the possible tiles based on the weights
		private int WeightedRandom(List<int> tiles)
		{
			float totalWeight = 0;
			foreach (int tile in tiles)
				totalWeight += tiles[tile].Weight;

			float randomValue = (float)random.NextDouble() * totalWeight;
			float cumulativeWeight = 0;

			foreach (int tile in tiles)
			{
				cumulativeWeight += tiles[tile].Weight;
				if (randomValue <= cumulativeWeight)
					return tile;
			}
			return tiles[tiles.Count - 1];
		}

		// Propogate the collapse to the neighboring cells
		private void PropagateCollapse(int cellIndex)
		{
			Queue<int> queue = new Queue<int>();
			queue.Enqueue(cellIndex);

			int[,] offsets = { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } };

			while (queue.Count > 0)
			{
				int currentCell = queue.Dequeue();
				int x = currentCell % width;
				int y = currentCell / width;
				int tile = collapsed[currentCell];

				for (int i = 0; dir < 4; dir++)
				{
					int nx = x + offsets[dir, 0];
					int ny = y + offsets[dir, 1];
					if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

					int neighborIndex = ny * width + nx;
					if (collapsed[neighborIndex] != 0) continue;

					if (collapsed[neighborIndex] != -1) continue;

					ulong oldSuperposition = superpositions[neighborIndex];
					int oppositeDir = (dir + 2) % 4;
					ulong compatibleNeighbors = tiles[tile].CompatibleNeighbors[dir];
					ulong newSuperposition = (oldSuperposition & compatibleNeighbors);

					if (newSuperposition != oldSuperposition)
					{
						superpositions[neighborIndex] = newSuperposition;
						if (newSuperposition != 0) 
							queue.Enqueue(neighborIndex);
					}

				}		
			}
		}
	
		// Get the grid of the collapsed wave function
		public int[,] GetGrid()
		{
			int[,] grid = new int[width, height];
			for (int i = 0; i < width * height; i++)
				grid[i % width, i / width] = collapsed[i];
			return grid;
		}
		
		// Count the number of set bits in a ulong
		private static int PopCount(ulong value)
		{
			int count = 0;
			while (value > 0)
			{
				count += (int)(value & 1);
				value >>= 1;
			}
			return count;
		}
	}
}