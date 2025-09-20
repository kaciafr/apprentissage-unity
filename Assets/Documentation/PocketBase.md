# PocketBase Integration Guide

## Overview

This project uses PocketBase as the backend solution for authentication, real-time data synchronization, and file storage.

## Setup

### 1. PocketBase Configuration
```csharp
// Configure PocketBase client
var config = new PocketBaseConfig
{
    ServerUrl = "https://your-pocketbase-server.com",
    ApiKey = "your-api-key",
    EnableRealtime = true
};
```

### 2. Authentication
```csharp
// User registration
await PocketBaseClient.Instance.RegisterAsync(email, password, userData);

// User login
var authResult = await PocketBaseClient.Instance.LoginAsync(email, password);

// Check authentication status
bool isAuthenticated = PocketBaseClient.Instance.IsAuthenticated;
```

### 3. Data Operations

#### Create Record
```csharp
var playerData = new PlayerData
{
    Name = "Player Name",
    Level = 1,
    Experience = 0
};

var record = await PocketBaseClient.Instance.CreateAsync("players", playerData);
```

#### Read Records
```csharp
// Get all records
var players = await PocketBaseClient.Instance.GetListAsync<PlayerData>("players");

// Get specific record
var player = await PocketBaseClient.Instance.GetByIdAsync<PlayerData>("players", recordId);

// Search with filters
var activePlayers = await PocketBaseClient.Instance.GetListAsync<PlayerData>(
    "players",
    filter: "active = true"
);
```

#### Update Record
```csharp
playerData.Level = 2;
await PocketBaseClient.Instance.UpdateAsync("players", recordId, playerData);
```

#### Delete Record
```csharp
await PocketBaseClient.Instance.DeleteAsync("players", recordId);
```

### 4. Real-time Subscriptions
```csharp
// Subscribe to collection changes
PocketBaseClient.Instance.Subscribe("players", (action, record) =>
{
    switch (action)
    {
        case "create":
            OnPlayerCreated(record);
            break;
        case "update":
            OnPlayerUpdated(record);
            break;
        case "delete":
            OnPlayerDeleted(record);
            break;
    }
});
```

### 5. File Storage
```csharp
// Upload file
var fileData = File.ReadAllBytes("path/to/file.png");
var uploadResult = await PocketBaseClient.Instance.UploadFileAsync(
    "collection",
    recordId,
    "fieldName",
    fileData,
    "filename.png"
);

// Get file URL
string fileUrl = PocketBaseClient.Instance.GetFileUrl(collection, recordId, filename);
```

## Data Models

### Example Player Data Model
```csharp
[System.Serializable]
public class PlayerData : IPocketBaseRecord
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public Vector3 Position { get; set; }
    public DateTime LastLogin { get; set; }
    public bool IsOnline { get; set; }
}
```

## Error Handling

```csharp
try
{
    var result = await PocketBaseClient.Instance.GetByIdAsync<PlayerData>("players", id);
}
catch (PocketBaseException ex)
{
    Debug.LogError($"PocketBase Error: {ex.Message}");
    // Handle specific error types
    switch (ex.ErrorCode)
    {
        case 404:
            // Record not found
            break;
        case 401:
            // Authentication required
            break;
        default:
            // General error handling
            break;
    }
}
```

## Best Practices

1. **Connection Management**: Always check connection status
2. **Data Caching**: Implement local caching for offline support
3. **Rate Limiting**: Respect API rate limits
4. **Security**: Never expose API keys in client code
5. **Validation**: Validate data on both client and server
6. **Error Recovery**: Implement retry mechanisms for network failures

## Troubleshooting

### Common Issues
- **Connection Timeout**: Check network connectivity
- **Authentication Failure**: Verify credentials and session
- **Rate Limit Exceeded**: Implement request throttling
- **Data Sync Conflicts**: Handle concurrent modifications