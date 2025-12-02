using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;  //DOTween‚ðŽg‚¤‚½‚ß‚ÉŒÄ‚Ño‚µ‚½

public class TitleAnimation: MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 
        this.transform.DOMove(new Vector3(2.4f, 8.0f, -14.7f),2.0f)
            .SetEase(Ease.InOutBack)
            .OnKill(() =>
            {
                this.Appeal();
            }
            );
    }

    public void Appeal()
    {
        float a = 2.6f;
        float b = 2.3f;

        this.transform.DOScale(new Vector3(a, a, a), 0.4f)
            .SetEase(Ease.OutQuart)
        .OnKill(() =>
        {
            this.transform.DOScale(new Vector3(b, b, b), 0.4f)
            .SetEase(Ease.InQuart)
            .OnKill(() =>
            {
                this.Appeal();
            });
        });
    }
}
