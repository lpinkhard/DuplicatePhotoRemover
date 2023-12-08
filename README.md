# Duplicate Photo Remover

Duplicate Photo Remover is a C# WPF (Windows Presentation Foundation) application that allows users to easily remove similar photos within a selected directory and its subdirectories. It uses perceptual hashing (dHash) to identify and compare images, helping you reclaim storage space by eliminating duplicate or highly similar photos.

## Features

- Quickly scan and identify similar images within a specified directory and its subdirectories.
- Keep one copy of each unique image while removing duplicates.
- Intuitive and user-friendly interface with output log.
- Supports various image formats, including JPEG, PNG, TIFF, DNG, and WEBP.

## Prerequisites

- Windows operating system (Windows 7 or later).
- .NET Framework 6.0 or later.
- Visual Studio (or another IDE for C# development) for building and running the application.

## Installation

1. Clone the repository to your local machine:
```
git clone https://github.com/lpinkhard/DuplicatePhotoRemover.git
```
2. Open the solution file (`DuplicatePhotoRemover.sln`) in Visual Studio.
3. Build the solution to generate the executable.
4. Run the application by executing the generated `.exe` file located in the `bin` or `bin/Debug` directory.

## Usage

1. Launch the Duplicate Photo Remover application.
2. Click the "Select Directory" button to choose the directory where you want to remove duplicate photos.
3. Click the "Remove Duplicates" button to delete the duplicate photos. Be cautious, as deleted photos cannot be recovered.
4. The application will notify you once the removal process is complete.

## Customization

You can customize the application by modifying the code according to your needs. For example, you can improve the user interface, change the hashing algorithm, or implement additional features.

## Contributing

Contributions to this project are welcome. If you have any suggestions, bug reports, or feature requests, please open an issue or create a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE.txt](LICENSE.txt) file for details.

## Acknowledgments

- [ImageMagick](https://imagemagick.org) for image processing capabilities.
