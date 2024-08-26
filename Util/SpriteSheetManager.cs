using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using TGASharpLib;
using ValveSpriteSheetUtil.Util;
namespace ValveSpriteSheetUtil
{
   public class SpriteSheetManager
   {
      public string fileName { get; set; }
      public string mksFile { get; set; }

      private List<string> frames {  get; set; } = new List<string>();

      public SpriteSheetManager() 
      {
         
      }
      public string CleanFileName(string name)
      {
         return string.Concat(name.Split(Path.GetInvalidFileNameChars()));
      }
      public string CreateMKSFile(string prefix, string fileName, bool splitSequences, bool loop)
      {
         using (MKSFileHandler handler = new MKSFileHandler())
         {
            (mksFile, frames) = handler.CreateMKSFile(prefix, fileName, splitSequences, loop);
         }

         return $"Done! {fileName}.mks created. Make any changes now.";
      }

      public void OpenMKSFileForEditing()
      {
         if (string.IsNullOrEmpty(AppSettingsHelper.GetSetting(x => x.TextEditorPath)) && AppSettingsHelper.GetSetting(x => x.UseDefTextEditor) == false)
         {
            bool? result = MessageBoxUtils.ShowSelectPathDialog("No path provided for Text Editor, do you want to select a specific text editor or try to use the default text editor?",
               x => x.UseDefTextEditor); // specific setting for don't ask again option.

            if (result == true)
            {
               string selectedPath = IOHelper.OpenFileDialog(IOHelper.FilterType.ExecutableFiles);
               if (!string.IsNullOrEmpty(selectedPath))
               {
                  AppSettingsHelper.SetSetting(x => x.TextEditorPath, selectedPath);
               }
            }

            if (result == false)
            {
               AppSettingsHelper.SetSetting(x => x.UseDefTextEditor, true);
            }
         }

         try
         {
            Console.WriteLine("Save & Close text editor to continue.");

            Process process = new Process();

            if (!AppSettingsHelper.GetSetting(x => x.UseDefTextEditor))
            {
               process.StartInfo = new ProcessStartInfo()
               {
                  FileName = AppSettingsHelper.GetSetting(x => x.TextEditorPath),
                  Arguments = mksFile,
                  UseShellExecute = true
               };
            }
            else
            {
               process.StartInfo = new ProcessStartInfo()
               {
                  FileName = mksFile,
                  UseShellExecute = true
               };
            }

            process.Start();
            process.WaitForExit();

            string postEditValidationResult = ValidateMKSFile();
            if (!string.IsNullOrEmpty(postEditValidationResult))
            {
               Console.WriteLine($"Validation failed: {postEditValidationResult}");
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine($"Couldn't open {mksFile} in text editor. Error: {ex.Message}");
         }
      }


      private string ValidateMKSFile()
      {
         if (!File.Exists(mksFile))
            return "Error: The .mks file does not exist after editing.";

         // Add more validation checks here if needed.
         // Example: Validate the content of the .mks file

         return null;
      }


      public string CreateVTFFile()
      {
            ConsoleLog.WriteLine($"Frames: {frames.Count}", Status.Info);
         if (frames == null || frames.Count == 0 || string.IsNullOrEmpty(mksFile))
            return "MKS file or frames are not properly initialized.";

         string tf2Bin = Path.Combine(AppSettingsHelper.GetSetting(x => x.TeamFortressFolder), "bin", "x64");

         try
         {
            //Copy/move all relevant files to /bin
            foreach (string frame in frames)
            {
               File.Copy(AppSettingsHelper.GetSetting(x => x.FrameFolder) + "\\" + frame, tf2Bin + "\\" + frame, true);
            }
            File.Move(mksFile, tf2Bin + "\\" + fileName + ".mks", true);

            if (File.Exists(tf2Bin + "\\" + fileName + ".sht"))
            {
               File.Delete(tf2Bin + "\\" + fileName + ".sht");
            }

            
            //Create SHT and TGA
            var processStartInfo = new System.Diagnostics.ProcessStartInfo();
            processStartInfo.WorkingDirectory = tf2Bin;
            processStartInfo.FileName = "cmd.exe";
            processStartInfo.Arguments = "/C mksheet.exe " + fileName + ".mks";
            System.Diagnostics.Process mksheet = System.Diagnostics.Process.Start(processStartInfo);
            mksheet.WaitForExit();


            if (File.Exists(tf2Bin + "\\" + fileName + ".sht"))
            {
               ConsoleLog.WriteLine($"{fileName}.sht file created successfully!", Status.Info);
            }
            else
            {
               Console.WriteLine($"mksheet.exe exited with: {mksheet.ExitCode}");
               Console.WriteLine(tf2Bin);

               if (!File.Exists(tf2Bin + "\\" + fileName + ".mks"))
               {
                  ConsoleLog.WriteLine($"{fileName}.mks file not found!", Status.Error);
               }
            }

            //Delete files that are no longer needed
            foreach (string frame in frames)
            {
               File.Delete(tf2Bin + "\\" + frame);
            }
            File.Delete(tf2Bin + "\\" + fileName + ".mks");

            //If materialsrc does not yet exist, create it
            if (!File.Exists(AppSettingsHelper.GetSetting(x => x.TeamFortressFolder) + "\\tf\\materialsrc"))
            {
               _ = Directory.CreateDirectory(AppSettingsHelper.GetSetting(x => x.TeamFortressFolder) + "\\tf\\materialsrc");
            }
            
            //Move tga and sht files to materialsrc
            File.Move(tf2Bin +"\\"+ fileName + ".sht", AppSettingsHelper.GetSetting(x => x.TeamFortressFolder) + "\\tf\\materialsrc\\" + fileName + ".sht");
            File.Move(tf2Bin +"\\"+ fileName + ".tga", AppSettingsHelper.GetSetting(x => x.TeamFortressFolder) + "\\tf\\materialsrc\\" + fileName + ".tga");

            ConsoleLog.WriteLine("Building VTF file, please wait, this may take a while...", Status.Info);
            return CreateVTFFromSHT(tf2Bin, AppSettingsHelper.GetSetting(x => x.VTEXConfig));
         }
         catch (Exception ex)
         {
            return ex.Message;
         }
      }
      private string CreateVTFFromSHT(string tf2Bin, string vtexParams)
      {
         string vtfLocation = Path.Combine(AppSettingsHelper.GetSetting(x => x.FrameFolder), $"{fileName}.vtf");
         if (File.Exists(vtfLocation))
         {
            File.Delete(vtfLocation);
         }
         if (vtexParams.Length > 0)
         {
            File.WriteAllText(AppSettingsHelper.GetSetting(x => x.TeamFortressFolder) + "\\tf\\materialsrc\\" + fileName + ".txt", vtexParams, new UTF8Encoding(false));
         }
         var processInfo = new ProcessStartInfo("cmd.exe")
         {
            WorkingDirectory = Environment.GetEnvironmentVariable("SYSTEMROOT"),
            Arguments = $"/C set VGAME={AppSettingsHelper.GetSetting(x => x.TeamFortressFolder)} & set VPROJECT={Path.Combine(AppSettingsHelper.GetSetting(x => x.TeamFortressFolder), "tf")} & \"{Path.Combine(tf2Bin, "vtex.exe")}\" -nopause \"{Path.Combine(AppSettingsHelper.GetSetting(x => x.TeamFortressFolder), "tf", "materialsrc", $"{fileName}.sht")}\"",
            CreateNoWindow = true,
            UseShellExecute = false
         };
         var vtexProcess = Process.Start(processInfo);
         vtexProcess.WaitForExit();

         // Clean up files
         File.Delete(Path.Combine(AppSettingsHelper.GetSetting(x => x.TeamFortressFolder), "tf", "materialsrc", $"{fileName}.sht"));
         File.Delete(Path.Combine(AppSettingsHelper.GetSetting(x => x.TeamFortressFolder), "tf", "materialsrc", $"{fileName}.tga"));
         if (vtexParams.Length > 0)
         {
            File.Delete(AppSettingsHelper.GetSetting(x => x.TeamFortressFolder) + "\\tf\\materialsrc\\" + fileName + ".txt");
         }

         if (File.Exists(vtfLocation))
         {
            File.Delete(vtfLocation);
         }
         File.Move(Path.Combine(AppSettingsHelper.GetSetting(x => x.TeamFortressFolder), "tf", "materials", $"{fileName}.vtf"), vtfLocation);

         return $"Done! {fileName}.vtf created.";
      }
      public string ConvertPngToTga(string prefix)
      {
         string[] files = Directory.GetFiles(AppSettingsHelper.GetSetting(x => x.FrameFolder));

         foreach (var file in files)
         {
            if (Path.GetFileName(file).StartsWith(prefix) && Path.GetExtension(file).ToLower().Equals(".png"))
            {
               using Bitmap clone = new Bitmap(file);
               using Bitmap newbmp = clone.Clone(new Rectangle(0, 0, clone.Width, clone.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
               TGA tga = (TGA)newbmp;
               tga.Save(Path.Combine(Path.GetDirectoryName(file), $"{Path.GetFileNameWithoutExtension(file)}.tga"));
            }
         }

         return "Conversion complete.";
      }
      public string CreateVMTFile(string fileName, bool blendFrames, bool depthBlend, bool additive)
      {
         var vmtPath = Path.Combine(AppSettingsHelper.GetSetting(x => x.FrameFolder), $"{fileName}.vmt");

         using FileStream fs = File.Create(vmtPath);
         fs.Write(Encoding.UTF8.GetBytes($"\"SpriteCard\"\r\n{{\r\n"));
         fs.Write(Encoding.UTF8.GetBytes($"\t\"$basetexture\" \"{fileName}\"\r\n"));
         fs.Write(Encoding.UTF8.GetBytes($"\t\"$blendframes\" {(blendFrames ? "1" : "0")}\r\n"));
         fs.Write(Encoding.UTF8.GetBytes($"\t\"$depthblend\" {(depthBlend ? "1" : "0")}\r\n"));
         fs.Write(Encoding.UTF8.GetBytes($"\t\"$additive\" {(additive ? "1" : "0")}\r\n}}"));

         return $"Done! {fileName}.vmt created.";
      }
   }
}
