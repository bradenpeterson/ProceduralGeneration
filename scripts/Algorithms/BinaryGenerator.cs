using System;
using System.Collections.Generic;

/// Axis-aligned rectangle in tile space
public struct RectInt
{
    public int X;
    public int Y;
    public int Width;
    public int Height;

    public RectInt(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}

/// BSP generator output: logical extents and tile-space rectangles.
public struct BspResult
{
    public int Width;
    public int Height;
    public List<RectInt> Rooms;
    public List<RectInt> Corridors;
}

public static class BinaryGenerator
{
    /// Generate a BSP layout for a width×height area (in tiles).
    /// minDepth/maxDepth control recursion depth; minRegionSize controls minimum region size.
    public static BspResult Generate(
        int width,
        int height,
        int minDepth,
        int maxDepth,
        int minRegionSize,
        float splitChance,
        int? seed = null)
    {
        if (width <= 0 || height <= 0)
        {
            return new BspResult
            {
                Width = 0,
                Height = 0,
                Rooms = new List<RectInt>(),
                Corridors = new List<RectInt>()
            };
        }

        var rng = seed.HasValue ? new Random(seed.Value) : new Random();

        Node root = BuildTree(
            x: 0,
            y: 0,
            width: width,
            height: height,
            depth: 0,
            minDepth: minDepth,
            maxDepth: maxDepth,
            minRegionSize: minRegionSize,
            splitChance: splitChance,
            rng: rng);

        var rooms = new List<RectInt>();
        var corridors = new List<RectInt>();

        // Room/corridor placement will be added later.

        return new BspResult
        {
            Width = width,
            Height = height,
            Rooms = rooms,
            Corridors = corridors
        };
    }

    /// A node in the binary space partitioning tree.
    private class Node
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int Depth;

        public Node Left;
        public Node Right;

        public int RoomX;
        public int RoomY;
        public int RoomWidth;
        public int RoomHeight;

        public bool IsLeaf => Left == null && Right == null;
        public bool HasRoom => RoomWidth > 0 && RoomHeight > 0;
    }

    /// Builds a binary space partitioning tree and returns the root node.
    private static Node BuildTree(
        int x,
        int y,
        int width,
        int height,
        int depth,
        int minDepth,
        int maxDepth,
        int minRegionSize,
        float splitChance,
        Random rng)
    {
        var node = new Node
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Depth = depth
        };

        // Stop if we've reached the maximum depth or the region is too small to split.
        if (depth >= maxDepth || width < 2 * minRegionSize || height < 2 * minRegionSize)
            return node;

        // Decide whether we must split or may stop.
        bool mustSplit = depth < minDepth;
        if (!mustSplit)
        {
            // After minDepth, we only split with probability splitChance.
            if (rng.NextDouble() >= splitChance)
                return node;
        }

        // Decide split direction: prefer the longer side, else random.
        bool splitVertically;
        if (width > height)
            splitVertically = true;
        else if (height > width)
            splitVertically = false;
        else
            splitVertically = rng.Next(2) == 0;

        if (splitVertically)
        {
            int minSplitX = x + minRegionSize;
            int maxSplitX = x + width - minRegionSize;
            if (minSplitX > maxSplitX)
                return node; // cannot split further

            int splitX = rng.Next(minSplitX, maxSplitX + 1);

            int leftWidth = splitX - x;
            int rightWidth = x + width - splitX;

            node.Left = BuildTree(
                x: x,
                y: y,
                width: leftWidth,
                height: height,
                depth: depth + 1,
                minDepth: minDepth,
                maxDepth: maxDepth,
                minRegionSize: minRegionSize,
                splitChance: splitChance,
                rng: rng);

            node.Right = BuildTree(
                x: splitX,
                y: y,
                width: rightWidth,
                height: height,
                depth: depth + 1,
                minDepth: minDepth,
                maxDepth: maxDepth,
                minRegionSize: minRegionSize,
                splitChance: splitChance,
                rng: rng);
        }
        else
        {
            int minSplitY = y + minRegionSize;
            int maxSplitY = y + height - minRegionSize;
            if (minSplitY > maxSplitY)
                return node; // cannot split further

            int splitY = rng.Next(minSplitY, maxSplitY + 1);

            int topHeight = splitY - y;
            int bottomHeight = y + height - splitY;

            node.Left = BuildTree(
                x: x,
                y: y,
                width: width,
                height: topHeight,
                depth: depth + 1,
                minDepth: minDepth,
                maxDepth: maxDepth,
                minRegionSize: minRegionSize,
                splitChance: splitChance,
                rng: rng);

            node.Right = BuildTree(
                x: x,
                y: splitY,
                width: width,
                height: bottomHeight,
                depth: depth + 1,
                minDepth: minDepth,
                maxDepth: maxDepth,
                minRegionSize: minRegionSize,
                splitChance: splitChance,
                rng: rng);
        }

        return node;
    }
}