class Program
{
    private static void Main()
    {
        TradeConfigs trade_configs = new TradeConfigs();
        trade_configs.stock_multiplier = 3.0f;
        trade_configs.stock_chance_multiplier = 2.0f;
        trade_configs.ammo_multiplier = 2.0f;
        trade_configs.buy_multiplier = 1.0f;
        trade_configs.sell_multiplier = 0.8f;

        TradeModifier trade_modifier = new TradeModifier();
        trade_modifier.modify_trade(trade_configs);

        Console.WriteLine("\n--- Batch processing complete! ---");
    }
}