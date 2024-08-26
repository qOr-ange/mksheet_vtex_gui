using System.Linq.Expressions;
using System.Windows;
using ValveSpriteSheetUtil.Util;
using ValveSpriteSheetUtil.Views;
using ValveSpriteSheetUtil.Views.UserControls;

namespace ValveSpriteSheetUtil.Util
{
   // base DialogWindow size
   // Width="500"
   // Height="300"
   internal static class MessageBoxUtils
   {
      public static bool? ShowSelectPathDialog<T>(
         string message,
         Expression<Func<AppSettings, T>> dontAskAgainProperty)
      {
         var dialog = new TemplatesFolderDialog();
         dialog.SetMessage(message);

         bool dontAskAgainClicked = false;

         var window = new DialogWindow("Select Path", dialog, 500, 125);

         dialog.AddButton("Yes", null, (s, e) =>
         {
            window.DialogResult = true;
            window.Close();
         });

         dialog.AddButton("No", null, (s, e) =>
         {
            window.DialogResult = false;
            window.Close();
         });

         dialog.AddButton("Don't ask again", 110, (s, e) =>
         {
            // Use the generic SetSetting method with the specified type
            var settingValue = default(T);
            AppSettingsHelper.SetSetting(dontAskAgainProperty, settingValue);
            dontAskAgainClicked = true;
            window.DialogResult = false;  // To match the original intent of returning false
            window.Close();
         });

         window.ShowDialog();

         return dontAskAgainClicked ? false : window.DialogResult;
      }


      public static bool? ShowPermissionRequiredDialog()
      {
         var dialog = new TemplatesFolderDialog();
         dialog.SetMessage("The application needs to create vtexTemplates.json in the selected folder. This action may be blocked by Windows Defender Controlled Folder Access.\n\nIf blocked, you may need to allow the application through Windows Defender settings.\n\nDo you want to proceed?");

         bool dontAskAgainClicked = false;

         var window = new DialogWindow("Permission Required", dialog, 500, 300);

         dialog.AddButton("Yes", null, (s, e) =>
         {
            window.DialogResult = true;
            window.Close();
         });

         dialog.AddButton("No", null, (s, e) =>
         {
            window.DialogResult = false;
            window.Close();
         });

         window.ShowDialog();

         return dontAskAgainClicked ? false : window.DialogResult;
      }
      public static void ShowAccessDeniedDialog()
      {
         var dialog = new TemplatesFolderDialog();
         dialog.SetMessage("Failed to create the template file. This might have been blocked by Windows Defender's Controlled Folder Access.\n\nPlease allow this application through Controlled Folder Access settings or choose a different location.");

         var window = new DialogWindow("Access Denied", dialog, 500, 300);

         dialog.AddButton("OK", null, (s, e) =>
         {
            window.DialogResult = true;
            window.Close();
         });

         window.ShowDialog();
      }
      public static void ShowErrorDialog(string errorMessage)
      {
         var dialog = new TemplatesFolderDialog();
         dialog.SetMessage($"Failed to create the template file: {errorMessage}");

         var window = new DialogWindow("Error", dialog, 500, 200);

         dialog.AddButton("OK", null, (s, e) =>
         {
            window.DialogResult = true;
            window.Close();
         });

         window.ShowDialog();
      }
   }
}
