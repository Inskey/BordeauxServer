using System;

namespace BordeauxRCServer.Net
{
    internal class ConnectionArgs : EventArgs
    {
        internal Connection Connection { get; private set; }

        internal ConnectionArgs(Connection connection)
        {
            this.Connection = connection;
        }
    }
}