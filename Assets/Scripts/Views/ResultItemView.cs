using UnityEngine;
using UnityEngine.UI;

using SimpleJSON;

public class ResultItemView : MonoBehaviour
{
    public JSONNode model;
    public Text titleText;
    public Button displayDetailedInfoButton;

    public void UpdateView(JSONNode _model)
    {
        model = _model;

        //try to extract Finnish title
        //if Finnish title is not present than display the title in the first available language
        if (!string.IsNullOrEmpty(model["title"]["fi"]))
        {
            titleText.text = model["title"]["fi"];
        }
        else
        {
            titleText.text = model["title"][0];
        }
    }

    public void OnClickDisplayModel()
    {
        ItemDetailedView.Instance.DisplayItem(model);
    }
}
