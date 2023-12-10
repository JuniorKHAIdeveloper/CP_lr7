using System;
using System.Collections.Generic;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace lr7
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private StorageFolder currentFolder;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            currentFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            labelCurrDir.Text += currentFolder.Path;
            FillListBox(currentFolder);
        }
        private async void FillListBox(StorageFolder directory)
        {
            if (directory != null)
            {
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Content = "Name\t\t\tCreation Date\t\t\tSize\t\t\tType";
                filesList.Items.Clear();
                filesList.Items.Add(listBoxItem);
                IReadOnlyList<IStorageItem> itemsList = await directory.GetItemsAsync();
                foreach (var item in itemsList)
                {
                    listBoxItem = new ListBoxItem();
                    listBoxItem.Tag = item;
                    if (item is StorageFile file)
                    {
                        BasicProperties fileProperties = await item.GetBasicPropertiesAsync();
                        listBoxItem.Content = $"{file.Name,-50} | {file.DateCreated,-15}, {GetFileSize(fileProperties.Size),-10}, {file.FileType,15}";
                        filesList.Items.Add(listBoxItem);
                    }
                    else
                    {
                        StorageFolder folder = (StorageFolder)item;
                        listBoxItem.Content = $"{folder.Name,-50} {folder.DateCreated,-15} {" ",-10} {folder.DisplayType,15}";
                        listBoxItem.DoubleTapped += ListBoxItem_DoubleTapped;
                        filesList.Items.Add(listBoxItem);
                    }
                }
            }
        }

        private void ListBoxItem_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)sender;
            if (item.Tag is StorageFolder folder)
            {
                currentFolder = folder;
                FillListBox(currentFolder);
                labelCurrDir.Text = "Current directory: " + currentFolder.Path;
            }
        }

        private string GetFileSize(ulong sizeInBytes)
        {
            ulong resultSize = sizeInBytes;
            string units = " Б";
            if (resultSize > 1024)
            {
                resultSize /= 1024;
                units = " КБ";
            }
            if (resultSize > 1024)
            {
                resultSize /= 1024;
                units = " МБ";
            }
            if (resultSize > 1024)
            {
                resultSize /= 1024;
                units = " ГБ";
            }
            return resultSize.ToString() + units;
        }

        private async void btnChangeDir_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.ViewMode = PickerViewMode.List;
            folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                currentFolder = folder;
                FillListBox(currentFolder);
                labelCurrDir.Text = "Current directory: " + currentFolder.Path;
            }
        }


        private async void btnCreateDir_Click(object sender, RoutedEventArgs e)
        {
            if (CreatedDirName.Text != String.Empty)
            {
                await currentFolder.CreateFolderAsync(CreatedDirName.Text);
                FillListBox(currentFolder);
            }
        }

        private async void btnDeleteDir_Click(object sender, RoutedEventArgs e)
        {
            if (filesList.SelectedItem == null)
                return;
            ListBoxItem item = filesList.SelectedItem as ListBoxItem;
            if (item.Tag is StorageFolder folder)
            {
                await folder.DeleteAsync();
                filesList.Items.Remove(item);
            }
        }
    }
}
