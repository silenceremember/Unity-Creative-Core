/// <summary>
/// Состояния игры. Переход управляется через GameStateChannel.
/// </summary>
public enum GameState
{
    Menu        = 0,  // главное меню + настройки (один канвас-набор)
    IntroCrawl  = 1,  // Star Wars кроул
    VisualNovel = 2,  // визуальная новелла The Smith Family
    Gameplay    = 3,  // исследование (ambient нарратор, таймер, кликер)
    Quest       = 4,  // квест с картинами
}
