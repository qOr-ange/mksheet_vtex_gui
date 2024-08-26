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
      public string CleanFileName(string name) => string.Concat(name.Split(Path.GetInvalidFileNameChars()));
      
      public void CreateMKSFile(string prefix, string fileName, bool splitSequences, bool loop)
      {
         using (MKSFileHandler mks = new MKSFileHandler()) {
            (mksFile, frames) = mks.CreateMKSFile(prefix, fileName, splitSequences, loop);
         }
         ConsoleLog.WriteLine($"Done! {fileName}.mks created. Make any changes now.", Status.Success);
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
            if (postEditValidationResult != null) 
            { 
               ConsoleLog.WriteLine(postEditValidationResult, Status.Error);
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

         var lines = File.ReadAllLines(mksFile);

         bool inSequence = false;
         int sequenceCount = 0;

         foreach (var line in lines)
         {
            var trimmedLine = line.Trim();

            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//"))
               continue;

            if (trimmedLine.StartsWith("sequence"))
            {
               inSequence = true;
               sequenceCount++;
               continue;
            }

            if (trimmedLine.StartsWith("//&"))
            {
               ConsoleLog.WriteLine(trimmedLine.Substring(3).Trim(), Status.Info);
               continue;
            }

            if (inSequence && trimmedLine.StartsWith("frame"))
            {
               // Example: frame mymaterial1.tga 1
               var frameParts = trimmedLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
               if (frameParts.Length < 3)
                  return "Error: Invalid frame definition in sequence " + sequenceCount;
               continue;
            }

            if (inSequence && trimmedLine.StartsWith("loop", StringComparison.OrdinalIgnoreCase))
            {
               continue;
            }

            if (trimmedLine.StartsWith("sequence") && inSequence)
            {
               inSequence = false;
               continue;
            }

            if (trimmedLine.StartsWith("packmode"))
            {
               var validModes = new[] { "rgb", "rgb+a" };
               var mode = trimmedLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
               if (!validModes.Contains(mode))
                  return "Error: Invalid packmode defined.";
               continue;
            }

            return $"Error: Unexpected line format: {trimmedLine}";
         }

         if (sequenceCount == 0)
            return "Error: No sequence defined in the .mks file.";

         return null;
      }


      public void CreateVTFFile()
      {
         if (frames == null || frames.Count == 0 || string.IsNullOrEmpty(mksFile))
         {
            ConsoleLog.WriteLine($"Oops, Something went wrong!", Status.Error);
            if (frames == null)
            {
               ConsoleLog.WriteLine($"Frames were null.", Status.Warning);
            }
            if (frames.Count == 0)
            {
               ConsoleLog.WriteLine($"Frames count is 0", Status.Warning);
            }
            if (string.IsNullOrEmpty(mksFile))
            {
               ConsoleLog.WriteLine($"{fileName}.mks was empty.", Status.Warning);
            }
            return;
         }

         ConsoleLog.WriteLine($"Frames: {frames.Count}", Status.Info);

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
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            System.Diagnostics.Process mksheet = System.Diagnostics.Process.Start(processStartInfo);



            mksheet.OutputDataReceived += (sender, e) =>
            {
               if (!string.IsNullOrEmpty(e.Data))
               {
                  ConsoleLog.WriteLine("mksheet.exe | " + e.Data, Status.None);
               }
            };

            mksheet.ErrorDataReceived += (sender, e) =>
            {
               if (!string.IsNullOrEmpty(e.Data))
               {
                  ConsoleLog.WriteLine("mksheet.exe | " + e.Data, Status.Error);
               }
            };
            mksheet.BeginOutputReadLine();
            mksheet.BeginErrorReadLine();
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
            CreateVTFFromSHT(tf2Bin, AppSettingsHelper.GetSetting(x => x.VTEXConfig));
         }
         catch (Exception ex)
         {
            ConsoleLog.WriteLine(ex.Message, Status.Error);
         }
      }
      private void CreateVTFFromSHT(string tf2Bin, string vtexParams)
      {
         string outputFolder = Path.Combine(AppSettingsHelper.GetSetting(x => x.FrameFolder), "[output]");
         Directory.CreateDirectory(outputFolder);
         string vtfLocation = Path.Combine(outputFolder, $"{fileName}.vtf");



         if (vtexParams.Length > 0)
         {
            File.WriteAllText(AppSettingsHelper.GetSetting(x => x.TeamFortressFolder) + "\\tf\\materialsrc\\" + fileName + ".txt", vtexParams, new UTF8Encoding(false));
         }
         var processInfo = new ProcessStartInfo("cmd.exe")
         {
            WorkingDirectory = Environment.GetEnvironmentVariable("SYSTEMROOT"),
            Arguments = $"/C set VGAME={AppSettingsHelper.GetSetting(x => x.TeamFortressFolder)} & set VPROJECT={Path.Combine(AppSettingsHelper.GetSetting(x => x.TeamFortressFolder), "tf")} & \"{Path.Combine(tf2Bin, "vtex.exe")}\" -nopause \"{Path.Combine(AppSettingsHelper.GetSetting(x => x.TeamFortressFolder), "tf", "materialsrc", $"{fileName}.sht")}\"",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
         };

         var vtexProcess = Process.Start(processInfo);

         vtexProcess.OutputDataReceived += (sender, e) =>
         {
            if (!string.IsNullOrEmpty(e.Data))
            {
               ConsoleLog.WriteLine("vtex.exe | " + e.Data, Status.None);
            }
         };

         vtexProcess.ErrorDataReceived += (sender, e) =>
         {
            if (!string.IsNullOrEmpty(e.Data))
            {
               ConsoleLog.WriteLine("vtex.exe | " + e.Data, Status.Error);
            }
         };
         vtexProcess.BeginOutputReadLine();
         vtexProcess.BeginErrorReadLine();
         vtexProcess.WaitForExit();

         // Clean up files
         File.Delete(Path.Combine(AppSettingsHelper.GetSetting(x => x.TeamFortressFolder), "tf", "materialsrc", $"{fileName}.sht"));
         File.Delete(Path.Combine(AppSettingsHelper.GetSetting(x => x.TeamFortressFolder), "tf", "materialsrc", $"{fileName}.tga"));
         if (vtexParams.Length > 0)
         {
            File.Delete(AppSettingsHelper.GetSetting(x => x.TeamFortressFolder) + "\\tf\\materialsrc\\" + fileName + ".txt");
         }

         File.Move(Path.Combine(AppSettingsHelper.GetSetting(x => x.TeamFortressFolder), "tf", "materials", $"{fileName}.vtf"), vtfLocation, true);
         ConsoleLog.WriteLine($"Done! {fileName}.vtf created.", Status.Success);
         ConsoleLog.WriteLine($"Path: {vtfLocation}", Status.Info);


         OpenOutputFolder(outputFolder);
      }

      private void OpenOutputFolder(string outputFolder)
      {
         Process p = new Process()
         {
            StartInfo = new ProcessStartInfo()
            {
               FileName = outputFolder,
               UseShellExecute = true,
               Verb = "open"
            }
         };
         p.Start();
      }


      public void ConvertPngToTga(string prefix)
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

         ConsoleLog.WriteLine("Conversion complete.", Status.Success);
      }
      public void CreateVMTFile(string fileName, bool blendFrames, bool depthBlend, bool additive)
      {
         var vmtPath = Path.Combine(AppSettingsHelper.GetSetting(x => x.FrameFolder), $"{fileName}.vmt");

         using FileStream fs = File.Create(vmtPath);
         fs.Write(Encoding.UTF8.GetBytes($"\"SpriteCard\"\r\n{{\r\n"));
         fs.Write(Encoding.UTF8.GetBytes($"\t\"$basetexture\" \"{fileName}\"\r\n"));
         fs.Write(Encoding.UTF8.GetBytes($"\t\"$blendframes\" {(blendFrames ? "1" : "0")}\r\n"));
         fs.Write(Encoding.UTF8.GetBytes($"\t\"$depthblend\" {(depthBlend ? "1" : "0")}\r\n"));
         fs.Write(Encoding.UTF8.GetBytes($"\t\"$additive\" {(additive ? "1" : "0")}\r\n}}"));

         ConsoleLog.WriteLine($"Done! {fileName}.vmt created.", Status.Success);
      }
   }
}
