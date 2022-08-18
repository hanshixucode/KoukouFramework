using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTName : MonoBehaviour
{
    private void Awake()
    {
        var localpos = transform.localPosition;
        var worldpos = transform.TransformPoint(localpos);
        Debug.LogError(this.gameObject.name + "local:"+localpos+"world"+worldpos);

    }
}
