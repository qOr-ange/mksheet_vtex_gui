using Microsoft.Win32;

namespace ValveSpriteSheetUtil.Util
{
   public static class IOHelper
   {
      public enum FilterType
      {
         ExecutableFiles,
      }

      public static string OpenFolderDialog()
      {
         var folderDialog = new OpenFolderDialog
         {
            Title = "Select Folder.",
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

      private static string GetFilterString(FilterType filter)
      {
         return filter switch
         {
            FilterType.ExecutableFiles => "Executable Files (*.exe)|*.exe",
            _ => "All Files (*.*)|*.*",
         };
      }
      public static string OpenFileDialog(FilterType filter)
      {
         var folderDialog = new OpenFileDialog
         {
            Title = "Select a file.",
            Filter = GetFilterString(filter),
            ValidateNames = true,
            AddToRecent = true,
         };

         if (folderDialog.ShowDialog() == true)
         {
            var selectedPath = folderDialog.FileName;
            return selectedPath;
         }

         return null;
      }
   }
}
