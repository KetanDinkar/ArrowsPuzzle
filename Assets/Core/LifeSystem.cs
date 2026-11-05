using UnityEngine;

public class LifeSystem : MonoBehaviour
{
    public int maxLives = 3;
    public int Lives { get; private set; }
    public event System.Action<int> OnLivesChanged;
    public event System.Action OnLivesEmpty;

    void Awake() { Lives = maxLives; }

    public void Lose(int amount = 1)
    {
        if (Lives <= 0) return;
        Lives = Mathf.Max(0, Lives - amount);
        OnLivesChanged?.Invoke(Lives);
        if (Lives == 0) OnLivesEmpty?.Invoke();
    }
}