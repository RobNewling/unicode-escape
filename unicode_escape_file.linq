void Main()
{
    // Specify your input file name here
    string inputFileName = @"C:\path\to\your\input.txt";
    
    // Uncomment the line below to test with sample strings instead of file processing
    // RunTests();
    
    ProcessFile(inputFileName);
}

/// <summary>
/// Processes a file, converting Unicode escape sequences and saving to a new output file
/// </summary>
/// <param name="inputFilePath">Path to the input file</param>
public static void ProcessFile(string inputFilePath)
{
    try
    {
        // Check if input file exists
        if (!File.Exists(inputFilePath))
        {
            Console.WriteLine($"Error: Input file '{inputFilePath}' does not exist.");
            Console.WriteLine("Please update the inputFileName variable with the correct path.");
            return;
        }
        
        // Generate output file name
        var outputFilePath = GenerateOutputFileName(inputFilePath);
        
        Console.WriteLine($"Processing file: {inputFilePath}");
        Console.WriteLine($"Output file: {outputFilePath}");
        Console.WriteLine();
        
        // Read input file
        var inputContent = File.ReadAllText(inputFilePath, Encoding.UTF8);
        Console.WriteLine($"Input file size: {inputContent.Length} characters");
        
        // Convert Unicode escape sequences
        var convertedContent = ConvertUnicodeEscapes(inputContent);
        Console.WriteLine($"Converted content size: {convertedContent.Length} characters");
        
        // Write to output file
        File.WriteAllText(outputFilePath, convertedContent, Encoding.UTF8);
        
        Console.WriteLine($"✓ Successfully processed and saved to: {outputFilePath}");
        
        // Show a preview of changes if any were made
        if (inputContent != convertedContent)
        {
            Console.WriteLine("\n=== Preview of Changes ===");
            ShowPreview(inputContent, convertedContent);
        }
        else
        {
            Console.WriteLine("\n✓ No Unicode escape sequences found in the input file.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing file: {ex.Message}");
    }
}

/// <summary>
/// Generates output file name by appending "_output" before the file extension
/// </summary>
/// <param name="inputFilePath">Original file path</param>
/// <returns>Output file path with "_output" suffix</returns>
public static string GenerateOutputFileName(string inputFilePath)
{
    var directory = Path.GetDirectoryName(inputFilePath);
    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFilePath);
    var extension = Path.GetExtension(inputFilePath);
    
    var outputFileName = $"{fileNameWithoutExtension}_output{extension}";
    
    return string.IsNullOrEmpty(directory) 
        ? outputFileName 
        : Path.Combine(directory, outputFileName);
}

/// <summary>
/// Shows a preview of the first few differences between input and output
/// </summary>
/// <param name="original">Original content</param>
/// <param name="converted">Converted content</param>
public static void ShowPreview(string original, string converted)
{
    // Find and show first few Unicode escape sequences that were converted
    var pattern = @"\\u([0-9A-Fa-f]{4})";
    var matches = System.Text.RegularExpressions.Regex.Matches(original, pattern);
    
    int showCount = Math.Min(5, matches.Count);
    Console.WriteLine($"Found {matches.Count} Unicode escape sequences. Showing first {showCount}:");
    
    for (int i = 0; i < showCount; i++)
    {
        var match = matches[i];
        var hexCode = match.Groups[1].Value;
        var codePoint = Convert.ToInt32(hexCode, 16);
        var character = (char)codePoint;
        
        Console.WriteLine($"  \\u{hexCode} → '{character}'");
    }
}

/// <summary>
/// Test method with sample strings - uncomment the call in Main() to use
/// </summary>
public static void RunTests()
{
    // Test strings with various Unicode escape sequences
    var testStrings = new[]
    {
        "Hello \\u0057orld!",                    // \u0057 = W
        "Caf\\u00e9 \\u0026 Restaurant",         // \u00e6 = é, \u0026 = &
        "Price: \\u0024100.50",                  // \u0024 = $
        "Quote: \\u201cHello\\u201d",            // \u201c = ", \u201d = "
        "Arrow: \\u2192",                        // \u2192 = →
        "Mixed: ABC\\u0031\\u0032\\u0033DEF",     // \u0031\u0032\u0033 = 123
        "No escape sequences here",               // No changes expected
        "\\u03B1\\u03B2\\u03B3",                // Greek letters α β γ
        "Japanese: \\u3053\\u3093\\u306B\\u3061\\u306F", // こんにちは (Hello)
    };
    
    Console.WriteLine("=== Unicode Escape Character Converter - Test Mode ===\n");
    
    foreach (var input in testStrings)
    {
        var converted = ConvertUnicodeEscapes(input);
        Console.WriteLine($"Original:  {input}");
        Console.WriteLine($"Converted: {converted}");
        Console.WriteLine();
    }
}

/// <summary>
/// Converts Unicode escape sequences (like \u0041) to their corresponding characters
/// </summary>
/// <param name="input">String containing Unicode escape sequences</param>
/// <returns>String with escape sequences converted to actual Unicode characters</returns>
public static string ConvertUnicodeEscapes(string input)
{
    if (string.IsNullOrEmpty(input))
        return input;
    
    // Use regex to find all Unicode escape sequences
    // Pattern matches \u followed by exactly 4 hexadecimal digits
    var pattern = @"\\u([0-9A-Fa-f]{4})";
    
    return System.Text.RegularExpressions.Regex.Replace(input, pattern, match =>
    {
        // Extract the 4-digit hex code
        var hexCode = match.Groups[1].Value;
        
        // Convert hex to integer, then to character
        var codePoint = Convert.ToInt32(hexCode, 16);
        var character = (char)codePoint;
        
        return character.ToString();
    });
}

/// <summary>
/// Alternative method using StringBuilder for potentially better performance with large strings
/// </summary>
public static string ConvertUnicodeEscapesManual(string input)
{
    if (string.IsNullOrEmpty(input))
        return input;
    
    var result = new StringBuilder();
    
    for (int i = 0; i < input.Length; i++)
    {
        // Check if we found the start of a Unicode escape sequence
        if (i <= input.Length - 6 && 
            input[i] == '\\' && 
            input[i + 1] == 'u')
        {
            // Extract the 4-digit hex code
            var hexString = input.Substring(i + 2, 4);
            
            // Validate that all 4 characters are valid hex digits
            if (IsValidHex(hexString))
            {
                // Convert to character and append
                var codePoint = Convert.ToInt32(hexString, 16);
                result.Append((char)codePoint);
                
                // Skip the entire escape sequence
                i += 5;
            }
            else
            {
                // Not a valid escape sequence, just append the backslash
                result.Append(input[i]);
            }
        }
        else
        {
            // Regular character, just append
            result.Append(input[i]);
        }
    }
    
    return result.ToString();
}

/// <summary>
/// Helper method to validate if a string contains only valid hexadecimal characters
/// </summary>
private static bool IsValidHex(string hex)
{
    return hex.All(c => 
        (c >= '0' && c <= '9') || 
        (c >= 'A' && c <= 'F') || 
        (c >= 'a' && c <= 'f'));
}

/// <summary>
/// Utility method to convert regular text back to Unicode escape sequences (reverse operation)
/// </summary>
public static string ConvertToUnicodeEscapes(string input)
{
    if (string.IsNullOrEmpty(input))
        return input;
    
    var result = new StringBuilder();
    
    foreach (char c in input)
    {
        // Convert non-ASCII characters to Unicode escape sequences
        if (c > 127)
        {
            result.Append($"\\u{(int)c:X4}");
        }
        else
        {
            result.Append(c);
        }
    }
    
    return result.ToString();
}
