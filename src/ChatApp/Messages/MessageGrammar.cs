using System.Text.RegularExpressions;


namespace ChatApp.Messages;

public static class MessageGrammar
{
    public static bool IsId(string contentComponent)
    {
        const string pattern = @"^[A-Za-z0-9-.]{1,20}$";
        return Regex.IsMatch(contentComponent, pattern);
    }

    public static bool IsSecret(string contentComponent)
    {
        const string pattern = @"^[A-Za-z0-9-]{1,128}$";
        return Regex.IsMatch(contentComponent, pattern);
    }

    public static bool IsContent(string contentComponent)
    {
        const string pattern = @"^[\x20-\x7E\s]{1,1400}$";
        return Regex.IsMatch(contentComponent, pattern);
    }
    
    public static bool IsDname(string contentComponent)
    {
        const string pattern = @"^[\x21-\x7E\s]{1,20}$";
        return Regex.IsMatch(contentComponent, pattern);
    }
    
    public static bool IsComponentIS(string contentComponent)
    {
        return contentComponent.Equals("IS", StringComparison.OrdinalIgnoreCase);
    }
    
    public static bool IsComponentAS(string contentComponent)
    {
        return contentComponent.Equals("AS", StringComparison.OrdinalIgnoreCase);
    }
    
    public static bool IsComponentUSING(string contentComponent)
    {
        return contentComponent.Equals("USING", StringComparison.OrdinalIgnoreCase);
    }
    
    public static bool IsComponentOKorNOK(string contentComponent)
    {
        return contentComponent.Equals("OK", StringComparison.OrdinalIgnoreCase) 
               || contentComponent.Equals("NOK", StringComparison.OrdinalIgnoreCase);
    }
    
}