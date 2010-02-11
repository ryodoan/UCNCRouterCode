﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNCRouterCommand
{
    public abstract class CNCRMessage
    {
        private CNCRMESSAGE_TYPE _msgType;

        public CNCRMessage(CNCRMESSAGE_TYPE msgType)
        {
            _msgType = msgType;
        }

        public abstract byte[] toSerial();

        public CNCRMESSAGE_TYPE MessageType
        {
            get { return _msgType; }
        }
    }
}
