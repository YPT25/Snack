using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;  //DOTween‚ğg‚¤‚½‚ß‚ÉŒÄ‚Ño‚µ‚½

public class TitleAnimation: MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 
        this.transform.DOMove(new Vector3(2.4f, 8.0f, -14.7f),2.0f)
            .SetEase(Ease.InOutBack);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
