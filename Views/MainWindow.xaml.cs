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

         string result = spriteSheetManager.CreateMKSFile(prefix, fileName, splitSequences, loop);
         CreateVTFButton.IsEnabled = true;
         OpenMKSButton.IsEnabled = true;
         ConsoleLog.WriteLine(result, Status.Info);
      }
      private void CreateVTFFile_Click(object sender, RoutedEventArgs e)
      {
         spriteSheetManager.fileName = vtfNameTextBox.Text;
         string result = spriteSheetManager.CreateVTFFile();
         Console.WriteLine(result);
      }
      private void ConvertPNG_Click(object sender, RoutedEventArgs e)
      {
         string prefix = prefixTextBox.Text;
         string result = spriteSheetManager.ConvertPngToTga(prefix);
         Console.WriteLine(result);
      }
      private void CreateVMTFile_Click(object sender, RoutedEventArgs e)
      {
         string fileName = spriteSheetManager.CleanFileName(vtfNameTextBox.Text);
         bool blendFrames = (bool)blendFramesCheckBox.IsChecked;
         bool depthBlend = (bool)depthBlendCheckBox.IsChecked;
         bool additive = (bool)additiveCheckBox.IsChecked;

         string result = spriteSheetManager.CreateVMTFile(fileName, blendFrames, depthBlend, additive);
         Console.WriteLine(result);
      }
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
      public string CleanFileName(string name)
      {
         return string.Concat(name.Split(System.IO.Path.GetInvalidFileNameChars()));
      }
      private void OpenVtexConfig_Click(object sender, RoutedEventArgs e)
      {
         var vtexConfigWindow = new VTEXConfigWindow();
         if (vtexConfigWindow.ShowDialog() == true)
         {
         }
      }
      private void ToggleDimmerOverlay()
      {
         if (DimmerOverlay.Visibility == Visibility.Hidden)
         {
            DimmerOverlay.Visibility = Visibility.Visible;
         }
         else
         {
            DimmerOverlay.Visibility = Visibility.Hidden;
         }
      }
      private void OpenMKSButton_Click(object sender, RoutedEventArgs e)
      {
         ToggleDimmerOverlay();
         spriteSheetManager.OpenMKSFileForEditing();
         ToggleDimmerOverlay();
      }
   }
}
