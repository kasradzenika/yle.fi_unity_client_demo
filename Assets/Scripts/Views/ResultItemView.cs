using UnityEngine;
using UnityEngine.UI;

public class ResultItemView : MonoBehaviour
{
    public string title;
    public Text titleText;

    public void UpdateView()
    {
        titleText.text = title;
    }
}
