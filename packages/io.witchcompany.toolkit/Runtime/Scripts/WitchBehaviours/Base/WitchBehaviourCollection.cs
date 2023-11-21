using UnityEngine;
using WitchCompany.Toolkit.Attribute;
using WitchCompany.Toolkit.Scripts.WitchBehaviours;

namespace WitchCompany.Toolkit.Module
{
    public abstract class WitchBehaviourCollection : WitchBehaviour
    {
        [Header("컬렉션 아이템 배치 정보")]
        [SerializeField, ReadOnly] protected int index;
        [SerializeField] private int salesItemId;
        
        public int BlockLocationId => index + (int)AssetType;
        public int SalesItemId => salesItemId;
        public abstract AssetType AssetType { get; }
    }
}