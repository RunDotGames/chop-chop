using UnityEngine;

namespace RDG.Chop_Chop.Scripts.Faction {

  public interface Factioned {
    public FactionSo Faction { get; }
  }
  public class FactionedBeh : MonoBehaviour, Factioned {

    [SerializeField] private FactionSo faction;

    public FactionSo Faction => faction;
  }
}
