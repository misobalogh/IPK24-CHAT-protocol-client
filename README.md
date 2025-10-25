# Client for chat server using IPK24-CHAT protocol
## IPK - Project 1

## 1. Introduction
The goal of this project was to create a client for a console chat application server that uses the IPK24-CHAT protocol for communication. Two variants needed to be implemented - UDP and TCP. Both variants had their specializations and problems that arose from them.

## 2. Table of Contents
1. [Introduction](#1-introduction)
2. [Table of Contents](#2-table-of-contents)
3. [How to Run the Project](#3-how-to-run-the-project)
4. [Basic Theory for the Project](#4-basic-theory-for-the-project)\
    4.1 [TCP](#41-tcp-transmission-control-protocol)\
    4.2 [UDP](#42-udp-user-datagram-protocol)\
    4.3 [Socket](#43-socket)
5. [Project Structure](#5-project-structure)
6. [Testing](#6-testing) \
    6.1 [Unit Tests](#61-unit-tests) \
    6.2 [Test Scenario](#62-test-scenario) \
    6.3 [Testing the Application on Reference Server](#63-testing-the-application-on-reference-server)
7. [Bibliography](#7-bibliography)


## 3. How to Run the Project
The project can be built using the make command in the root directory. After building the ipk24chat-client binary, it can be run with parameters defined in the project assignment.
For more information, use the `-h` parameter.
Additional make targets:
- `make` - builds the project
- `make help` - prints useful information about running the project
- `make test` - runs unit tests for the project
- `make udp` - runs the client with default parameters and UDP protocol
- `make tcp` - runs the client with default parameters and TCP protocol

## 4. Basic Theory for the Project

### 4.1 TCP (Transmission Control Protocol)
TCP provides a reliable data delivery service in the order they were sent in the form of a byte stream to applications.
Messages are transmitted over the network using TCP segments, where each segment is sent as an Internet Protocol datagram.
Reliable message delivery takes more overhead in terms of transmission time and data transfer size. It is used
for file transfers, sending emails, or is also used by SSH (Secure Shell).

### 4.2 UDP (User Datagram Protocol)
UDP is a simple protocol for application programs to send messages to other programs
with minimal protocol mechanism requirements. Unlike TCP, however, it is not reliable and there is no guarantee
of message delivery or protection against duplicate messages. It is used in cases where latency is important,
not the reliability of delivering all data - for example, video calls.

### 4.3 Socket
Sockets are used for interaction between client and server. In the client-server model, the socket on the server
waits for requests from the client. The server first creates an address through which the server can be found from the client side.
When the address is created, the server waits for a request from the client. The client also connects to the server using a socket,
data exchange occurs, the server handles the client's request and sends a response to the client.

## 5. Project Structure
I tried to divide the project into subproblems, which I divided into logical units or classes,
that solve the given subproblem. The program division and mutual cooperation of classes is shown in the class diagram:

![Chat App Class Diagram](/doc/ChatAppClassDiagram.png)
*(Diagram is available in the `doc/` directory)*

The entry point of the program is in the Program file by running the `Main()` method. First, an instance of the CommandLineOptions class is created,
which invokes its `ParseArguments()` method in its constructor. It assigns the values of individual arguments to its
attributes, or terminates the program with an error. The obtained settings from the arguments are then passed to an instance of the `UserInputHandler` class,
which processes user inputs and commands.

### `UserInputHandler`
This class creates an instance of the `UdpClient` or `TcpClient` class according to the selected protocol variant setting.
Both these classes inherit from the abstract `ClientBase` class, which declares three methods:
- `SendMessageAsync()` - which is used for asynchronous message sending
- `ReceiveMessageAsync()` - for asynchronous message receiving
- `Close()` - releases resources and terminates communication.

When processing user input, the class first calls the asynchronous `ReceiveMessageAsync()` method and then in a `while` loop
reads standard input and processes it. If the input starts with a `/` character, it attempts to execute a command if one exists. If not,
it displays a warning to the user. Otherwise, it treats the input as a regular message. If the respective command or sending a message is unacceptable
in the current client state, it notifies the user. In case some messages need a response or confirmation of some message,
it stores user inputs in a queue and as soon as the awaited response arrives, it sends all messages that were waiting in the queue until it encounters
a message that again needs confirmation from the server. The class also maintains a username, which can optionally be changed with the `/rename` command.

### `TcpClient`
The class is used to process messages via TCP protocol. It creates a `reader`, `writer`, `network stream`, connects to an *endpoint* and only
reads from or writes messages to this `stream`.

### `UdpClient`
Communication must work on dynamic ports, so the class, after the first message from the server, besides the `confirm` confirmation, remembers the port from where the message came,
and will direct all its messages to that port. This class, upon each received message (except `confirm` message), immediately sends a confirm message back to the server.
It also has a timer that starts when sending a message, and when this timer expires, it attempts to send the message again. It makes a maximum number of attempts as defined
through the `MaxRetransmissions` parameter.

### `ClientState`
This class implements the finite state machine from the assignment. It switches to different states using received messages from the server.
The client then allows certain actions only in certain states.

### `Message`
The abstract `Message` class has descendants that represent specific message types. It has three declared methods that concrete subclasses must then implement:
- `CraftTcp()` - used to create the given message type in the correct format for the TCP variant
- `CraftUdp()` - used to create the given message type in the correct (byte) format for the UDP variant
- `PrintOutput()` - some messages when received at the client should be displayed on the client's output, and this method does that in the correct format

### `MessageGrammar`
Used to check the correct format of messages using *regexes*.

### `MessageParser`
The class processes a message and evaluates what kind of message it is, what parameters it has, and whether it is in the correct format.

### `ErrorHandler`
Helper class for notifying errors to the user and potentially terminating the application.


## 6. Testing
I mostly tested the project manually, using various programs and checking the output.
During testing, I used the `Wireshark` application with a plugin for the IPK24-CHAT protocol to check received and sent messages.
For testing the TCP variant, I used `netcat`, where I simulated communication with the server.


### 6.1 Unit Tests
I tested classes where it made sense using unit tests. Specifically for the `MessageParser`
and `ClientState` classes. When testing `ClientState`, I tried all possible inputs in given states. When testing the
`MessageParser` class, I tried several inputs that should pass and several messages that had either the wrong number of parameters, or the given parts of the message
did not correspond to the grammar of messages used in the `IPK24-CHAT` protocol. Tests can be run with the `make test` command.

### 6.2 Test Scenario
Since testing was mostly done manually, I prepared a test scenario where the inputs that the
user should enter and the outputs that are expected on the server side are listed. The test scenario can be viewed in the `tests` folder
under the name `test_communication_scenarios.txt`. The scenario made testing easier because I didn't have to think of inputs and check outputs against the assignment every time I tested the program's functionality. Server inputs in the test scenario are mainly for the TCP variant, when I used
`netcat` and inserted the given inputs into the terminal where it was running.

Here is the first test scenario as an example (numbers indicate the order of sending and receiving messages):
#### Terminal with `ipk24chat-client`:
```
$ ./ipk24chat-client -t tcp -s 127.0.0.1 -p 4567
/auth user1 123 userNick                                            1.
Success: ok      <- received reply                                  4.
Hello                                                               5.
user2: Hello back <- received mocked message from another user      8.
*C-d*                                                               9.
```
*`netcat` must be started first*

#### Terminal with `netcat`
```
$ nc -4 -l -C -v 127.0.0.1 4567
Listening on localhost 4567
Connection received on localhost 52806
AUTH user1 AS userNick USING 123                                    2.
reply ok is ok              <- send reply                           3.
MSG FROM userNick IS Hello                                          6.
msg from user2 is Hello back  <- mock another user input            7.
BYE                                                                 10.
```

### 6.3 Testing the Application on Reference Server

In the final phases of the project, I also used a discord server to verify the correct solution. When sending and receiving messages, I had
`Wireshark` running, where I could see all the details about the messages I was sending and receiving. Here I also used the test scenario described [above](#62-test-scenario).

![udp_example](/doc/wireshark_example_udp.jpg)

![tcp_example](/doc/wireshark_example_tcp.jpg)

## 7. Bibliography
[RFC768] Postel, J. User Datagram Protocol [online]. March 1997. [cited 2024-04-01]. DOI: 10.17487/RFC0768. Available at:\
https://datatracker.ietf.org/doc/html/rfc768 \
[RFC9293] Eddy, W. Transmission Control Protocol (TCP) [online]. August 2022. [cited 2024-04-01]. DOI: 10.17487/RFC9293. Available at:\
https://datatracker.ietf.org/doc/html/rfc9293#name-key-tcp-concept \
[IBM] IBM. How Sockets Work [online]. [cited 2024-04-01]. Available at:\
https://www.ibm.com/docs/en/i/7.3?topic=programming-how-sockets-work \
[Microsoft] Microsoft. System.Timers.Timer Class [online]. [used 2024-04-01]. Available at:\
https://learn.microsoft.com/en-us/dotnet/api/system.timers.timer?view=net-8.0 \
[Microsoft] Microsoft. Asynchronous Programming in C# [online]. [used 2024-04-01]. Available at:\
https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/ \
[Git-Cliff] Git-Cliff. Git-Cliff Documentation [online]. [used 2024-04-01]. Available at:\
https://git-cliff.org/docs/ \
[JetBrains] JetBrains. ReSharper Rider Samples Repository [.gitignore file] [online]. [cited 2024-04-01]. Available at:\
https://github.com/JetBrains/resharper-rider-samples/blob/master/.gitignore
