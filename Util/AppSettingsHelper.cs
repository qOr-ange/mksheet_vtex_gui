using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ValveSpriteSheetUtil.Util
{
   public static class AppSettingsHelper
   {
      private static AppSettings appSettings = new AppSettings();

      public static T GetSetting<T>(Expression<Func<AppSettings, T>> selector)
      {
         var property = GetProperty(selector);
         return (T)property.GetValue(appSettings);
      }

      public static void SetSetting<T>(Expression<Func<AppSettings, T>> selector, T value)
      {
         var property = GetProperty(selector);
         property.SetValue(appSettings, value);
      }

      public static void SaveSettings() { 
         appSettings.Save(); 
         ConsoleLog.WriteLine("Settings saved.", Status.Success);
      }

      private static PropertyInfo GetProperty<T>(Expression<Func<AppSettings, T>> selector)
      {
         if (selector.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo property)
         {
            return property;
         }

         throw new ArgumentException("Selector must be a property expression.");
      }
   }
}
