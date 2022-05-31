using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpecialAbility
{
    public abstract string description { get; }
    public abstract void Activate();
}
