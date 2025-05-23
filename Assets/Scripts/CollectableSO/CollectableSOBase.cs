using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CollectableSO : ScriptableObject
{
   public abstract void Collect(GameObject objectThatCollected);
}
