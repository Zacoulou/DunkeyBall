using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using UnityEngine;

public abstract class SpecialAbility
{
    public abstract string description { get; }
    public abstract void Activate();
}
