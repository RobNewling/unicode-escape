void Main()
{
    var baseUrl = "https://your-api.com";
    var downloadDir = @"C:\temp\downloaded-payloads";
    var token = "your-token-here";
    
    Directory.CreateDirectory(downloadDir);
    
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    
    // Get file list
    var fileList = client.GetStringAsync($"{baseUrl}/admin/payloads/list").Result;
    var files = fileList.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                       .Select(f => f.Trim())
                       .Where(f => !string.IsNullOrEmpty(f))
                       .ToList();
    
    $"Downloading {files.Count} files...".Dump();
    
    foreach (var filename in files)
    {
        $"Downloading {filename}...".Dump();
        
        var fileBytes = client.GetByteArrayAsync($"{baseUrl}/admin/payloads/download/{Uri.EscapeDataString(filename)}").Result;
        var localPath = Path.Combine(downloadDir, filename);
        File.WriteAllBytes(localPath, fileBytes);
        
        $"{filename} ({fileBytes.Length:N0} bytes)".Dump();
    }
    
    "Download Complete".Dump();
}
