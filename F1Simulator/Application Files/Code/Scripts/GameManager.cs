using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string SelectedCar; // "Easy" or "Difficult"
    public string SelectedTrack; // e.g., "Track1" or "Track2"

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
