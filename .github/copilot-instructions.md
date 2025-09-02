# GitHub Copilot Instructions for Fav Locker (Continued) Mod Development

## Mod Overview and Purpose

**Mod Name**: Fav Locker (Continued)

**Description**: This mod is an update of the original mod by Ekais. It enhances the functionality of Fav Locker and integrates support for several additional mods focused on power armor integration within RimWorld. Fav Locker allows colonists to manage, wear, and store their favorite apparels with ease, aiming to streamline transitions between everyday and combat attire.

## Key Features and Systems

- **Favorite Container System**: Allows registration of colonists' favorite apparels. It simplifies the process of dressing and undressing by enabling quick toggling between stored outfits.
- **Power Armor Station**: Functions similarly to the Power Armor Station from Fallout 4, designed specifically for managing end-game power armors with reduced time for apparel changes.
- **Hit Points Filter**: Added to lockers for better apparel management.
- **Repair Functionality**: Power Armor Stations can repair armors using power and steel, once the Advanced Fabrication research is completed.

## Coding Patterns and Conventions

- **Class Structures**: Classes implement interfaces like `IComparable`, `IGettableDestination`, and various Comp classes. Naming conventions follow a clear and descriptive pattern.
  
- **Method Naming**: Methods are named to clearly reflect their action, e.g., `setCommandOfWearFavorite`, `canCommandOfWearFavorite`.

- **Inheritance and Interfaces**: The code extensively utilizes inheritance and interfaces to ensure modular and reusable components.

## XML Integration

- XML files are used for defining configurations and settings related to apparel, pawns, and mod settings. Manage these configurations to ensure new features are accurately reflected in the game.

## Harmony Patching

- The mod uses Harmony to modify existing gameplay elements without altering the original game code. Ensure patches are applied responsibly to maintain compatibility and prevent potential conflicts with other mods.

## Suggestions for Copilot

To effectively utilize GitHub Copilot in this project, consider the following tips:

1. **Boilerplate Code**: Use Copilot to quickly generate boilerplate code like class definitions, method stubs, and interface implementations.
2. **Patch Optimization**: When adding new Harmony patches, allow Copilot to suggest patterns for efficient and conflict-free integration.
3. **Error Handling**: Leverage Copilot for generating standard error-handling routines to manage exceptions effectively within Unity and C# contexts.
4. **Code Documentation**: Utilize Copilot to assist in writing clear and concise comments to explain complex logic or new features.
5. **XML File Edits**: Utilize Copilot’s capabilities to update and integrate XML configurations for new functionalities seamlessly.

By following these guidelines, you'll ensure that the integration of GitHub Copilot maximizes productivity and maintains code quality across the Fav Locker mod project.
