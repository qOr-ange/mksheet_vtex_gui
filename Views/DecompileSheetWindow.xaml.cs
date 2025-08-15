using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using ValveSpriteSheetUtil.Util;
using VTFSheetDecompilerLib;

namespace ValveSpriteSheetUtil.Views {
   public partial class DecompileSheetWindow : Window {
      public DecompileSheetWindow() {
         InitializeComponent();
         Title = "Decompile VTF Sheet";
      }

      private void BrowseVtfButton_Click(object sender, RoutedEventArgs e) {
         var openFileDialog = new OpenFileDialog();
         openFileDialog.Filter = "VTF Files (*.vtf)|*.vtf|All Files (*.*)|*.*";
         if (openFileDialog.ShowDialog() == true) {
            VtfPathTextBox.Text = openFileDialog.FileName;
            string vtfDirectory = Path.GetDirectoryName(openFileDialog.FileName);
            if (vtfDirectory != null) {
               OutputPathTextBox.Text = Path.Combine(vtfDirectory, "_output");
            }
         }
      }

      private void BrowseOutputButton_Click(object sender, RoutedEventArgs e) {
         // Use the provided IOHelper.OpenFolderDialog method.
         string folderPath = IOHelper.OpenFolderDialog();
         if (!string.IsNullOrEmpty(folderPath)) {
            OutputPathTextBox.Text = folderPath;
         }
      }

      private void DecompileButton_Click(object sender, RoutedEventArgs e) {
         string vtfPath = VtfPathTextBox.Text;
         string outputPath = OutputPathTextBox.Text;

         if (string.IsNullOrWhiteSpace(vtfPath) || !File.Exists(vtfPath)) {
            System.Windows.MessageBox.Show("Please select a valid VTF file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
         }

         if (string.IsNullOrWhiteSpace(outputPath)) {
            System.Windows.MessageBox.Show("Please select a valid output directory.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
         }

         try {
            byte[] rawSheetData = VTFSheetDecompiler.DumpVtfSheetResource(vtfPath);
            if (rawSheetData == null) {
               System.Windows.MessageBox.Show("Could not find a sprite sheet resource in the VTF file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
               return;
            }

            string mksFilePath = GenerateMksCheckBox.IsChecked == true ? Path.Combine(outputPath, "reconstructed.mks") : null;
            var sequences = VTFSheetDecompiler.ProcessVtfSheetResource(rawSheetData, mksFilePath);

            if (PngRadioButton.IsChecked == true) {
               VTFSheetDecompiler.ExtractPNGSpritesFromAtlas(vtfPath, sequences, Path.Combine(outputPath, "sprites_png"));
            } else {
               VTFSheetDecompiler.ExtractTGASpritesFromAtlas(vtfPath, sequences, Path.Combine(outputPath, "sprites_tga"));
            }

            System.Windows.MessageBox.Show("Decompilation successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
         } catch (Exception ex) {
            System.Windows.MessageBox.Show($"An error occurred during decompilation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
         }
      }

      private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
   }
}
