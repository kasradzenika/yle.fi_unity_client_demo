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
    [SerializeField]
    protected int offsetStep = 10;
    protected int offset = 0;
    protected string keyword;
    [System.NonSerialized]
    public bool canLoadMoreItems = false;
    [System.NonSerialized]
    public RequestOperationType curOperationType;
    protected Coroutine queryCoroutine;

    public UnityEvent emptyQueryEvent;
    public UnityEvent newQuerySentEvent;
    public UnityEvent loadMoreResultsSentEvent;
    public UnityEvent itemsReceivedEvent;
    public UnityEvent noItemsReceivedEvent;
    public UnityEvent errorReceiveEvent;

    public void SubmitNewSearch(string _keyword)
    {
        keyword = _keyword;
        offset = 0;
        canLoadMoreItems = false;
        curOperationType = RequestOperationType.LoadNewItems;

        SubmitQuery();
    }

    public void LoadMoreResults()
    {
        offset += offsetStep;
        curOperationType = RequestOperationType.LoadMoreItems;

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
        if(curOperationType == RequestOperationType.LoadNewItems)
            yield return new WaitForSeconds(minQueryDelay);

        //don't query empty strings
        if (keyword == "")
        {
            emptyQueryEvent.Invoke();
            curOperationType = RequestOperationType.Idle;

            //let the animation fade out the results
            yield return new WaitForSeconds(0.3f);

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
        
        if(curOperationType == RequestOperationType.LoadMoreItems)
        {
            loadMoreResultsSentEvent.Invoke();
        }
        //I'm loading new results
        else if (curOperationType == RequestOperationType.LoadNewItems)
        {
            newQuerySentEvent.Invoke();
        }

        yield break;
    }

    protected void ReceiveResponse(AsyncOperation asyncOp)
    {
        UnityWebRequest queryRequest = ((UnityWebRequestAsyncOperation)asyncOp).webRequest;
        
        if (!string.IsNullOrEmpty(queryRequest.error))
        {
            ErrorResultView.Instance.UpdateView(queryRequest.error);

            errorReceiveEvent.Invoke();
        }
        else
        {
            //extract the data from the response
            string responseString = Encoding.UTF8.GetString(queryRequest.downloadHandler.data);
            JSONNode responseJSON = JSON.Parse(responseString);
            JSONNode meta = responseJSON["meta"];
            JSONNode data = responseJSON["data"];

            //invoke events only if the search keyword was not changed since this request was sent
            //otherwise disregard this response
            string queryKeyword = meta["q"];
            if (queryKeyword == keyword)
            {
                int indexOfLastItem = meta["offset"].AsInt + (int)meta["limit"].AsInt;
                int itemCount = meta["count"].AsInt;

                canLoadMoreItems = indexOfLastItem < itemCount;

                //got some items
                if (data.Count > 0)
                {
                    SearchResultsView.Instance.ReceiveModels(data);
                    itemsReceivedEvent.Invoke();
                }
                //got no items :(
                else
                {
                    noItemsReceivedEvent.Invoke();
                }
            }
            else
            {
                Debug.Log("Disregarding a response. Reason: new keyword");
            }
        }

        curOperationType = RequestOperationType.Idle;
    }
}

public enum RequestOperationType
{
    Idle,
    LoadNewItems,
    LoadMoreItems
}