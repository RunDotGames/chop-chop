using System.Collections.Generic;
using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Util {
  public class EventedCollection<T> {

    private HashSet<T> items = new HashSet<T>();

    public void OnChange(T item, bool isAdded) {
      if (!isAdded) {
        items.Remove(item);
        return;
      }

      items.Add(item);
    }

    public IEnumerable<T> Items => items;
  }
}
