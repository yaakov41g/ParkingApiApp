namespace ParkingApiApp.Utilities
{
    public static class ConsolePager
    {
        public static void PageText(string text, int linesPerPage = 20)
        {
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int totalLines = lines.Length;

            for (int i = 0; i < totalLines; i++)
            {
                Console.WriteLine(lines[i]);

                if ((i + 1) % linesPerPage == 0 && i + 1 < totalLines)
                {
                    Console.WriteLine($"-- Showing {i + 1}/{totalLines} lines. Press any key to continue...");
                    Console.ReadKey(true);
                }
            }

            Console.WriteLine("-- End of output --");
        }
    }
}
