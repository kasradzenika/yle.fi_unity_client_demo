using UnityEngine;
using System.Collections.Generic;

public class PoolController : MonoBehaviour
{
    public string poolId;
    public GameObject poolItemPrefab;
    private List<PoolItem> items;
    private List<PoolItem> spawnedItems;

    public static Dictionary<string, PoolController> instances;

	void Awake ()
    {
        if (instances == null)
            instances = new Dictionary<string, PoolController>();
        instances.Add(poolId, this);

        items = new List<PoolItem>();
        spawnedItems = new List<PoolItem>();

        foreach (PoolItem item in GetComponentsInChildren<PoolItem>(true))
        {
            PutItem(item);
        }
	}

    private void OnDestroy()
    {
        instances.Remove(poolId);
    }

    public PoolItem GetItem(bool random = true)
    {
        //if no items are left in the pool spawn a new prefab
        //this should preferably NOT happen in production
        if (items.Count == 0)
            SpawnNewPoolItem();

        int index = 0;
        if (random)
            index = Random.Range(0, items.Count);

        PoolItem t = null;
        if (items.Count > 0)
        {
            t = items[index];
            spawnedItems.Add(items[index]);
            items.RemoveAt(index);
            t.gameObject.SetActive(true);
        }
        return t;
    }

    public void PutItem(PoolItem t)
    {
        //NOTE this can be removed later for optimization
        if(items.Contains(t))
        {
            Debug.LogError("Aready in the pool! - " + transform.name);
            return;
        }
        if(spawnedItems.Contains(t))
        {
            spawnedItems.Remove(t);
        }

        t.transform.SetParent( transform );
        t.gameObject.SetActive(false);
        items.Add(t);
    }

    public static PoolItem GetItemFromPool(string _poolId)
    {
        PoolItem t = null;
        if (instances.ContainsKey(_poolId))
            t = instances[_poolId].GetItem();

        return t;
    }

    public static void PutItemInPool(PoolItem t, string _poolId)
    {
        if (instances.ContainsKey(_poolId))
            instances[_poolId].PutItem(t);
        else
            Debug.LogWarning("No pool with id: " + _poolId);
    }

    public static void ReturnAllSpawnedItems(string _poolId)
    {
        if (instances.ContainsKey(_poolId))
            instances[_poolId].ReturnAllSpawnedItems();
        else
            Debug.LogWarning("No pool with id: " + _poolId);
    }

    public void ReturnAllSpawnedItems()
    {
        while(spawnedItems.Count > 0)
        {
            PutItem(spawnedItems[0]);
        }
    }

    public void SpawnNewPoolItem()
    {
        GameObject newItem = Instantiate(poolItemPrefab);

        PutItem(newItem.GetComponent<PoolItem>());
    }
}
