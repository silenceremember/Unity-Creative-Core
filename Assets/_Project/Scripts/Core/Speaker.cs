/// <summary>
/// Visual novel speaker identity.
/// </summary>
public enum Speaker
{
    John,
    Mary,
    Player
}

/// <summary>
/// Localized display names for Speaker.
/// </summary>
public static class SpeakerExtensions
{
    public static string GetDisplayName(this Speaker speaker, GameLanguage lang)
    {
        return lang switch
        {
            GameLanguage.English => speaker switch
            {
                Speaker.John   => "John",
                Speaker.Mary   => "Mary",
                Speaker.Player => "Player 1",
                _ => speaker.ToString()
            },
            _ => speaker switch
            {
                Speaker.John   => "Джон",
                Speaker.Mary   => "Мэри",
                Speaker.Player => "Player 1",
                _ => speaker.ToString()
            }
        };
    }
}
