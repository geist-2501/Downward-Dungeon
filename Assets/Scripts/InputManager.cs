using System.Collections;
using UnityEngine;

namespace G2
{
    public enum InputType
    {
        Keyboard,
        WirelessController
    }

    public enum Button
    {
        Hold,
        Tap,
        Up
    }

    public static class InputManager
    {

        const string ID_JUMP = "Jump";
        const string ID_MOVE = "Move";
        const string ID_DASH = "Dash";
        const string ID_REWIND = "Rewind";
        const string ID_ESCAPE = "Esc";


        public static InputType GetInputMethod()
        {
            string[] _connectedJoysticks = Input.GetJoystickNames();

            foreach (string _joystick in _connectedJoysticks)
            {
                Debug.Log(_joystick);
            }

            if (_connectedJoysticks.Length > 0)
            {
                return _connectedJoysticks[0] != "" ? InputType.WirelessController : InputType.Keyboard;
            }
            else
            {
                Debug.Log("No joysticks found, using keyboard.");
                return InputType.Keyboard;
            }

        }

        public static bool JumpButton(Button _type)
        {
            switch (_type)
            {
                case Button.Tap:
                    return Input.GetButtonDown(ID_JUMP);

                case Button.Up:
                    return Input.GetButtonUp(ID_JUMP);

                case Button.Hold:
                    return Input.GetButton(ID_JUMP);

                default:
                    return false;
            }
        }

        public static Vector2 Movement(InputType _type)
        {
            switch (_type)
            {
                case InputType.WirelessController:
                    float c_x = Input.GetAxis("c_" + ID_MOVE + "_x");
                    float c_y = Input.GetAxis("c_" + ID_MOVE + "_y");
                    Debug.Log(c_x + " " + c_y);
                    return new Vector2(c_x, c_y);
                
                case InputType.Keyboard:
                    float k_x = Input.GetAxis("k_" + ID_MOVE + "_x"); 
                    float k_y = Input.GetAxis("k_" + ID_MOVE + "_y"); 
                    return new Vector2(k_x, k_y);

                default:
                    return Vector2.zero;
            }
        }

        public static Vector2 MovementRaw(InputType _type)
        {
            switch (_type)
            {
                case InputType.WirelessController:
                    float c_x = Input.GetAxisRaw("c_" + ID_MOVE + "_x");
                    float c_y = Input.GetAxisRaw("c_" + ID_MOVE + "_y");
                    return new Vector2(c_x, c_y);
                
                case InputType.Keyboard:
                    float k_x = Input.GetAxisRaw("k_" + ID_MOVE + "_x"); 
                    float k_y = Input.GetAxisRaw("k_" + ID_MOVE + "_y"); 
                    return new Vector2(k_x, k_y);

                default:
                    return Vector2.zero;
            }
        }


        public static bool DashButton(Button _type)
        {
            switch (_type)
            {
                case Button.Tap:
                    return Input.GetButtonDown(ID_DASH);

                case Button.Hold:
                    return Input.GetButton(ID_DASH);

                case Button.Up:
                    return Input.GetButtonUp(ID_DASH);

                default:
                    return false;
            }
        }

        public static bool RewindButton(Button _type)
        {
            switch (_type)
            {
                case Button.Tap:
                    return Input.GetButtonDown(ID_REWIND);

                case Button.Hold:
                    return Input.GetButton(ID_REWIND);

                case Button.Up:
                    return Input.GetButtonUp(ID_REWIND);

                default:
                    return false;
            }
        }

        public static bool EscapeButton(Button _type)
        {
            switch (_type)
            {
                case Button.Tap:
                    return Input.GetButtonDown(ID_ESCAPE);

                case Button.Hold:
                    return Input.GetButton(ID_ESCAPE);

                case Button.Up:
                    return Input.GetButtonUp(ID_ESCAPE);

                default:
                    return false;
            }
        }
    }
}