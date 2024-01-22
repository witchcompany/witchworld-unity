using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WitchCompany.Toolkit.Module
{
    public enum DisplayType
    {
        Original,
        Flexible,
        [InspectorName("1:1")]
        Square,
        [InspectorName("9:16")]
        NineToSixteen,
        [InspectorName("16:9")]
        SixteenToNine,
    }
}
