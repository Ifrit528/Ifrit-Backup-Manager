using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
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
            public SaveObject(string source, string destination)
            {
                Source = source;
                Destination = destination;
            }

            public string Source { get; set; }
            public string Destination { get; set; }

            public string GetTime
            {
                get
                {
                    return System.DateTime.Now.ToString("d");
                }
            }
        }

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
                    ObservableCollection<SaveObject> saveObject = await JsonSerializer.DeserializeAsync<ObservableCollection<SaveObject>>(openStream);
                    foreach (SaveObject obj in saveObject)
                    {
                        currentWindow.pathList.Add(obj);
                    }
                }
            }
        }

        private void List_Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SaveObject saveObject = (SaveObject)button.DataContext;
            MessageBox.Show($"{saveObject.Source}, {saveObject.Destination}");
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Window selectionWindow = new AddButtonDialogue();
            selectionWindow.Show();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ListView listView = this.ListView1;
            SaveObject selectedItem = (SaveObject)listView.SelectedItem;

            if (listView.SelectedItem != null)
            {
                MessageBox.Show(selectedItem.Source);
            } else
            {
                MessageBox.Show("Nothing is selected.");
            }
        }

    }
}