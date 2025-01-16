using UnityEngine;
using UnityEngine.SceneManagement;

public class TrackSelection : MonoBehaviour
{

    public static string SelectedTrack = "";

    public void SelectTrack1()
    {
        DataManager.Instance.SelectedTrack = "Monza";
        SceneManager.LoadScene("SampleScene"); // Load the simulation
    }

    public void SelectTrack2()
    {
        DataManager.Instance.SelectedTrack = "Redbull";
        SceneManager.LoadScene("SampleScene"); // Load the simulation
    }
}