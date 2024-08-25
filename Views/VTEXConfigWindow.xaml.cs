using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace ValveSpriteSheetUtil
{
   public partial class VTEXConfigWindow : Window
   {
      private string _vtexConfig;
      private static readonly Dictionary<string, (string defaultValue, Func<string, bool> validate)> ValidParamsDict = new()
      {
          { "allmips",                       (null, ValidateBooleanFlag) },
          { "alphatest",                     ( null, ValidateBooleanFlag) },
          { "alphatest_hifreq_threshhold",   (null, ValidateAlphaThreshold) },
          { "alphatest_threshhold",          (null, ValidateAlphaThreshold) },
          { "alphatodistance",               (null, ValidateBooleanFlag) },
          { "anisotropic",                   (null, ValidateBooleanFlag) },
          { "bumpscale",                     (null, ValidateNonNegativeFloat) },
          { "clamps",                        (null, ValidateBooleanFlag) },
          { "clampt",                        (null, ValidateBooleanFlag) },
          { "clampu",                        (null, ValidateBooleanFlag) },
          { "distancespread",                (null, ValidateAlphaThreshold) },
          { "dudv",                          (null, ValidateBooleanFlag) },
          { "dxt5",                          ("1", ValidateBooleanFlag) },
          { "invertgreen",                   (null, ValidateBooleanFlag) },
          { "manualmip",                     (null, ValidateBooleanFlag) },
          { "maxheight",                     ("1024", ValidatePositiveInteger) },
          { "maxheight_360",                 ("1024", ValidatePositiveInteger) },
          { "maxwidth",                      ("1024", ValidatePositiveInteger) },
          { "maxwidth_360",                  ("1024", ValidatePositiveInteger) },
          { "mipblend",                      (null, ValidateMipBlend) },
          { "nocompress",                    (null, ValidateBooleanFlag) },
          { "nodebug",                       (null, ValidateBooleanFlag) },
          { "nolod",                         ("1", ValidateBooleanFlag) },
          { "nomip",                         ("1", ValidateBooleanFlag) },
          { "nonice",                        ("1", ValidateBooleanFlag) },
          { "normal",                        (null, ValidateBooleanFlag) },
          { "normalalphatodudvluminance",    (null, ValidateBooleanFlag) },
          { "normaltodudv",                  (null, ValidateBooleanFlag) },
          { "numchannels",                   ("4", ValidateNumChannels) },
          { "oneovermiplevelinalpha",        (null, ValidateAlphaThreshold) },
          { "pfm",                           (null, ValidateBooleanFlag) },
          { "pfmscale",                      ("1.0", ValidateNonNegativeFloat) },
          { "pointsample",                   (null, ValidateBooleanFlag) },
          { "premultcolorbyoneovermiplevel", (null, ValidateAlphaThreshold) },
          { "procedural",                    (null, ValidateBooleanFlag) },
          { "reduce",                        ("2", ValidatePowerOfTwo) },
          { "reducex",                       ("2", ValidatePowerOfTwo) },
          { "reducey",                       ("2", ValidatePowerOfTwo) },
          { "rendertarget",                  (null, ValidateBooleanFlag) },
          { "singlecopy",                    (null, ValidateBooleanFlag) },
          { "skybox",                        (null, ValidateBooleanFlag) },
          { "specvar",                       (null, ValidateBooleanFlag) },
          { "spheremap_negz",                (null, ValidateBooleanFlag) },
          { "spheremap_z",                   (null, ValidateBooleanFlag) },
          { "spheremap_negy",                (null, ValidateBooleanFlag) },
          { "spheremap_y",                   (null, ValidateBooleanFlag) },
          { "spheremap_negx",                (null, ValidateBooleanFlag) },
          { "spheremap_x",                   (null, ValidateBooleanFlag) },
          { "ssbump",                        (null, ValidateBooleanFlag) },
          { "srgb",                          ("1", ValidateBooleanFlag) },
          { "startframe",                    ("0", ValidateNonNegativeInteger) },
          { "endframe",                      ("0", ValidateNonNegativeInteger) },
          { "stripalphachannel",             (null, ValidateBooleanFlag) },
          { "stripcolorchannel",             (null, ValidateBooleanFlag) },
          { "trilinear",                     (null, ValidateBooleanFlag) },
          { "volumetexture",                 (null, ValidateBooleanFlag) }
      };

      public VTEXConfigWindow(string vtexConfig)
      {
         _vtexConfig = vtexConfig;
         InitializeComponent();
         Loaded += OnWindowLoaded;
         AutocompletePopup.IsOpen = false;

         // TODO: set up stuffs to save templates
         TemplateComboBox.Items.Add("item 0");
         TemplateComboBox.Items.Add("item 1");
         TemplateComboBox.Items.Add("item 2");
         TemplateComboBox.Items.Add("item 3");
         TemplateComboBox.Items.Add("item 4");
      }

      private void OnCancelClick(object sender, RoutedEventArgs e)
      {
         Close();
      }
      private void OnSaveClick(object sender, RoutedEventArgs e)
      {
         string userInput = GetTextBoxContent();
         var (isValid, errorMessage) = ParseAndValidate(userInput);

         if (isValid)
         {
            _vtexConfig = userInput;
            DialogResult = true;
            Close();
         }
         else
         {
            MessageBox.Show($"Invalid input: {errorMessage}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
         }
      }

      private void OnWindowLoaded(object sender, RoutedEventArgs e)
      {
         LoadTextBoxContent();
      }
      private void LoadTextBoxContent()
      {
         InputTextBox.Text = _vtexConfig;
      }
      public string GetTextBoxContent()
      {
         return InputTextBox.Text;
      }

      private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
      {
         UpdateAutocomplete();
      }
      private void UpdateAutocomplete()
      {
         int caretIndex = InputTextBox.CaretIndex;
         if (caretIndex >= 0)
         {
            var currentWord = GetCurrentLine();
            if (currentWord != null) Console.WriteLine(currentWord);


            if (!string.IsNullOrEmpty(currentWord))
            {
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
            else
            {
               AutocompletePopup.IsOpen = false;
            }
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

         var windowWidth = this.ActualWidth;
         var popupWidth = AutocompletePopup.ActualWidth;

         if (popupTopLeft.X + popupWidth > windowWidth)
         {
            AutocompletePopup.HorizontalOffset = windowWidth - popupWidth - 10;
         }
      }
      private string GetCurrentLine()
      {
         return InputTextBox.GetLineText(InputTextBox.GetLineIndexFromCharacterIndex(InputTextBox.CaretIndex));
      }
      private void OnAutocompleteSelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         var selectedItem = AutocompleteListBox.SelectedItem as string;
         if (selectedItem == null)
            return;

         var caretIndex = InputTextBox.CaretIndex;
         if (caretIndex < 0)
            return;

         // Get the default value from the dictionary
         var defaultValue = ValidParamsDict.TryGetValue(selectedItem.ToLowerInvariant(), out var entry)
             ? entry.defaultValue
             : null;

         // Create the text to insert
         var textToInsert = string.IsNullOrEmpty(defaultValue)
             ? selectedItem
             : $"{selectedItem} {defaultValue}";

         // Find the start of the current word to be replaced
         int startOfRemoval = caretIndex;
         while (startOfRemoval > 0 && !char.IsWhiteSpace(InputTextBox.Text[startOfRemoval - 1]))
         {
            startOfRemoval--;
         }

         // Replace the text with the selected item and default value
         InputTextBox.Text = InputTextBox.Text.Remove(startOfRemoval, caretIndex - startOfRemoval) + textToInsert;
         InputTextBox.CaretIndex = startOfRemoval + textToInsert.Length;

         // Close the autocomplete popup
         AutocompletePopup.IsOpen = false;
      }

      private (bool isValid, string errorMessage) ParseAndValidate(string input)
      {
         var lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

         foreach (var line in lines)
         {
            var parts = line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
               continue;
            }

            string parameter = parts[0].ToLowerInvariant();
            if (!ValidParamsDict.Keys.Contains(parameter))
            {
               return (false, $"Unknown parameter: {parameter}");
            }

            if (parts.Length > 1)
            {
               string value = parts[1];
               if (!IsValidParameter(parameter, value))
               {
                  return (false, $"Invalid value for parameter '{parameter}': {value}");
               }
            }
         }

         return (true, string.Empty);
      }
      private bool IsValidParameter(string parameter, string value)
      {
         return parameter switch
         {
            "maxheight" or "maxwidth" or "maxheight_360" or "maxwidth_360" => ValidatePositiveInteger(value),
            "reduce" or "reducex" or "reducey" => ValidatePowerOfTwo(value),
            "bumpscale" or "pfmscale" => ValidateNonNegativeFloat(value),
            "alphatest" or "nodebug" or "nolod" or "nomip" or "nonice" or "normal" or "normalalphatodudvluminance"
            or "normaltodudv" or "nocompress" or "allmips" or "anisotropic" or "pointsample" or "manualmip"
            or "dxt5" or "stripalphachannel" or "stripcolorchannel" or "rendertarget" or "procedural"
            or "singlecopy" or "ssbump" or "trilinear" or "volumetexture" or "invertgreen" or "srgb" => ValidateBooleanFlag(value),
            "alphatest_threshhold" or "alphatest_hifreq_threshhold" or "distancespread" or "oneovermiplevelinalpha"
            or "premultcolorbyoneovermiplevel" => ValidateAlphaThreshold(value),
            "clamps" or "clampt" or "clampu" => ValidateBooleanFlag(value),
            "numchannels" => ValidateNumChannels(value),
            "mipblend" => ValidateMipBlend(value),
            "startframe" or "endframe" => ValidateNonNegativeInteger(value),
            _ => true,
         };
      }
      private static bool ValidatePositiveInteger(string value) =>
          int.TryParse(value, out int intValue) && intValue > 0;
      private static bool ValidatePowerOfTwo(string value) =>
          int.TryParse(value, out int intValue) && (intValue & (intValue - 1)) == 0 && intValue > 0;
      private static bool ValidateNonNegativeFloat(string value) =>
          float.TryParse(value, out float floatValue) && floatValue >= 0;
      private static bool ValidateBooleanFlag(string value) =>
          value == "1" || value == "0";
      private static bool ValidateAlphaThreshold(string value) =>
          float.TryParse(value, out float alphaValue) && alphaValue >= 0 && alphaValue <= 1;
      private static bool ValidateNumChannels(string value) =>
          int.TryParse(value, out int numChannels) && numChannels >= 1 && numChannels <= 4;
      private static bool ValidateNonNegativeInteger(string value) =>
          int.TryParse(value, out int intValue) && intValue >= 0;
      private static bool ValidateMipBlend(string value)
      {
         var validKeys = new[] { "r", "g", "b", "a", "detail", "skip" };
         var parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

         foreach (var part in parts)
         {
            var keyValue = part.Split('=');
            if (keyValue.Length != 2 || !validKeys.Contains(keyValue[0]) ||
                (keyValue[0] != "detail" && keyValue[0] != "skip" &&
                (!int.TryParse(keyValue[1], out int channelValue) || channelValue < 0 || channelValue > 255)))
            {
               return false;
            }
         }

         return true;
      }
   }
}
