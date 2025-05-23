using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask _whoCanCollect = LayerMaskHelper.CreateLayerMask(9);

    private Collectables _collectable;

    private void Awake()
    {
        _collectable = GetComponent<Collectables>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(LayerMaskHelper.ObjIsInLayerMask(collision.gameObject, _whoCanCollect))
        {
            _collectable.Collect(collision.gameObject);

            Destroy(gameObject);
        }
    }
}
