using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactorySdk.DTO
{
    public enum ServerStateEnum
    {
        Offline = 0,
        Idle = 1,
        Loading = 2,
        Playing = 3
    }

    public class ServerSubState
    {
        public byte SubStateId { get; set; }
        public ushort SubStateVersion { get; set; }
    }

    public class ServerStateUdp
    {
        public ulong Identifier { get; set; }
        public byte ServerStateInt { get; set; }
        public uint ServerNetCL { get; set; }
        public ulong ServerFlags { get; set; }
        public byte SubStateCount { get; set; }
        public ServerSubState[] SubStates { get; set; }
        public ushort ServerNameLength { get; set; }
        public string ServerName { get; set; }

        private const int SERVER_SUB_STATE_SIZE = 3;

        public ServerStateEnum ServerState => (ServerStateEnum)ServerStateInt;

        public static ServerStateUdp Deserialize(byte[] data)
        {
            ServerStateUdp response = new ServerStateUdp();

            response.Identifier = BitConverter.ToUInt64(data, 0);
            response.ServerStateInt = data[8];
            response.ServerNetCL = BitConverter.ToUInt32(data, 9);
            response.ServerFlags = BitConverter.ToUInt64(data, 13);
            response.SubStateCount = data[21];

            response.SubStates = new ServerSubState[response.SubStateCount];
            for (int i = 0; i < response.SubStateCount; i++)
            {
                response.SubStates[i] = new ServerSubState
                {
                    SubStateId = data[(SERVER_SUB_STATE_SIZE * i) + 22],
                    SubStateVersion = BitConverter.ToUInt16(data, (SERVER_SUB_STATE_SIZE * i) + 23)
                };
            }

            response.ServerNameLength = BitConverter.ToUInt16(data, (3 * response.SubStateCount) + 22);

            int startNameIndex = (SERVER_SUB_STATE_SIZE * response.SubStateCount) + 23;
            response.ServerName = Encoding.UTF8.GetString(data.Skip(startNameIndex + 1).Take(response.ServerNameLength).ToArray());

            return response;
        }
    }

    public class ServerStateResponse
    {
        public byte MessageType { get; set; }
        public byte ProtocolVersion { get; set; }
        public ServerStateUdp ServerState { get; set; }

        public static ServerStateResponse Deserialize(byte[] data)
        {
            ServerStateResponse response = new ServerStateResponse();

            response.MessageType = data[2];
            response.ProtocolVersion = data[3];
            response.ServerState = ServerStateUdp.Deserialize(data.Skip(4).ToArray());

            return response;
        }
    }
}
