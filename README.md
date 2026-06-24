# Unity Code Atlas
<img width="3837" height="2045" alt="Screenshot 2026-06-23 124658" src="https://github.com/user-attachments/assets/d3244609-928c-4cb8-8d11-dbcfdcfef725" />

Unity Code Atlas is a Unity Editor tool for visual exploration of scene architecture and source-backed execution flow.

It is designed for developers joining an unfamiliar Unity project and needing a fast way to answer:

- Which scripts are active in this scene?
- How are those scripts connected?
- Which methods create links between scripts?
- How does execution move from Unity lifecycle methods into other methods?

## Features

- "Scan Scene" workflow focused on the currently open Unity scene
- "Code Map" for visual script relationship exploration
- method-level evidence dots on script links
- "Execution Flow " for static lifecycle-based flowchart generation
- zoom, pan, selection, and source navigation
- UI Toolkit based editor interface

## How It Works

The tool scans scene-relevant scripts, parses classes, fields, and methods, builds dependency relationships, and renders those results in two visual modules:

1. Code Map
2. Execution Flow

The execution-flow view is a static analyzer. It does not claim exact runtime tracing. Instead, it builds a source-backed call graph from Unity lifecycle methods and discovered method calls, then presents the result as a readable flowchart with evidence lines.

## Open In Unity

Open from:

Tools > Unity Code Atlas > Open

## Main Workflow

1. Open a Unity scene.
2. Launch Unity Code Atlas.
3. Click Scan Scene.
4. Select a scene script.
5. Explore relationships in Code Map.
6. Switch to Execution Flow and click Analyze Flow.
7. Click nodes or evidence to open the source.

## Technical Notes

- Built as an Editor-only package under - Assets/Code Atlas/Editor
- Uses UI Toolkit for the current dashboard
- Uses a lightweight source scanner and graph builders
- Supports method-level evidence on script connections

## Use Case

This project is aimed at Unity engineering workflows where architecture discoverability matters more than dashboards or scoring. It is especially useful for onboarding, debugging scene wiring, and understanding how MonoBehaviour systems interact in large projects.
