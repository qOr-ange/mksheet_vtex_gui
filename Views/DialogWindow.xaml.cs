using System.Windows;

namespace ValveSpriteSheetUtil.Views
{
   public partial class DialogWindow : Window
   {
      public DialogWindow(string title, UIElement content, int width, int height)
      {
         InitializeComponent();
         this.Height = height;
         this.Width = width;
         this.Title = title;
         this.Content.Content = content;
      }
   }
}
