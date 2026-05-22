using System.Windows;


namespace Backup_Manager
{
    /// <summary>
    /// Interaction logic for AddButtonDialogue.xaml
    /// </summary>
    public partial class AddButtonDialogue : Window
    {
        string sourcePath;
        string destinationPath;
        string nameText;
        string numberOfBackups;
        int index = -1;

        public AddButtonDialogue()
        {
            InitializeComponent();
        }

        public AddButtonDialogue(MainWindow.SaveObject saveObject, int index)
        {
            InitializeComponent();
            this.NameBox.Text = saveObject.Name;
            this.NumberOfBackups.Text = saveObject.NumberOfBackups;
            this.SourceResponseBox.Text = saveObject.Source;
            this.DestinationResponseBox.Text = saveObject.Destination;
            this.index = index;
        }

        public void SourceFolderDialogClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFolderDialog dialog = new();
            dialog.Multiselect = false;
            dialog.Title = "Select a Source Folder";
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                this.SourceResponseBox.Text = dialog.FolderName;
            }
        }
        
        public void DestinationFolderDialogClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFolderDialog dialog = new();
            dialog.Multiselect = false;
            dialog.Title = "Select a Destination Folder";
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                this.DestinationResponseBox.Text = dialog.FolderName;
            }
        }

        public async void SubmitClick(object sender, RoutedEventArgs e)
        {
            sourcePath = this.SourceResponseBox.Text;
            destinationPath = this.DestinationResponseBox.Text;
            nameText = this.NameBox.Text;
            numberOfBackups = this.NumberOfBackups.Text;

            if (numberOfBackups.Trim() == "") { numberOfBackups = "3"; }

            foreach (char character in numberOfBackups)
            {
                if (!Char.IsNumber(character))
                {
                    MessageBox.Show("Please input a number.");
                    return;
                }
                if (Convert.ToInt32(character) == 0)
                {
                    MessageBox.Show("Please input a number greater than zero.");
                    return;
                }
            }

            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            MainWindow.SaveObject saveObject = new MainWindow.SaveObject(sourcePath, destinationPath, nameText, numberOfBackups);
            if (index >= 0)
            {
                mainWindow.PathList[index] = saveObject;
            }
            else
            {
                mainWindow.PathList.Add(saveObject);
            }

            await MainWindow.Serializer.SaveToFile();
            this.Close();
        }
    }
}
