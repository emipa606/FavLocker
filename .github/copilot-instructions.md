# GitHub Copilot Instructions for Fav Locker (Continued)

Welcome to the Fav Locker (Continued) mod project! This mod is an update to Ekais' original mod and provides additional support for power armor stations in other mods, including Vanilla Armor Expanded, CAS: Power Armor (Continued), CAS: Recon Armor (Continued), CAS: Cataphract Armor (Continued), Ultratech: Altered Carbon Remastered, and Save Our Ship 2. The mod also includes a hit points filter for lockers and the ability to repair armor using power and steel at power armor stations.

## Mod Overview and Purpose

Fav Locker (Continued) allows players to create containers where they can register their favorite apparels. The recommended way to use it is to register combat apparels in your favorites, allowing colonists to easily change between normal and combat apparels. This mod aims to enhance the gameplay experience by providing a more efficient and convenient way for colonists to manage their apparels.

## Key Features and Systems

### Container System
- **Fav Locker**: A container where you can register any apparels. Colonists can change into their favorite apparels with one click.
- **Power Armor Station**: A station that allows colonists to register power armor and change into it quickly.

### Armor Management
- **Hit Points Filter**: Adds a hit points filter to the lockers to allow players to manage the condition of their apparels.
- **Armor Repair**: Power armor stations can repair armor using power and steel when the Advanced Fabrication research is unlocked.

## Coding Patterns and Conventions

- **Code Organization**: The mod is organized into several classes, each responsible for specific functionality. For example, `Building_Locker` handles the logic for the Fav Locker, while `Building_PowerArmorStation` handles the logic for the Power Armor Station.
- **Naming Conventions**: Class names and method names are descriptive and follow the naming conventions used in the RimWorld codebase.
- **Modding Best Practices**: The mod adheres to best practices for RimWorld modding, including proper XML integration and Harmony patching.

## XML Integration

The mod uses XML files to define various game entities such as job definitions, locker section definitions, thing definitions, and work giver definitions. These files are located in the `Defs` directory and are essential for the mod's functionality. The following XML files are included in the mod:

- `Jobs_Misc.xml`
- `LockerSections.xml`
- `Buildings_Locker.xml`
- `Buildings_Misc.xml`
- `WorkGivers.xml`

## Harmony Patching

The mod uses Harmony patching to modify the behavior of vanilla RimWorld methods and add new functionality. The following Harmony patches are included in the mod:

- Patches to the `Building_PowerArmorStation` class to add the ability to repair armor using power and steel.
- Patches to the `Building_Locker` class to add the hit points filter to the lockers.

## Suggestions for Copilot

1. **Understanding the Codebase**: Familiarize yourself with the existing codebase to understand the functionality of each class and method.
2. **XML Integration**: Ensure that any new XML definitions are correctly integrated into the mod's XML files.
3. **Harmony Patching**: When adding new functionality, consider using Harmony patching to modify vanilla RimWorld methods.
4. **Testing and Validation**: Always test any new features or modifications thoroughly to ensure that they work as expected and do not cause any issues.
5. **Documentation**: Keep the documentation up to date with any new changes or additions to the mod.

By following these guidelines, you can effectively contribute to the Fav Locker (Continued) mod project and help enhance the gameplay experience for players.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

