using UnityEngine;

public class PoolItem : MonoBehaviour
{
    public string poolId;
	
    public virtual void ReturnToPool()
    {
        PoolController.PutItemInPool(this, poolId);
    }
}
