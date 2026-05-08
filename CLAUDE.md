# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Puzzler** is a C# WPF desktop application for playing and solving logic puzzles. It supports four puzzle types: **Sudoku**, **Kakuru**, **Griddler**, and **Triddler** (a triangular-cell nonogram variant).

## Build & Run Commands

**Using the dev script (PowerShell, from repo root):**
```powershell
.\dev.ps1 build     # Build full solution
.\dev.ps1 run       # Launch WPF app
.\dev.ps1 test      # Run all xUnit tests
.\dev.ps1 watch     # Run with hot-reload
.\dev.ps1 clean     # Remove bin/ and obj/
```

**Direct dotnet commands:**
```powershell
dotnet build Source/Solvers.sln
dotnet run --project Source/Presentation.WPF/Presentation.WPF.csproj
dotnet test Source/Solvers.sln --logger "console;verbosity=normal"

# Run a single test class
dotnet test Source/Solvers.sln --filter "FullyQualifiedName~SolverSmokeTests"
```

## Architecture

The solution uses a strict 5-layer architecture. Each puzzle type (Sudoku, Kakuru, Griddler, Triddler) has its own implementation class in every layer, inheriting from a generic base.

```
Presentation.WPF        → WPF/XAML UI (MainWindow, PuzzlerCanvas, InputWindow)
PresentationLogic       → Rendering abstraction (decouples logic from WPF)
Logic                   → Solving algorithms (constraint propagation, backtracking)
Data                    → JSON deserialization, file I/O, web scraping (HtmlAgilityPack)
Common                  → Shared models: Board, Cell, Group, base classes + enums
```

**Key pattern:** Every layer has a `*Generic<TPuzzle, TBoard>` base class (e.g., `LogicLayerGeneric`, `PresentationLogicGeneric`) that puzzle-specific classes inherit from. The same Board/Cell/Group hierarchy is used in `Common/Models/`.

**Rendering:** `PresentationLogic` uses its own coordinate abstractions (`PuzzlerColor`, `PuzzlerFont`, `PuzzlerPoint`, `IDrawingSurface`) so game rendering logic is not tied to WPF types. Triddler cells are rendered as triangles with custom geometry.

## Project Structure

```
Source/
├── Common/Models/Base/      # Generic base classes (BoardGenericBase, CellBase, GroupBase, ...)
├── Common/Models/{Puzzle}/  # Puzzle-specific models
├── Data/                    # DataLayer per puzzle type
├── Logic/                   # Solver + Factory per puzzle type
├── PresentationLogic/       # Rendering logic per puzzle type
├── Presentation.WPF/        # WPF UI only; no game logic here
├── Tests/                   # xUnit tests + JSON test fixtures in TestData/
└── Tools/PuzzleXmlToJson/   # CLI utility for migrating puzzle data formats
```

**Python tooling:** `Tools/triddler_from_image.py` generates Triddler puzzle JSON from photos using image processing.

## Tests

- **SolverSmokeTests** — loads puzzles from `Tests/TestData/` JSON fixtures and verifies they solve within a timeout
- **SolverSudokuStrategyTests** — unit tests for individual solving strategies
- **SolverBenchmarkTests** — performance benchmarks

Test data covers easy/hard variants for each puzzle type, including large grids (16×16, 25×25 Sudoku).

## Key Config Files

- `Source/Solvers.sln` — solution entry point
- `Directory.Build.props` — global settings: `LangVersion=latest`, `Nullable=enable`
- `.vscode/tasks.json` / `launch.json` — VSCode build and debug tasks
- `Puzzler.code-workspace` — VSCode workspace targeting the solution

## Tech Stack

- **C# / .NET 10.0**, nullable enabled, latest language version
- **WPF** for UI
- **xUnit** for tests
- **Combinatorics** NuGet package (combinatorial solving helpers)
- **HtmlAgilityPack** (Data layer, web puzzle parsing)
