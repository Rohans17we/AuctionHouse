# Database Configuration Guide

The Auction House application now supports both InMemory and SQLite database providers through configuration.

## Configuration Files

### appsettings.json (Production)
```json
{
  "DatabaseSettings": {
    "Provider": "SQLite",
    "ConnectionStrings": {
      "SQLite": "Data Source=TheAuctionHouseDB.sqlite",
      "InMemory": "TheAuctionHouseInMemoryDB"
    }
  }
}
```

### appsettings.Development.json (Development)
```json
{
  "DatabaseSettings": {
    "Provider": "InMemory",
    "ConnectionStrings": {
      "SQLite": "Data Source=TheAuctionHouseDB.sqlite",
      "InMemory": "TheAuctionHouseInMemoryDB"
    }
  }
}
```

## Switching Database Providers

### Option 1: Change Configuration Files
1. **For SQLite**: Set `"Provider": "SQLite"` in the appropriate appsettings file
2. **For InMemory**: Set `"Provider": "InMemory"` in the appropriate appsettings file

### Option 2: Environment Variables
You can override the configuration using environment variables:
```bash
# Use SQLite
set DatabaseSettings__Provider=SQLite

# Use InMemory  
set DatabaseSettings__Provider=InMemory
```

### Option 3: Command Line Arguments
```bash
# Use SQLite
dotnet run --DatabaseSettings:Provider=SQLite

# Use InMemory
dotnet run --DatabaseSettings:Provider=InMemory
```

## Database Behaviors

### SQLite Provider
- Data persists between application restarts
- Automatic database creation and migration application
- Database file: `TheAuctionHouseDB.sqlite`
- Ideal for: Production, development with data persistence

### InMemory Provider  
- Data is lost when application stops
- No database files created
- Fast startup and execution
- Ideal for: Testing, development, demos

## Automatic Features

- **Auto-Migration**: When using SQLite, migrations are automatically applied on startup
- **Console Logging**: The application logs which database provider is being used
- **Fallback**: If configuration is missing, defaults to InMemory database

## Connection String Customization

You can modify the connection strings in the configuration files:

```json
{
  "DatabaseSettings": {
    "Provider": "SQLite",
    "ConnectionStrings": {
      "SQLite": "Data Source=C:\\MyCustomPath\\auction.db",
      "InMemory": "CustomInMemoryDBName"
    }
  }
}
```

## Current Environment Settings

- **Development**: Uses InMemory database by default
- **Production**: Uses SQLite database by default
