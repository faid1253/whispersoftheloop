# Whispers of the Loop - Unity Prototype

A 3D puzzle-platformer game featuring spirit form shifting, time loops, and atmospheric exploration.

## Game Overview

**Title:** Whispers of the Loop  
**Genre:** 3D Puzzle-Platformer / Atmospheric Exploration  
**Perspective:** Third-person, free camera  
**Core Inspiration:** Journey, Solar Ash, The Pathless, The Witness, RIME, GRIS

### High Concept
You are a glowing spirit drifting through a surreal dreamworld, piecing together fragments of a forgotten identity. Explore distorted, gravity-defying landscapes, solve environmental puzzles, and navigate time loops that shift the world each reset.

## Core Gameplay Mechanics

### 1. Spirit Form Shifting
- **Light Form:** Interact with solar platforms, power light bridges, reveal hidden glyphs
- **Shadow Form:** Phase through walls, walk on shadow bridges, bypass obstacles
- **Key:** Press `Q` to switch between forms
- Puzzles require mid-action switching between forms

### 2. Time Loop System
- Default loop length: 5 minutes
- Environment subtly changes if timer expires (altered geometry, new paths)
- Checkpoints temporarily pause timer
- Collecting memory fragments grants time bonuses

### 3. Memory Fragment Collection
- Fragments placed in high-challenge spots
- Each collected fragment visually "repairs" parts of the hub world
- Grants significant time bonus when collected
- Required to unlock portals and progress

## Controls

### Movement
- **WASD:** Move
- **Space:** Jump
- **Mouse:** Camera control

### Spirit Form
- **Q:** Toggle between Light and Shadow forms

### Puzzle Interaction
- **Arrow Keys:** Rotate mirrors when in range
- **R:** Reset mirror rotation (when debugging)

### Debug Controls
- **R:** Force loop reset (debug mode only)

## Project Structure

```
WhispersOfTheLoop/
├── Assets/
│   ├── Scripts/
│   │   ├── Player/
│   │   │   ├── PlayerController.cs
│   │   │   └── FormShift.cs
│   │   ├── Gameplay/
│   │   │   ├── LoopManager.cs
│   │   │   ├── MemoryFragment.cs
│   │   │   ├── Checkpoint.cs
│   │   │   ├── Portal.cs
│   │   │   ├── WorldShuffler.cs
│   │   │   └── FragmentCounter.cs
│   │   ├── Puzzles/
│   │   │   ├── LightSource.cs
│   │   │   ├── Mirror.cs
│   │   │   └── LightReceiver.cs
│   │   └── UI/
│   │       └── LoopTimerUI.cs
│   ├── Scenes/
│   │   └── HubAndTestLoop.unity
│   ├── Materials/
│   │   ├── PlayerLightForm.mat
│   │   ├── PlayerShadowForm.mat
│   │   ├── LightBeam.mat
│   │   └── MemoryFragment.mat
│   ├── Prefabs/
│   ├── Art/
│   └── UI/
└── ProjectSettings/
```

## Setup Instructions

### 1. Unity Requirements
- Unity 2021.3 LTS or newer
- Universal Render Pipeline (URP) recommended
- Cinemachine package (for camera control)

### 2. Layer Setup
The project uses the following layers:
- **Player** (Layer 8): Player character
- **Ground** (Layer 9): Standard walkable surfaces
- **GhostWalkable** (Layer 10): Surfaces only accessible in Shadow form
- **Interactable** (Layer 11): Objects that can be interacted with
- **Mirror** (Layer 12): Reflective surfaces for light puzzles
- **LightBeam** (Layer 13): Light beam collision detection
- **PhaseWall** (Layer 14): Walls that can be phased through in Shadow form

### 3. Tags Setup
The following tags are used:
- **Player**: Player character
- **MemoryFragment**: Collectible memory fragments
- **Checkpoint**: Timer pause points
- **Portal**: Scene transition points
- **Mirror**: Rotatable mirrors for light puzzles
- **LightSource**: Light beam emitters
- **LightReceiver**: Light beam targets

## Scene Blockout Guide

### Hub World Setup
1. Create a floating island platform
2. Add Portal objects leading to loop areas
3. Place LoopManager with spawn point
4. Add FragmentCounter to track progress

### Test Loop Area Setup
1. Create platforms and walkways
2. Add at least one MemoryFragment
3. Place Checkpoint for timer management
4. Set up light puzzle (LightSource → Mirror → LightReceiver)
5. Configure WorldShuffler for loop variations

### Player Setup
1. Create Capsule with CharacterController
2. Add PlayerController and FormShift scripts
3. Set up materials for Light/Shadow forms
4. Configure camera (Cinemachine FreeLook recommended)

### Light Puzzle Setup
1. **LightSource:** Emits light beam, requires Light form to activate
2. **Mirror:** Rotatable reflector, use arrow keys when in range
3. **LightReceiver:** Target that activates connected objects

## Script Documentation

### Core Scripts

#### PlayerController.cs
Handles character movement, jumping, and physics.
- Smooth movement with camera-relative direction
- Gravity and jump mechanics
- Ground detection

#### FormShift.cs
Manages Light/Shadow form switching.
- Visual changes (materials, particles)
- Physics layer interactions
- Audio feedback

#### LoopManager.cs
Controls the time loop system.
- Timer countdown and reset
- Player respawn on loop reset
- Progress tracking and time bonuses

### Gameplay Scripts

#### MemoryFragment.cs
Collectible items that grant time bonuses.
- Floating animation
- Collection effects
- Progress reporting

#### Checkpoint.cs
Temporary timer pause points.
- Proximity detection
- Timer pause duration
- Visual feedback

#### Portal.cs
Scene transition or teleportation.
- Activation requirements (fragment count)
- Visual effects
- Scene loading or local teleport

#### WorldShuffler.cs
Handles environment changes on loop reset.
- Multiple variant types (position, rotation, scale, materials)
- Randomization options
- Smooth transitions

### Puzzle Scripts

#### LightSource.cs
Emits light beams for puzzles.
- Raycast-based beam calculation
- Form requirement checking
- Reflection handling

#### Mirror.cs
Rotatable reflective surfaces.
- Player proximity detection
- Input handling for rotation
- Rotation limits and reset

#### LightReceiver.cs
Targets for light beams.
- Activation/deactivation logic
- Connected object control
- Chain reactions

### UI Scripts

#### LoopTimerUI.cs
Displays the loop timer.
- Time formatting
- Color changes based on remaining time
- Visual effects (pulsing, flashing)

#### FragmentCounter.cs
Tracks collected memory fragments.
- Progress persistence
- Event system for UI updates
- Save/load functionality

## Art Direction

### Visual Style
- Soft watercolor gradients across sky & terrain
- Neon light trails & particles subtly guide player
- Character: Shifting transparency, silhouette changes in spirit form
- Portals ripple like disturbed water
- Memory fragments: Suspended crystalline glass with soft glow

### Color Palette
- **Light Form:** Warm whites and golds
- **Shadow Form:** Cool blues and purples
- **Environment:** Soft pastels with ethereal lighting
- **UI:** Minimal, translucent elements

## Development Tips

### Performance Optimization
- Use object pooling for particles and effects
- Implement LOD system for distant objects
- Optimize light beam calculations
- Use efficient collision detection

### Debugging
- Enable debug info in LoopManager for testing
- Use Gizmos for visualizing interaction ranges
- Console logs provide detailed feedback
- Manual loop reset with R key

### Extending the Game
- Add more puzzle types (pressure plates, moving platforms)
- Implement multiple loop areas with different themes
- Create more complex form-switching mechanics
- Add narrative elements and memory reconstruction

## Known Issues & Limitations

1. Light beam reflections limited to 5 bounces
2. Form switching requires manual material assignment
3. Save system uses PlayerPrefs (local only)
4. No audio implementation in base scripts
5. Particle effects require manual setup

## Future Enhancements

- Visual scripting integration
- Advanced particle systems
- Dynamic music system
- Narrative cutscenes
- Multiple difficulty levels
- Achievement system

## Credits

**Game Design:** Based on "Whispers of the Loop" concept  
**Programming:** Unity C# scripts  
**Inspiration:** Journey, Solar Ash, The Pathless, The Witness, RIME, GRIS

---

*This prototype provides a solid foundation for developing the full Whispers of the Loop experience. All core mechanics are implemented and ready for expansion.*

