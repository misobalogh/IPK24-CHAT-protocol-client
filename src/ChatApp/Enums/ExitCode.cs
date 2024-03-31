/*
 * File: ExitCode.cs
 * Description: Enum for exit codes of the application.
 * Author: Michal Balogh, xbalog06
 * Date: 30.03.2024
 */

namespace ChatApp.Enums;

public enum ExitCode
{
    Success = 0,
    ConnectionError = 1,
    UnknownParam = 11,
    MissingArg = 12,
    UnknownCommand = 21,
    UnknownMessageType = 31,
}