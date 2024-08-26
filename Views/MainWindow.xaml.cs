using Microsoft.Win32;
using System.Windows;
using ValveSpriteSheetUtil.Util;
using static System.Net.Mime.MediaTypeNames;

namespace ValveSpriteSheetUtil
{
   public partial class MainWindow : Window
   {
      private SpriteSheetManager spriteSheetManager;
      public MainWindow()
      {
         Thread.Sleep(100);
         Console.Title = "Debug Console";
         Console.OutputEncoding = System.Text.Encoding.Unicode;
         InitializeComponent();
         InitializeSpriteSheetManager();
         DimmerOverlay.Visibility = Visibility.Hidden;

         this.Closing += (sender, e) =>
            AppSettingsHelper.SaveSettings(); 
      }



      private void InitializeSpriteSheetManager()
      {
         spriteSheetManager = new SpriteSheetManager();

         tf2FolderTextBox.Text = AppSettingsHelper.GetSetting(x => x.TeamFortressFolder);
         frameTextBox.Text = AppSettingsHelper.GetSetting(x => x.FrameFolder);
         prefixTextBox.Text = AppSettingsHelper.GetSetting(x => x.Prefix);
         vtfNameTextBox.Text = AppSettingsHelper.GetSetting(x => x.FileName);
      }
      private void SelectTf2Folder_Click(object sender, RoutedEventArgs e)
      {
         string selectedPath = IOHelper.OpenFolderDialog();
         if (!string.IsNullOrEmpty(selectedPath))
         {
            AppSettingsHelper.SetSetting(x => x.TeamFortressFolder, selectedPath);
            tf2FolderTextBox.Text = selectedPath;
         }
      }
      private void SelectFrameFolder_Click(object sender, RoutedEventArgs e)
      {
         string selectedPath = IOHelper.OpenFolderDialog();
         if (!string.IsNullOrEmpty(selectedPath))
         {
            AppSettingsHelper.SetSetting(x => x.FrameFolder, selectedPath);
            frameTextBox.Text = selectedPath;
         }
      }
      
      private void Tf2FolderTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
         AppSettingsHelper.SetSetting(x => x.TeamFortressFolder, tf2FolderTextBox.Text);
      }
      private void FrameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
         AppSettingsHelper.SetSetting(x => x.FrameFolder, frameTextBox.Text);
      }
      private void CreateMKSFile_Click(object sender, RoutedEventArgs e)
      {
         string prefix = prefixTextBox.Text;
         string fileName = spriteSheetManager.CleanFileName(vtfNameTextBox.Text);
         bool splitSequences = (bool)splitCheckBox.IsChecked;
         bool loop = (bool)loopCheckBox.IsChecked;

         if (!string.IsNullOrEmpty(prefix) && 
            !string.IsNullOrEmpty(fileName)) 
         { 
            spriteSheetManager.CreateMKSFile(prefix, fileName, splitSequences, loop);
            CreateVTFButton.IsEnabled = true;
            OpenMKSButton.IsEnabled = true;
         }
         else
         {
            ConsoleLog.WriteLine("Prefix and/or output Filename not set", Status.Warning);
         }

      }
      private async void CreateVTFFile_Click(object sender, RoutedEventArgs e)
      {
         ToggleDimmerOverlay();
         CreateVTFButton.IsEnabled = false;
         OpenMKSButton.IsEnabled = false;
         spriteSheetManager.fileName = vtfNameTextBox.Text;

         await Task.Run(() =>
         {
            spriteSheetManager.CreateVTFFile();
         });

         ToggleDimmerOverlay();
      }

      private void ConvertPNG_Click(object sender, RoutedEventArgs e) 
         => spriteSheetManager.ConvertPngToTga(prefixTextBox.Text);

      private void CreateVMTFile_Click(object sender, RoutedEventArgs e) => 
         spriteSheetManager.CreateVMTFile(
            spriteSheetManager.CleanFileName(vtfNameTextBox.Text),
            (bool)blendFramesCheckBox.IsChecked,
            (bool)depthBlendCheckBox.IsChecked,
            (bool)additiveCheckBox.IsChecked
         );
      
      private void PrefixTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
      {
         prefixTextBox.Text = CleanFileName(prefixTextBox.Text);
         prefixTextBox.SelectionStart = prefixTextBox.Text.Length;
         AppSettingsHelper.SetSetting(x => x.Prefix, prefixTextBox.Text);
      }
      private void NameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
      {
         vtfNameTextBox.Text = CleanFileName(vtfNameTextBox.Text);
         vtfNameTextBox.SelectionStart = vtfNameTextBox.Text.Length;
         AppSettingsHelper.SetSetting(x => x.FileName, vtfNameTextBox.Text);
      }
      public string CleanFileName(string name) => 
         string.Concat(name.Split(System.IO.Path.GetInvalidFileNameChars()));
      
      private void OpenVtexConfig_Click(object sender, RoutedEventArgs e)
      {
         var vtexConfigWindow = new VTEXConfigWindow();
         if (vtexConfigWindow.ShowDialog() == true)
         {
         }
      }
      private void ToggleDimmerOverlay() => 
         DimmerOverlay.Visibility = (DimmerOverlay.Visibility == Visibility.Hidden) 
         ? Visibility.Visible : Visibility.Hidden;
      
      private async void OpenMKSButton_Click(object sender, RoutedEventArgs e)
      {
         ToggleDimmerOverlay();
         await Task.Run(() =>
         {
            spriteSheetManager.OpenMKSFileForEditing();
         });
         ToggleDimmerOverlay();
      }
   }
}
