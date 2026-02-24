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

        PlaceRooms(root, rooms, rng);
        ConnectRooms(root, corridors, rng);

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

    /// Places a room in every leaf region such that the room always contains the region center.
    private static void PlaceRooms(Node node, List<RectInt> rooms, Random rng)
    {
        if (node == null)
            return;

        if (node.IsLeaf)
        {
            // Region center in tile coordinates
            int cx = node.X + node.Width / 2;
            int cy = node.Y + node.Height / 2;

            // Maximum extents we can grow from the center without leaving the region
            int maxLeft  = cx - node.X;
            int maxRight = node.X + node.Width  - 1 - cx;
            int maxUp    = cy - node.Y;
            int maxDown  = node.Y + node.Height - 1 - cy;

            // Randomly choose how far the room extends in each direction
            int left  = rng.Next(0, maxLeft  + 1);
            int right = rng.Next(0, maxRight + 1);
            int up    = rng.Next(0, maxUp    + 1);
            int down  = rng.Next(0, maxDown  + 1);

            int roomX = cx - left;
            int roomY = cy - up;
            int roomWidth  = left + right + 1;
            int roomHeight = up + down + 1;

            node.RoomX = roomX;
            node.RoomY = roomY;
            node.RoomWidth = roomWidth;
            node.RoomHeight = roomHeight;

            rooms.Add(new RectInt(roomX, roomY, roomWidth, roomHeight));
        }
        else
        {
            PlaceRooms(node.Left, rooms, rng);
            PlaceRooms(node.Right, rooms, rng);
        }
    }

    /// Places corridors in the binary space partitioning tree.
    private static void ConnectRooms(Node node, List<RectInt> corridors, Random rng)
    {
        if (node == null || node.IsLeaf) 
        {
            return;
        }

        ConnectRooms(node.Left, corridors, rng);
        ConnectRooms(node.Right, corridors, rng);

        var leftCenter = GetSubtreeRoomCenter(node.Left, rng);
        var rightCenter = GetSubtreeRoomCenter(node.Right, rng);
        AddCorridor(leftCenter, rightCenter, corridors, rng);
    }

    // Helper function to get the center of a room in a subtree.
    private static (int x, int y) GetSubtreeRoomCenter(Node node, Random rng)
    {
        if (node == null)
            return (0, 0);

        if (node.HasRoom)
        {
            return (node.RoomX + node.RoomWidth / 2, node.RoomY + node.RoomHeight / 2);
        }

        // Collect room centers from leaves in this subtree and pick one at random
        var centers = new List<(int x, int y)>();
        CollectLeafRoomCenters(node, centers);
        if (centers.Count > 0)
            return centers[rng.Next(centers.Count)];

        // Fallback: region center if no rooms found (shouldn't happen with current PlaceRooms)
        int cx = node.X + node.Width / 2;
        int cy = node.Y + node.Height / 2;
        return (cx, cy);
    }

    private static void CollectLeafRoomCenters(Node node, List<(int x, int y)> centers)
    {
        if (node == null)
            return;

        if (node.IsLeaf)
        {
            if (node.HasRoom)
            {
                int cx = node.RoomX + node.RoomWidth / 2;
                int cy = node.RoomY + node.RoomHeight / 2;
                centers.Add((cx, cy));
            }
            return;
        }

        CollectLeafRoomCenters(node.Left, centers);
        CollectLeafRoomCenters(node.Right, centers);
    }

    private static void AddCorridor((int x, int y) from, (int x, int y) to, List<RectInt> corridors, Random rng)
    {
        int x1 = from.x;
        int y1 = from.y;
        int x2 = to.x;
        int y2 = to.y;

        bool horizontalFirst = rng.Next(2) == 0;

        if (horizontalFirst)
        {
            int xMin = Math.Min(x1, x2);
            int xMax = Math.Max(x1, x2);
            int lengthH = xMax - xMin + 1;
            corridors.Add(new RectInt(xMin, y1, lengthH, 1));

            int yMin = Math.Min(y1, y2);
            int yMax = Math.Max(y1, y2);
            int lengthV = yMax - yMin + 1;
            corridors.Add(new RectInt(x2, yMin, 1, lengthV));
        }
        else
        {
            int yMin = Math.Min(y1, y2);
            int yMax = Math.Max(y1, y2);
            int lengthV = yMax - yMin + 1;
            corridors.Add(new RectInt(x1, yMin, 1, lengthV));

            int xMin = Math.Min(x1, x2);
            int xMax = Math.Max(x1, x2);
            int lengthH = xMax - xMin + 1;
            corridors.Add(new RectInt(xMin, y2, lengthH, 1));
        }
    }
}