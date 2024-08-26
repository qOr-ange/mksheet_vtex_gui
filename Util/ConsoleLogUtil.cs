using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValveSpriteSheetUtil.Util
{
   public enum Status
   {
      None,
      Success,
      Debug,
      Info,
      Warning,
      Error,
      Critical
   }
   public static class ConsoleLog
   {
      private static Status _minLoggedSeverity = Status.None;
      private static readonly ANSI[] _Colors = {
         new ANSI(220, 220, 220), // None
         new ANSI(0, 220, 42),    // Success
         new ANSI(120, 120, 180), // Debug
         new ANSI(0, 230, 230),   // Info
         new ANSI(220, 180, 0),   // Warning
         new ANSI(255, 37, 81),   // Error
         new ANSI(180, 70, 200)   // Critical
      };

      public static void SetMinLoggedSeverity(Status minSeverity) => _minLoggedSeverity = minSeverity;
      public static void WriteLine(string message, Status severity, int indent = 0)
      {
         if (severity >= _minLoggedSeverity)
            Console.WriteLine($"{new string(' ', indent)}{_Colors[(int)severity]}●{_Colors[0]} {message}");
      }
   }
   public struct ANSI
   {
      public string Value { get; private set; }
      public ANSI(int R, int G, int B)
      {
         Value = $"\u001b[38;2;{R};{G};{B}m";
      }
      public ANSI(string value)
      {
         Value = value;
      }
      public override string ToString()
      {
         return Value;
      }

      public static ANSI MoveCursorHome() => new ANSI("\u001b[H");
      public static ANSI ClearScreen() => new ANSI("\u001b[2J");
      public static ANSI ClearLineToEnd() => new ANSI("\u001b[K");
      public static ANSI MoveCursorUp(int lines) => new ANSI($"\u001b[{lines}A");
      public static ANSI MoveCursorDown(int lines) => new ANSI($"\u001b[{lines}B");
      public static ANSI MoveCursorForward(int chars) => new ANSI($"\u001b[{chars}C");
      public static ANSI MoveCursorBackward(int chars) => new ANSI($"\u001b[{chars}D");

      public static ANSI SaveCursorPosition() => new ANSI("\u001b[s");
      public static ANSI RestoreCursorPosition() => new ANSI("\u001b[u");
      public static ANSI HideCursor() => new ANSI("\u001b[?25l");
      public static ANSI ShowCursor() => new ANSI("\u001b[?25h");

      public static ANSI ResetAllFormatting() => new ANSI("\u001b[0m");
      public static ANSI BoldText() => new ANSI("\u001b[1m");
      public static ANSI ItalicText() => new ANSI("\u001b[3m");
      public static ANSI UnderlineText() => new ANSI("\u001b[4m");
      public static ANSI InvertTextColors() => new ANSI("\u001b[7m");

      public static ANSI ForegroundColor(int colorCode)
      {
         return new ANSI($"\u001b[{colorCode}m");
      }
      public static ANSI ForegroundColor(int r, int g, int b)
      {
         return new ANSI($"\u001b[38;2;{r};{g};{b}m");
      }
      public static ANSI BackgroundColor(int colorCode)
      {
         return new ANSI($"\u001b[{colorCode}m");
      }
      public static ANSI BackgroundColor(int r, int g, int b)
      {
         return new ANSI($"\u001b[48;2;{r};{g};{b}m");
      }
      public static void ClearLine(int y)
      {
         Console.SetCursorPosition(0, y);
         Console.Write($"{ClearLineToEnd()}");
      }
      public static void ClearLine()
      {
         (int x, int y) pos = Console.GetCursorPosition();
         Console.SetCursorPosition(0, pos.y - 1);
         Console.Write($"{ClearLineToEnd()}");
      }
   }
}
