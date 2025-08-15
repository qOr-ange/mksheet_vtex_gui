using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using ValveSpriteSheetUtil.Util;
using ValveSpriteSheetUtil.Views;
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
      
      private void OpenMKSButton_Click(object sender, RoutedEventArgs e)
      {
         ToggleDimmerOverlay();
         spriteSheetManager.OpenMKSFileForEditing();
         ToggleDimmerOverlay();
      }
      private void SettingsButton_Click(object sender, RoutedEventArgs e)
      {
         var settings = new SettingsWindow();
         if (settings.ShowDialog() == true)
         {
         }
      }
      private async void HelpButton_Click(object sender, RoutedEventArgs e)
      {
         var assembly = Assembly.GetExecutingAssembly();
         var resourceName = "ValveSpriteSheetUtil.Views.Help.Help.html";
         var imageResourceName = "ValveSpriteSheetUtil.Views.Help.logo.png";

         try
         {
            string htmlContent;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
               htmlContent = reader.ReadToEnd();
            }

            string tempFolderPath = Path.GetTempPath();
            string tempHtmlPath = Path.Combine(tempFolderPath, "Help.html");
            string tempImagePath = Path.Combine(tempFolderPath, "logo.png");

            using (Stream imageStream = assembly.GetManifestResourceStream(imageResourceName))
            using (FileStream fileStream = new FileStream(tempImagePath, FileMode.Create, FileAccess.Write))
            {
               imageStream.CopyTo(fileStream);
            }

            string updatedHtmlContent = htmlContent.Replace("path-to-your-image/logo.png", tempImagePath);
            File.WriteAllText(tempHtmlPath, updatedHtmlContent);

            await Task.Run(() =>
            {
               Process helpInfo = new Process()
               {
                  StartInfo = new ProcessStartInfo(tempHtmlPath)
                  {
                     UseShellExecute = true

                  }
               };
               helpInfo.Start();
               helpInfo.WaitForExit();
               helpInfo.Exited += (sender, e) =>
               {
                  File.Delete(tempHtmlPath);
                  File.Delete(tempImagePath);
               };
            });
            
         }
         catch (Exception ex)
         {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
         }
      }
      private void DecompileSheetButton_Click(object sender, RoutedEventArgs e) {
         var decompileWindow = new DecompileSheetWindow();
         decompileWindow.ShowDialog();
      }
   }
}
