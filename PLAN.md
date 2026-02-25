## Random Walk Dungeon Demo – Implementation Plan

This plan walks through implementing the Random Walk dungeon generator and renderer in this project, step by step. It assumes the existing project already has working demos for Cellular Automata, Perlin Noise, and Wave Function Collapse, and will reuse those patterns wherever possible.

---

## 1. High-Level Goals

- **Algorithm / Data**: Implement a reusable Random Walk dungeon generator that produces a clean data representation (grid / rooms & hallways) with parameters:
  - `minSteps`
  - `maxSteps`
  - `branchChance`
  - `allowLoops`
- **Rendering**: Implement a renderer that takes the algorithm output and draws it:
  - First pass: simple drawing (e.g. `Node2D._Draw()` like other demos).
  - Bonus: support TileMap-based rendering for rooms/hallways.
- **Scene & UI**: Create a dedicated Random Walk demo scene with:
  - Parameter controls (sliders, checkboxes, etc.).
  - A `Generate` / `Regenerate` button.
  - A `Back`/`Main Menu` button.
  - Camera controls consistent with other demos.

---

## 2. Understand Existing Patterns in the Project

1. **Review an existing “algorithm + renderer + controller + UI” demo** (Wave Collapse is a good model):
  - `WaveCollapseGenerator.cs`
  - `WaveTileMapRenderer.cs`
  - `WaveCollapseController.cs`
  - `WaveParameters.cs`
2. **Note the separation of concerns**:
  - Generator: pure data, no drawing.
  - Renderer: knows how to visualize a given data structure.
  - Controller: wires UI ↔ generator ↔ renderer and handles scene lifecycle.
  - UI: exposes serialized fields, raises signals / calls on controller.
3. **Copy/adapt the pattern** for Random Walk, keeping naming consistent:
  - `RandomWalkGenerator.cs`
  - `RandomWalkRenderer.cs` (or a TileMap-based version if going for bonus).
  - `RandomWalkController.cs`
  - `RandomWalkParameters.cs`

*(Most of these already exist in your project; the plan below assumes we are filling in `RandomWalkGenerator` and `RandomWalkRenderer`, and wiring everything together.)*

---

## 3. Data Structures for Random Walk

Decide how the dungeon will be represented, in a way that is easy to render and test.

1. **Grid-based representation (recommended, matches other demos)**:
  - Use a 2D array or dictionary keyed by integer coordinates.
  - For example:
    - `bool[,] walkable` or an enum like `CellType.Empty`, `CellType.Room`, `CellType.Hallway`.
  - Track bounds (`width`, `height`) and/or dynamic min/max x/y if using a dictionary.
2. **Optional graph view** (if useful for debug or UI):
  - Room node structure:
    - `Vector2I position`
    - `List<Room> neighbors`
  - Edge/hallway structure:
    - Just implied by neighbors list, or explicit `Hallway` objects.
3. **Pick a primary structure**:
  - **Primary**: a simple grid with cell types (easiest to render and reason about).
  - **Secondary**: optional adjacency list if needed for more advanced uses.

**Action**: Define a `RandomWalkResult` data class/struct in `RandomWalkGenerator.cs`:

- `CellType[,] cells` (or similar).
- `int width`, `int height`.
- `Vector2I startPosition`.
- Any other metadata you want (e.g. list of room positions).

---

## 4. RandomWalkGenerator – Class Design

1. **Create the generator class** in `scripts/Algorithms/RandomWalkGenerator.cs`:
  - Namespace consistent with the rest of the project.
  - Public API something like:
    - `public class RandomWalkGenerator`
      - `public RandomWalkGenerator(int width, int height, RandomWalkConfig config)`
      - `public RandomWalkResult Generate()`
2. **Define a configuration struct/class**:
  - `int MinSteps`
  - `int MaxSteps`
  - `float BranchChance`
  - `bool AllowLoops`
  - Optional:
    - `int Seed` or `RandomNumberGenerator rng` for determinism.
    - `Vector2I StartPosition` (or computed as center).
3. **Responsibility of `Generate()`**:
  - Initialize grid/graph data.
  - Create or pick the starting room.
  - Invoke the recursive random walk.
  - Return a `RandomWalkResult` that is **read-only** from the outside.

---

## 5. Implementing the Recursive Random Walk

Implement the algorithm from the pseudo code, adapted to your grid.

1. **Choose coordinate system**:
  - Use `x = column`, `y = row`.
  - Directions: up/down/left/right as `Vector2I[] directions`.
  - Ensure consistency with renderer (no x/y flip).
2. **Maintain per-cell state**:
  - `CellType[,] cells` or `bool[,] hasRoom`.
  - Optionally `bool[,] isHallway` or treat hallway cells same as rooms.
  - A helper to check if a cell is inside bounds.
3. **Valid moves**:
  - Build a list of “valid directions” from `currentRoom`:
    - Compute `nextPos = currentPos + dir`.
    - If out of bounds → invalid.
    - If `AllowLoops == false`:
      - Valid only if target cell is empty (no room yet).
    - If `AllowLoops == true`:
      - Valid if either:
        - Target cell is empty (new room), or
        - Target cell already contains a room and there is not already a hallway in that direction (optional check to avoid duplicate edges).
  - If no valid directions and `stepCount < MinSteps`:
    - This is the case where backtracking might be needed. For this project you **do not need to implement full backtracking**, so:
      - Accept that the branch ends early in this implementation, or
      - Optionally, log/visualize that the branch “failed.”
4. **Core recursive method signature**:
  - `private void RandomWalk(Vector2I currentPos, int stepCount)`
5. **Base case logic** (following the pseudo code):
  - If `stepCount >= MinSteps` **and** (`stepCount >= MaxSteps` **or** no valid step from `currentPos`):
    - `return;`
6. **Step logic**:
  - Get list of valid directions from `currentPos`.
  - If list is empty:
    - `return;`
  - Choose a random direction from the list.
  - Compute `nextPos`.
  - Determine if `nextPos` is new or existing:
    - If cell is empty → mark as new room and create hallway from `currentPos` to `nextPos` (update grid accordingly).
    - If `AllowLoops` and cell is already a room → optionally mark the edge/hallway for rendering (e.g. mark corridor cells or store graph edge).
  - Recurse:
    - `RandomWalk(nextPos, stepCount + 1);`
7. **Branching**:
  - After returning from the recursion above:
    - Roll a random float in `[0, 1)`.
    - If `< BranchChance`:
      - Start a new branch from **current position** (not `nextPos`):
        - `RandomWalk(currentPos, stepCount);`
  - Ensure you have a recursion guard so `MaxSteps` and grid bounds prevent infinite recursion.

---

## 6. Mapping Grid / Graph to Renderable Data

Decide how the renderer will interpret the `RandomWalkResult`.

1. **Grid interpretation**:
  - Each cell is drawn as:
    - Empty: background color (or nothing).
    - Room: floor tile / filled square.
    - Hallway: thinner or same tile as room, optionally different color.
2. **TileMap approach (for bonus)**:
  - Define a `TileSet` with:
    - Floor tile(s).
    - Wall/empty or leave empty cells blank.
  - Map cell types to tile IDs.
  - Optionally use multiple layers:
    - Layer 0: floors.
    - Layer 1: decorations or walls.
3. **Spatial scaling**:
  - Decide `worldUnitsPerCell` (e.g. 1 unit per cell).
  - Position drawing or tiles so that `(0,0)` in grid is at a known world origin (e.g. `(0,0)` or centered).

---

## 7. RandomWalkRenderer – Class Design

1. **Create renderer class** in `scripts/Renderers/RandomWalkRenderer.cs`:
  - Match base type used in other renderers:
    - If they subclass `Node2D` and override `_Draw()`, do the same.
    - If they use a `TileMap`, do the same.
2. **Public API**:
  - A method like:
    - `public void Render(RandomWalkResult result)`
  - Store the result in a private field and:
    - For `Node2D` drawing:
      - Call `QueueRedraw()`.
    - For `TileMap`:
      - Clear existing tiles.
      - Loop over grid and set tiles accordingly.
3. **Implement `_Draw()` (if using custom drawing)**:
  - For each cell in `result.cells`:
    - Compute world position.
    - Choose color based on cell type.
    - Draw rectangle/circle/whatever.
  - Optionally draw:
    - Start room in a special color.
    - Outlines for rooms or hallways.
4. **Implement TileMap version (for bonus)**:
  - In `Render(result)`:
    - Call `Clear()` on the `TileMapLayer`.
    - For each grid cell:
      - `SetCell(x, y, tileId)` depending on type.

---

## 8. Wiring Generator + Renderer via RandomWalkController

1. **Open `RandomWalkController.cs`** and inspect existing patterns from other controllers:
  - How they receive parameters from UI.
  - How they instantiate or reference renderer/generator.
  - How they handle the `Generate` button.
2. **Controller responsibilities**:
  - Hold references to:
    - `RandomWalkParameters` (UI script).
    - `RandomWalkRenderer`.
  - Create/hold a `RandomWalkGenerator` instance when needed.
  - On `Generate`/`Regenerate`:
    - Read parameters from the UI object.
    - Create a `RandomWalkConfig` struct.
    - Instantiate `RandomWalkGenerator` with size + config.
    - Call `Generate()` to get `RandomWalkResult`.
    - Pass result into `RandomWalkRenderer.Render(result)`.
3. **Scene lifecycle**:
  - On `_Ready()`:
    - Cache node references (renderer, parameters, back button, etc.).
  - Optional:
    - Auto-generate once at startup using default parameters.

---

## 9. RandomWalkParameters – UI Hook-Up

1. **Open `RandomWalkParameters.cs`** to see what already exists.
2. **Expose parameters** using exported fields / Godot inspector:
  - Sliders or SpinBoxes for:
    - `MinSteps`
    - `MaxSteps`
    - `BranchChance`
  - Checkbox for:
    - `AllowLoops`
3. **Public API for controller**:
  - Provide getters or properties:
    - `public int MinSteps => ...`
    - `public int MaxSteps => ...`
    - `public float BranchChance => ...`
    - `public bool AllowLoops => ...`
4. **Generate/Regenerate button**:
  - Connect button pressed signal to a method on the controller.
  - That method reads parameters and triggers generation (Section 8).
5. **Main menu / Back button**:
  - Add a button to return to the main menu scene.
  - Wire it the same way other demos do:
    - Call into a scene manager or use `GetTree().ChangeSceneToFile(...)`.

---

## 10. Random Walk Demo Scene Setup

1. **Create the Random Walk demo scene** if not already present:
  - Root node type consistent with other demo scenes (e.g. `Node2D` or `Control` with a `Node2D` child).
2. **Add nodes**:
  - `RandomWalkController` script on a suitable root node.
  - `RandomWalkRenderer` as a child node.
  - UI scene (with `RandomWalkParameters` script) either:
    - Instanced as a child of a GUI CanvasLayer, or
    - Kept in its own scene and added via a loader script (follow existing pattern).
3. **Camera controls**:
  - Add or reuse existing camera controller used in other demos.
  - Ensure click-drag pan and scroll zoom behave like the other scenes.
4. **Connect scene from main menu**:
  - Add a Random Walk button to the main menu (if not already there).
  - On click, change scene to the Random Walk demo scene.

---

## 11. Visual & UX Polish

1. **Parameter ranges**:
  - Choose reasonable min/max for controls (e.g. `MinSteps` 5–200, `MaxSteps` 10–500).
  - Ensure `MaxSteps >= MinSteps` in code:
    - Clamp, or swap values if user input is inconsistent.
2. **Colors / styling**:
  - Distinguish:
    - Start room.
    - Normal rooms.
    - Hallways.
    - Optional loops (e.g. draw them slightly different).
3. **Feedback on invalid setups**:
  - If generation fails or produces an empty/too small dungeon:
    - Optionally show a small label: “Try increasing Max Steps or allowing loops”.
4. **Regenerate UX**:
  - Keep the previous parameters when pressing `Regenerate`.
  - Optionally support a `Seed` field so that the same settings + seed give reproducible results.

---

## 12. Testing and Validation

1. **Unit-ish tests (manual or lightweight)**:
  - Run generator multiple times with:
    - `AllowLoops = false`, check that no cycles appear (visually the graph should be tree-like).
    - `AllowLoops = true`, ensure re-visits are possible.
  - Validate that:
    - All rooms are reachable from the start.
    - No coordinates fall outside the grid.
2. **Parameter edge cases**:
  - `MinSteps = 0`, `MaxSteps` small.
  - Very high `BranchChance` (lots of branches).
  - Very low `BranchChance` (long corridors).
3. **Scene behavior**:
  - All five demos are reachable from main menu.
  - Camera controls match other scenes.
  - Back button from Random Walk demo returns to main menu.

---

## 13. Optional: Backtracking Improvements

Backtracking is not required, but if you decide to add it later:

1. **Track the current path** of rooms in a stack.
2. **When you get stuck before `MinSteps`**:
  - Pop the last room from the stack.
  - Optionally remove the hallway and room from data structures.
  - Try alternative directions from the previous room.
3. **Recursive style**:
  - This can be integrated into the recursive calls:
    - On entering recursion, push the room.
    - On exiting, pop it and undo if needed.

---

## 14. Integration Checklist

Use this checklist when implementing:

- **Algorithm**
  - `RandomWalkConfig` with all parameters.
  - `RandomWalkResult` with clear grid representation.
  - Recursive `RandomWalk` implementation with min/max steps, branching, loops.
- **Rendering**
  - `RandomWalkRenderer` with `Render(RandomWalkResult)` method.
  - Visual distinction of rooms, hallways, start room.
  - (Bonus) TileMap-based renderer using TileMapLayer.
- **Scene & UI**
  - Random Walk demo scene exists and loads.
  - `RandomWalkController` wires UI → generator → renderer.
  - `RandomWalkParameters` exposes min/max steps, branch chance, allow loops, and calls generate.
  - Back/Main Menu button works.
  - Camera controls match other demos.
- **Project Integration**
  - Main menu has Random Walk option.
  - All five demos are reachable and returnable.

