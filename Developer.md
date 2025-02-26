# Developer Guide for DisplayRotator

This document provides an overview of the DisplayRotator project, including its architecture, key components, and development guidelines.

## Project Structure

The project consists of the following main components:

- `MainForm.cs`: The main form that handles the application's UI and interactions.
- `ShortcutSettingsForm.cs`: The form for configuring shortcut keys and enabling/disabling rotation settings.
- `SettingsManager.cs`: Manages the loading, saving, and handling of settings.
- `Settings.cs`: Represents the settings data structure and handles serialization/deserialization.

## Key Components

### MainForm

The `MainForm` class is responsible for:

- Displaying the notification icon and context menu.
- Handling screen rotation based on user input or hotkeys.
- Registering and unregistering hotkeys.

#### Key Methods

- `InitializeComponent()`: Sets up the form, notification icon, and context menu.
- `ShowShortcutSettings()`: Opens the `ShortcutSettingsForm` for configuring shortcuts.
- `UpdateMenuItems()`: Updates the context menu items based on the current settings.
- `RegisterHotKeys()`: Registers the hotkeys for screen rotation.
- `UnregisterHotKeys()`: Unregisters the hotkeys.
- `RotateScreen(int orientation)`: Rotates the screen to the specified orientation.

### ShortcutSettingsForm

The `ShortcutSettingsForm` class is responsible for:

- Displaying the UI for configuring shortcut keys and enabling/disabling rotation settings.
- Saving the settings when the form is closed.

#### Key Methods

- `InitializeComponents()`: Sets up the form layout and controls.
- `LoadDisplaySettings()`: Loads the current display settings from the `SettingsManager`.
- `SaveDisplaySettings()`: Saves the display settings to the `SettingsManager`.
- `SetShortcut(int rotationId)`: Opens a dialog to set a shortcut key for a specific rotation.
- `ClearShortcut(int rotationId)`: Clears the shortcut key for a specific rotation.

### SettingsManager

The `SettingsManager` class is responsible for:

- Loading and saving settings.
- Managing shortcut keys and enabled/disabled states for rotations.

#### Key Methods

- `SetShortcut(int rotationId, Keys key)`: Sets a shortcut key for a specific rotation.
- `RemoveShortcut(int rotationId)`: Removes the shortcut key for a specific rotation.
- `GetShortcut(int rotationId)`: Gets the shortcut key for a specific rotation.
- `GetShortcutText(int rotationId)`: Gets the text representation of the shortcut key for a specific rotation.
- `IsEnabled(int rotationId)`: Checks if a specific rotation is enabled.
- `SetEnabled(int rotationId, bool enabled)`: Enables or disables a specific rotation.
- `SaveSettings()`: Saves the settings to a file.
- `LoadSettings()`: Loads the settings from a file.
- `InitializeDefaultSettings()`: Initializes default settings if they are not already set.

### Settings

The `Settings` class represents the settings data structure and handles serialization/deserialization.

#### Key Properties

- `Dictionary<int, Keys> Shortcuts`: Stores the shortcut keys for each rotation.
- `Dictionary<int, bool> EnabledSettings`: Stores the enabled/disabled states for each rotation.

#### Key Methods

- `static Settings Load()`: Loads the settings from a file.
- `void Save()`: Saves the settings to a file.

## Development Guidelines

### Coding Standards

- Follow C# coding conventions.
- Use meaningful variable and method names.
- Keep methods short and focused on a single task.
- Add comments to explain complex logic.

### Version Control

- Use Git for version control.
- Create feature branches for new features or bug fixes.
- Commit changes with clear and concise commit messages.
- Create pull requests for code reviews before merging into the main branch.

### Testing

- Test new features and bug fixes thoroughly.
- Write unit tests for critical components.
- Ensure that the application works as expected before committing changes.

### Contribution

- Fork the repository and create a new branch for your changes.
- Make your changes and test them thoroughly.
- Submit a pull request with a clear description of your changes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.
