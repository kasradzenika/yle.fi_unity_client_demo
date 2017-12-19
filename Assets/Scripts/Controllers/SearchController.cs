using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

using System.Collections;
using System.Text;

public class SearchController : MonoBehaviour
{
    public string baseURL;
    public string apiCreds;
    public string[] standardParams;

    public float minQueryDelay = 0.5f;
    
    protected Coroutine queryCoroutine;

    public void SubmitQuery(string query)
    {
        //don't query empty strings
        if (query == "")
            return;

        if (queryCoroutine != null)
            StopCoroutine(queryCoroutine);

        queryCoroutine = StartCoroutine(SubmitQueryCoroutine(query));
    }
    
    private IEnumerator SubmitQueryCoroutine(string query)
    {
        //don't send too many queries per second
        //when rapidly typing in the search bar make sure 'minQueryDelay' seconds has passed since the last character
        //or else t
        yield return new WaitForSeconds(minQueryDelay);
        
        //build the request url string and send
        string requestURL = string.Format("{0}?q={1}&{2}&{3}", baseURL, query, apiCreds, string.Join("&", standardParams));
        DownloadHandler downloadHandler = new DownloadHandlerBuffer();
        UnityWebRequest webRequest = new UnityWebRequest(requestURL, "GET", downloadHandler, null);
        UnityWebRequestAsyncOperation asyncOp = webRequest.SendWebRequest();
        asyncOp.completed += ReceiveResponse;

        queryCoroutine = null;
        yield break;
    }

    protected void ReceiveResponse(AsyncOperation asyncOp)
    {
        UnityWebRequest originalRequest = ((UnityWebRequestAsyncOperation)asyncOp).webRequest;
        string responseString = Encoding.ASCII.GetString(originalRequest.downloadHandler.data);

        Debug.Log(SimpleJSON.JSON.Parse(responseString)["data"][0]["title"]["fi"]);
    }
}
