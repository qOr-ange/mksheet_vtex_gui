namespace ValveSpriteSheetUtil.Util
{
   public static class AppSettingsHelper
   {
      private static AppSettings appSettings = new AppSettings();

      public static string TF2FolderPath
      {
         get => appSettings.TeamFortressFolder;
         set
         {
            appSettings.TeamFortressFolder = value;
            appSettings.Save();
         }
      }
      public static string FramesFolder
      {
         get => appSettings.FrameFolder;
         set
         {
            appSettings.FrameFolder = value;
            appSettings.Save();
         }
      }
      public static string VTEXConfig
      {
         get => appSettings.VTEXConfig;
         set
         {
            appSettings.VTEXConfig = value;
            appSettings.Save();
         }
      }
      public static string Prefix
      {
         get => appSettings.Prefix;
         set
         {
            appSettings.Prefix = value;
            appSettings.Save();
         }
      }
      public static string FileName
      {
         get => appSettings.FileName;
         set
         {
            appSettings.FileName = value;
            appSettings.Save();
         }
      }
      public static string VTEXTemplates
      {
         get => appSettings.VTEXTemplates;
         set
         {
            appSettings.VTEXTemplates = value;
            appSettings.Save();
         }
      }
      public static bool DontAskAgain_Templates
      {
         get => appSettings.tmlpDontAskAgain;
         set
         {
            appSettings.tmlpDontAskAgain = value;
            appSettings.Save();
         }
      }
   }
}
