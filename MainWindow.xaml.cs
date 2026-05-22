using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;


namespace Backup_Manager
{
    public partial class MainWindow : Window
    {
        static public readonly MainWindow currentWindow = (MainWindow)Application.Current.MainWindow;
        static public readonly string fileName = "paths.json";

        public MainWindow()
        {
            pathList = [];
            DataContext = this;
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Serializer.ReadSaveFile();
        }

        private ObservableCollection<SaveObject> pathList;
        public ObservableCollection<SaveObject> PathList
        {
            get { return pathList; }
            set { pathList = value; }
        }

        public class SaveObject
        {
            public SaveObject(string source, string destination, string name, string numberOfBackups="3")
            {
                Source = source;
                Destination = destination;
                Name = name;
                NumberOfBackups = numberOfBackups;
                GetTime = System.DateTime.Now.ToString("g");
            }

            public string Source { get; set; }
            public string Destination { get; set; }
            public string Name { get; set; }
            public string NumberOfBackups { get; set; }
            public string GetTime {  get; set; }
        }

        // File saving and reading
        public static class Serializer
        {
            public static async Task SaveToFile()
            {
                ObservableCollection<SaveObject> list = currentWindow.PathList;
                await using FileStream fileStream = File.Create(fileName);
                await JsonSerializer.SerializeAsync(fileStream, list);
            }

            public static async Task ReadSaveFile()
            {
                if (File.Exists(fileName))
                {
                    using FileStream openStream = File.OpenRead(fileName);
                    if (openStream != null && openStream.Length != 0)
                    {
                        ObservableCollection<SaveObject> saveObject = await JsonSerializer.DeserializeAsync<ObservableCollection<SaveObject>>(openStream);
                        foreach (SaveObject obj in saveObject)
                        {
                            currentWindow.PathList.Add(obj);
                        }
                    }
                }
            }

        }

        // Cycle directories down one
        private static void CountDownDirectories(string directory, string sourceName, int numberOfBackups)
        {
            string baseName = Path.Combine(directory, sourceName);
            Directory.Delete(baseName, true);

            for (int i = 0; i < numberOfBackups; i++)
            {
                string oldName = baseName + $"({i})";
                if (Directory.Exists(oldName))
                {
                    if (i == 1) { Directory.Move(oldName, baseName); }
                    else { Directory.Move(oldName, baseName + $"({i-1})"); }
                }
            }
        }

        static void CopyDirectory(string source, string destination, int numberOfBackups, bool firstPass=true, bool recursive = true)
        {
            var sourceDir = new DirectoryInfo(source);
            var destinationDir = new DirectoryInfo(destination);
            string saveDirName = sourceDir.Name;

            if (!sourceDir.Exists) { MessageBox.Show($"Source directory not found: {sourceDir.FullName}"); return; }
            if (!destinationDir.Exists) { Directory.CreateDirectory(destinationDir.FullName); }

            // Preserve source folder and maintain number of backups
            if (firstPass)
            {
                var existingSources = Directory.GetDirectories(destinationDir.FullName, $"{sourceDir.Name}*");
                int i = existingSources.Length;

                while (Directory.Exists(Path.Combine(destination, saveDirName)))
                {
                    if (i > numberOfBackups - 1)
                    {
                        CountDownDirectories(destinationDir.FullName, sourceDir.Name, numberOfBackups);
                        --i;
                    }

                    saveDirName = sourceDir.Name + $"({i})";
                    ++i;
                }

                destinationDir.CreateSubdirectory(saveDirName);
                destination = Path.Combine(destination, saveDirName);
            }

            DirectoryInfo[] recursiveDirectories = sourceDir.GetDirectories();

            foreach (FileInfo file in sourceDir.GetFiles())
            {
                string destinationPath = Path.Combine(destination, file.Name);
                file.CopyTo(destinationPath, true);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDirectory in recursiveDirectories)
                {
                    string newDirectory = Path.Combine(destination, subDirectory.Name);
                    CopyDirectory(subDirectory.FullName, newDirectory, numberOfBackups, false);
                }
            }
        }

        private static int ReturnIndexOf(SaveObject saveObject)
        {
            int index = -1;
            var PathList = currentWindow.PathList;

            for (int i = 0; i < PathList.Count; i++)
            {
                if (PathList[i].Name == saveObject.Name && PathList[i].Source == saveObject.Source && PathList[0].Destination == saveObject.Destination)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private async void List_Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SaveObject saveObject = (SaveObject)button.DataContext;
            int numberOfBackups = Convert.ToInt32(saveObject.NumberOfBackups);            
            int index = ReturnIndexOf(saveObject);

            if (index == -1) { MessageBox.Show("Error finding index."); return; }

            PathList[index].GetTime = System.DateTime.Now.ToString("g");
            await Serializer.SaveToFile();
            this.ListView1.Items.Refresh();

            CopyDirectory(saveObject.Source, saveObject.Destination, numberOfBackups);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Window selectionWindow = new AddButtonDialogue();
            selectionWindow.Show();
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            var items = this.ListView1.SelectedItems;
            if (items.Count > 0)
            {
                for (int i = this.PathList.Count - 1; i >= 0; i--)
                {
                    if (items.Contains(PathList[i]))
                    {
                        this.PathList.RemoveAt(i);
                    }
                }
                await Serializer.SaveToFile();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SaveObject saveObject = (SaveObject)ListView1.SelectedItem;
            int index = ReturnIndexOf(saveObject);
            if (index == -1) { return; }
            Window selectionWindow = new AddButtonDialogue(saveObject, index);
            selectionWindow.Show();
        }

        private void Source_Click(object sender, RoutedEventArgs e)
        {
            TextBlock sourceBlock = (TextBlock)sender;
            if (Directory.Exists(sourceBlock.Text)) { Process.Start("explorer.exe", sourceBlock.Text); }
        }

        private void Destination_Click(object sender, RoutedEventArgs e)
        {
            TextBlock destinationBlock = (TextBlock)sender;
            if (Directory.Exists(destinationBlock.Text)) { Process.Start("explorer.exe", destinationBlock.Text); }
        }
    }
}