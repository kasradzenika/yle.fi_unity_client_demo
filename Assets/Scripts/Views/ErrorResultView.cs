using UnityEngine;
using UnityEngine.UI;

public class ErrorResultView : MonoBehaviourSingleton<ErrorResultView>
{
    public string errorPrefix;
    public string error;
    public Text errorText;

    public void UpdateView()
    {
        errorText.text = string.Format("{0}{1}", errorPrefix, error);
        Debug.LogWarning("Error: " + error);
    }

    public void UpdateView(string _error)
    {
        error = _error;
        UpdateView();
    }
}
