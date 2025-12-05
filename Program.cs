var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure port - Railway sets PORT environment variable automatically
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
if (!string.IsNullOrEmpty(port))
{
    app.Urls.Clear();
    app.Urls.Add($"http://0.0.0.0:{port}");
}

// Configure the HTTP request pipeline
app.UseCors();

// Configure default files
var defaultFilesOptions = new DefaultFilesOptions();
defaultFilesOptions.DefaultFileNames.Clear();
defaultFilesOptions.DefaultFileNames.Add("index.html");
app.UseDefaultFiles(defaultFilesOptions);

app.UseStaticFiles();

// API endpoint for vehicle lookup
app.MapGet("/api/vehicleinfo", async (string regno, IHttpClientFactory httpClientFactory) =>
{
    if (string.IsNullOrWhiteSpace(regno))
    {
        return Results.BadRequest(new { error = "Registration number is required" });
    }

    try
    {
        // ============================================
        // API CONFIGURATION - mParivahan API
        // ============================================
        // API: RTO Vehicle Details - RC - PUC - Insurance (mParivahan) by Fire API
        // Get your API key from: RapidAPI → My Apps → Your App
        
        // mParivahan API endpoint - Get Vehicle / RC Details
        string apiUrl = "https://rto-vehicle-details-rc-puc-insurance-mparivahan.p.rapidapi.com/api/rc-vehicle/search-data";
        
        // Get API key from environment variable (for production) or appsettings.json (for local dev)
        string apiKey = Environment.GetEnvironmentVariable("RAPIDAPI_KEY") 
            ?? app.Configuration["RAPIDAPI_KEY"] 
            ?? "54f47daecemsh7ab18423f71f4e3p1dbe48jsn657e9b1ecc54";  // Fallback for local development
        
        string apiHost = "rto-vehicle-details-rc-puc-insurance-mparivahan.p.rapidapi.com";

        // Real API call to mParivahan API
        var httpClient = httpClientFactory.CreateClient();
        
        // Set RapidAPI headers
        httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
        httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", apiHost);
        
        // Increased timeout - API can be slow (increased to 120 seconds)
        httpClient.Timeout = TimeSpan.FromSeconds(120);

        // API uses 'vehicle_no' as the parameter name
        // Add retry logic for timeout errors
        HttpResponseMessage? response = null;
        int maxRetries = 2;
        int retryCount = 0;
        
        while (retryCount <= maxRetries)
        {
            try
            {
                response = await httpClient.GetAsync($"{apiUrl}?vehicle_no={Uri.EscapeDataString(regno)}");
                break; // Success, exit retry loop
            }
            catch (TaskCanceledException) when (retryCount < maxRetries)
            {
                retryCount++;
                await Task.Delay(3000); // Wait 3 seconds before retry
                continue;
            }
        }
        
        if (response == null)
        {
            // Return a helpful error message
            return Results.Problem(
                detail: "The API service is currently unavailable or taking too long to respond. Please try again in a few moments.",
                statusCode: 504
            );
        }

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Results.NotFound(new { error = "Vehicle not found" });
            }
            if (response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout || 
                response.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
            {
                // Return a more user-friendly error message
                return Results.Problem(
                    detail: "The API service is currently unavailable or experiencing high traffic. Please wait a moment and try again.",
                    statusCode: 504
                );
            }
            var errorContent = await response.Content.ReadAsStringAsync();
            
            // Handle specific error cases
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return Results.Problem(
                    detail: "API subscription error: You are not subscribed to this API. Please subscribe to the 'RTO Vehicle Details - RC - PUC - Insurance (mParivahan)' API on RapidAPI.",
                    statusCode: 403
                );
            }
            
            return Results.Problem(
                detail: $"API returned status code: {response.StatusCode}. {errorContent}",
                statusCode: (int)response.StatusCode
            );
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        
        // Helper function to extract values from JSON with multiple possible key names
        string? GetJsonValue(System.Text.Json.JsonElement element, params string[] keys)
        {
            // Try direct property access (case-sensitive)
            foreach (var key in keys)
            {
                if (element.TryGetProperty(key, out var value))
                {
                    if (value.ValueKind == System.Text.Json.JsonValueKind.String)
                        return value.GetString();
                    if (value.ValueKind == System.Text.Json.JsonValueKind.Number)
                        return value.ToString();
                    if (value.ValueKind == System.Text.Json.JsonValueKind.True || value.ValueKind == System.Text.Json.JsonValueKind.False)
                        return value.GetBoolean().ToString();
                }
            }
            
            // Try case-insensitive search in root level
            foreach (var prop in element.EnumerateObject())
            {
                foreach (var key in keys)
                {
                    if (string.Equals(prop.Name, key, StringComparison.OrdinalIgnoreCase))
                    {
                        if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                            return prop.Value.GetString();
                        if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.Number)
                            return prop.Value.ToString();
                        if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.True || prop.Value.ValueKind == System.Text.Json.JsonValueKind.False)
                            return prop.Value.GetBoolean().ToString();
                    }
                }
            }
            
            // Try searching in nested objects (one level deep)
            foreach (var prop in element.EnumerateObject())
            {
                if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    foreach (var key in keys)
                    {
                        if (prop.Value.TryGetProperty(key, out var nestedValue))
                        {
                            if (nestedValue.ValueKind == System.Text.Json.JsonValueKind.String)
                                return nestedValue.GetString();
                            if (nestedValue.ValueKind == System.Text.Json.JsonValueKind.Number)
                                return nestedValue.ToString();
                        }
                    }
                }
            }
            
            return null;
        }
        
        // Helper function to format dates from API (handles "01/12/2023 00:00:00" format)
        string? FormatDate(string? dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr) || dateStr == "N/A")
                return null;
            
            // Try to parse common date formats
            if (DateTime.TryParse(dateStr, out var date))
            {
                return date.ToString("yyyy-MM-dd");
            }
            
            // Handle "dd/MM/yyyy HH:mm:ss" format
            if (System.Text.RegularExpressions.Regex.IsMatch(dateStr, @"\d{2}/\d{2}/\d{4}"))
            {
                if (DateTime.TryParseExact(dateStr.Split(' ')[0], "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    return parsedDate.ToString("yyyy-MM-dd");
                }
            }
            
            return dateStr; // Return original if can't parse
        }
        
        // Parse and transform the API response to match frontend format
        try
        {
            // Parse the JSON response
            var apiData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(jsonResponse);
            
            // Transform to expected format - mParivahan API uses rc_ prefix
            // Using exact field names from API response for all vehicles
            var transformedResponse = new
            {
                vehicle_details = new
                {
                    // Model - rc_maker_model (e.g., "METEOR 350", "MT 15")
                    model = GetJsonValue(apiData, "rc_maker_model") ?? "N/A",
                    
                    // Fuel Type - rc_fuel_desc (e.g., "PETROL", "DIESEL")
                    fuel_type = GetJsonValue(apiData, "rc_fuel_desc") ?? "N/A",
                    
                    // Manufacturer - rc_maker_desc (e.g., "ROYAL ENFIELD", "Yamaha")
                    manufacturer = GetJsonValue(apiData, "rc_maker_desc") ?? "N/A",
                    
                    // RTO - rc_registered_at is more descriptive (e.g., "KL18, RTO") or fallback to rc_rto_code
                    rto = GetJsonValue(apiData, "rc_registered_at") ?? GetJsonValue(apiData, "rc_rto_code") ?? "N/A",
                    
                    // Registration Date - rc_regn_dt (e.g., "08/19/2021 00:00:00")
                    registration_date = FormatDate(GetJsonValue(apiData, "rc_regn_dt")) ?? "N/A",
                    
                    // Insurance Valid Upto - rc_insurance_upto (e.g., "08/18/2026 00:00:00")
                    insurance_valid_upto = FormatDate(GetJsonValue(apiData, "rc_insurance_upto")) ?? "N/A",
                    
                    // Insurance Company - rc_insurance_comp (e.g., "The New India Assurance Company Ltd.")
                    insurance_company = GetJsonValue(apiData, "rc_insurance_comp") ?? "N/A",
                    
                    // Owner Name - rc_owner_name (e.g., "NEERAJ R")
                    owner_name = GetJsonValue(apiData, "rc_owner_name") ?? "N/A",
                    
                    // Seat Capacity - rc_seat_cap (e.g., "2")
                    seat_capacity = GetJsonValue(apiData, "rc_seat_cap") ?? "N/A",
                    
                    // Vehicle Category - rc_vch_catg (e.g., "2W", "4W")
                    vehicle_category = GetJsonValue(apiData, "rc_vch_catg") ?? "N/A",
                    
                    // Vehicle Class - rc_vh_class_desc (e.g., "Bike", "Car")
                    vehicle_class = GetJsonValue(apiData, "rc_vh_class_desc") ?? "N/A",
                    
                    // Manufacturing Year - rc_manu_month_yr (e.g., 2021)
                    manufacturing_year = GetJsonValue(apiData, "rc_manu_month_yr") ?? "N/A",
                    
                    // Cubic Capacity - rc_cubic_cap (e.g., "349")
                    cubic_capacity = GetJsonValue(apiData, "rc_cubic_cap") ?? "N/A",
                    
                    // Fitness Upto - rc_fit_upto (e.g., "19-Aug-2036")
                    fitness_upto = GetJsonValue(apiData, "rc_fit_upto") ?? "N/A",
                    
                    // Insurance Policy Number - rc_insurance_policy_no (e.g., "71070031210960008746")
                    insurance_policy_no = GetJsonValue(apiData, "rc_insurance_policy_no") ?? "N/A",
                    
                    // State Code - rc_state_code (e.g., "KL", "KA")
                    state_code = GetJsonValue(apiData, "rc_state_code") ?? "N/A"
                }
            };
            
            return Results.Ok(transformedResponse);
        }
        catch (System.Text.Json.JsonException ex)
        {
            // If JSON parsing fails, log and return error
            return Results.Problem(
                detail: $"Failed to parse API response: {ex.Message}. Raw response: {jsonResponse.Substring(0, Math.Min(500, jsonResponse.Length))}",
                statusCode: 500
            );
        }
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem(
            detail: $"Error calling vehicle API: {ex.Message}",
            statusCode: 500
        );
    }
    catch (TaskCanceledException)
    {
        return Results.Problem(
            detail: "Request timeout - the API took too long to respond",
            statusCode: 504
        );
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: $"An error occurred: {ex.Message}",
            statusCode: 500
        );
    }
})
.WithName("GetVehicleInfo");

app.Run();


