using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using SimpleJSON;

using System.Collections;
using System.Text;

public class SearchController : MonoBehaviourSingleton<SearchController>
{
    public string baseURL;
    public string apiCreds;
    public string[] standardParams;

    public float minQueryDelay = 0.5f;

    [SerializeField]
    protected int limit = 10;  
    protected int offset = 0;
    protected string keyword;
    protected Coroutine queryCoroutine;

    public UnityEvent emptyQueryEvent;
    public UnityEvent querySentEvent;
    public UnityEvent itemsReceivedEvent;
    public UnityEvent noItemsReceivedEvent;
    public UnityEvent errorReceiveEvent;

    public void SubmitNewSearch(string _keyword)
    {
        keyword = _keyword;
        SubmitQuery();
    }

    public void SubmitOffset(int _offset)
    {
        offset = _offset;
        SubmitQuery();
    }

    protected void SubmitQuery()
    {
        if (queryCoroutine != null)
            StopCoroutine(queryCoroutine);

        queryCoroutine = StartCoroutine(SubmitQueryCoroutine());
    }
    
    private IEnumerator SubmitQueryCoroutine()
    {
        //don't send too many queries per second
        //when rapidly typing in the search bar make sure 'minQueryDelay' seconds has passed since the last character
        //or else t
        yield return new WaitForSeconds(minQueryDelay);

        //don't query empty strings
        if (keyword == "")
        {
            emptyQueryEvent.Invoke();
            SearchResultsView.Instance.ResetView();

            yield break;
        }

        //build the request url string and send
        string requestURL = string.Format(
            "{0}?q={1}&offset={2}&limit={3}&{4}&{5}",
            baseURL,    //0
            keyword,    //1
            offset,     //2
            limit,      //3
            apiCreds,   //4
            string.Join("&", standardParams)    //5
        );
        DownloadHandler responseHandler = new DownloadHandlerBuffer();
        UnityWebRequest queryRequest = new UnityWebRequest(requestURL, "GET", responseHandler, null);
        UnityWebRequestAsyncOperation asyncOp = queryRequest.SendWebRequest();
        asyncOp.completed += ReceiveResponse;

        queryCoroutine = null;
        querySentEvent.Invoke();

        yield break;
    }

    protected void ReceiveResponse(AsyncOperation asyncOp)
    {
        UnityWebRequest queryRequest = ((UnityWebRequestAsyncOperation)asyncOp).webRequest;

        if(!string.IsNullOrEmpty(queryRequest.error))
        {
            ErrorResultView.Instance.UpdateView(queryRequest.error);

            errorReceiveEvent.Invoke();
        }
        else
        {
            //extract the data from the response
            string responseString = Encoding.ASCII.GetString(queryRequest.downloadHandler.data);
            JSONNode data = JSON.Parse(responseString)["data"];
            
            //got some items
            if (data.Count > 0)
            {
                SearchResultsView.Instance.ReceiveModels(data);
                itemsReceivedEvent.Invoke();
            }
            //got no items :(
            else
            {
                Debug.LogWarning("No items received");
                noItemsReceivedEvent.Invoke();
            }
        }
    }
}
