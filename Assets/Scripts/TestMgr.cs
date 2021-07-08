using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMgr : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Destroy")]
    public void DestroyCHildren()
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Debug.Log(0);
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
}
