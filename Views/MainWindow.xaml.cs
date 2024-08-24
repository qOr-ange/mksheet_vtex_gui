using Microsoft.Win32;
using System.Windows;

namespace ValveSpriteSheetUtil
{
   public partial class MainWindow : Window
   {
      AppSettings appSettings = new AppSettings();
      private SpriteSheetManager spriteSheetManager;

      public MainWindow()
      {
         Thread.Sleep(100);
         Console.Title = "Debug Console";
         InitializeComponent();
         InitializeSpriteSheetManager();
      }

      private void InitializeSpriteSheetManager()
      {
         
         string tf2Folder = appSettings.TeamFortressFolder;
         string frameFolder = appSettings.FrameFolder;
         spriteSheetManager = new SpriteSheetManager(tf2Folder, frameFolder);

         tf2FolderTextBox.Text = tf2Folder;
         frameTextBox.Text = frameFolder;
         prefixTextBox.Text = appSettings.Prefix;
         vtfNameTextBox.Text = appSettings.FileName;

      }
      private void SelectTf2Folder_Click(object sender, RoutedEventArgs e)
      {
         string selectedPath = OpenFolderDialog();
         if (!string.IsNullOrEmpty(selectedPath))
         {
            spriteSheetManager.SetTf2Folder(selectedPath);
            appSettings.TeamFortressFolder = selectedPath;
            appSettings.Save();
            tf2FolderTextBox.Text = selectedPath;
         }
      }
      private void SelectFrameFolder_Click(object sender, RoutedEventArgs e)
      {
         string selectedPath = OpenFolderDialog();
         if (!string.IsNullOrEmpty(selectedPath))
         {
            spriteSheetManager.SetFrameFolder(selectedPath);
            appSettings.FrameFolder = selectedPath;
            appSettings.Save();
            frameTextBox.Text = selectedPath;
         }
      }
      private string OpenFolderDialog()
      {
         var folderDialog = new OpenFolderDialog
         {
            Title = "Select Team Fortress 2 folder",
            ValidateNames = false,
            AddToRecent = true,
         };

         if (folderDialog.ShowDialog() == true)
         {
            var selectedPath = folderDialog.FolderName;
            return selectedPath;
         }

         return null;
      }
      private void Tf2FolderTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
      {
         spriteSheetManager.SetTf2Folder(tf2FolderTextBox.Text);
         appSettings.TeamFortressFolder = tf2FolderTextBox.Text;
         appSettings.Save();
      }
      private void FrameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
      {
         spriteSheetManager.SetFrameFolder(frameTextBox.Text);
         appSettings.FrameFolder = frameTextBox.Text;
         appSettings.Save();
      }
      private void CreateMKSFile_Click(object sender, RoutedEventArgs e)
      {
         string prefix = prefixTextBox.Text;
         string fileName = spriteSheetManager.CleanFileName(vtfNameTextBox.Text);
         bool splitSequences = (bool)splitCheckBox.IsChecked;
         bool loop = (bool)loopCheckBox.IsChecked;

         string result = spriteSheetManager.CreateMKSFile(prefix, fileName, splitSequences, loop);
         CreateVTFButton.IsEnabled = true;
         Console.WriteLine(result);
      }
      private void CreateVTFFile_Click(object sender, RoutedEventArgs e)
      {
         spriteSheetManager.SetFileName(vtfNameTextBox.Text);
         string result = spriteSheetManager.CreateVTFFile(appSettings.VTEXConfig);
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
         appSettings.Prefix = prefixTextBox.Text;
         appSettings.Save();
         prefixTextBox.Text = CleanFileName(prefixTextBox.Text);
         prefixTextBox.SelectionStart = prefixTextBox.Text.Length;
      }
      private void NameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
      {
         appSettings.FileName = vtfNameTextBox.Text;
         appSettings.Save();
         vtfNameTextBox.Text = CleanFileName(vtfNameTextBox.Text);
         vtfNameTextBox.SelectionStart = vtfNameTextBox.Text.Length;
      }
      public string CleanFileName(string name)
      {
         return string.Concat(name.Split(System.IO.Path.GetInvalidFileNameChars()));
      }
      private void OpenVtexConfig_Click(object sender, RoutedEventArgs e)
      {
         var vtexConfigWindow = new VTEXConfigWindow(appSettings.VTEXConfig);

         if (vtexConfigWindow.ShowDialog() == true)
         {
            appSettings.VTEXConfig = vtexConfigWindow.GetTextBoxContent();
            appSettings.Save(); 
         }
      }
   }
}
