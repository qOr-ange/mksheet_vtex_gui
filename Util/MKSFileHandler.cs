using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ValveSpriteSheetUtil.Util
{
   internal class MKSFileHandler : IDisposable
   {
      private string mksFile;
      public (string mksFile, List<string> frames) CreateMKSFile(string prefix, string fileName, bool splitSequences, bool loop)
      {
         string validationResult = ValidateInput(prefix, fileName);
         if (!string.IsNullOrEmpty(validationResult))
            return (validationResult, null);

         List<string> frames = GetFilteredFrames(prefix);
         if (frames.Count == 0)
            return ($"No frames with prefix {prefix} found.", null);

         SortFramesByNumericSuffix(frames);

         CreateMKSFileContent(fileName, splitSequences, loop, frames);

         return (mksFile, frames);
      }
      private string ValidateInput(string prefix, string fileName)
      {
         if (string.IsNullOrEmpty(fileName))
            return "Please enter a desired name for your VTF.";

         if (!Directory.Exists(AppSettingsHelper.GetSetting(x => x.TeamFortressFolder)))
            return $"{AppSettingsHelper.GetSetting(x => x.TeamFortressFolder)} is not a valid directory.";

         if (!Directory.Exists(AppSettingsHelper.GetSetting(x => x.FrameFolder)))
            return $"{AppSettingsHelper.GetSetting(x => x.FrameFolder)} is not a valid directory.";

         return null;
      }
      private List<string> GetFilteredFrames(string prefix)
      {
         string[] files = Directory.GetFiles(AppSettingsHelper.GetSetting(x => x.FrameFolder));
         List<string> filteredFrames = new List<string>();

         bool warningLogged = false;

         foreach (var file in files)
         {
            string fileName = Path.GetFileName(file);
            string fileExtension = Path.GetExtension(file).ToLower();

            if (fileName.StartsWith(prefix) && fileExtension.Equals(".tga"))
            {
               string newFileName = fileName;

               if (fileName.Contains(" "))
               {
                  if (!warningLogged)
                  {
                     ConsoleLog.WriteLine("TGA names contain spaces. They will be replaced with underscores.", Status.Warning);
                     warningLogged = true;
                  }

                  newFileName = fileName.Replace(" ", "_");
                  string newFilePath = Path.Combine(Path.GetDirectoryName(file), newFileName);
                  File.Move(file, newFilePath);
               }

               filteredFrames.Add(newFileName);
            }
         }

         return filteredFrames;
      }


      private void SortFramesByNumericSuffix(List<string> frames)
      {
         frames.Sort((x, y) =>
         {
            int GetNumericSuffix(string fileName)
            {
               var parts = fileName.Split('_');
               var lastPart = parts.Last();
               return int.TryParse(Path.GetFileNameWithoutExtension(lastPart), out int result) ? result : 0;
            }

            return GetNumericSuffix(x).CompareTo(GetNumericSuffix(y));
         });
      }
      private void CreateMKSFileContent(string fileName, bool splitSequences, bool loop, List<string> frames)
      {
         mksFile = Path.Combine(AppSettingsHelper.GetSetting(x => x.FrameFolder), $"{fileName}.mks");
         var mksContent = new StringBuilder();

         for (int i = 0; i < frames.Count; i++)
         {
            if (i == 0)
            {
               mksContent.AppendLine("sequence 0");
               if (loop) mksContent.AppendLine("LOOP");
            }
            else if (splitSequences)
            {
               mksContent.AppendLine($"\r\nsequence {i}");
               if (loop) mksContent.AppendLine("LOOP");
            }

            mksContent.Append($"frame {frames[i]} 1");

            if (i < frames.Count - 1)
            {
               mksContent.AppendLine();
            }
         }

         File.WriteAllText(mksFile, mksContent.ToString(), new UTF8Encoding(false));
      }
      public void Dispose()
      {
         mksFile = null;
      }
   }
}
