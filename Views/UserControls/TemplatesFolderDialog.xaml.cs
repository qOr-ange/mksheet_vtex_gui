using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ValveSpriteSheetUtil.Views.UserControls
{
   public partial class TemplatesFolderDialog : UserControl
   {
      public TemplatesFolderDialog()
      {
         InitializeComponent();
      }

      public void SetMessage(string message)
      {
         MessageTextBlock.Text = message;
      }

      public void AddButton(string content, int? width, RoutedEventHandler clickHandler)
      {
         if (width == null) { width = 80; }

         var button = new Button
         {
            Content = content,
            Width = (int)width,
            Height = 20,
            Margin = new Thickness(2, 0, 2, 0),
            SnapsToDevicePixels = true,
            Style = (Style)Application.Current.Resources["GeneralButtonStyle"],
            BorderThickness = new Thickness(1,1,1,1),
         };

         // Apply custom styles
         button.Foreground = new SolidColorBrush((Color)Application.Current.Resources["Text.Secondary"]);
         button.BorderBrush = new SolidColorBrush((Color)Application.Current.Resources["BG.Lighter"]);
         button.Background = new SolidColorBrush((Color)Application.Current.Resources["BG.Light"]);

         button.Click += clickHandler;
         ButtonStackPanel.Children.Add(button);
      }


      public void ClearButtons()
      {
         ButtonStackPanel.Children.Clear();
      }
   }
}
