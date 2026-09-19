![Generated image 1](/Assets/logo.png)

# Folder Organizer 📂

**Folder Organizer** is a C# application designed to help users organize their files by automatically moving them into designated folders based on predefined rules.

## Badges

I've earned these recognitions: [![MIT License](https://img.shields.io/badge/License-MIT-blue.svg)](https://choosealicense.com/licenses/mit/)[![Version](https://img.shields.io/npm/v/my-package.svg)](https://www.npmjs.com/package/my-package)[![Tests](https://img.shields.io/github/actions/workflow/status/MYUSER/REPO/tests.yml)](https://github.com/MYUSER/REPO/actions)

## Table of Contents 📜

- [Folder Organizer 📂](#folder-organizer-)
  - [Badges](#badges)
  - [Table of Contents 📜](#table-of-contents-)
  - [About 🚀](#about-)
  - [Features ✨](#features-)
  - [Tech Stack 💻](#tech-stack-)
  - [Installation 💿](#installation-)
  - [Usage 🗂️](#usage-️)
    - [How to use 🛠️](#how-to-use-️)
  - [Contributing 🤝](#contributing-)
  - [License 📄](#license-)
  - [Project Structure 📦](#project-structure-)
  - [Footer 🌐](#footer-)

## About 🚀

Folder Organizer is a desktop application developed in C# that aims to simplify file management. It allows users to define rules for organizing their files, and then automatically scans and moves files to the appropriate directories. This is particularly useful for keeping download folders, desktop, or other frequently cluttered areas tidy.

## Features ✨

* **Rule-Based Organization:** Define custom rules to categorize and move files based on their type, name, or other criteria.
* **Automated Scanning:** Scans specified directories for files that match the defined organization rules.
* **File Movement:** Efficiently moves files to their designated organized folders.
* **History Tracking:** (Implied by `HistoryService.cs` and `HistoryViewModel.cs`) Likely maintains a log of organization operations.
* **Ignore Rules:** (Implied by `IgnoreService.cs` and `IgnoreRule.cs`) Ability to specify files or patterns to ignore during the organization process.
* **User-Friendly Interface:** (Implied by WPF `.xaml` files) Likely features a graphical user interface for easy management and configuration.

## Tech Stack 💻

* **Language:** C#
* **Framework:** .NET (WPF for UI)
* **Database:** SQLite (indicated by `.sqlite` and `.db` files, e.g., `organizer_database.sqlite`)

## Installation 💿

This project appears to be a standard .NET WPF application. To build and run it, you will need:

1. **Visual Studio:** A recent version of Visual Studio (e.g., 2019 or later) with the .NET desktop development workload installed.
2. **.NET SDK:** Ensure you have the .NET SDK installed.

**Steps:**

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/mrjohndowe/Folder_Organizer.git
   cd Folder_Organizer
   ```
2. **Open in Visual Studio:** Open the `FolderOrganizer.sln` file in Visual Studio.
3. **Build the Project:** Build the solution using Visual Studio (Build > Build Solution).
4. **Run the Application:** Run the project (Debug > Start Debugging or Ctrl+F5).

*Note: As there are no explicit dependency files like `packages.config` or `.csproj` content provided, this installation guide is based on standard C# WPF project structures. Specific NuGet packages would need to be managed through Visual Studio.*

## Usage 🗂️

Folder Organizer is designed to automate the process of sorting files into organized directories. Its primary use case is to declutter common file storage locations like the Downloads folder, Desktop, or any directory where files tend to accumulate.

**General Workflow:**

1. **Define Rules:** Use the application's interface to create rules. For example, you might create a rule to move all `.jpg` and `.png` files to an "Images" folder, `.pdf` files to a "Documents" folder, and `.mp3` files to a "Music" folder.
2. **Specify Scan Location:** Indicate the folder(s) you want the application to monitor and organize (e.g., your Downloads folder).
3. **Run Scan:** Initiate a scan. The application will identify files that match your rules.
4. **Review and Move:** The application may offer a preview of the planned moves. Once confirmed, it will execute the file movements.

*Example Scenario: Cleaning up your Downloads folder.*

Imagine your Downloads folder is filled with installer files, documents, images, and archives. You can configure Folder Organizer to:

* Move all `.exe` and `.msi` files to a "Software" folder.
* Move all `.pdf`, `.docx`, `.xlsx` files to a "Documents" folder.
* Move all `.zip`, `.rar` files to an "Archives" folder.

After setting these rules and selecting your Downloads folder as the target, running the organizer will automatically sort these files, making your Downloads folder much more manageable.

### How to use 🛠️

1. Launch the **Folder Organizer** application.
2. Navigate to the **Settings** or **Rules** section.
3. Create new **Organization Rules** by specifying:
   * **File Type/Pattern:** e.g., `*.jpg`, `*.png`, `*.pdf`
   * **Destination Folder:** e.g., `C:\Users\YourUser\Pictures`, `C:\Users\YourUser\Documents`
4. Optionally, configure **Ignore Rules** to exclude specific files or patterns from being moved.
5. Go to the main screen and select the **source folder** you wish to organize (e.g., your main Downloads folder).
6. Click the **Scan** or **Organize** button.
7. Review the planned operations in the **Preview** window.
8. Confirm the operations to move the files.

## Contributing 🤝

Contributions are welcome! If you'd like to contribute to Folder Organizer, please follow these steps:

1. Fork the repository.
2. Create a new branch for your feature or bug fix (`git checkout -b feature/AmazingFeature`).
3. Make your changes and commit them (`git commit -m 'Add some AmazingFeature'`).
4. Push to the branch (`git push origin feature/AmazingFeature`).
5. Open a Pull Request.

Please ensure your code adheres to the existing style and includes tests where appropriate.

## License 📄

This project is licensed under the **MIT License** - see the [LICENSE.txt](LICENSE.txt) file for details.

## Project Structure 📦

The project follows a typical structure for a C# WPF application, with organized directories for different components:

```
Folder_Organizer/
├── FolderOrganizer.sln            # Solution file
├── FolderOrganizer.csproj         # Project file
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml              # Main application window
├── MainWindow.xaml.cs           # Main window code-behind
├── Models/
│   ├── MoveOperation.cs       # Represents a file move operation
│   ├── HistoryEntry.cs        # Represents an entry in the history log
│   ├── IgnoreRule.cs          # Defines a rule to ignore files
│   └── OrganizationRule.cs    # Defines a rule for organizing files
├── Organizer/
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── Database/
│   │   └── organizer_database.sqlite # SQLite database file
│   ├── Helpers/
│   │   ├── FileIconHelper.cs    # Utility for file icons
│   │   └── PathHelper.cs        # Utility for path operations
│   ├── Services/
│   │   ├── DatabaseService.cs     # Service for database interactions
│   │   ├── FileClassifier.cs    # Classifies files
│   │   ├── FileMoveService.cs     # Handles file moving logic
│   │   ├── FileScanner.cs       # Scans directories for files
│   │   ├── HistoryService.cs    # Service for managing history
│   │   └── IgnoreService.cs     # Service for managing ignore rules
│   ├── ViewModels/
│   │   ├── HistoryViewModel.cs  # ViewModel for history view
│   │   ├── MainViewModel.cs     # ViewModel for the main window
│   │   ├── PreviewViewModel.cs  # ViewModel for previewing operations
│   │   └── SettingsViewModel.cs # ViewModel for settings
│   └── Views/
│       ├── HistoryWindow.xaml   # UI for history
│       ├── MainWindow.xaml    # UI for main window (possible duplicate)
│       ├── PreviewWindow.xaml   # UI for previewing moves
│       └── SettingsWindow.xaml  # UI for settings
├── Services/
│   ├── ClassificationService.cs # Another classification service? (Potential redundancy or specific function)
│   └── ScanService.cs         # Another scanning service? (Potential redundancy or specific function)
├── app.manifest                 # Application manifest
└── LICENSE.txt                  # Project license file
```

*Note: There appear to be some duplicate file paths (e.g.,* `App.xaml`*,* `App.xaml.cs`*,* `MainWindow.xaml`*) and potentially redundant service implementations (*`Services/` *vs.* `Organizer/Services/`*). This might indicate different parts of the application or a need for refactoring.*

## Footer 🌐

***

**Folder Organizer** | [mrjohndowe/Folder_Organizer](https://github.com/mrjohndowe/Folder_Organizer)

Built with ❤️ by [MrJohnDowe](https://github.com/mrjohndowe)

:star: Like this project? Give it a star!

:fork\_and\_knife: Found a bug? Open an issue!

***

Made with ❤️ by [MrJohnDowe](https://github.com/mrjohndowe).
