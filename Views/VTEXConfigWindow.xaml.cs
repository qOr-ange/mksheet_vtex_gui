using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ValveSpriteSheetUtil.Util;

namespace ValveSpriteSheetUtil
{
   public partial class VTEXConfigWindow : Window
   {
      VtexTemplateHelper vtexTemplateHelper = new VtexTemplateHelper();
      private static readonly Dictionary<string, (string defaultValue, Func<string, bool> validate)> ValidParamsDict = new()
        {
            { "allmips", (null, ValidateBooleanFlag) },
            { "alphatest", (null, ValidateBooleanFlag) },
            { "alphatest_hifreq_threshhold", (null, ValidateAlphaThreshold) },
            { "alphatest_threshhold", (null, ValidateAlphaThreshold) },
            { "alphatodistance", (null, ValidateBooleanFlag) },
            { "anisotropic", (null, ValidateBooleanFlag) },
            { "bumpscale", (null, ValidateNonNegativeFloat) },
            { "clamps", (null, ValidateBooleanFlag) },
            { "clampt", (null, ValidateBooleanFlag) },
            { "clampu", (null, ValidateBooleanFlag) },
            { "distancespread", (null, ValidateAlphaThreshold) },
            { "dudv", (null, ValidateBooleanFlag) },
            { "dxt5", ("1", ValidateBooleanFlag) },
            { "invertgreen", (null, ValidateBooleanFlag) },
            { "manualmip", (null, ValidateBooleanFlag) },
            { "maxheight", ("1024", ValidatePositiveInteger) },
            { "maxheight_360", ("1024", ValidatePositiveInteger) },
            { "maxwidth", ("1024", ValidatePositiveInteger) },
            { "maxwidth_360", ("1024", ValidatePositiveInteger) },
            { "mipblend", (null, ValidateMipBlend) },
            { "nocompress", (null, ValidateBooleanFlag) },
            { "nodebug", (null, ValidateBooleanFlag) },
            { "nolod", ("1", ValidateBooleanFlag) },
            { "nomip", ("1", ValidateBooleanFlag) },
            { "nonice", ("1", ValidateBooleanFlag) },
            { "normal", (null, ValidateBooleanFlag) },
            { "normalalphatodudvluminance", (null, ValidateBooleanFlag) },
            { "normaltodudv", (null, ValidateBooleanFlag) },
            { "numchannels", ("4", ValidateNumChannels) },
            { "oneovermiplevelinalpha", (null, ValidateAlphaThreshold) },
            { "pfm", (null, ValidateBooleanFlag) },
            { "pfmscale", ("1.0", ValidateNonNegativeFloat) },
            { "pointsample", (null, ValidateBooleanFlag) },
            { "premultcolorbyoneovermiplevel", (null, ValidateAlphaThreshold) },
            { "procedural", (null, ValidateBooleanFlag) },
            { "reduce", ("2", ValidatePowerOfTwo) },
            { "reducex", ("2", ValidatePowerOfTwo) },
            { "reducey", ("2", ValidatePowerOfTwo) },
            { "rendertarget", (null, ValidateBooleanFlag) },
            { "singlecopy", (null, ValidateBooleanFlag) },
            { "skybox", (null, ValidateBooleanFlag) },
            { "specvar", (null, ValidateBooleanFlag) },
            { "spheremap_negz", (null, ValidateBooleanFlag) },
            { "spheremap_z", (null, ValidateBooleanFlag) },
            { "spheremap_negy", (null, ValidateBooleanFlag) },
            { "spheremap_y", (null, ValidateBooleanFlag) },
            { "spheremap_negx", (null, ValidateBooleanFlag) },
            { "spheremap_x", (null, ValidateBooleanFlag) },
            { "ssbump", (null, ValidateBooleanFlag) },
            { "srgb", ("1", ValidateBooleanFlag) },
            { "startframe", ("0", ValidateNonNegativeInteger) },
            { "endframe", ("0", ValidateNonNegativeInteger) },
            { "stripalphachannel", (null, ValidateBooleanFlag) },
            { "stripcolorchannel", (null, ValidateBooleanFlag) },
            { "trilinear", (null, ValidateBooleanFlag) },
            { "volumetexture", (null, ValidateBooleanFlag) }
        };

      public VTEXConfigWindow()
      {
         InitializeComponent();
         Loaded += OnWindowLoaded;
         AutocompletePopup.IsOpen = false;
         InitializeTemplateComboBox();
      }

      private void InitializeTemplateComboBox()
      {
         vtexTemplateHelper.LoadTemplates(AppSettingsHelper.GetSetting(x => x.VTEXTemplates));
         foreach (var e in vtexTemplateHelper.VtexTemplates)
         {
            TemplateComboBox.Items.Add(e.Key);
         }
      }
      private void TemplateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         if (vtexTemplateHelper.VtexTemplates.TryGetValue((string)TemplateComboBox.SelectedItem, out string value))
         {
            InputTextBox.Text = value;
         }
      }
      private void OnAddTemplateClick(object sender, RoutedEventArgs e)
      {
         var newTemplateName = NewTemplateTextBox.Text.Trim();
         if (string.IsNullOrEmpty(newTemplateName))
         {
            MessageBox.Show("Please enter a name for the new template.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
         }

         vtexTemplateHelper.AddTemplate(newTemplateName, InputTextBox.Text);
         vtexTemplateHelper.SaveTemplates();

         TemplateComboBox.Items.Insert(TemplateComboBox.Items.Count - 1, new ComboBoxItem { Content = newTemplateName });
         TemplateComboBox.SelectedItem = newTemplateName;

         NewTemplateTextBox.Clear();
      }

      private void OnCancelClick(object sender, RoutedEventArgs e) => Close();
      private void OnSaveClick(object sender, RoutedEventArgs e)
      {
         string userInput = GetTextBoxContent();
         var (isValid, errorMessage) = ParseAndValidate(userInput);

         if (isValid)
         {
            AppSettingsHelper.SetSetting(x => x.VTEXConfig, userInput);
            DialogResult = true;
            Close();
         }
         else
         {
            MessageBox.Show($"Invalid input: {errorMessage}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
         }
      }
      private void OnWindowLoaded(object sender, RoutedEventArgs e) => LoadTextBoxContent();
      private void LoadTextBoxContent() => InputTextBox.Text = AppSettingsHelper.GetSetting(x => x.VTEXConfig);
      public string GetTextBoxContent() => InputTextBox.Text;
      private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e) => UpdateAutocomplete();
      private void UpdateAutocomplete()
      {
         int caretIndex = InputTextBox.CaretIndex;
         if (caretIndex < 0) return;

         string currentWord = GetCurrentLine();
         if (string.IsNullOrEmpty(currentWord))
         {
            AutocompletePopup.IsOpen = false;
            return;
         }

         var suggestions = ValidParamsDict.Keys
             .Where(key => key.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
             .ToList();

         if (suggestions.Any())
         {
            AutocompleteListBox.ItemsSource = suggestions;
            AutocompletePopup.IsOpen = true;
            PositionAutocompletePopup(caretIndex);
         }
         else
         {
            AutocompletePopup.IsOpen = false;
         }
      }
      private void PositionAutocompletePopup(int caretIndex)
      {
         var caretRect = InputTextBox.GetRectFromCharacterIndex(caretIndex);
         var transform = InputTextBox.TransformToAncestor(this);
         var popupTopLeft = transform.Transform(new Point(caretRect.Left, caretRect.Bottom));

         const double verticalOffsetAdjustment = 30;
         AutocompletePopup.HorizontalOffset = popupTopLeft.X;
         AutocompletePopup.VerticalOffset = popupTopLeft.Y - verticalOffsetAdjustment;

         double windowWidth = ActualWidth;
         double popupWidth = AutocompletePopup.ActualWidth;

         if (popupTopLeft.X + popupWidth > windowWidth)
         {
            AutocompletePopup.HorizontalOffset = windowWidth - popupWidth - 10;
         }
      }
      private string GetCurrentLine() => InputTextBox.GetLineText(InputTextBox.GetLineIndexFromCharacterIndex(InputTextBox.CaretIndex));
      private void OnAutocompleteSelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         if (AutocompleteListBox.SelectedItem is not string selectedItem) return;

         int caretIndex = InputTextBox.CaretIndex;
         if (caretIndex < 0) return;

         string defaultValue = ValidParamsDict.TryGetValue(selectedItem.ToLowerInvariant(), out var entry)
             ? entry.defaultValue
             : null;

         string textToInsert = string.IsNullOrEmpty(defaultValue)
             ? selectedItem
             : $"{selectedItem} {defaultValue}";

         ReplaceCurrentWordWithSelection(caretIndex, textToInsert);
      }
      private void ReplaceCurrentWordWithSelection(int caretIndex, string textToInsert)
      {
         int startOfRemoval = caretIndex;
         while (startOfRemoval > 0 && !char.IsWhiteSpace(InputTextBox.Text[startOfRemoval - 1]))
         {
            startOfRemoval--;
         }

         InputTextBox.Text = InputTextBox.Text.Remove(startOfRemoval, caretIndex - startOfRemoval) + textToInsert;
         InputTextBox.CaretIndex = startOfRemoval + textToInsert.Length;

         AutocompletePopup.IsOpen = false;
      }
      private (bool isValid, string errorMessage) ParseAndValidate(string input)
      {
         var lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

         foreach (var line in lines)
         {
            var parts = line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            string parameter = parts[0].ToLowerInvariant();
            if (!ValidParamsDict.ContainsKey(parameter))
            {
               return (false, $"Unknown parameter: {parameter}");
            }

            if (parts.Length > 1)
            {
               string value = parts[1];
               var validator = ValidParamsDict[parameter].validate;

               if (!validator(value))
               {
                  return (false, $"Invalid value '{value}' for parameter: {parameter}");
               }
            }
         }

         return (true, string.Empty);
      }

      #region Validation Methods

      private static bool ValidateBooleanFlag(string value) => value == "1" || value == "0";

      private static bool ValidateAlphaThreshold(string value) => float.TryParse(value, out float result) && result is >= 0f and <= 1f;

      private static bool ValidateNonNegativeFloat(string value) => float.TryParse(value, out float result) && result >= 0f;

      private static bool ValidatePositiveInteger(string value) => int.TryParse(value, out int result) && result > 0;

      private static bool ValidateNonNegativeInteger(string value) => int.TryParse(value, out int result) && result >= 0;

      private static bool ValidateMipBlend(string value) => value == "0" || value == "1";

      private static bool ValidateNumChannels(string value) => int.TryParse(value, out int result) && result is >= 1 and <= 4;

      private static bool ValidatePowerOfTwo(string value) => int.TryParse(value, out int result) && result is > 0 && (result & (result - 1)) == 0;

      #endregion

      
   }
}
