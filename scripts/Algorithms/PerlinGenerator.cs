using Godot;
using System;
using System.Linq;

public static class PerlinGenerator
{
    private static int[] PermutationTable;
    private static Vector2[] GradientsTable = new Vector2[] {
        new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0,-1),
        new Vector2(1, 1).Normalized(), new Vector2(-1, 1).Normalized(),
        new Vector2(1,-1).Normalized(), new Vector2(-1,-1).Normalized(),
        new Vector2(2, 1).Normalized(), new Vector2(-2, 1).Normalized(),
        new Vector2(2,-1).Normalized(), new Vector2(-2,-1).Normalized(),
        new Vector2(1, 2).Normalized(), new Vector2(-1, 2).Normalized(),
        new Vector2(1,-2).Normalized(), new Vector2(-1,-2).Normalized()
    };

    public static float[,] Generate(
        int width,
        int height,
        bool fbm = false,
        int octaves = 4,
        float persistence = 0.5f,
        float scale = 0.01f,
        int? seed = null)
    {
        // Check if dimensions are valid
		if (width <= 0 || height <= 0)
			return new float[0, 0];

        InitializeTables(seed);
        float[,] noiseMap = new float[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float x = i * scale;
                float y = j * scale;
                
                float noiseValue = fbm
                    ? FractalNoise2D(x, y, octaves, persistence)
                    : PerlinNoise2D(x, y);
                noiseMap[i, j] = noiseValue;
            }
        }
                
        return noiseMap;
    }

    private static void InitializeTables(int? seed = null)
    {
        Random rng = seed.HasValue ? new Random(seed.Value) : new Random();

        // Initialize permutation table PermutationTable with values 0-255 in random order
        PermutationTable = Enumerable.Range(0, 256).ToArray();
        for (int i = PermutationTable.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            int temp = PermutationTable[i];
            PermutationTable[i] = PermutationTable[j];
            PermutationTable[j] = temp;
        }

        // Extend PermutationTable to size 512 by duplicating the first 256 values
        PermutationTable = PermutationTable.Concat(PermutationTable).ToArray();
    }
    
    public static float PerlinNoise2D(float x, float y)
    {
        // Find the grid cell coordinates
        int x0 = Mathf.FloorToInt(x);
        int y0 = Mathf.FloorToInt(y);
        int x1 = x0 + 1;
        int y1 = y0 + 1;

        // Calculate distance vectors from input point to grid corners
        float dx = x - x0;
        float dy = y - y0;

        // Get permutation values for each grid corner
        int p00 = PermutationTable[PermutationTable[x0 & 255] + (y0 & 255)] & (GradientsTable.Length - 1);
        int p10 = PermutationTable[PermutationTable[x1 & 255] + (y0 & 255)] & (GradientsTable.Length - 1);
        int p01 = PermutationTable[PermutationTable[x0 & 255] + (y1 & 255)] & (GradientsTable.Length - 1);
        int p11 = PermutationTable[PermutationTable[x1 & 255] + (y1 & 255)] & (GradientsTable.Length - 1);

        // Select gradient vectors from the gradient table
        Vector2 g00 = GradientsTable[p00];
        Vector2 g10 = GradientsTable[p10];
        Vector2 g01 = GradientsTable[p01];
        Vector2 g11 = GradientsTable[p11];

        // Calculate dot products
        float d00 = g00.Dot(new Vector2(dx, dy).Normalized());
        float d10 = g10.Dot(new Vector2(dx - 1, dy).Normalized());
        float d01 = g01.Dot(new Vector2(dx, dy - 1).Normalized());
        float d11 = g11.Dot(new Vector2(dx - 1, dy - 1).Normalized());

        // Apply smooth interpolation function (ease curve)
        float smoothX = SmoothStep(dx);
        float smoothY = SmoothStep(dy);

        // Interpolate horizontally, then vertically
        float ix0 = Mathf.Lerp(d00, d10, smoothX);
        float ix1 = Mathf.Lerp(d01, d11, smoothX);
        return Mathf.Lerp(ix0, ix1, smoothY);
    }

    private static float SmoothStep(float t)
    {
        return t*t*t*(t*(t*6-15)+10);
    }

    public static float FractalNoise2D(float x, float y, int octaves, float persistence)
    {
        float total = 0;
        float amplitude = 1;
        float frequency = 1;
        float maxValue = 0;

        for (int i = 0; i < octaves; i++)
        {
            total += PerlinNoise2D(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= 2;
        }

        return total / maxValue;
    }
}

