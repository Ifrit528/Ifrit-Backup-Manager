using System.Collections.ObjectModel;
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
            public SaveObject(string source, string destination, string name)
            {
                Source = source;
                Destination = destination;
                Name = name;
            }

            public string Source { get; set; }
            public string Destination { get; set; }
            public string Name { get; set; }

            public string GetTime
            {
                get
                {
                    return System.DateTime.Now.ToString("g");
                }
            }
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

        static void CopyDirectory(string source, string destination, bool recursive=true)
        {
            var sourceDir = new DirectoryInfo(source);
            var destinationDir = new DirectoryInfo(destination);

            //if (!sourceDir.Exists) { throw new DirectoryNotFoundException($"Source directory not found: {sourceDir.FullName}"); }
            if (!sourceDir.Exists) { MessageBox.Show($"Source directory not found: {sourceDir.FullName}"); return; }
            if (!destinationDir.Exists) { Directory.CreateDirectory(destinationDir.FullName); }

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
                    CopyDirectory(subDirectory.FullName, newDirectory);
                }
            }
        }

        private void List_Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SaveObject saveObject = (SaveObject)button.DataContext;

            CopyDirectory(saveObject.Source, saveObject.Destination);
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
    }
}