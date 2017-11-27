using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageDriver.Services
{
    public enum PacketTypes
    {
        Text,
        Sound
    }
    public enum PacketStates
    {
        WaitStart,
        WaitType,
        WaitBody,
        WaitEnd
    }
}
