using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIMgr : MonoBehaviour
{
    public InputAction SetRespInputBtn;
    public InputAction EnableTrailInputBtn;
    public InputAction ClearTrailInputBtn;

    [Header("UIWidget")]
    public Toggle RespControlToggle;
    public Toggle ShowTrailToggle;
    public Button ClearTrailBtn;

    private static UIMgr instance;
    public static UIMgr Instance
    {
        get { return instance; }
    }

    public static bool IsValid
    {
        get { return instance != null; }
    }

    private void Awake()
    {
        #region µ¥Àý
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
        #endregion

        InitInput();
    }

    private void OnEnable()
    {
        SetRespInputBtn.Enable();
        EnableTrailInputBtn.Enable();
        ClearTrailInputBtn.Enable();

        ClearTrailBtn.onClick.AddListener(ClearTrail);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ClearTrail()
    {
        PoolManager.instance.ReturnPoolAll("Trail");
    }

    private void InitInput()
    {
        SetRespInputBtn.performed += ctx =>
        {
            RespControlToggle.isOn = !RespControlToggle.isOn;
        };

        EnableTrailInputBtn.performed += ctx =>
        {
            ShowTrailToggle.isOn = !ShowTrailToggle.isOn;
        };

        ClearTrailInputBtn.performed += ctx =>
        {
            ClearTrail();
        };
    }


}
