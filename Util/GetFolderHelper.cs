using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValveSpriteSheetUtil.Util
{
   public static class GetFolderHelper
   {
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
   }
}
