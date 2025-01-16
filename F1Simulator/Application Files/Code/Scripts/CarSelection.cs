using UnityEngine;
using UnityEngine.SceneManagement;

public class CarSelection : MonoBehaviour
{

    public static string SelectedCar = "";
    public void SelectEasyCar()
    {
        DataManager.Instance.SelectedCar = "Easy";
        SceneManager.LoadScene("TrackSelection"); // Load the track selection screen
    }

    public void SelectDifficultCar()
    {
        DataManager.Instance.SelectedCar = "Difficult";
        SceneManager.LoadScene("TrackSelection"); // Load the track selection screen
    }
}