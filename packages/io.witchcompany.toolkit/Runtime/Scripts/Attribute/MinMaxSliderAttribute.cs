﻿using System;
using UnityEngine;

namespace WitchCompany.Toolkit.Attribute
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        public float MinValue { get; private set; }
        public float MaxValue { get; private set; }

        public MinMaxSliderAttribute(float minValue, float maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}
