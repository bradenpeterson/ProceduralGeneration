using Godot;
using System;
using System.Linq;

public static class PerlinGenerator
{
    private static int[] P; // Permutation table
    private static Vector2[] G; // Gradient table

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
                float noiseValue = fbm
                    ? FractalNoise2D(i, j, octaves, persistence, scale)
                    : PerlinNoise2D(i * scale, j * scale);
                noiseMap[i, j] = noiseValue;
            }
        }
                
        return noiseMap;
    }

    private static void InitializeTables(int? seed = null)
    {
        Random rng = seed.HasValue ? new Random(seed.Value) : new Random();

        // Initialize permutation table P with values 0-255 in random order
        P = Enumerable.Range(0, 256).ToArray();
        for (int i = P.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            int temp = P[i];
            P[i] = P[j];
            P[j] = temp;
        }

        // Extend P to size 512 by duplicating the first 256 values
        P = P.Concat(P).ToArray();

        // Initialize gradient table G with random unit vectors
        const int gradientCount = 12;
        G = new Vector2[gradientCount];
        for (int i = 0; i < gradientCount; i++)
        {
            double angle = rng.NextDouble() * Math.PI * 2.0;
            G[i] = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }
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
        int p00 = P[P[x0 & 255] + (y0 & 255)];
        int p10 = P[P[x1 & 255] + (y0 & 255)];
        int p01 = P[P[x0 & 255] + (y1 & 255)];
        int p11 = P[P[x1 & 255] + (y1 & 255)];

        // Map permutation values to gradient table indices using modulo
        int gIndex00 = p00 % G.Length;
        int gIndex10 = p10 % G.Length;
        int gIndex01 = p01 % G.Length;
        int gIndex11 = p11 % G.Length;

        // Select gradient vectors from the gradient table
        Vector2 g00 = G[gIndex00];
        Vector2 g10 = G[gIndex10];
        Vector2 g01 = G[gIndex01];
        Vector2 g11 = G[gIndex11];

        // Calculate dot products
        float d00 = g00.Dot(new Vector2(dx, dy));
        float d10 = g10.Dot(new Vector2(dx - 1, dy));
        float d01 = g01.Dot(new Vector2(dx, dy - 1));
        float d11 = g11.Dot(new Vector2(dx - 1, dy - 1));

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

    public static float FractalNoise2D(float x, float y, int octaves, float persistence, float scale)
    {
        float total = 0;
        float amplitude = 1;
        float frequency = scale;
        float maxValue = 0;

        for (int i = 0; i < octaves; i++)
        {
            total += PerlinNoise2D(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= 2;
        }

        return total / maxValue; // Normalize to [-1, 1] range
    }
}

