# Whispers of the Loop - Setup Guide

This guide will help you set up and start working with the Whispers of the Loop Unity prototype.

## Quick Start

### 1. Extract the Project
1. Download and extract `WhispersOfTheLoop_UnityPrototype.zip`
2. Place the extracted folder in your Unity projects directory

### 2. Open in Unity
1. Open Unity Hub
2. Click "Open" and select the `WhispersOfTheLoop` folder
3. Unity will import the project (this may take a few minutes)

### 3. Scene Setup
1. Open the scene: `Assets/Scenes/HubAndTestLoop.unity`
2. The scene contains a basic camera and lighting setup

## Building Your First Prototype

### Step 1: Create the Player
1. **Create Player GameObject:**
   - Right-click in Hierarchy → 3D Object → Capsule
   - Name it "Player"
   - Set Tag to "Player"
   - Set Layer to "Player" (Layer 8)

2. **Add Components:**
   - Add `Character Controller` component
   - Add `PlayerController` script
   - Add `FormShift` script

3. **Setup Materials:**
   - Create a child object with a Renderer
   - Assign `PlayerLightForm` material
   - In FormShift script, assign this renderer to `formRenderers` array

### Step 2: Create the Hub World
1. **Create Hub Platform:**
   - Right-click in Hierarchy → 3D Object → Cube
   - Scale to create a floating island (e.g., 20, 1, 20)
   - Set Layer to "Ground" (Layer 9)

2. **Add Loop Manager:**
   - Create empty GameObject named "LoopManager"
   - Add `LoopManager` script
   - Create spawn point (empty GameObject) and assign to `loopSpawnPoint`

3. **Add Fragment Counter:**
   - Create empty GameObject named "FragmentCounter"
   - Add `FragmentCounter` script
   - Set `totalFragmentsInLevel` to desired number

### Step 3: Create Memory Fragments
1. **Create Fragment:**
   - Right-click in Hierarchy → 3D Object → Sphere
   - Scale down (e.g., 0.5, 0.5, 0.5)
   - Set Tag to "MemoryFragment"
   - Set Layer to "Interactable" (Layer 11)

2. **Setup Fragment:**
   - Add `MemoryFragment` script
   - Assign unique `fragmentID`
   - Add `Sphere Collider` and set `isTrigger = true`
   - Assign `MemoryFragment` material

3. **Add Effects:**
   - Add `Light` component for glow
   - Add `Particle System` for collection effect

### Step 4: Create Checkpoints
1. **Create Checkpoint:**
   - Right-click in Hierarchy → 3D Object → Cylinder
   - Set Tag to "Checkpoint"
   - Set Layer to "Interactable" (Layer 11)

2. **Setup Checkpoint:**
   - Add `Checkpoint` script
   - Add `Collider` and set `isTrigger = true`
   - Assign LoopManager reference

### Step 5: Create Light Puzzle
1. **Create Light Source:**
   - Create empty GameObject named "LightSource"
   - Add `LightSource` script
   - Add `LineRenderer` component
   - Assign `LightBeam` material to LineRenderer

2. **Create Mirror:**
   - Right-click in Hierarchy → 3D Object → Cube
   - Scale to make it thin (e.g., 2, 2, 0.1)
   - Set Tag to "Mirror"
   - Set Layer to "Mirror" (Layer 12)
   - Add `Mirror` script

3. **Create Light Receiver:**
   - Right-click in Hierarchy → 3D Object → Cube
   - Set Tag to "LightReceiver"
   - Add `LightReceiver` script
   - Create objects to activate/deactivate

### Step 6: Setup Camera
1. **Install Cinemachine:**
   - Window → Package Manager
   - Search for "Cinemachine"
   - Install the package

2. **Create FreeLook Camera:**
   - Cinemachine → Create FreeLook Camera
   - Set Follow and Look At to Player
   - Adjust camera settings as needed

### Step 7: Create UI
1. **Create Canvas:**
   - Right-click in Hierarchy → UI → Canvas
   - Set Render Mode to "Screen Space - Overlay"

2. **Add Timer UI:**
   - Right-click on Canvas → UI → Text (TextMeshPro)
   - Position in top-right corner
   - Add `LoopTimerUI` script
   - Assign LoopManager reference

## Layer Configuration

Make sure these layers are set up correctly:

| Layer | Name | Purpose |
|-------|------|---------|
| 8 | Player | Player character |
| 9 | Ground | Standard walkable surfaces |
| 10 | GhostWalkable | Shadow form only surfaces |
| 11 | Interactable | Fragments, checkpoints, etc. |
| 12 | Mirror | Reflective surfaces |
| 13 | LightBeam | Light beam collision |
| 14 | PhaseWall | Walls that can be phased through |

## Physics Settings

The FormShift script automatically handles layer collisions:
- In Light form: Player cannot walk on GhostWalkable surfaces
- In Shadow form: Player can walk on GhostWalkable surfaces

## Testing Your Prototype

### Basic Movement Test
1. Press Play
2. Use WASD to move
3. Use Space to jump
4. Press Q to switch forms (should see color change)

### Loop System Test
1. Wait for timer to reach zero
2. Player should respawn at spawn point
3. World should shuffle (if WorldShuffler is set up)

### Fragment Collection Test
1. Walk into a memory fragment
2. Fragment should disappear
3. Timer should increase
4. Fragment counter should update

### Light Puzzle Test
1. Switch to Light form (Q)
2. Approach light source (should activate)
3. Use arrow keys near mirror to rotate
4. Align beam to hit receiver
5. Connected objects should activate

## Common Issues & Solutions

### Player Falls Through Ground
- Check that ground objects have Collider components
- Ensure ground is on "Ground" layer
- Verify CharacterController is properly configured

### Form Switching Not Working
- Check that materials are assigned in FormShift script
- Verify layer collision settings in Physics settings
- Ensure Player is on correct layer

### Light Beams Not Appearing
- Check that LineRenderer has LightBeam material
- Verify LightSource script has proper layer masks
- Ensure mirrors are on "Mirror" layer

### Timer Not Updating
- Check that LoopTimerUI has LoopManager reference
- Verify UI Text component is assigned
- Check that LoopManager is active in scene

### Fragments Not Collecting
- Ensure fragments have Collider with isTrigger = true
- Check that fragment has MemoryFragment script
- Verify Player tag is set correctly

## Advanced Setup

### Adding World Shuffling
1. Create objects that should change on loop reset
2. Add `WorldShuffler` script to empty GameObject
3. Configure variants with different positions/rotations
4. Connect to LoopManager's OnLoopReset event

### Creating Ghost Walkable Paths
1. Create platforms for shadow form only
2. Set Layer to "GhostWalkable" (Layer 10)
3. Player can only walk on these in Shadow form

### Setting Up Portals
1. Create portal object (ring or archway)
2. Add `Portal` script
3. Set activation requirements (fragment count)
4. Configure target scene or position

## Performance Tips

1. **Use Object Pooling:** For particles and effects
2. **Optimize Light Beams:** Limit reflection count
3. **LOD System:** For distant objects
4. **Efficient Colliders:** Use simple shapes when possible
5. **Texture Compression:** Optimize materials

## Next Steps

Once you have the basic prototype working:

1. **Add More Puzzles:** Pressure plates, moving platforms
2. **Create Multiple Areas:** Different themed loop zones
3. **Implement Narrative:** Memory reconstruction system
4. **Polish Visuals:** Particle effects, post-processing
5. **Add Audio:** Sound effects and ambient music

## Troubleshooting

If you encounter issues:

1. Check the Console for error messages
2. Verify all script references are assigned
3. Ensure layers and tags are set correctly
4. Test individual components in isolation
5. Refer to the README.md for detailed documentation

## Support

For additional help:
- Check Unity documentation for component details
- Review script comments for implementation notes
- Use Unity's built-in debugging tools
- Test incrementally as you build

---

*Happy prototyping! This foundation provides everything needed to create the full Whispers of the Loop experience.*

