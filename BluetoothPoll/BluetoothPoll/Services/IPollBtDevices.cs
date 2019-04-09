using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BluetoothPoll.Models;

namespace BluetoothPoll.Services
{
    public  interface IPollBtDevices
    {
	    Task<bool> Search();
    }
}
