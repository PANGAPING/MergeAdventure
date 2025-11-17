using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringValueAttribute : Attribute
{
    private string value;

    public StringValueAttribute(string value)
    {
        this.value = value;
    }

    public string Value
    {
        get { return value; }
    }
}
