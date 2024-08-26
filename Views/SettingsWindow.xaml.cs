using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ValveSpriteSheetUtil.Util;

namespace ValveSpriteSheetUtil.Views
{
   public partial class SettingsWindow : Window
   {
      private bool _savesettings = false;
      public SettingsWindow()
      {
         InitializeComponent();
         Title = "Settings";
         this.Loaded += (sender, e) =>
         {
            VTEXTemplatesPathTextBox.Text = AppSettingsHelper.GetSetting(x => x.VTEXTemplates);
            TextEditorPathTextBox.Text = AppSettingsHelper.GetSetting(x => x.TextEditorPath);
            TMPLDontShowAgainCheckBox.IsChecked = AppSettingsHelper.GetSetting(x => x.tmlpDontAskAgain);
            UseDefaulttextEditorCheckBox.IsChecked = AppSettingsHelper.GetSetting(x => x.UseDefTextEditor);
         }; 

         this.Closing += (sender, e) => 
         {
            if (_savesettings) 
            {
               AppSettingsHelper.SetSetting(x => x.VTEXTemplates, VTEXTemplatesPathTextBox.Text);
               AppSettingsHelper.SetSetting(x => x.TextEditorPath, TextEditorPathTextBox.Text);
               AppSettingsHelper.SetSetting(x => x.tmlpDontAskAgain, (bool)TMPLDontShowAgainCheckBox.IsChecked);
               AppSettingsHelper.SetSetting(x => x.UseDefTextEditor, (bool)UseDefaulttextEditorCheckBox.IsChecked);
               AppSettingsHelper.SaveSettings();
            }
         };
      }

      private void SaveButton_Click(object sender, RoutedEventArgs e)
      {
         _savesettings = true;
         Close();
      }

      private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
   }
}
