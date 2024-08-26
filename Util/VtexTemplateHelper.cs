using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using ValveSpriteSheetUtil.Util;

internal class VtexTemplateHelper
{
   private readonly string _documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
   private readonly string _templatesFileName = "vtexTemplates.json";
   private string TemplatesFilePath => Path.Combine(_documentsPath, _templatesFileName);
   public Dictionary<string, string> VtexTemplates { get; set; } = new();

   public void LoadTemplates(string vtexTemplatespath)
   {
      if (string.IsNullOrEmpty(vtexTemplatespath))
         vtexTemplatespath = TemplatesPromtFolder();

      if (!File.Exists(vtexTemplatespath))
      {
         bool? result = MessageBoxUtils.ShowPermissionRequiredDialog();
         if (result == true)
         {
            try
            {
               var emptyJson = JsonSerializer.Serialize(new Dictionary<string, Dictionary<string, string>>(), new JsonSerializerOptions { WriteIndented = true });
               File.WriteAllText(vtexTemplatespath, emptyJson);
            }
            catch (UnauthorizedAccessException)
            {
               MessageBoxUtils.ShowAccessDeniedDialog();
               return;
            }
            catch (Exception ex)
            {
               MessageBoxUtils.ShowErrorDialog(ex.Message);
               return;
            }
         }
         else
         {
            return;
         }
      }

      try
      {
         string jsonContent = File.ReadAllText(vtexTemplatespath);
         var jsonTemplates = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonContent);

         VtexTemplates.Clear();
         foreach (var template in jsonTemplates)
         {
            var joinedParams = string.Join(Environment.NewLine, template.Value.Select(kv => $"{kv.Key} {kv.Value}"));
            VtexTemplates.Add(template.Key, joinedParams);
         }
      }
      catch (Exception ex)
      {
         MessageBoxUtils.ShowErrorDialog($"Failed to load templates: {ex.Message}");
      }
   }
   private string TemplatesPromtFolder()
   {
      bool? result = MessageBoxUtils.ShowSelectPathDialog("No path provided for vtexTemplates.json. Do you want to select a custom location?", 
         x => x.tmlpDontAskAgain);

      if (result == true)
      {
         string selectedPath = IOHelper.OpenFolderDialog();
         if (!string.IsNullOrEmpty(selectedPath))
         {
            AppSettingsHelper.SetSetting(x => x.VTEXTemplates, Path.Combine(selectedPath, "vtexTemplates.json"));
            return Path.Combine(selectedPath, "vtexTemplates.json");
         }
      }
      return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "vtexTemplates.json");
   }
   public void SaveTemplates()
   {
      try
      {
         var jsonTemplates = new Dictionary<string, Dictionary<string, string>>();

         foreach (var template in VtexTemplates)
         {
            var paramDict = template.Value
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries))
                .ToDictionary(parts => parts[0], parts => parts.Length > 1 ? parts[1] : string.Empty);

            jsonTemplates.Add(template.Key, paramDict);
         }

         string jsonContent = JsonSerializer.Serialize(jsonTemplates, new JsonSerializerOptions { WriteIndented = true });
         File.WriteAllText(TemplatesFilePath, jsonContent);
      }
      catch (Exception ex)
      {
         MessageBoxUtils.ShowErrorDialog($"Failed to save templates: {ex.Message}");
      }
   }
   public void AddTemplate(string name, string data)
   {
      if (VtexTemplates.ContainsKey(name))
      {
         VtexTemplates[name] = data;
      }
      else
      {
         VtexTemplates.Add(name, data);
      }
   }
}
