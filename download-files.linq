void Main()
{
    // Configuration
    var baseUrl = "https://api.com"; 
    var downloadDirectory = @"C:\temp\downloaded-payloads"; // Save files locally
    var listEndpoint = "/example/list";
    var downloadEndpoint = "/example/";
    
    // Auth
    var headers = new Dictionary<string, string>
    {
        // { "Authorization", "Bearer your-token" },
        // { "X-API-Key", "your-api-key" }
    };
    
    try
    {
        Directory.CreateDirectory(downloadDirectory);
        $"Created/verified download directory: {downloadDirectory}".Dump();
        
        // Download all files
        var result = DownloadAllPayloadsAsync(baseUrl, downloadDirectory, headers).Result;
        
        // Display results
        $"Download completed!".Dump();
        $"Total files processed: {result.TotalFiles}".Dump();
        $"Successfully downloaded: {result.SuccessCount}".Dump();
        $"Failed downloads: {result.FailedCount}".Dump();
        
        if (result.FailedFiles.Any())
        {
            "Failed files:".Dump();
            result.FailedFiles.Dump();
        }
        
        $"Files saved to: {downloadDirectory}".Dump();
    }
    catch (Exception ex)
    {
        $"Error: {ex.Message}".Dump();
        ex.StackTrace.Dump();
    }
}

public async Task<DownloadResult> DownloadAllPayloadsAsync(string baseUrl, string downloadDirectory, Dictionary<string, string> headers = null)
{
    using var httpClient = new HttpClient();
    
    // Add headers if provided
    if (headers != null)
    {
        foreach (var header in headers)
        {
            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
    }
    
    var result = new DownloadResult();
    
    try
    {
        // Step 1: Get the list of files
        $"Fetching file list from {baseUrl}{listEndpoint}...".Dump();
        
        var listResponse = await httpClient.GetStringAsync($"{baseUrl}{listEndpoint}");
        var fileListData = JsonSerializer.Deserialize<FileListResponse>(listResponse);
        
        if (fileListData?.Files is null || !fileListData.Files.Any())
        {
            "No files found to download.".Dump();
            return result;
        }
        
        result.TotalFiles = fileListData.Files.Count;
        $"Found {result.TotalFiles} files to download".Dump();
        
        // Display file info
        fileListData.Files.Take(5).Dump("First 5 files (preview)");
        if (fileListData.Files.Count > 5)
        {
            $"... and {fileListData.Files.Count - 5} more files".Dump();
        }
        
        // Step 2: Download each file
        var semaphore = new SemaphoreSlim(3); // Limit concurrent downloads
        var downloadTasks = fileListData.Files.Select(async file =>
        {
            await semaphore.WaitAsync();
            try
            {
                return await DownloadSingleFileAsync(httpClient, baseUrl, downloadDirectory, file.Filename);
            }
            finally
            {
                semaphore.Release();
            }
        });
        
        var downloadResults = await Task.WhenAll(downloadTasks);
        
        // Aggregate results
        foreach (var downloadResult in downloadResults)
        {
            if (downloadResult.Success)
            {
                result.SuccessCount++;
            }
            else
            {
                result.FailedCount++;
                result.FailedFiles.Add(downloadResult.Filename + ": " + downloadResult.Error);
            }
        }
    }
    catch (Exception ex)
    {
        throw new Exception($"Failed to download payloads: {ex.Message}", ex);
    }
    
    return result;
}

public async Task<SingleDownloadResult> DownloadSingleFileAsync(HttpClient httpClient, string baseUrl, string downloadDirectory, string filename)
{
    try
    {
        $"Downloading {filename}...".Dump();
        
        var downloadUrl = $"{baseUrl}{downloadEndpoint}{Uri.EscapeDataString(filename)}";
        var response = await httpClient.GetAsync(downloadUrl);
        
        if (!response.IsSuccessStatusCode)
        {
            return new SingleDownloadResult 
            { 
                Filename = filename, 
                Success = false, 
                Error = $"HTTP {response.StatusCode}: {response.ReasonPhrase}" 
            };
        }
        
        var content = await response.Content.ReadAsByteArrayAsync();
        var localFilePath = Path.Combine(downloadDirectory, filename);
        
        await File.WriteAllBytesAsync(localFilePath, content);
        
        $"✓ Downloaded {filename} ({content.Length:N0} bytes)".Dump();
        
        return new SingleDownloadResult { Filename = filename, Success = true };
    }
    catch (Exception ex)
    {
        return new SingleDownloadResult 
        { 
            Filename = filename, 
            Success = false, 
            Error = ex.Message 
        };
    }
}

// Data classes for JSON deserialization
public class FileListResponse
{
    public List<FileInfo> Files { get; set; } = new();
    public long TotalSize { get; set; }
    public int TotalFiles { get; set; }
    public string TaskId { get; set; }
}

public class FileInfo
{
    public string Filename { get; set; }
    public long Size { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
    public string Type { get; set; }
}

public class DownloadResult
{
    public int TotalFiles { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> FailedFiles { get; set; } = new();
}

public class SingleDownloadResult
{
    public string Filename { get; set; }
    public bool Success { get; set; }
    public string Error { get; set; }
}
