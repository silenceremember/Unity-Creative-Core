/// <summary>
/// Game states. Transitions are managed via GameStateChannel.
/// </summary>
public enum GameState
{
    Menu        = 0,  // main menu + settings
    IntroCrawl  = 1,  // Star Wars crawl
    VisualNovel = 2,  // visual novel "The Smith Family"
    Gameplay    = 3,  // exploration (ambient narrator, timer, clicker)
    Quest       = 4,  // painting quest
    Final       = 5,  // final sequence (fall, freeze, quit)
}
