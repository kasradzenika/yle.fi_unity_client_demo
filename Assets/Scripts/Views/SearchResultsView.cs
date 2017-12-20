using UnityEngine;
using SimpleJSON;

using System.Collections.Generic;

public class SearchResultsView : MonoBehaviourSingleton<SearchResultsView>
{
    public string searchItemPoolId;
    public Transform itemsParent;
    protected List<PoolItem> currentItems;
    public JSONNode models;

    private void Start()
    {
        currentItems = new List<PoolItem>();
    }

    public void ReceiveModels(JSONNode _models)
    {
        models = _models;
        UpdateView();
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
