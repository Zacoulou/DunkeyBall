using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMath
{
    //Function that allows linear interpolation in terms of degrees (0 - 360 degrees)
    //The jump from 360 -> 0 and vice versa causes linear interpolation to reverse directions
    //rather than take the shortest path. This method takes the shortest path.
    public static float LerpThrough360Degrees(float a, float b, float t)
    {
        //Offset by 360 to avoid passing through "360 = 0". Makes all calculations positive
        float offset = 360f;
        a += offset;
        b += offset;
        float distance = b - a;

        //When passing through "360 = 0" from positive to negative, use shortest path
        //EX. if going from 0 -> 345 (the long way), instead go 0 -> -15 (the short way)
        //This can be achieved by subtracting 360 from the target point
        if (distance > 180f)
        {
            b -= 360f;
        }
        //Reverse in the event of going from negative to positive
        else if (distance < -180f)
        {
            a -= 360f;
        }

        //Recalculate distance with new values and lerp based on percentage completion
        distance = b - a;
        float position = a + distance * t;

        return position - 360f; //Remove initial offset, bringing values into appropriate range
    }
}
