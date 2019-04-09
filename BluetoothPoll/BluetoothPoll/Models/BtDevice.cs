using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothPoll.Models
{
    public class BtDevice
    {
	    public const string NewDevice = "NewDevice";

		public string Mac { get; set; }
		public string Name { get; set; }
    }
}
