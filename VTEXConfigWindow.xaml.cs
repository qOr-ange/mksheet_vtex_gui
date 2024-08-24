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
      private static readonly HashSet<string> ValidParams = new HashSet<string>
      {
          "allmips", "alphatest", "alphatest_hifreq_threshhold", "alphatest_threshhold", "alphatodistance",
          "anisotropic", "bumpscale", "clamps", "clampt", "clampu", "distancespread", "dudv", "dxt5",
          "invertgreen", "manualmip", "maxheight", "maxheight_360", "maxwidth", "maxwidth_360", "mipblend",
          "nocompress", "nodebug", "nolod", "nomip", "nonice", "normal", "normalalphatodudvluminance",
          "normaltodudv", "numchannels", "oneovermiplevelinalpha", "pfm", "pfmscale", "pointsample",
          "premultcolorbyoneovermiplevel", "procedural", "reduce", "reducex", "reducey", "rendertarget",
          "singlecopy", "skybox", "specvar", "spheremap_negz", "spheremap_z", "spheremap_negy", "spheremap_y",
          "spheremap_negx", "spheremap_x", "ssbump", "startframe", "endframe", "stripalphachannel",
          "stripcolorchannel", "trilinear", "volumetexture"
      };

      public VTEXConfigWindow(string vtexConfig)
      {
         _vtexConfig = vtexConfig;
         InitializeComponent();
         Loaded += OnWindowLoaded;
         AutocompletePopup.IsOpen = false;
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
         var document = new FlowDocument();
         var paragraph = new Paragraph();
         paragraph.Inlines.Add(new Run(_vtexConfig));
         document.Blocks.Add(paragraph);
         InputTextBox.Document = document;
      }
      public string GetTextBoxContent()
      {
         var textRange = new TextRange(InputTextBox.Document.ContentStart, InputTextBox.Document.ContentEnd);
         return textRange.Text;
      }



      private void OnRichTextBoxTextChanged(object sender, TextChangedEventArgs e)
      {
         UpdateAutocomplete();
         HighlightInvalidParameters();
      }
      private void UpdateAutocomplete()
      {
         var caretPosition = InputTextBox.CaretPosition;
         if (caretPosition != null)
         {
            var currentWord = GetCurrentWord(caretPosition);
            if (!string.IsNullOrEmpty(currentWord))
            {
               var suggestions = ValidParams
                   .Where(p => p.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
                   .ToList();

               if (suggestions.Any())
               {
                  AutocompleteListBox.ItemsSource = suggestions;
                  AutocompletePopup.IsOpen = true;

                  PositionAutocompletePopup(caretPosition);
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

      private void PositionAutocompletePopup(TextPointer caretPosition)
      {
         var rect = caretPosition.GetCharacterRect(LogicalDirection.Backward);
         var transform = InputTextBox.TransformToAncestor(this);
         var popupTopLeft = transform.Transform(new Point(rect.X, rect.Y + rect.Height));

         const double verticalOffsetAdjustment = 20;
         AutocompletePopup.HorizontalOffset = popupTopLeft.X;
         AutocompletePopup.VerticalOffset = popupTopLeft.Y - verticalOffsetAdjustment;

         var windowWidth = this.ActualWidth;
         var popupWidth = AutocompletePopup.ActualWidth;

         if (popupTopLeft.X + popupWidth > windowWidth)
         {
            AutocompletePopup.HorizontalOffset = windowWidth - popupWidth - 10;
         }
      }
      private string GetCurrentWord(TextPointer position)
      {
         if (position == null)
            return string.Empty;

         string word = string.Empty;

         TextPointer start = position.GetPositionAtOffset(0, LogicalDirection.Backward);
         while (start != null)
         {
            string text = start.GetTextInRun(LogicalDirection.Backward);

            if (string.IsNullOrEmpty(text) || char.IsWhiteSpace(text[0]))
            {
               break;
            }

            word = text.Substring(0, text.Length - 1);
            start = start.GetPositionAtOffset(-1, LogicalDirection.Backward);
         }

         string currentText = position.GetTextInRun(LogicalDirection.Backward);
         if (!string.IsNullOrEmpty(currentText))
         {
            word = currentText.Substring(0, currentText.Length);
         }

         return word.Trim();
      }
      private void OnAutocompleteSelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         var selectedItem = AutocompleteListBox.SelectedItem as string;
         if (selectedItem == null)
            return;

         var caretPosition = InputTextBox.CaretPosition;
         if (caretPosition == null)
            return;

         TextPointer startOfRemoval = caretPosition.GetPositionAtOffset(0, LogicalDirection.Backward);
         while (startOfRemoval != null)
         {
            string textInRun = startOfRemoval.GetTextInRun(LogicalDirection.Backward);

            if (!string.IsNullOrEmpty(textInRun) && !char.IsWhiteSpace(textInRun[0]))
            {
               startOfRemoval = startOfRemoval.GetPositionAtOffset(-1, LogicalDirection.Backward);
            }
            else
            {
               break;
            }
         }

         if (startOfRemoval != null)
         {
            startOfRemoval = startOfRemoval.GetPositionAtOffset(0, LogicalDirection.Forward);
         }

         TextPointer endOfRemoval = caretPosition;

         if (startOfRemoval != null && endOfRemoval != null)
         {
            TextRange textRange = new TextRange(startOfRemoval, endOfRemoval);
            textRange.Text = selectedItem;
         }

         AutocompletePopup.IsOpen = false;

         var newCaretPosition = startOfRemoval.GetPositionAtOffset(selectedItem.Length, LogicalDirection.Forward);
         InputTextBox.CaretPosition = newCaretPosition ?? InputTextBox.CaretPosition;
      }







      private void HighlightInvalidParameters()
      {
         var document = InputTextBox.Document;
         var textRange = new TextRange(document.ContentStart, document.ContentEnd);

         var lines = textRange.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

         foreach (var line in lines)
         {
            var parts = line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            string parameter = parts[0].ToLowerInvariant();
            bool isValidParameter = ValidParams.Contains(parameter);

            if (!isValidParameter)
            {
               HighlightText(line, Brushes.Red);
            }
            else
            {
               HighlightText(line, (Brush)Application.Current.Resources["Brush.Text.Secondary"]);
            }
         }
      }

      private void HighlightText(string text, Brush color)
      {
         var document = InputTextBox.Document;
         var start = document.ContentStart;
         var end = document.ContentEnd;

         while (start != null && start.CompareTo(end) < 0)
         {
            if (start.GetTextInRun(LogicalDirection.Forward).Contains(text))
            {
               var textRange = new TextRange(start, start.GetPositionAtOffset(text.Length, LogicalDirection.Forward));
               textRange.ApplyPropertyValue(TextElement.ForegroundProperty, color);
            }

            start = start.GetNextContextPosition(LogicalDirection.Forward);
         }
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
            if (!ValidParams.Contains(parameter))
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
            or "singlecopy" or "ssbump" or "trilinear" or "volumetexture" or "invertgreen" => ValidateBooleanFlag(value),
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
      private bool ValidateMipBlend(string value)
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
