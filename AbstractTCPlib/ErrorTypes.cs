using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbstractTCPlib
{
    public enum ErrorTypes
    {
        ExceededByteMaxValueOfInt,
        TCPWriteException,
        ClientReceiveBufferSize,
        InRecieveCodeError
    }
}
