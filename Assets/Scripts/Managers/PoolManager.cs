using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public List<PoolObject> poolObjects = new List<PoolObject>();

    public static PoolManager instance;

    private Dictionary<string, Queue<GameObject>> poolDic = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        #region 单例
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        DontDestroyOnLoad(gameObject);
        # endregion
    }

    private void Start()
    {
        //填充字典
        foreach(PoolObject obj in poolObjects)
        {
            Queue<GameObject> newQueue = new Queue<GameObject>();

            for(int i=0;i<obj.size;i++)
            {
                GameObject newObject = Instantiate(obj.prefab, gameObject.transform);
                newObject.SetActive(false);
                newQueue.Enqueue(newObject);
            }

            poolDic.Add(obj.objectTag, newQueue);

        }

    }

    public GameObject GetFromPool(string tag)
    {
        //找不到对象池
        if(!poolDic.ContainsKey(tag))
        {
            Debug.LogError("Object pool doesn't contain this object's pool");
            return null;
        }

        //不够，填充队列
        if(poolDic[tag].Count==0)
        {
            FillPool(tag);
        }

        GameObject newObj = poolDic[tag].Dequeue();

        newObj.SetActive(true);

        return newObj;
    }

    public GameObject GetFromPool(string tag, Vector3 iniPosition)
    {
        //找不到对象池
        if (!poolDic.ContainsKey(tag))
        {
            Debug.LogError("Object pool doesn't contain this object's pool");
            return null;
        }

        //不够，填充队列
        if (poolDic[tag].Count == 0)
        {
            FillPool(tag);
        }

        GameObject newObj = poolDic[tag].Dequeue();

        //设置出队物体属性
        newObj.SetActive(true);
        newObj.transform.position = iniPosition;
        return newObj;
    }

    public GameObject GetFromPool(string tag, Vector3 iniPosition, Quaternion iniRotation)
    {
        //找不到对象池
        if (!poolDic.ContainsKey(tag))
        {
            Debug.LogError("Object pool doesn't contain this object's pool");
            return null;
        }

        //不够，填充队列
        if (poolDic[tag].Count == 0)
        {
            FillPool(tag);
        }

        GameObject newObj = poolDic[tag].Dequeue();

        //设置出队物体属性
        newObj.SetActive(true);
        newObj.transform.position = iniPosition;
        newObj.transform.rotation = iniRotation;

        return newObj;
    }

    //填池子
    public void FillPool(string tag)
    {
        for(int i=0;i< poolObjects.Count;i++)
        {
            if(poolObjects[i].objectTag==tag)
            {
                for (int j = 0; j < 10; j++)
                {
                    GameObject newObject = Instantiate(poolObjects[i].prefab, gameObject.transform);
                    newObject.SetActive(false);
                    poolDic[tag].Enqueue(newObject);
                }
                break;
            }
        }

        
    }

    //回池子
    public void ReturnPool(GameObject poolObj,string tag)
    {
        if(poolObj.activeSelf)
        {
            if (!poolDic.ContainsKey(tag))
            {
                Debug.LogError("Object pool doesn't contain this object's pool");
                return;
            }

            //不显示
            poolObj.SetActive(false);

            //重置父物体
            if (poolObj.transform.parent != transform)
            {
                poolObj.transform.parent = transform;
            }

            //回对应池子队列
            poolDic[tag].Enqueue(poolObj);

        }


    }

    //一种tag的物体全部回池子
    public void ReturnPoolAll(string tag)
    {
        ReturnPool[] returnPoolComps = (ReturnPool[])FindObjectsOfType(typeof(ReturnPool));
        foreach (var returnComp in returnPoolComps)
        {
            if(string.Equals(returnComp.objPoolTag,tag))
            {
                ReturnPool(returnComp.gameObject, tag);
            }
        }
    }

    [System.Serializable]
    public class PoolObject
    {
        public string objectTag;
        public GameObject prefab;
        public int size;
    }

}
