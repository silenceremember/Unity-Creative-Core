using UnityEngine;

/// <summary>
/// Вешается на любой GameObject. Активирует/деактивирует его (или другой target)
/// в зависимости от текущего GameState.
///
/// Настройка в инспекторе:
///  1. Выбери GameStateChannel
///  2. Укажи в activeInStates[] список состояний когда объект должен быть активен
///  3. Опционально: укажи targetOverride если управляешь не этим GameObject
/// </summary>
public class GameStateListener : MonoBehaviour
{
    [Tooltip("Канал состояний игры")]
    public GameStateChannel channel;

    [Tooltip("Состояния в которых этот объект (или target) активен")]
    public GameState[] activeInStates;

    [Tooltip("Если назначен — управляет этим объектом, иначе управляет собственным GameObject")]
    public GameObject targetOverride;

    private GameObject _target;

    void Awake()
    {
        _target = targetOverride != null ? targetOverride : gameObject;
    }

    void OnEnable()
    {
        if (channel != null)
        {
            channel.OnStateChanged += OnStateChanged;
            // Применяем сразу текущее состояние
            Apply(channel.Current);
        }
    }

    void OnDisable()
    {
        if (channel != null)
            channel.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState state) => Apply(state);

    private void Apply(GameState state)
    {
        bool shouldBeActive = System.Array.IndexOf(activeInStates, state) >= 0;
        if (_target != null && _target.activeSelf != shouldBeActive)
            _target.SetActive(shouldBeActive);
    }
}
