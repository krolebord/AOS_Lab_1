using System;

namespace AOS.Client.Utils
{
    public static class ConsoleUtils
    {
        public static void WriteColoredLine(string text, ConsoleColor fontColor, ConsoleColor? backgroundColor = null)
        {
            WriteColored(text, fontColor, backgroundColor);
            Console.WriteLine();
        }

        public static void WriteColored(string text, ConsoleColor fontColor, ConsoleColor? backgroundColor = null)
        {
            var startBackgroundColor = Console.BackgroundColor;
            var startForegroundColor = Console.ForegroundColor;

            Console.BackgroundColor = backgroundColor ?? startBackgroundColor;
            Console.ForegroundColor = fontColor;

            Console.Write(text);

            Console.BackgroundColor = startBackgroundColor;
            Console.ForegroundColor = startForegroundColor;
        }

        public static void WriteTopEdge()
        {
            Console.Write('╔');
            Console.Write("".PadRight(Console.WindowWidth - 2, '═'));
            Console.WriteLine('╗');
        }

        public static void WriteMiddleEdge()
        {
            Console.Write('╠');
            Console.Write("".PadRight(Console.WindowWidth - 2, '═'));
            Console.WriteLine('╣');
        }

        public static void WriteBottomEdge()
        {
            Console.Write('╚');
            Console.Write(string.Empty.PadRight(Console.WindowWidth - 2, '═'));
            Console.WriteLine('╝');
        }

        public static void WriteRow()
        {
            Console.Write('║');
            Console.Write(new string(' ', Console.WindowWidth - 2));
            Console.WriteLine('║');
        }

        public static void WriteRow(string text)
        {
            Console.Write('║');
            Console.Write(text.PadRight(Console.WindowWidth - 2));
            Console.WriteLine('║');
        }

        public static void WriteColoredRow(string text, ConsoleColor fontColor, ConsoleColor? backgroundColor = null)
        {
            Console.Write('║');
            WriteColored(text.PadRight(Console.WindowWidth - 2), fontColor, backgroundColor);
            Console.WriteLine('║');
        }
    }
}
