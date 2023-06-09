using UnityEngine;
using UnityEngine.Events;

namespace WitchCompany.Toolkit.Module
{
    [CreateAssetMenu(fileName = "DataChanger - ", menuName = "WitchToolkit/Data/Changer")]
    public class WitchDataChangerSO : ScriptableObject
    {
        [Header("키")]
        public string key = "key";

        [Header("서버에 쓸지?")] 
        public bool saveOnServer = true;

        [Header("연산할 값")] 
        public ArithmeticOperator valueOperator = ArithmeticOperator.Assignment;
        public int value = 0;

        [HideInInspector] public UnityEvent<WitchDataChangerSO> onEvent = new();
        
        public void Invoke() => onEvent.Invoke(this);
    }
}
