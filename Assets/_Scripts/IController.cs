using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public interface IController
{
    void onLeftJoystickMovement(CallbackContext obj);

    void onPressButtonWest();
    void onReleaseButtonWest();

    void onPressButtonSouth();
    void onReleaseButtonSouth();

    void onPressButtonEast();
    void onReleaseButtonEast();

    void onPressButtonNorth();
    void onReleaseButtonNorth();

    void onPressRightTrigger();
    void onReleaseRightTrigger();

    void onPressLeftTrigger();
    void onReleaseLeftTrigger();

    void onPressRightBumper();
    void onReleaseRightBumper();

    void onPressLeftBumper();
    void onReleaseLeftBumper();
}
