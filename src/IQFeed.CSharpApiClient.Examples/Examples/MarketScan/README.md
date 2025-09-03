# Market Scan Documentation

## Overview

The IQFeed.CSharpApiClient provides comprehensive market scanning capabilities through the `SymbolFacade` class. This allows you to filter and search for symbols using various criteria including markets, security types, industry codes, and text patterns.

## Quick Start

```csharp
using IQFeed.CSharpApiClient.Lookup;
using IQFeed.CSharpApiClient.Lookup.Symbol.Enums;

// Create and connect to lookup client
var lookupClient = LookupClientFactory.CreateNew();
lookupClient.Connect();

try
{
    // Search for symbols starting with "AAPL"
    var appleSymbols = await lookupClient.Symbol.GetSymbolsByFilterAsync(
        FieldToSearch.Symbols, "AAPL*", null, null);
    
    foreach (var symbol in appleSymbols.Take(10))
    {
        Console.WriteLine($"{symbol.Symbol} - {symbol.Description}");
    }
}
finally
{
    lookupClient.Disconnect();
}
```

## Market Scanning Methods

### 1. Symbol and Description Filtering

**GetSymbolsByFilterAsync()** - The primary method for market scanning
- `FieldToSearch`: Search in Symbols or Descriptions
- `searchString`: Pattern to search for (supports wildcards *)
- `FilterType`: Optional filter by ListedMarkets or SecurityTypes
- `filterValues`: Array of market/security type IDs

```csharp
// Search symbols containing "TECH"
var techSymbols = await lookupClient.Symbol.GetSymbolsByFilterAsync(
    FieldToSearch.Symbols, "*TECH*", null, null);

// Search descriptions containing "Apple"
var appleCompanies = await lookupClient.Symbol.GetSymbolsByFilterAsync(
    FieldToSearch.Descriptions, "*Apple*", null, null);

// Filter by specific markets
var nasdaqSymbols = await lookupClient.Symbol.GetSymbolsByFilterAsync(
    FieldToSearch.Symbols, "*", FilterType.ListedMarkets, new[] { 7 }); // 7 is typically NASDAQ

// Filter by security types
var optionSymbols = await lookupClient.Symbol.GetSymbolsByFilterAsync(
    FieldToSearch.Symbols, "*", FilterType.SecurityTypes, new[] { 5 }); // 5 is typically Options
```

### 2. Industry Classification Scanning

**GetSymbolsBySicCodeAsync()** - Filter by Standard Industrial Classification codes
```csharp
// Find technology companies (SIC 73xx)
var techCompanies = await lookupClient.Symbol.GetSymbolsBySicCodeAsync("73");

// Find financial services (SIC 60xx)  
var financialCompanies = await lookupClient.Symbol.GetSymbolsBySicCodeAsync("60");
```

**GetSymbolsByNaicsCodeAsync()** - Filter by North American Industry Classification System codes
```csharp
// Find information sector companies (NAICS 51xx)
var infoCompanies = await lookupClient.Symbol.GetSymbolsByNaicsCodeAsync("51");

// Find software publishers (NAICS 5112)
var softwareCompanies = await lookupClient.Symbol.GetSymbolsByNaicsCodeAsync("5112");
```

### 3. Reference Data Methods

**GetListedMarketsAsync()** - Get available markets for filtering
```csharp
var markets = await lookupClient.Symbol.GetListedMarketsAsync();
foreach (var market in markets)
{
    Console.WriteLine($"ID: {market.ListedMarketId} - {market.ShortName} ({market.LongName})");
}
```

**GetSecurityTypesAsync()** - Get available security types for filtering
```csharp
var securityTypes = await lookupClient.Symbol.GetSecurityTypesAsync();
foreach (var secType in securityTypes)
{
    Console.WriteLine($"ID: {secType.SecurityTypeId} - {secType.ShortName} ({secType.LongName})");
}
```

**GetSicCodesAsync()** / **GetNaicsCodesAsync()** - Get industry classification codes
```csharp
var sicCodes = await lookupClient.Symbol.GetSicCodesAsync();
var naicsCodes = await lookupClient.Symbol.GetNaicsCodesAsync();
```

## Common Market Scanning Scenarios

### 1. Technology Stock Screener
```csharp
// Method 1: By SIC code
var techBySic = await lookupClient.Symbol.GetSymbolsBySicCodeAsync("73");

// Method 2: By description search
var techByName = await lookupClient.Symbol.GetSymbolsByFilterAsync(
    FieldToSearch.Descriptions, "*Technology*", null, null);

// Method 3: By symbol pattern
var techBySymbol = await lookupClient.Symbol.GetSymbolsByFilterAsync(
    FieldToSearch.Symbols, "*TECH*", null, null);
```

### 2. Exchange-Specific Screening
```csharp
// Get available markets first
var markets = await lookupClient.Symbol.GetListedMarketsAsync();
var nyseMarket = markets.FirstOrDefault(m => m.ShortName.Contains("NYSE"));

if (nyseMarket != null)
{
    // Get all NYSE-listed symbols
    var nyseSymbols = await lookupClient.Symbol.GetSymbolsByFilterAsync(
        FieldToSearch.Symbols, "*", FilterType.ListedMarkets, 
        new[] { nyseMarket.ListedMarketId });
}
```

### 3. Security Type Filtering
```csharp
var securityTypes = await lookupClient.Symbol.GetSecurityTypesAsync();
var optionType = securityTypes.FirstOrDefault(st => st.ShortName.ToLower().Contains("option"));

if (optionType != null)
{
    var allOptions = await lookupClient.Symbol.GetSymbolsByFilterAsync(
        FieldToSearch.Symbols, "*", FilterType.SecurityTypes, 
        new[] { optionType.SecurityTypeId });
}
```

### 4. Combined Filtering
```csharp
// Find technology stocks in major markets
var majorMarkets = markets.Where(m => 
    m.ShortName.Contains("NYSE") || 
    m.ShortName.Contains("NASDAQ")).Select(m => m.ListedMarketId);

var techStocksInMajorMarkets = await lookupClient.Symbol.GetSymbolsByFilterAsync(
    FieldToSearch.Descriptions, "*Technology*", FilterType.ListedMarkets, majorMarkets);
```

## Message Types

### SymbolByFilterMessage
- `Symbol`: The symbol ticker
- `ListedMarketId`: Market where symbol is listed
- `SecurityTypeId`: Type of security
- `Description`: Company/security description

### SymbolBySicCodeMessage / SymbolByNaicsCodeMessage  
- `Symbol`: The symbol ticker
- `SicCode` / `NaicsCode`: Industry classification code
- `ListedMarketId`: Market where symbol is listed
- `SecurityTypeId`: Type of security
- `Description`: Company description

## Tips and Best Practices

1. **Use Wildcards**: The `*` wildcard can be used for pattern matching in searches
2. **Limit Results**: Use LINQ's `Take()` method to limit large result sets
3. **Discover IDs First**: Use the reference data methods to discover valid market and security type IDs
4. **Combine Criteria**: Layer multiple filters for precise results
5. **Handle Async**: All methods are async - use `await` or `.Result` appropriately
6. **Connection Management**: Always connect before making requests and disconnect when done

## Running the Example

To see all these features in action, enable the MarketScanExample:

1. Navigate to `src/IQFeed.CSharpApiClient.Examples/Examples/MarketScan/MarketScanExample.cs`
2. Change `Enable => false` to `Enable => true`
3. Configure your IQFeed credentials
4. Run the examples project

The example demonstrates all scanning capabilities with real market data.