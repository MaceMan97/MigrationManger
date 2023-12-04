using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationManger
{
    public class CommandLineSelector
    {

        public int SelectedOption { get; set; }
        public List<string> Options { get; set; }
        
        public CommandLineSelector(List<string> options)
        {
            Options = options;
        }

        public int PrintOptions()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.ResetColor();
            Console.WriteLine("\nUse ⬆️  and ⬇️  to navigate and press \u001b[32mEnter/Return\u001b[0m to select:");
            var topOption = Options.Count;
            (int left, int top) = Console.GetCursorPosition();
            var option = 1;
            var decorator = ">> \u001b[32m";
            ConsoleKeyInfo key;
            bool isSelected = false;

            while (!isSelected)
            {
                Console.SetCursorPosition(left, top);

                foreach (var o in Options)
                {
                    Console.WriteLine($"{(option == Options.IndexOf(o) + 1 ? decorator : "   ")} {o}\u001b[0m");
                }


                key = Console.ReadKey(false);

                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        option = option == 1 ? topOption : option - 1;
                        break;

                    case ConsoleKey.DownArrow:
                        option = option == topOption ? 1 : option + 1;
                        break;

                    case ConsoleKey.Enter:
                        isSelected = true;
                        break;
                }
            }

            return option;
        }
    }
}
