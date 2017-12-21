using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;

using System.Collections.Generic;

public class SearchResultsView : MonoBehaviourSingleton<SearchResultsView>
{
    public string searchItemPoolId;
    public Transform itemsParent;
    public float loadMoreResultsThreshold = 100f;
    public ScrollRect scroll;
    public GameObject loadingMoreResultsGameObject;
    protected bool expandingResults = false;
    protected List<PoolItem> currentItems;
    public JSONNode models;

    private void Start()
    {
        currentItems = new List<PoolItem>();
    }

    public void ReceiveModels(JSONNode _models)
    {
        models = _models;
        expandingResults = false;
        loadingMoreResultsGameObject.SetActive(false);

        UpdateView();
    }

    float relativeYPos = 0;
    public void OnScrollView(Vector2 delta)
    {
        if (SearchController.Instance.curOperationType != RequestOperationType.Idle ||
            delta.y == 0f)  //make sure the scroll is ONLY user-generated
            return;

        //I've figured this out with trial and error
        //not quite sure what's going on but it works
        relativeYPos = scroll.content.rect.height - scroll.content.localPosition.y;
        //Debug.Log(relativeYPos + " "  + (scroll.viewport.rect.height / 2 - loadMoreResultsThreshold));

        if(SearchController.Instance.canLoadMoreItems &&
            relativeYPos < scroll.viewport.rect.height / 2 - loadMoreResultsThreshold)
        {
            expandingResults = true;

            SearchController.Instance.LoadMoreResults();
            loadingMoreResultsGameObject.SetActive(true);
            loadingMoreResultsGameObject.transform.SetAsLastSibling();
        }
    }

    public void ResetView()
    {
        foreach (PoolItem item in currentItems)
        {
            item.ReturnToPool();
        }

        currentItems.Clear();
    }

    public void UpdateView()
    {
        PoolItem newPoolItem;
        ResultItemView newItemView;
        for (int i = 0; i < models.Count; i++)
        {
            newPoolItem = PoolController.instances[searchItemPoolId].GetItem();
            newPoolItem.transform.SetParent(itemsParent);
            newPoolItem.transform.localScale = Vector3.one;
            currentItems.Add(newPoolItem);

            //this is a bit heavy but as long as it's not more than 10 times per frame it's ok
            newItemView = newPoolItem.GetComponent<ResultItemView>();

            //I blindly consider all the films have a title on at least one language. Choose the first language and display
            //EDIT: I used to use ["fi"] to get Finnish title instead of [0] but not all programs have Finnish titles
            newItemView.title = models[i]["title"][0];
            newItemView.UpdateView();
        }
    }
}
