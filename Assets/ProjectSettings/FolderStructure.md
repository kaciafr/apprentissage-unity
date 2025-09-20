# Folder Structure Guide

## 🎯 Naming Conventions

This project follows descriptive naming conventions to make the codebase self-documenting and easier to navigate.

## 📁 Top-Level Organization

### **GameCore/**
Contains the fundamental systems that make the game work.
- **Scripts/**: All game logic and code
- **Systems/**: Modular game systems (Save, Audio, Input, etc.)
- **Managers/**: Singleton managers for global functionality

### **GameContent/**
All visual and audio assets that players interact with.
- **Characters/**: Player characters, NPCs, avatars
- **Environment/**: Levels, terrain, buildings, props
- **Items/**: Weapons, tools, collectibles, inventory items
- **Audio/**: Music, sound effects, voice acting
- **Animations/**: Character and object animations
- **ImportedAssets/**: Temporary folder for newly imported content

### **GameUI/**
Everything related to user interface and user experience.
- **Menus/**: Main menu, settings, pause menu
- **HUD/**: Health bars, mini-maps, score displays
- **Dialogs/**: Pop-ups, confirmation windows, tutorials

### **ThirdPartyAssets/**
External dependencies and purchased assets.
- **Plugins/**: External code libraries and APIs
- **TextMeshPro/**: Unity's text rendering system
- **KevinIglesias/**: Animation packs from asset store

### **ProjectSettings/**
Configuration files and project-wide settings.
- **CoreSettings/**: ScriptableObjects for game configuration

### **EditorTools/**
Development utilities and custom editor scripts.
- **CustomTools/**: Project-specific editor utilities

### **Documentation/**
Project documentation and guides.

## 🔄 Asset Workflow

### 1. **Import New Assets**
- Place temporarily in `GameContent/ImportedAssets/`
- Review and organize into appropriate folders
- Delete from ImportedAssets when organized

### 2. **Character Assets**
- Models → `GameContent/Characters/Models/`
- Textures → `GameContent/Characters/Textures/`
- Animations → `GameContent/Characters/Animations/`
- Prefabs → `GameContent/Characters/Prefabs/`

### 3. **Environment Assets**
- Terrain → `GameContent/Environment/Terrain/`
- Buildings → `GameContent/Environment/Buildings/`
- Props → `GameContent/Environment/Props/`
- Lighting → `GameContent/Environment/Lighting/`

### 4. **Audio Organization**
- Music → `GameContent/Audio/Music/`
- SFX → `GameContent/Audio/SoundEffects/`
- Voice → `GameContent/Audio/Voice/`
- Ambient → `GameContent/Audio/Ambient/`

## 🎨 Naming Standards

### **Files**
- Use PascalCase: `PlayerController.cs`
- Be descriptive: `InventorySlotUI.cs` not `Slot.cs`
- Include type suffix: `PlayerData.cs`, `AudioManager.cs`

### **Folders**
- Use descriptive names: `CharacterAnimations` not `Anims`
- Avoid abbreviations: `UserInterface` not `UI`
- Group by function: `NetworkSystems`, `GameplayMechanics`

### **Prefabs**
- Include object type: `Player_Character.prefab`
- Use context: `UI_HealthBar.prefab`, `Weapon_Sword.prefab`

### **Scripts**
- Controllers: `PlayerController.cs`, `CameraController.cs`
- Managers: `AudioManager.cs`, `SaveManager.cs`
- Data: `PlayerData.cs`, `InventoryData.cs`
- UI: `MainMenuUI.cs`, `InventoryUI.cs`

## 🚀 Benefits

1. **Self-Documenting**: Folder names explain their purpose
2. **Scalable**: Easy to add new categories without confusion
3. **Team-Friendly**: New developers understand the structure immediately
4. **Maintainable**: Clear separation of concerns
5. **Professional**: Industry-standard organization

## 🔍 Quick Reference

```
Need scripts? → GameCore/Scripts/
Need models? → GameContent/Characters/ or Environment/
Need UI elements? → GameUI/
Need sounds? → GameContent/Audio/
Need third-party assets? → ThirdPartyAssets/
Need project settings? → ProjectSettings/
```

---

*This structure is designed for commercial-grade Unity projects and can scale from indie games to AAA productions.*