using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Backup_Manager
{
    /// <summary>
    /// Interaction logic for AddButtonDialogue.xaml
    /// </summary>
    public partial class AddButtonDialogue : Window
    {
        string sourcePath;
        string destinationPath;

        public AddButtonDialogue()
        {
            InitializeComponent();
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

            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            MainWindow.SaveObject saveObject = new MainWindow.SaveObject(sourcePath, destinationPath);
            mainWindow.PathList.Add(saveObject);

            await MainWindow.Serializer.SaveToFile();
            this.Close();
        }
    }
}
