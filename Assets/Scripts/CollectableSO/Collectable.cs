using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent (typeof(CollectableTrigger))]
public class Collectables : MonoBehaviour
{
    [SerializeField] private CollectableSO _collectable;
    private void Reset()
    {
        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    public void Collect(GameObject objectThatCollected)
    {
        _collectable.Collect(objectThatCollected);
    }
}
