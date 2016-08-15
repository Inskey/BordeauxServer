using System;

namespace BordeauxRCServer.Net
{
    internal delegate void ClientDisconnectedHandler(object sender, ConnectionArgs args);
    internal delegate void DataReceivedHandler(object sender, ConnectionArgs args, string received);
    internal delegate void DataSendHandler(string sent);
}