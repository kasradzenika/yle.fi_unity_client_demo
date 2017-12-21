using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Networking;

using SimpleJSON;

using System.Collections;

public class ItemDetailedView : MonoBehaviourSingleton<ItemDetailedView>
{
    public JSONNode model;
    public Image displayImage;
    public Image displayImageBg;
    public Sprite noImageSprite;
    public float imageMaxSize;
    public float imageBgOffset;
    public float noImageSize;
    public float noImageBgSize;
    public string imageHost;
    public string imageTransformations;
    public Text titleText;
    public Text descriptionText;
    public Text partOfSeriesText;
    public Text typeMediaText;
    public float viewResetDelay = 1f;

    public UnityEvent onDisplayView;
    public UnityEvent onHideView;
    public UnityEvent onResetView;

    public void DisplayItem(JSONNode _model)
    {
        model = _model;

        if (!string.IsNullOrEmpty(model["description"]["fi"]))
            titleText.text = model["title"]["fi"];
        else
            titleText.text = model["title"][0];

        if (!string.IsNullOrEmpty(model["description"]["fi"]))
            descriptionText.text = model["description"]["fi"];
        else
            descriptionText.text = model["description"][0];

        if (!string.IsNullOrEmpty(model["partOfSeries"]["title"]["fi"]))
            partOfSeriesText.text = model["partOfSeries"]["title"]["fi"];
        else
            partOfSeriesText.text = model["partOfSeries"]["title"][0];

        typeMediaText.text = model["typeMedia"].Value;

        StartCoroutine(LoadItemImage());

        onDisplayView.Invoke();
    }

    public void OnClickClose()
    {
        StartCoroutine(CloseViewCoroutine());
    }

    private IEnumerator CloseViewCoroutine()
    {
        onHideView.Invoke();

        //wait for animation before the reset
        yield return new WaitForSeconds(viewResetDelay);

        displayImage.sprite = noImageSprite;

        displayImage.rectTransform.offsetMin = new Vector2(-noImageSize, -noImageSize);
        displayImage.rectTransform.offsetMax = new Vector2(noImageSize, noImageSize);

        displayImageBg.rectTransform.offsetMin = new Vector2(-noImageBgSize, -noImageBgSize);
        displayImageBg.rectTransform.offsetMax = new Vector2(noImageBgSize, noImageBgSize);

        onResetView.Invoke();

        yield break;
    }

    private IEnumerator LoadItemImage()
    {
        string imageUrl = string.Format("{0}/{1}/{2}.png", imageHost, imageTransformations, model["image"]["id"].Value);

        DownloadHandlerTexture imageDownloader = new DownloadHandlerTexture();
        UnityWebRequest imageRequest = new UnityWebRequest(imageUrl, "GET", imageDownloader, null);
        UnityWebRequestAsyncOperation imageRequestOperation = imageRequest.SendWebRequest();

        //wait for response
        yield return imageRequestOperation;

        if (imageRequest.error == null)
        {
            yield return new WaitUntil(() => imageDownloader.isDone);

            if(imageDownloader.texture != null)
            {
                Texture2D imageTex = imageDownloader.texture;
                displayImage.sprite = Sprite.Create(imageTex, new Rect(0f, 0f, imageTex.width, imageTex.height), Vector2.one);

                //fit the image UI to to the downlaoded texture:
                if (imageTex.width > imageTex.height)
                {
                    displayImage.rectTransform.offsetMin = new Vector2(-imageMaxSize, -imageMaxSize * imageTex.height / imageTex.width);
                    displayImage.rectTransform.offsetMax = new Vector2(imageMaxSize, imageMaxSize * imageTex.height / imageTex.width);

                    //for border I could use Unity's built-in UI shadow effect but it gave me hell as of 2017.3.0f3
                    displayImageBg.rectTransform.offsetMin = new Vector2(-imageMaxSize - imageBgOffset, -imageMaxSize * imageTex.height / imageTex.width - imageBgOffset);
                    displayImageBg.rectTransform.offsetMax = new Vector2(imageMaxSize + imageBgOffset, imageMaxSize * imageTex.height / imageTex.width + imageBgOffset);
                }
                else
                {
                    displayImage.rectTransform.offsetMin = new Vector2(-imageMaxSize * imageTex.width / imageTex.height, -imageMaxSize);
                    displayImage.rectTransform.offsetMax = new Vector2(imageMaxSize * imageTex.height / imageTex.width, imageMaxSize);

                    displayImageBg.rectTransform.offsetMin = new Vector2(-imageMaxSize * imageTex.width / imageTex.height - imageBgOffset, -imageMaxSize - imageBgOffset);
                    displayImageBg.rectTransform.offsetMax = new Vector2(imageMaxSize * imageTex.height / imageTex.width + imageBgOffset, imageMaxSize + imageBgOffset);
                }
            }
        }
        else
        {
            Debug.LogWarning("item image couldn't be downloaded");
        }

        yield break;
    }
}
