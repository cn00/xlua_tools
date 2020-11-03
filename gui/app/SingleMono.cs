using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics;

public interface SingleDefault
{
    SingleDefault SingleDefault { get; }
}

public class SingleMono<T> where T : class, new()
{
    static T mInstance = null;


    public static T Instance
    {
        get
        {
            if(mInstance == null)
            {
                mInstance = new T();
            }
            return mInstance;
        }
    }

    public virtual void Awake()
    {
        Inited = false;
        // StartCoroutine(Init()); // 手动管理启动顺序
    }

    public bool Inited
    {
        get;
        protected set;
    }

    /// <summary>
    /// load resources etc.
    /// </summary>
    /// <returns></returns>
    public virtual bool Init()
    {
        Inited = true;
        Debug.WriteLine(typeof(T).ToString() + "inited.");
        return true;
    }
}
