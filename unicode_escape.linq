void Main()
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

	Console.WriteLine("=== Unicode Escape Character Converter ===\n");

	foreach (var input in testStrings)
	{
		var converted = ConvertUnicodeEscapes(input);
		Console.WriteLine($"Original:  {input}");
		Console.WriteLine($"Converted: {converted}");
		Console.WriteLine();
	}

	// Interactive section - uncomment and modify as needed
	/*
    Console.WriteLine("=== Enter your own string ===");
    Console.Write("Input: ");
    var userInput = Console.ReadLine();
    if (!string.IsNullOrEmpty(userInput))
    {
        var result = ConvertUnicodeEscapes(userInput);
        Console.WriteLine($"Result: {result}");
    }
    */
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
