# Unity Learning Project

> A professional Unity project template with PocketBase integration for multiplayer and data management.

## 🚀 Features

- **Professional Project Structure**: Industry-standard folder organization
- **PocketBase Integration**: Real-time database and authentication
- **Modular Architecture**: Clean, maintainable code structure
- **Multiplayer Support**: Built-in networking capabilities
- **Asset Management**: Organized art, audio, and prefab systems

## 📁 Project Structure

```
Assets/
├── _Project/               # Main project files
│   ├── Scripts/
│   │   ├── Runtime/        # Runtime scripts
│   │   │   ├── Core/       # Core game systems
│   │   │   ├── Networking/ # Network functionality
│   │   │   ├── UI/         # User interface
│   │   │   ├── Gameplay/   # Game mechanics
│   │   │   ├── Data/       # Data structures
│   │   │   └── Utils/      # Utility functions
│   │   └── Editor/         # Editor-only scripts
│   ├── Art/                # Visual assets
│   │   ├── Models/         # 3D models
│   │   ├── Textures/       # Texture files
│   │   ├── Materials/      # Unity materials
│   │   ├── Audio/          # Sound effects & music
│   │   └── Animations/     # Animation files
│   ├── Prefabs/            # Game object prefabs
│   ├── ScriptableObjects/  # Data containers
│   └── Settings/           # Project settings
├── Plugins/                # Third-party plugins
│   └── PocketBase/         # PocketBase integration
├── ThirdParty/             # External assets
└── Documentation/          # Project documentation
```

## 🛠️ Setup Instructions

### Prerequisites
- Unity 2022.3 LTS or later
- Git with LFS support
- PocketBase server (for networking features)

### Installation

1. **Clone the repository:**
   ```bash
   git clone --recursive https://github.com/kaciafr/apprentissage-unity.git
   cd apprentissage-unity
   ```

2. **Initialize submodules:**
   ```bash
   git submodule update --init --recursive
   ```

3. **Open in Unity:**
   - Launch Unity Hub
   - Click "Open" and select the project folder
   - Unity will import all assets automatically

### PocketBase Setup

1. **Configure PocketBase:**
   - Update connection settings in `Assets/_Project/Settings/`
   - Set your PocketBase server URL
   - Configure authentication settings

2. **Database Schema:**
   - Import the provided PocketBase schema
   - Set up user authentication
   - Configure real-time subscriptions

## 🏗️ Architecture

### Core Systems
- **Game Manager**: Central game state management
- **Scene Management**: Smooth scene transitions
- **Save System**: Persistent data storage
- **Event System**: Decoupled communication

### PocketBase Integration
- **Authentication**: User login/registration
- **Real-time Sync**: Live data synchronization
- **Cloud Storage**: Asset and save file storage
- **Multiplayer**: Player session management

## 🎯 Best Practices

### Code Standards
- **Namespaces**: All scripts use proper namespacing
- **Interfaces**: Dependency injection patterns
- **SOLID Principles**: Clean, maintainable architecture
- **Documentation**: Comprehensive XML comments

### Asset Organization
- **Naming Convention**: Clear, consistent file naming
- **Import Settings**: Optimized for target platforms
- **Version Control**: Git LFS for large assets
- **Performance**: LOD systems and optimization

## 📚 Documentation

- [API Reference](Documentation/API.md)
- [Architecture Guide](Documentation/Architecture.md)
- [PocketBase Integration](Documentation/PocketBase.md)
- [Deployment Guide](Documentation/Deployment.md)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Follow coding standards
4. Submit a pull request

## 📄 License

This project is available for commercial use. See [LICENSE](LICENSE) for details.

## 🔗 Links

- [PocketBase Documentation](https://pocketbase.io/docs/)
- [Unity Best Practices](https://unity.com/how-to/organizing-your-project)
- [Project Repository](https://github.com/kaciafr/apprentissage-unity)

---

**Built with ❤️ for professional Unity development**