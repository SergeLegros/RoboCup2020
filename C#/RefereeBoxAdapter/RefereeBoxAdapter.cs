﻿using AdvancedTimers;
using EventArgsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using TCPAdapter;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Converters;
using System.Globalization;
using Utilities;

namespace RefereeBoxAdapter
{
    public class RefereeBoxAdapter
    {
        TCPAdapter.TCPAdapter tcpAdapter;

        public RefereeBoxAdapter()
        {
            new Thread(StartRefBoxAdapter).Start();
        }

        private void StartRefBoxAdapter()
        {
            tcpAdapter = new TCPAdapter.TCPAdapter("172.16.1.2", 28097, "Referee Box Adapter");
            tcpAdapter.OnDataReceivedEvent += TcpAdapter_OnDataReceivedEvent;
        }

        private void TcpAdapter_OnDataReceivedEvent(object sender, DataReceivedArgs e)
        {
            //On deserialize le message JSON en provenance de la Referee Box
            string s = Encoding.ASCII.GetString(e.Data);
            var refBoxCommand = JsonConvert.DeserializeObject<RefBoxMessage>(s);
            OnRefereeBoxReceivedCommand(refBoxCommand);
        }

        //Output events
        public event EventHandler<RefBoxMessageArgs> OnRefereeBoxCommandEvent;
        public virtual void OnRefereeBoxReceivedCommand(RefBoxMessage msg)
        {
            var handler = OnRefereeBoxCommandEvent;
            if (handler != null)
            {
                handler(this, new RefBoxMessageArgs { refBoxMsg = msg});
            }
        }
    }

    public class RefBoxMessage 
    {
        public RefBoxCommand command { get; set; }
        public string targetTeam { get; set; }
        public int robotID { get; set; }
    }

    public class RefBoxMessageArgs : EventArgs
    {
        public RefBoxMessage refBoxMsg { get; set; }
    }
}
