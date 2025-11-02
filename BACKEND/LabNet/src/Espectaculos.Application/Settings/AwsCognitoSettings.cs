namespace Espectaculos.Application.Settings;

public class AwsCognitoSettings
{
    public string UserPoolId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
}

public class AwsSettings
{
    public string Region { get; set; } = default!;
    public CognitoSettings Cognito { get; set; } = default!;
}

public class CognitoSettings
{
    public string UserPoolId { get; set; } = default!;
    public string ClientId { get; set; } = default!;
}