using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DSDV_protocol
{
    class RouterReplica : IComparable<RouterReplica>
    {
        
        private string sequenceNumber;
        private Timer lossTimer;

        public event EventHandler<EventArgs> LostConnection;

        public bool Off
        {
            get;
            set;
        }

        public int Sent
        {
            get;
            set;
        }

        public string NextHop
        {
            get;
            set;
        }

        public int Distance
        {
            get;
            set;
        }

        public string Id
        {
            get;
            set;
        }

        public int SequenceCount
        {
            get;
            set;
        }
        public string SequenceNumber
        {
            get
            {
                return sequenceNumber;
            }
            set
            {
                sequenceNumber = value;
                SequenceCount = sequenceNumber.GetSequenceCount();
            }
        }

        public RouterReplica(string _id, string _nextHop, string _sequenceNumber, int _distance)
        {
            Id = _id;
            SequenceNumber = _sequenceNumber;
            NextHop = _nextHop;
            Distance = _distance;
            lossTimer = new Timer(20000);
            lossTimer.Elapsed += RouterLost;
            Off = false;
            Sent = 0;
        }

        private void RouterLost(object sender, ElapsedEventArgs e)
        {
            LostConnection(this, new EventArgs());
            lossTimer.Stop();
            lossTimer.Close();
        }

        public int CompareTo(RouterReplica other)
        {
            return Id.CompareTo(other.Id);
        }
        
        public void StartTimer()
        {
            try
            {
                lossTimer.Start();
            }
            catch (ObjectDisposedException)
            {

            }
            
        }

        public void StopTimer()
        {
            try
            {
                lossTimer.Stop();
            }
            catch (ObjectDisposedException)
            {

            }
        }

        public void DisposeTimer()
        {
            StopTimer();
            try
            {
                lossTimer.Dispose();
            }
            catch (ObjectDisposedException)
            {

            }
        }

        public void GenerateSequenceNumber(bool _isNormalUpdate)
        {
            if (_isNormalUpdate)
                SequenceCount += 2;
            else
                SequenceCount += 1;
            SequenceNumber = Id + "-" + SequenceCount;
        }
    }
}
