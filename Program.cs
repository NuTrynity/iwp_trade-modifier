class Program
{
    // Modifiers
    static float stock_multiplier = 3f;
    static float ammo_multiplier = 2.0f;
    static float stock_chance_multiplier = 2f;
    static float buy_multiplier = 3.0f;
    static float sell_multiplier = 0.1f;

    private static void Main()
    {
        // Setup paths
        string source_folder = "Input";     // Put your original .ltx files here
        string output_folder = "Output";    // Processed files will go here
        
        // Create folders if they don't exist
        if (!Directory.Exists(source_folder)) Directory.CreateDirectory(source_folder);
        if (!Directory.Exists(output_folder)) Directory.CreateDirectory(output_folder);

        // 2. Automatically get all .ltx files from the source folder
        string[] input_files = Directory.GetFiles(source_folder, "*.ltx");

        if (input_files.Length == 0)
        {
            Console.WriteLine($"No .ltx files found in {source_folder}. Add some files and try again!");
            return;
        }

        foreach (string filePath in input_files)
        {
            // Path.GetFileName gives us "trade_bandit.ltx" instead of "TradeConfigs/trade_bandit.ltx"
            string file_name = Path.GetFileName(filePath);
            
            string[] lines = File.ReadAllLines(filePath);
            List<string> output_lines = new List<string>();

            int line_number = 1;

            // For debugging, if the program actually had a section
            int sell_section = 0;
            int buy_section = 0;
            int supplies_section = 0;

            // There are three sections in a trade file
            // 1: Sell, 2: Buy, And 3: Supplies
            int section_state = 1;

            foreach (string line in lines)
            {
                if (line.Contains("[trade_generic_sell]"))
                {
                    section_state = 1;
                    sell_section = line_number;
                }
                else if (line.Contains("[trade_generic_buy]"))
                {
                    section_state = 2;
                    buy_section = line_number;
                }
                else if (line.Contains("[supplies_generic]") || line.Contains("[supplies_1]"))
                {
                    section_state = 3;
                    supplies_section = line_number;
                }

                // Pattern matching
                //--parts[0]-------parts[1]------------
                //  Name    =   Number 1,   Number 2
                //--name--------values[0]---values[1]--
                if (line.Contains("=") && line.Contains(","))
                {
                    string[] parts = line.Split('=');
                    string name = parts[0];
                    string[] values = parts[1].Split(',');

                    if (values.Length >= 2 &&
                        float.TryParse(values[0].Trim(), out float val1) &&
                        float.TryParse(values[1].Trim(), out float val2))
                    {
                        float value1 = 0.0f;
                        float value2 = 0.0f;

                        if (section_state == 1) // Sell Section
                        {
                            value1 = val1 * sell_multiplier;
                            value2 = val2 * sell_multiplier;
                        }
                        else if (section_state == 2) // Buy Section
                        {
                            value1 = val1 * buy_multiplier;
                            value2 = val2 * buy_multiplier;
                        }
                        else if (section_state == 3) // Supplies Section
                        {
                            if (line.Contains("ammo_"))
                                value1 = (int)(val1 * stock_chance_multiplier * ammo_multiplier);
                            else
                                value1 = (int)(val1 * stock_multiplier);

                            value2 = Math.Min(val2 * stock_chance_multiplier, 1.0f);
                        }
                        
                        output_lines.Add($"{name} = {value1:F1}, {value2:F1}");
                        continue;
                    }
                }

                output_lines.Add(line);
                line_number++;
            }

            // Save to Output folder with the same file_name
            string fullOutputPath = Path.Combine(output_folder, file_name);
            File.WriteAllLines(fullOutputPath, output_lines);
            
            Console.Write($"Processed: {file_name}, ");
            Console.Write($"Sell Section: {sell_section}, ");
            Console.Write($"Buy Section: {buy_section}, ");
            Console.Write($"Supplies Section: {supplies_section}\n");
        }

        Console.WriteLine("\n--- Batch processing complete! ---");
    }
}