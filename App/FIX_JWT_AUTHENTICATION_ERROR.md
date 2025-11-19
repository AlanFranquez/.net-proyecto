# Fix JWT Authentication Error - IDX10500

## Error Description
```
Microsoft.IdentityModel.Tokens.SecurityTokenSignatureKeyNotFoundException: IDX10500: 
Signature validation failed. No security keys were provided to validate the signature.
```

## Root Cause
Your backend API (`Espectaculos.WebApi`) is configured to use AWS Cognito for JWT authentication, but the signing keys from Cognito are not being retrieved properly.

## Solution

### Step 1: Update Backend Program.cs

Navigate to your backend:
```
C:\Users\Carlos\source\repos\.net-proyecto\BACKEND\LabNet\src\Espectaculos.WebApi\Program.cs
```

Replace the JWT Bearer configuration (around line 105-137) with:

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.MetadataAddress = $"{authority}/.well-known/openid-configuration";
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authority,
            ValidateAudience = true,
            ValidAudience = cognitoSettings.ClientId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Log.Warning("JWT Authentication failed: {Error}", ctx.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx =>
            {
                Log.Information("JWT Token validated successfully for user: {User}", 
                    ctx.Principal?.Identity?.Name ?? "Unknown");
                return Task.CompletedTask;
            },
            OnMessageReceived = ctx =>
            {
                if (string.IsNullOrEmpty(ctx.Request.Headers["Authorization"]))
                {
                    if (ctx.Request.Cookies.TryGetValue("espectaculos_session", out var tokenFromCookie))
                    {
                        ctx.Token = tokenFromCookie;
                    }
                }
                return Task.CompletedTask;
            }
        };
    });
```

### Step 2: Key Changes Explained

1. **`options.MetadataAddress`**: Explicitly sets the metadata endpoint where JWKS is located
2. **`options.RequireHttpsMetadata`**: Allows HTTP in development (HTTPS required in production)
3. **`ValidateIssuerSigningKey = true`**: Ensures the signing key is validated
4. **Event Handlers**: Added logging to help diagnose authentication issues

### Step 3: Verify AWS Cognito Configuration

Check that these values in `appsettings.json` are correct:

```json
"AWS:Cognito": {
  "Region": "us-east-1",
  "UserPoolId": "us-east-1_ibDjeR6P5",
  "ClientId": "1k8flsfjaen8q96ltrtfs5m7lu"
}
```

### Step 4: Test the Metadata Endpoint

Run this PowerShell command to verify Cognito is accessible:

```powershell
$authority = "https://cognito-idp.us-east-1.amazonaws.com/us-east-1_ibDjeR6P5"
$metadataUrl = "$authority/.well-known/openid-configuration"
Invoke-RestMethod -Uri $metadataUrl
```

Expected output should include `jwks_uri` field.

### Step 5: Restart Your Backend API

After making the changes:

```powershell
cd "C:\Users\Carlos\source\repos\.net-proyecto\BACKEND\LabNet\src\Espectaculos.WebApi"
dotnet build
dotnet run
```

### Step 6: Check Logs

After restart, you should see:
- ? No more IDX10500 errors
- ?? JWT validation logs showing success or specific failures
- ?? Better diagnostic information

## Alternative Solution (If Above Doesn't Work)

If the metadata endpoint is unreachable or blocked, you can manually configure the signing key:

```csharp
// Add this NuGet package first:
// dotnet add package Microsoft.IdentityModel.Tokens

var signingKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(builder.Configuration["ValidationTokens:Secret"]!));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["ValidationTokens:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["ValidationTokens:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey
        };
        
        // ... rest of events ...
    });
```

**Note:** This alternative uses HMAC-SHA256 with a shared secret instead of AWS Cognito's RSA keys.

## Common Issues

### Issue 1: Network/Firewall Blocking Cognito
**Symptom:** Metadata cannot be downloaded
**Solution:** Check firewall rules, proxy settings, or use alternative solution above

### Issue 2: Invalid AWS Cognito Credentials
**Symptom:** 404 or 401 when accessing metadata
**Solution:** Verify UserPoolId and Region are correct

### Issue 3: Mixed Authentication Schemes
**Symptom:** Some endpoints work, others don't
**Solution:** Ensure all endpoints use consistent `[Authorize]` attributes

## Testing

After applying the fix, test with a valid Cognito token:

```powershell
$token = "YOUR_COGNITO_JWT_TOKEN"
$headers = @{
    "Authorization" = "Bearer $token"
}
Invoke-RestMethod -Uri "https://localhost:7001/api/your-endpoint" -Headers $headers
```

## Need More Help?

If the issue persists:
1. Check the application logs for the new authentication event messages
2. Verify the token format using https://jwt.io
3. Ensure the token was issued by your Cognito User Pool
4. Check token expiration (`exp` claim)

---

**Created:** $(Get-Date)
**Backend Location:** `C:\Users\Carlos\source\repos\.net-proyecto\BACKEND\LabNet\`
**AWS Cognito Pool:** `us-east-1_ibDjeR6P5`
