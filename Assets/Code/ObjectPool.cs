using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PoolObjectOriginal
{
    //public string Name;
    public GameObject prefab;
    public ObjectPoolKey key;
    public int Quantity;
}

[System.Serializable]
public class QueueArray
{
    public GameObject[] arr;
    public IPooledObject[] pooledObjectComps;
    public int lastIndex;
}

public class ObjectPool : MonoBehaviour
{
    
    public PoolObjectOriginal[] originals;
    Dictionary<int, QueueArray> pool;
    
    static ObjectPool instance;
    public static ObjectPool s()
    {
        if(instance == null)
        {
            instance = FindObjectOfType<ObjectPool>();
        }
        
        return instance;
    }
    
    void Awake()
    {
        if(instance == null)
        {
            InitPoolOffline();
            // InGameConsole.LogFancy("ObjectPool is initialized!");
        }
        else
        {
            Destroy(this.gameObject);            
        }
    }
    
    
    public void InitPoolOffline()
    {
        pool = new Dictionary<int, QueueArray>(originals.Length);
        
        int len = originals.Length;
        
        for(int i = 0; i < len; i++)
        {
            
            QueueArray queueArray = new QueueArray();
            int objectQuantity = originals[i].Quantity;
            queueArray.arr = new GameObject[objectQuantity];
            queueArray.pooledObjectComps = new IPooledObject[objectQuantity];
            queueArray.lastIndex = 0;
            
            GameObject original = originals[i].prefab;
            
            Vector3 pos = new Vector3(2000, 2000, 2000);
            Quaternion rot = Quaternion.identity;
            
            for(int j = 0; j < objectQuantity; j++)
            {
                GameObject newPoolObject = Instantiate(original, pos, rot, transform);
               
                //    Debug.Log(originals[i].key.ToString());
                
                
                #if UNITY_EDITOR
                newPoolObject.name = newPoolObject.name.Replace("(Clone)", "");
                #endif
                newPoolObject.SetActive(false);
                
                queueArray.arr[j] = newPoolObject;
                IPooledObject ipo = newPoolObject.GetComponent<IPooledObject>();
                if(ipo != null)
                {
                    queueArray.pooledObjectComps[j] = newPoolObject.GetComponent<IPooledObject>(); 
                }
            }
            
            // if((int)originals[i].key == 25)
            // {
            //     InGameConsole.LogFancy(originals[i].key.ToString());
            //     InGameConsole.LogFancy(originals[i].key.ToString());
            //     InGameConsole.LogFancy(originals[i].key.ToString());
            //     InGameConsole.LogFancy(originals[i].key.ToString());
            //     InGameConsole.LogFancy(originals[i].key.ToString());
            // }
            
            int key = (int)originals[i].key;
            
            pool.Add(key, queueArray);
        }
    }
    
    public void ResetPool()
    {
        foreach(KeyValuePair<int, QueueArray> pair in pool)
        {
            int len = pair.Value.arr.Length;
            
            for(int i = 0; i < len; i++)
            {
                if(pair.Value.pooledObjectComps[i] != null)
                {
                    pair.Value.pooledObjectComps[i].InitialState();
                    
                }
                pair.Value.arr[i].SetActive(false);
            }
        }
        
        InGameConsole.LogFancy(string.Format("Pool {0} was reset!", this.gameObject.name));
    }
    
    public GameObject Get(ObjectPoolKey key, bool callInitialState = true)
    {
        GameObject Result = null;
        
        int poolKey = (int)key;
        
        if(pool.ContainsKey(poolKey))
        {
            QueueArray queueArray = pool[poolKey];
            
            int lastIndex = queueArray.lastIndex;
            
            Result = queueArray.arr[lastIndex];
            Result.SetActive(true);
            
            if(callInitialState)
            {
                if(queueArray.pooledObjectComps[lastIndex] != null)
                {
                    queueArray.pooledObjectComps[lastIndex].InitialState();
                }
            }
            
            queueArray.lastIndex++;
            if(queueArray.lastIndex >= queueArray.arr.Length)
            {
                queueArray.lastIndex = 0;
            }
        }
        else
        {
            InGameConsole.LogError(string.Format("Pool does not contain key {0}", key));
        }
        
        return Result;
    }
    
   
    
}
