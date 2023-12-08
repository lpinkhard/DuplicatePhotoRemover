using System;
using System.Text;
using System.Windows;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Drawing;

namespace DuplicatePhotoRemover
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // P/Invoke declarations
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern bool SHGetPathFromIDList(IntPtr pidl, StringBuilder pszPath);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct BROWSEINFO
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public IntPtr pszDisplayName;
            public string lpszTitle;
            public uint ulFlags;
            public IntPtr lpfn;
            public IntPtr lParam;
            public int iImage;
        }

        // Constants for BROWSEINFO.ulFlags
        private const uint BIF_RETURNONLYFSDIRS = 0x0001;
        private const uint BIF_NEWDIALOGSTYLE = 0x0040;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectDirectory_Click(object sender, RoutedEventArgs e)
        {
            BROWSEINFO bi = new BROWSEINFO();
            bi.ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE;
            bi.lpszTitle = "Select a folder";
            IntPtr buffer = Marshal.AllocHGlobal(260); // Allocate buffer for the display name
            bi.pszDisplayName = buffer;

            IntPtr pidl = SHBrowseForFolder(ref bi);
            if (pidl != IntPtr.Zero)
            {
                StringBuilder path = new StringBuilder(260);
                if (SHGetPathFromIDList(pidl, path))
                {
                    directoryPathTextBox.Text = path.ToString();
                }
                Marshal.FreeCoTaskMem(pidl);
            }
            Marshal.FreeHGlobal(buffer); // Free the buffer
        }

        private void RemoveDuplicates_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove duplicates?",
                                "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                string selectedDirectory = directoryPathTextBox.Text;
                if (!string.IsNullOrEmpty(selectedDirectory))
                {
                    RemoveDuplicatesAsync(selectedDirectory);
                }
                else
                {
                    MessageBox.Show("Please select a directory first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RemoveDuplicates(string directoryPath)
        {
            Dispatcher.Invoke(() =>
            {
                removeDuplicatesButton.IsEnabled = false;
            });
            LogToConsole("Scanning directory: " + directoryPath);

            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".tif", ".tiff", ".dng", ".webp" };

            List<string> imageFiles = new List<string>();

            foreach (var ext in imageExtensions)
            {
                try
                {
                    imageFiles.AddRange(Directory.GetFiles(directoryPath, "*" + ext, SearchOption.AllDirectories));
                }
                catch (UnauthorizedAccessException ex)
                {
                    LogToConsole($"Error: {ex.Message}");
                    Dispatcher.Invoke(() =>
                    {
                        removeDuplicatesButton.IsEnabled = true;
                    });
                    return;
                }
            }

            Dictionary<string, string> imageHashes = new Dictionary<string, string>();
            List<string> filesToDelete = new List<string>();

            foreach (string filePath in imageFiles)
            {
                try
                {
                    using (var image = new Bitmap(filePath))
                    {
                        // Resize the image to 9x8 and 8x9
                        Bitmap resized1 = ResizeImage(image, 9, 8);
                        Bitmap resized2 = ResizeImage(image, 8, 9);

                        // Calculate dHash for both resized images
                        string hash1 = CalculateDHash(resized1);
                        string hash2 = CalculateDHash(resized2);

                        // Interlace the hashes
                        string combinedHash = InterlaceHashes(hash1, hash2);

                        // Check if a similar image hash already exists
                        if (imageHashes.ContainsKey(combinedHash))
                        {
                            filesToDelete.Add(filePath);
                        }
                        else
                        {
                            imageHashes[combinedHash] = filePath;
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    LogToConsole("Skipping file (permission denied): " + filePath);
                }
                catch (Exception ex)
                {
                    LogToConsole("Error processing file: " + ex.Message);
                }
            }

            foreach (string fileToDelete in filesToDelete)
            {
                try
                {
                    File.Delete(fileToDelete);
                    LogToConsole("Deleted similar image: " + fileToDelete);
                }
                catch (IOException ex)
                {
                    LogToConsole("Error deleting file: " + ex.Message);
                }
            }

            LogToConsole("Similar images have been removed.");
            Dispatcher.Invoke(() =>
            {
                removeDuplicatesButton.IsEnabled = true;
            });
        }

        private void LogToConsole(string message)
        {
            Dispatcher.Invoke(() =>
            {
                logTextBox.AppendText(message + "\n");
                logTextBox.ScrollToEnd();
            });
        }

        private void RemoveDuplicatesAsync(string directoryPath)
        {
            Task.Run(() =>
            {
                RemoveDuplicates(directoryPath);
            });
        }

        static string CalculateDHash(Bitmap image)
        {
            StringBuilder hash = new StringBuilder();

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width - 1; x++)
                {
                    Color pixel1 = image.GetPixel(x, y);
                    Color pixel2 = image.GetPixel(x + 1, y);

                    int luminance1 = (int)(pixel1.R * 0.3 + pixel1.G * 0.59 + pixel1.B * 0.11);
                    int luminance2 = (int)(pixel2.R * 0.3 + pixel2.G * 0.59 + pixel2.B * 0.11);

                    if (luminance1 > luminance2)
                    {
                        hash.Append("1");
                    }
                    else
                    {
                        hash.Append("0");
                    }
                }
            }

            return hash.ToString();
        }

        static Bitmap ResizeImage(Image image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, width, height);
            }
            return resizedImage;
        }

        static string InterlaceHashes(string hash1, string hash2)
        {
            StringBuilder interlacedHash = new StringBuilder(Math.Max(hash1.Length, hash2.Length) * 2);

            int minLength = Math.Min(hash1.Length, hash2.Length);

            for (int i = 0; i < minLength; i++)
            {
                interlacedHash.Append(hash1[i]);
                interlacedHash.Append(hash2[i]);
            }

            // Append any remaining characters from the longer hash
            if (hash1.Length > minLength)
            {
                interlacedHash.Append(hash1.Substring(minLength));
            }
            else if (hash2.Length > minLength)
            {
                interlacedHash.Append(hash2.Substring(minLength));
            }

            return interlacedHash.ToString();
        }
    }
}
