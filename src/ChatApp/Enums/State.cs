/*
 * File: State.cs
 * Description: Enum for the state of the chat application
 * Author: Michal Balogh, xbalog06
 * Date: 30.3.2024
 */

namespace ChatApp.Enums;

/// <summary>
/// Client FSM states
/// </summary>
public enum State {
    Start = 0,
    Auth = 1,
    Open = 2,
    Error = 3,
    End = 4,
}