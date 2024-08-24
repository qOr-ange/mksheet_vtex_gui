using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using TGASharpLib;
namespace ValveSpriteSheetUtil
{
   public class SpriteSheetManager
   {
      private string tf2Folder;
      private string frameFolder;
      private string fileName;
      private string mksFile;
      private List<string> frames = new List<string>();

      public SpriteSheetManager(string tf2Folder, string frameFolder)
      {
         this.tf2Folder = tf2Folder;
         this.frameFolder = frameFolder;

      }

      public void SetFileName(string name)
      {
         fileName = name;
      }
      public void SetTf2Folder(string path)
      {
         tf2Folder = path;
      }
      public void SetFrameFolder(string path)
      {
         frameFolder = path;
      }
      public string CleanFileName(string name)
      {
         return string.Concat(name.Split(Path.GetInvalidFileNameChars()));
      }
      public string CreateMKSFile(string prefix, string fileName, bool splitSequences, bool loop)
      {
         if (string.IsNullOrEmpty(fileName))
            return "Please enter a desired name for your VTF.";

         if (!Directory.Exists(tf2Folder))
            return $"{tf2Folder} is not a valid directory.";

         if (!Directory.Exists(frameFolder))
            return $"{frameFolder} is not a valid directory.";

         // Find and filter frames
         string[] files = Directory.GetFiles(frameFolder);
         frames.Clear();
         foreach (var file in files)
         {
            if (Path.GetFileName(file).StartsWith(prefix) && Path.GetExtension(file).ToLower().Equals(".tga"))
            {
               if (Path.GetFileName(file).Contains(" "))
                  return "Error: mksheet.exe breaks when frame names contain spaces. Rename and try again.";

               frames.Add(Path.GetFileName(file));
            }
         }

         if (frames.Count == 0)
            return $"No frames with prefix {prefix} found.";

         // Custom sorting by numeric suffix
         frames.Sort((x, y) =>
         {
            // Extract numeric suffix from file names
            int GetNumericSuffix(string fileName)
            {
               var parts = fileName.Split('_');
               var lastPart = parts.Last();
               return int.TryParse(Path.GetFileNameWithoutExtension(lastPart), out int result) ? result : 0;
            }

            return GetNumericSuffix(x).CompareTo(GetNumericSuffix(y));
         });

         // Create MKS file
         mksFile = Path.Combine(frameFolder, $"{fileName}.mks");
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

            mksContent.Append($"frame {frames[i]} 1"); // Append the frame without a newline

            if (i < frames.Count - 1) // Add a new line only if this isn't the last frame
            {
               mksContent.AppendLine();
            }
         }

         File.WriteAllText(mksFile, mksContent.ToString(), new UTF8Encoding(false));

         return $"Done! {fileName}.mks created. Make any changes now.";
      }
      public string CreateVTFFile(string vtexParams)
      {
         if (frames == null || frames.Count == 0 || string.IsNullOrEmpty(mksFile))
            return "MKS file or frames are not properly initialized.";

         string tf2Bin = Path.Combine(tf2Folder, "bin", "x64");

         try
         {

            //Copy/move all relevant files to /bin
            foreach (string frame in frames)
            {
               File.Copy(frameFolder + "\\" + frame, tf2Bin + "\\" + frame, true);
               Console.WriteLine($"copied: {frameFolder + "\\" + frame} to: {tf2Bin + "\\" + frame}");
            }
            File.Move(mksFile, tf2Bin + "\\" + fileName + ".mks", true);

            //Create SHT and TGA
            var processStartInfo = new System.Diagnostics.ProcessStartInfo();
            processStartInfo.WorkingDirectory = tf2Folder + "\\bin\\x64\\";
            processStartInfo.FileName = "cmd.exe";
            processStartInfo.Arguments = "/C mksheet.exe " + fileName + ".mks";
            System.Diagnostics.Process mksheet = System.Diagnostics.Process.Start(processStartInfo);
            mksheet.WaitForExit();


            if (File.Exists(tf2Bin + "\\" + fileName + ".sht"))
            {
               Console.WriteLine(".sht file created successfully!");
            }
            else
            {
               Console.WriteLine($"mksheet.exe exited with: {mksheet.ExitCode}");
               Console.WriteLine(tf2Bin);

               if (!File.Exists(tf2Bin + "\\" + fileName + ".mks"))
               {
                  Console.WriteLine(".mks file not found!");
               }
            }



            //Delete files that are no longer needed
            foreach (string frame in frames)
            {
               File.Delete(tf2Bin + "\\" + frame);
            }
            File.Delete(tf2Bin + "\\" + fileName + ".mks");

            //If materialsrc does not yet exist, create it
            if (!File.Exists(tf2Folder + "\\tf\\materialsrc"))
            {
               _ = Directory.CreateDirectory(tf2Folder + "\\tf\\materialsrc");
            }
            
            //Move tga and sht files to materialsrc
            File.Move(tf2Bin +"\\"+ fileName + ".sht", tf2Folder + "\\tf\\materialsrc\\" + fileName + ".sht");
            File.Move(tf2Bin +"\\"+ fileName + ".tga", tf2Folder + "\\tf\\materialsrc\\" + fileName + ".tga");
            
            return CreateVTFFromSHT(tf2Bin, vtexParams);
         }
         catch (Exception ex)
         {
            return ex.Message;
         }
      }
      private string CreateVTFFromSHT(string tf2Bin, string vtexParams)
      {
         if (vtexParams.Length > 0)
         {
            File.WriteAllText(tf2Folder + "\\tf\\materialsrc\\" + fileName + ".txt", vtexParams, new UTF8Encoding(false));
         }
         var processInfo = new ProcessStartInfo("cmd.exe")
         {
            WorkingDirectory = Environment.GetEnvironmentVariable("SYSTEMROOT"),
            Arguments = $"/C set VGAME={tf2Folder} & set VPROJECT={Path.Combine(tf2Folder, "tf")} & \"{Path.Combine(tf2Bin, "vtex.exe")}\" -nopause \"{Path.Combine(tf2Folder, "tf", "materialsrc", $"{fileName}.sht")}\"",
            CreateNoWindow = true,
            UseShellExecute = false
         };
         var vtexProcess = Process.Start(processInfo);
         vtexProcess.WaitForExit();

         // Clean up files
         File.Delete(Path.Combine(tf2Folder, "tf", "materialsrc", $"{fileName}.sht"));
         File.Delete(Path.Combine(tf2Folder, "tf", "materialsrc", $"{fileName}.tga"));
         if (vtexParams.Length > 0)
         {
            File.Delete(tf2Folder + "\\tf\\materialsrc\\" + fileName + ".txt");
         }

         string vtfLocation = Path.Combine(frameFolder, $"{fileName}.vtf");
         if (File.Exists(vtfLocation))
         {
            File.Delete(vtfLocation);
         }
         File.Move(Path.Combine(tf2Folder, "tf", "materials", $"{fileName}.vtf"), vtfLocation);

         return $"Done! {fileName}.vtf created.";
      }
      public string ConvertPngToTga(string prefix)
      {
         string[] files = Directory.GetFiles(frameFolder);

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
         var vmtPath = Path.Combine(frameFolder, $"{fileName}.vmt");

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
