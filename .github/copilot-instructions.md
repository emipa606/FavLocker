# GitHub Copilot Instructions for RimWorld Modding Project

## Mod Overview and Purpose
This mod for RimWorld extends the game by introducing new functionalities involving apparel management using lockers and power armor stations. The mod's main purpose is to enhance the way players interact with and manage their pawns' apparel by providing streamlined interfaces and behaviors for favorite apparel storage and retrieval. It adds gameplay depth and convenience by allowing players to register and manage favorite pieces of apparel that pawns can automatically wear under specific conditions.

## Key Features and Systems
- **Locker and Power Armor Station Buildings**: Allows players to store and manage apparel items efficiently. These buildings come with commands to wear and remove favorite apparel from a designated inventory.
- **Job Drivers**: Custom job drivers such as `JobDriver_WearFavoriteOnPowerArmorStation` and `JobDriver_RemoveFavoriteOnPowerArmorStation` to automate apparel management tasks for pawns.
- **Gizmo Commands**: Custom commands for pawns to interact with lockers and power armor stations, including wearing and removing favorite apparel.
- **Comparison and Sorting**: Uses custom comparers for apparel, allowing sorting by body part group and layer, as well as by thing definition name.
- **Dialog and UI Integration**: Custom dialog for registering apparel, and UI elements like `LockerApparelWidget` for managing apparel in the interface.
- **Harmony Patching**: Extends and modifies game behavior using Harmony patches for integration with the base game's systems.

## Coding Patterns and Conventions
- **Interfaces and Comparables**: The code heavily uses interfaces like `IComparable` and `IThingHolder` to define comparable and container behaviors.
- **Encapsulation and Abstraction**: Use abstract classes like `Building_RegistableContainer` to encapsulate shared behaviors between derived classes.
- **Method Access Modifiers**: Private methods are extensively used to control class behavior and access, promoting encapsulation and reducing interference between classes.

## XML Integration
Though this specific structure did not include direct XML summaries, the mod likely integrates with RimWorld's XML configuration system for defining things like entity definitions (e.g., Apparel, Building) and other in-game elements. XML files are utilized within mod development for setting up def types, item recipes, traits, and other elements that do not require C# backing but define data-driven elements of the mod. Ensure XML definitions align with serialized member expectations in your C# code.

## Harmony Patching
Harmony is used to modify or extend game functionality:
- **Adding New Behaviors**: Introduce new methods or override existing ones to enhance functionality, as shown in `Patch_Pawn_GetGizmos`.
- **Reapplying Modifications**: To ensure compatibility across updates and save-game reloads, patches like `Patch_ResearchManager_ReapplyAllMods` are used to maintain consistent mod functionality.

## Suggestions for Copilot
- **Automated Method Generation**: Employ Copilot to generate common methods like comparers (`CompareTo`, `Compare` functions) or boilerplate code for dialog interfaces.
- **XML Definitions**: Suggest specific XML templates for new apparel and building definitions to align with mod logic defined in C#.
- **Harmony Integration**: Use Copilot to draft potential patches by analyzing the behavior of existing methods and suggesting alterations or insertions that fit the mod's needs.
- **Refactoring Assistance**: Help refactor classes for better modularity and maintainability, such as suggesting method extraction or consolidation.

By following these instructions, you can maximize the utility of GitHub Copilot in your RimWorld mod development projects, ensuring efficient and effective code generation that matches the specific needs of your mod enhancements.
