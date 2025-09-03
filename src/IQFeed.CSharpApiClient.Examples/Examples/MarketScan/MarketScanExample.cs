using System;
using System.Linq;
using System.Threading.Tasks;
using IQFeed.CSharpApiClient.Examples.Common;
using IQFeed.CSharpApiClient.Lookup;
using IQFeed.CSharpApiClient.Lookup.Symbol.Enums;

namespace IQFeed.CSharpApiClient.Examples.Examples.MarketScan
{
    /// <summary>
    /// Comprehensive example demonstrating market scanning capabilities using IQFeed Symbol Lookup API.
    /// This shows how to filter and scan for symbols using various criteria including markets, security types,
    /// industry codes, and text searches.
    /// </summary>
    public class MarketScanExample : IExampleAsync
    {
        public bool Enable => false; // *** SET TO TRUE TO RUN THIS EXAMPLE ***
        public string Name => nameof(MarketScanExample);

        public void Run()
        {
            RunAsync().Wait();
        }

        public async Task RunAsync()
        {
            Console.WriteLine("=== IQFeed Market Scan Example ===");
            Console.WriteLine("This example demonstrates various ways to scan and filter market symbols.");
            Console.WriteLine();

            var lookupClient = LookupClientFactory.CreateNew();
            lookupClient.Connect();

            try
            {
                // 1. Get available markets and security types for filtering
                await DisplayAvailableFilters(lookupClient);

                // 2. Search for symbols by text pattern
                await SearchSymbolsByText(lookupClient);

                // 3. Filter symbols by market and security type
                await FilterSymbolsByMarketAndSecurityType(lookupClient);

                // 4. Search by industry classification (SIC codes)
                await SearchBySicCode(lookupClient);

                // 5. Search by industry classification (NAICS codes)
                await SearchByNaicsCode(lookupClient);

                // 6. Combine multiple filtering criteria
                await CombinedMarketScan(lookupClient);
            }
            finally
            {
                lookupClient.Disconnect();
            }

            Console.WriteLine("\n=== Market Scan Examples Completed ===");
        }

        private static async Task DisplayAvailableFilters(LookupClient lookupClient)
        {
            Console.WriteLine("1. Available Markets and Security Types for Filtering");
            Console.WriteLine("".PadRight(55, '='));

            // Get available listed markets
            var markets = await lookupClient.Symbol.GetListedMarketsAsync();
            var marketsList = markets.Take(10).ToList(); // Limit output
            Console.WriteLine("Available Listed Markets (showing first 10):");
            foreach (var market in marketsList)
            {
                Console.WriteLine($"  ID: {market.ListedMarketId,3} - {market.ShortName} ({market.LongName})");
            }
            Console.WriteLine($"  ... and {markets.Count() - 10} more markets\n");

            // Get available security types
            var securityTypes = await lookupClient.Symbol.GetSecurityTypesAsync();
            var securityTypesList = securityTypes.Take(10).ToList(); // Limit output
            Console.WriteLine("Available Security Types (showing first 10):");
            foreach (var secType in securityTypesList)
            {
                Console.WriteLine($"  ID: {secType.SecurityTypeId,3} - {secType.ShortName} ({secType.LongName})");
            }
            Console.WriteLine($"  ... and {securityTypes.Count() - 10} more security types\n");
        }

        private static async Task SearchSymbolsByText(LookupClient lookupClient)
        {
            Console.WriteLine("2. Search Symbols by Text Pattern");
            Console.WriteLine("".PadRight(35, '='));

            // Search for symbols starting with "AAPL"
            Console.WriteLine("Searching for symbols starting with 'AAPL':");
            var appleSymbols = await lookupClient.Symbol.GetSymbolsByFilterAsync(
                FieldToSearch.Symbols, "AAPL*", null, null);
            
            foreach (var symbol in appleSymbols.Take(5))
            {
                Console.WriteLine($"  {symbol.Symbol,-15} - Market: {symbol.ListedMarketId,3}, SecType: {symbol.SecurityTypeId,3} - {symbol.Description}");
            }

            // Search descriptions containing "Apple"
            Console.WriteLine("\nSearching for descriptions containing 'Apple':");
            var appleDescriptions = await lookupClient.Symbol.GetSymbolsByFilterAsync(
                FieldToSearch.Descriptions, "*Apple*", null, null);
            
            foreach (var symbol in appleDescriptions.Take(5))
            {
                Console.WriteLine($"  {symbol.Symbol,-15} - Market: {symbol.ListedMarketId,3}, SecType: {symbol.SecurityTypeId,3} - {symbol.Description}");
            }
            Console.WriteLine();
        }

        private static async Task FilterSymbolsByMarketAndSecurityType(LookupClient lookupClient)
        {
            Console.WriteLine("3. Filter Symbols by Market and Security Type");
            Console.WriteLine("".PadRight(45, '='));

            // Filter for equity symbols in NASDAQ (typically market ID 7, but this varies)
            // First, let's find NASDAQ market ID
            var markets = await lookupClient.Symbol.GetListedMarketsAsync();
            var nasdaqMarket = markets.FirstOrDefault(m => m.ShortName.Contains("NASDAQ") || m.LongName.Contains("NASDAQ"));
            
            if (nasdaqMarket != null)
            {
                Console.WriteLine($"Scanning NASDAQ market (ID: {nasdaqMarket.ListedMarketId}) for equity symbols:");
                
                // Search for symbols in NASDAQ market with equity security type (typically ID 1)
                var nasdaqEquities = await lookupClient.Symbol.GetSymbolsByFilterAsync(
                    FieldToSearch.Symbols, "*", FilterType.ListedMarkets, new[] { nasdaqMarket.ListedMarketId });
                
                var limitedResults = nasdaqEquities.Take(10).ToList();
                foreach (var symbol in limitedResults)
                {
                    Console.WriteLine($"  {symbol.Symbol,-15} - Market: {symbol.ListedMarketId,3}, SecType: {symbol.SecurityTypeId,3} - {symbol.Description}");
                }
                Console.WriteLine($"  Found {nasdaqEquities.Count()} symbols in NASDAQ (showing first 10)\n");
            }

            // Filter by specific security type (options, futures, etc.)
            var securityTypes = await lookupClient.Symbol.GetSecurityTypesAsync();
            var optionSecType = securityTypes.FirstOrDefault(st => st.ShortName.ToLower().Contains("option"));
            
            if (optionSecType != null)
            {
                Console.WriteLine($"Scanning for {optionSecType.ShortName} securities (ID: {optionSecType.SecurityTypeId}):");
                var options = await lookupClient.Symbol.GetSymbolsByFilterAsync(
                    FieldToSearch.Symbols, "*", FilterType.SecurityTypes, new[] { optionSecType.SecurityTypeId });
                
                foreach (var option in options.Take(10))
                {
                    Console.WriteLine($"  {option.Symbol,-20} - Market: {option.ListedMarketId,3} - {option.Description}");
                }
                Console.WriteLine($"  Found {options.Count()} option symbols (showing first 10)\n");
            }
        }

        private static async Task SearchBySicCode(LookupClient lookupClient)
        {
            Console.WriteLine("4. Search by SIC (Standard Industrial Classification) Code");
            Console.WriteLine("".PadRight(55, '='));

            // Search for companies in technology sector (SIC code 73xx - Business Services)
            Console.WriteLine("Searching for companies with SIC code starting with '73' (Business Services/Technology):");
            var techCompanies = await lookupClient.Symbol.GetSymbolsBySicCodeAsync("73");
            
            foreach (var company in techCompanies.Take(10))
            {
                Console.WriteLine($"  {company.Symbol,-15} - SIC: {company.SicCode} - {company.Description}");
            }
            Console.WriteLine($"  Found {techCompanies.Count()} companies (showing first 10)\n");

            // Display available SIC codes for reference
            Console.WriteLine("Available SIC Code Categories (showing first 10):");
            var sicCodes = await lookupClient.Symbol.GetSicCodesAsync();
            foreach (var sic in sicCodes.Take(10))
            {
                Console.WriteLine($"  Code: {sic.SicCode,-6} - {sic.Description}");
            }
            Console.WriteLine($"  ... and {sicCodes.Count() - 10} more SIC codes\n");
        }

        private static async Task SearchByNaicsCode(LookupClient lookupClient)
        {
            Console.WriteLine("5. Search by NAICS (North American Industry Classification System) Code");
            Console.WriteLine("".PadRight(70, '='));

            // Search for companies in software sector (NAICS code 5112 - Software Publishers)
            Console.WriteLine("Searching for companies with NAICS code starting with '51' (Information sector):");
            var infoCompanies = await lookupClient.Symbol.GetSymbolsByNaicsCodeAsync("51");
            
            foreach (var company in infoCompanies.Take(10))
            {
                Console.WriteLine($"  {company.Symbol,-15} - NAICS: {company.NaicsCode} - {company.Description}");
            }
            Console.WriteLine($"  Found {infoCompanies.Count()} companies (showing first 10)\n");

            // Display available NAICS codes for reference
            Console.WriteLine("Available NAICS Code Categories (showing first 10):");
            var naicsCodes = await lookupClient.Symbol.GetNaicsCodesAsync();
            foreach (var naics in naicsCodes.Take(10))
            {
                Console.WriteLine($"  Code: {naics.NaicsCode,-6} - {naics.Description}");
            }
            Console.WriteLine($"  ... and {naicsCodes.Count() - 10} more NAICS codes\n");
        }

        private static async Task CombinedMarketScan(LookupClient lookupClient)
        {
            Console.WriteLine("6. Combined Market Scan Example");
            Console.WriteLine("".PadRight(35, '='));

            // Example: Find all symbols containing "TECH" in specific markets
            var markets = await lookupClient.Symbol.GetListedMarketsAsync();
            var primaryMarkets = markets.Where(m => 
                m.ShortName.Contains("NYSE") || 
                m.ShortName.Contains("NASDAQ") || 
                m.ShortName.Contains("AMEX")).Take(3).ToList();

            if (primaryMarkets.Any())
            {
                Console.WriteLine("Scanning for symbols containing 'TECH' in major markets:");
                var marketIds = primaryMarkets.Select(m => m.ListedMarketId).ToArray();
                
                var techSymbols = await lookupClient.Symbol.GetSymbolsByFilterAsync(
                    FieldToSearch.Symbols, "*TECH*", FilterType.ListedMarkets, marketIds);
                
                foreach (var symbol in techSymbols.Take(10))
                {
                    var market = primaryMarkets.FirstOrDefault(m => m.ListedMarketId == symbol.ListedMarketId);
                    Console.WriteLine($"  {symbol.Symbol,-15} - {market?.ShortName ?? "Unknown",-8} - {symbol.Description}");
                }
                Console.WriteLine($"  Found {techSymbols.Count()} symbols containing 'TECH' (showing first 10)\n");
            }

            // Pro tip for users
            Console.WriteLine("💡 Pro Tips for Market Scanning:");
            Console.WriteLine("  • Use wildcards (*) for pattern matching in symbol searches");
            Console.WriteLine("  • Combine multiple filter criteria for precise results");
            Console.WriteLine("  • Use SIC/NAICS codes to find companies in specific industries");
            Console.WriteLine("  • Filter by security types to focus on stocks, options, futures, etc.");
            Console.WriteLine("  • Use market filters to scan specific exchanges");
        }
    }
}