using System;
using System.Collections.Generic;

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
		var tiles = GetTiles(weights);
		var waveFunction = new WaveFunctionState(width, height, tiles, rng);

		if (waveFunction.Collapse())
		{
			return waveFunction.GetGrid();
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
        weights[3] = 1.0f; // Top left corner
        weights[6] = 1.0f; // Top right corner
        weights[9] = 1.0f; // Bottom left corner
		weights[12] = 1.0f; // Bottom right corner
        return weights;
    }

	// Create weights for vertical roads
	private static float[] CreateVerticalRoadsWeights()
    {
        float[] weights = new float[tileCount];
        weights[5] = 3.0f;  // Vertical road
        weights[7] = 1.0f;  // T-junction
        weights[13] = 1.0f;  // T-junction
        weights[15] = 2.0f; // 4-way
        return weights;
    }

	// Create weights for intersections
	private static float[] CreateIntersectionWeights()
    {
        float[] weights = new float[tileCount];
        weights[15] = 5.0f; // 4-way intersection
        weights[10] = 1.0f;  // H-road
        weights[5] = 1.0f;  // V-road
        return weights;
    }

	private class Tile
	{
		public int Id { get; set; }
		public float Weight { get; set; }
		public ulong[] CompatibleNeighbors { get; set; } = new ulong[4];
	}

	private static bool AreCompatible(int tile1, int tile2, int direction)
	{
		int[] oppositeDirections = { 2, 3, 0, 1 };
		bool tile1hasOpening = (tile1 & (1 << direction)) != 0;
		bool tile2hasOppositeOpening = (tile2 & (1 << oppositeDirections[direction])) != 0;
		return tile1hasOpening == tile2hasOppositeOpening;
	}

	private static Tile[] GetTiles(float[] weights)
	{
		Tile[] tiles = new Tile[tileCount];
		for (int i = 0; i < tileCount; i++)
		{
			tiles[i] = new Tile { Id = i, Weight = weights[i] };
		}

		for (int tileId = 0; tileId < tileCount; tileId++)
		{
			for (int direction = 0; direction < 4; direction++)
			{
				ulong compatibleNeighbors = 0;
				for (int otherTileId = 0; otherTileId < tileCount; otherTileId++)
				{
					if (AreCompatible(tileId, otherTileId, direction))
						compatibleNeighbors |= 1UL << otherTileId;
				}
				tiles[tileId].CompatibleNeighbors[direction] = compatibleNeighbors;
			}
		}
		return tiles;
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
				collapsed[i] = -1;
			}

			// Collapse the wave function
			while (true)
			{
				int cellIndex = FindMinEntropyCell();
				if (cellIndex == -1) return true; // All collapsed
				if (cellIndex == -2) return false; // Contradiction (entropy 0)
				if (!CollapseCell(cellIndex)) return false;
				if (!PropagateCollapse(cellIndex)) return false;
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

				if (entropy == 0) return -2; // Contradiction: no tiles compatible with neighbors
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
			}

			return candidates.Count > 0 ? candidates[random.Next(candidates.Count)] : -1;
		}

		// Collapse a cell by choosing a random tile from the possible tiles
		public bool CollapseCell(int cellIndex)
		{
			ulong superposition = superpositions[cellIndex];
			List<int> possibleTiles = new List<int>();

			// Only add tiles that are compatible with the neighbors
			for (int i = 0; i < tiles.Length; i++)
				if (((superposition & (1UL << i)) != 0) && tiles[i].Weight > 0)
					possibleTiles.Add(i);

			if (possibleTiles.Count == 0) return false;

			int chosenTile = WeightedRandom(possibleTiles);
			collapsed[cellIndex] = chosenTile;
			superpositions[cellIndex] = 1UL << chosenTile;
			return true;
		}

		// Choose a random tile from the possible tiles based on the weights
		public int WeightedRandom(List<int> tileIds)
		{
			float totalWeight = 0;
			foreach (int tileId in tileIds)
				totalWeight += tiles[tileId].Weight;

			float randomValue = (float)random.NextDouble() * totalWeight;
			float cumulativeWeight = 0;

			foreach (int tileId in tileIds)
			{
				cumulativeWeight += tiles[tileId].Weight;
				if (randomValue <= cumulativeWeight)
					return tileId;
			}
			return tileIds[tileIds.Count - 1];
		}

		// Propogate the collapse to the neighboring cells
		public bool PropagateCollapse(int cellIndex)
		{
			Queue<int> queue = new Queue<int>();
			queue.Enqueue(cellIndex);

			int[,] offsets = { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };

			while (queue.Count > 0)
			{
				int currentCell = queue.Dequeue();
				int x = currentCell % width;
				int y = currentCell / width;
				// Cell may have been enqueued after its superposition was reduced (not yet collapsed)
				int tile = collapsed[currentCell] >= 0
					? collapsed[currentCell]
					: (PopCount(superpositions[currentCell]) == 1 ? GetSingleTileFromMask(superpositions[currentCell]) : -1);
				if (tile < 0) continue;

				for (int dir = 0; dir < 4; dir++)
				{
					int nx = x + offsets[dir, 0];
					int ny = y + offsets[dir, 1];
					if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

					int neighborIndex = ny * width + nx;
					if (collapsed[neighborIndex] != -1) continue;

					ulong oldSuperposition = superpositions[neighborIndex];
					ulong compatibleNeighbors = tiles[tile].CompatibleNeighbors[dir];
					ulong newSuperposition = (oldSuperposition & compatibleNeighbors);

					if (newSuperposition != oldSuperposition)
					{
						superpositions[neighborIndex] = newSuperposition;
						if (newSuperposition == 0) return false; // Contradiction; FindMinEntropyCell will return -2
						queue.Enqueue(neighborIndex);
					}
				}		
			}
			return true;
		}
	
		// Get the grid of the collapsed wave function
		public int[,] GetGrid()
		{
			int[,] grid = new int[width, height];
			for (int i = 0; i < collapsed.Length; i++)
			{
				if (collapsed[i] >= 0)
					grid[i % width, i / width] = collapsed[i];
			}
			return grid;
		}

		public static int GetSingleTileFromMask(ulong mask)
		{
			for (int t = 0; t < 16; t++)
				if ((mask & (1UL << t)) != 0) return t;
			return 0;
		}
		
		// Count the number of set bits in a ulong
		public static int PopCount(ulong value)
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