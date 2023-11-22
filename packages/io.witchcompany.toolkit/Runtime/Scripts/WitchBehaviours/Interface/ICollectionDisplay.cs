using System.Dynamic;
using UnityEngine;

namespace WitchCompany.Toolkit.Scripts.WitchBehaviours.Interface
{
    public interface ICollectionDisplay
    {
        public int SalesItemId { get; }
        
        public int SalesItemIdDev { get; }
        public AssetType AssetType { get; }
        public int BehaviourIndex { get; }
        public int BlockLocationId => (int)AssetType + BehaviourIndex;
    }
}