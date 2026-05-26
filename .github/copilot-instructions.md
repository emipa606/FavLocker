# GitHub Copilot Instructions for Fav Locker (Continued)

## Mod Overview and Purpose
"Fav Locker (Continued)" is a mod designed to enhance apparel management in RimWorld by providing specialized storage solutions for your colonists' favorite outfits. Inspired by an original mod by Ekais, this project continues to expand on its capabilities, offering integration with several other popular mods to support varied armor and apparel types.

### Mod Features and Systems
- **Fav Locker**: A storage container that allows colonists to register and quickly change into their favorite apparels with the added benefit of faster apparel changes.
- **Power Armor Station**: A specialized station akin to Fallout 4's power armor stations, enabling repairs on power armors using power and steel, with a significant reduction in change time.
- **Integration with External Mods**: Comprehensive support for mods such as Vanilla Armour Expanded, CAS Armor series, Ultratech: Altered Carbon Remastered, Save Our Ship 2, and Rimworld: Spartan Foundry.

### Key Improvements
- Added hit points filter for apparel management.
- Enhanced compatibility with RimWorld Core ver1.2.
- Bugfixes addressing UI behaviors in the "Register Favorite" dialog.

## Coding Patterns and Conventions
- **OOP Principles**: Utilize object-oriented programming principles, leveraging inheritance and interfaces for class design.
- **Class Naming**: Classes follow PascalCase naming conventions with prefixes to denote their functionality (e.g., `Building_Locker`, `CompLocker`).
- **Method Design**: Method names are descriptive and convey their purpose clearly, adhering to camelCase.

## XML Integration
XML is used to define data-driven aspects of the mod, such as item definitions and language support. Key XML files should use clear and hierarchical structures to ensure maintainability and ease of modification.

## Harmony Patching
The project utilizes [Harmony](https://github.com/pardeike/Harmony) for method alterations:
- Ensure Harmony patches are well-contained and documented.
- Always use prefixes (`Prefix`) and postfixes (`Postfix`) to hook into or modify game behaviors without directly altering the base code.

## Suggestions for Copilot
To enhance Copilot's effectiveness in this project:
- **Document thoroughly**: Ensure all methods and class purposes are well-documented with XML comments to give Copilot more context.
- **Unit Tests**: Implement unit testing where possible to define expected behaviors clearly.
- **Code Examples**: Incorporate code snippets for common patterns, such as registering apparels or repairing armor, to guide Copilot in generating relevant code suggestions.

### Known Issues and Reporting
When encountering issues:
- Validate by running the mod standalone with its dependencies.
- Use the game's Log Uploader for bug reporting, mainly through the Discord channel dedicated to the mod's development.
- Contributions via GitHub are welcome, particularly solutions to known bugs or enhancements for feature requests.

### Future Plans
- Expand compatibility patches for additional power armor mods.
- Consider adding new functions to the Power Armor Station, like advanced armor modifications.

---

This document serves as a guide to understanding and extending the Fav Locker (Continued) mod. Contributors are encouraged to follow the outlined practices to maintain a coherent development environment.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.
