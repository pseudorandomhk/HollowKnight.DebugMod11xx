# DebugMod for Hollow Knight 11xx

Ask questions and request features in `#glitched-discussion` in the Hollow Knight speedrunning discord

### Installation
1. Install the appropriate modding API for 11xx
2. Place `DebugMod11xx.dll` in `<HK 11xx>/hollow_knight_Data/Managed/Mods` (create the folder if it doesn't exist already)
3. Play the game

### Functionality/keybindings
Change keybindings by editing `<Hollow Knight saves folder>/DebugMod11xx.GlobalSettings.json`. See "Mapping virtual axes to controls" in [this article](https://docs.unity3d.com/Manual/class-InputManager.html) for key naming conventions
- `toggleInfo` - toggles game info display in top-left corner
- `toggleInfiniteSoul` - toggles whether soul is constantly refilled to full
- `toggleInfiniteHealth` - toggles whether taking damage actually depletes health
- `toggleInvincibility` - toggles whether the knight can be damaged
- `toggleVignette` - toggles the vignette around the screen
- `toggleNoclip` - toggles whether the knight can be manually re-positioned, ignoring walls and other collision
- `cameraFollow` - toggles whether the camera follows the knight
- `cycleHitboxes` - cycles between different hitbox visibility states (none, important, all)
- `(increase/decrease)LoadExtension` - alters the load extender built into DebugMod
- `zoom(In/Out)`, `resetZoom` - alters the zoom of the game camera
- `(increase/decrease/reset)Timescale` - alters the game's timescale
- `saveSavestate` - saves the current game state to a file in `<Hollow Knight saves folder>/Savestates11xx`
- `loadSavestate` - loads a savestate created by `saveSavestate`. If used while not paused, loads the last saved or loaded savestate. If used while paused, shows a menu to search and select from all savestates in `<Hollow Knight saves folder/Savestates11xx` by file name. Use up/down arrow to move selector, enter to select a savestate to load, and escape to cancel
- `loadSavestateDuped` - loads a savestate with potentially multiple rooms
- `toggleCollision` - toggles "vanilla noclip", where the knight doesn't interact with collision and isn't affected by gravity
- `toggleDreamgateInvuln` - toggles dreamgate invulnerability, which also prevents geo from being collected
  
  **NOTE**: while this is active, `toggleInvincibility` has no effect

---

### Build
This section is for people who want to edit or contribute to the source code for DebugMod11xx
1. Fork/clone this repo
2. Create a `LocalBuildProperties.props` file in `DebugMod11xx`. See comments in `DebugMod11xx/DebugMod11xx.csproj` for content and format
3. Build the solution using an IDE or `dotnet build`. The resulting mod (`DebugMod11xx.dll`) should be copied directly the `Mods` directory in your 11xx HK install