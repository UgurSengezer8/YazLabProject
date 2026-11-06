using UnityEngine;
using UnityEngine.SceneManagement;

public class AnaMenuController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void YeniOyunBaslat()
    {
        SceneManager.LoadScene(0);

    }

    public void OyundanCik()
    {
        Debug.Log("Oyundan çıkıldı!");

        Application.Quit();
    }
}
