public class TradeConfigs
{
    // Modifiers, you can change them to whatever
    public float stock_multiplier;
    public float stock_chance_multiplier;
    public float ammo_multiplier;
    public float buy_multiplier;
    public float sell_multiplier;
}

public class TradeModifier
{
    public enum SECTION
    {
        SELL,
        BUY,
        SUPPLIES
    }

    public void modify_trade(TradeConfigs trade_configs)
    {
        SECTION current_section = SECTION.SELL;

        // Setup paths
        string source_folder = "TradeInput";     // Put your original .ltx files here
        string output_folder = "TradeOutput";    // Processed files will go here
        
        // Create folders if they don't exist
        if (!Directory.Exists(source_folder)) Directory.CreateDirectory(source_folder);
        if (!Directory.Exists(output_folder)) Directory.CreateDirectory(output_folder);

        // 2. Automatically get all .ltx files from the source folder
        string[] input_files = Directory.GetFiles(source_folder, "*.ltx");

        if (input_files.Length <= 0)
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
            string[] supply_sections = 
            {
                "[supplies_generic]",
                "[supplies_1]",
                "[supplies_heavy_pockets]"
            };
            // Keep stock for weapons, outfits, etc. to 1 only
            string[] single_stock_items = 
            {
                "wpn",
                "sil",
                "detector",
                "outfit",
                "helm",
                "equ"
            };

            foreach (string line in lines)
            {
                if (line.Contains("[trade_generic_sell]"))
                {
                    current_section = SECTION.SELL;
                    sell_section = line_number;
                }
                else if (line.Contains("[trade_generic_buy]"))
                {
                    current_section = SECTION.BUY;
                    buy_section = line_number;
                }
                else if (contains_words(line, supply_sections))
                {
                    current_section = SECTION.SUPPLIES;
                    supplies_section = line_number;
                }

                if (!has_words(line, ["=", ","]))
                {
                    output_lines.Add(line);
                    line_number++;

                    continue;
                }

                // Pattern matching
                //--parts[0]-------parts[1]------------
                //  Name    =   Number 1,   Number 2
                //--name--------values[0]---values[1]--
                string[] parts = line.Split('=');
                string name = parts[0];
                string[] values = parts[1].Split(',');

                if (values.Length >= 2 &&
                    float.TryParse(values[0].Trim(), out float val1) &&
                    float.TryParse(values[1].Trim(), out float val2))
                {
                    float value1 = 0.0f;
                    float value2 = 0.0f;

                    switch(current_section)
                    {
                        case SECTION.SELL:
                            value1 = val1 * trade_configs.sell_multiplier;
                            value2 = val2 * trade_configs.sell_multiplier;
                            
                            break;
                        case SECTION.BUY:
                            value1 = val1 * trade_configs.buy_multiplier;
                            value2 = val2 * trade_configs.buy_multiplier;

                            break;
                        case SECTION.SUPPLIES:
                            if (has_words(line, ["ammo", "grenade"]))
                                value1 = val1 * (trade_configs.stock_multiplier + trade_configs.ammo_multiplier);
                            else if (has_words(line, single_stock_items))
                                value1 = val1;
                            else
                                value1 = val1 * trade_configs.stock_multiplier;

                            value2 = Math.Clamp(val2 * trade_configs.stock_chance_multiplier, 0.1f, 1.0f);
                            
                            break;
                    }
                    
                    output_lines.Add($"{name} = {value1:F1}, {value2:F1}");
                    line_number++;
                }
                else
                {
                    output_lines.Add(line);
                    line_number++;
                }
            }

            // Save to Output folder with the same file_name
            string fullOutputPath = Path.Combine(output_folder, file_name);
            File.WriteAllLines(fullOutputPath, output_lines);
            
            Console.Write($"Processed {file_name}: ");
            Console.Write($"[Sell Section: {sell_section}, ");
            Console.Write($"Buy Section: {buy_section}, ");
            Console.Write($"Supplies Section: {supplies_section}]\n");
        }
    }

    public static bool contains_words(string line, string[] words)
    {
        foreach (string word in words)
        {
            if (line.Contains(word))
                return true;
        }

        return false;
    }

    public static bool has_words(string line, string[] words)
    {
        bool has_all_words = true;

        foreach (string word in words)
        {
            if (!line.Contains(word))
                return false;
        }

        return has_all_words;
    }
}