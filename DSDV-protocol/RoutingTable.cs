using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSDV_protocol
{
    class RoutingTable
    {
        private SortedDictionary<string, RouterReplica> routingTable = new SortedDictionary<string, RouterReplica>();
        private StringBuilder strBuilder = new StringBuilder();
        private Router self;

        public SortedDictionary<string, RouterReplica> GetData()
        {
            return routingTable;
        }

        public RoutingTable(SortedDictionary<string, RouterReplica> _neighbours, Router _self)
        {
            self = _self;
            for(int i = 0; i < _neighbours.Count; i++)
            {
                routingTable.Add(_neighbours.ElementAt(i).Key, _neighbours.ElementAt(i).Value);
            }
        }

        private void UpdateUnknownRouters(SortedDictionary<string, RouterReplica> _data, string _neighbourId)
        {
            for(int i = 0; i < _data.Count; i++)
            {
                if(!routingTable.Where(id=>id.Key == _data.ElementAt(i).Key).Any())
                {
                    RouterReplica replica = new RouterReplica(_data.ElementAt(i).Key, _neighbourId, _data.ElementAt(i).Value.SequenceNumber, int.MaxValue);
                    replica.LostConnection += self.LostConnection;
                    routingTable.Add(_data.ElementAt(i).Key, replica);
                }
            }
        }


        public void Update(SortedDictionary<string, RouterReplica> _neighbourData, string _neighbourId)
        {
            SortedDictionary<string, int> result = new SortedDictionary<string, int>();

            UpdateUnknownRouters(_neighbourData, _neighbourId);

            var replica = routingTable[_neighbourId];

            for (int i = 0; i < _neighbourData.Count; i++)
            {
                if (_neighbourData.ElementAt(i).Value.Distance == int.MaxValue)
                    result.Add(_neighbourData.ElementAt(i).Key, _neighbourData.ElementAt(i).Value.Distance);
                else
                    result.Add(_neighbourData.ElementAt(i).Key, replica.Distance + _neighbourData.ElementAt(i).Value.Distance);
            }

            for (int i = 0; i < result.Count; i++)
            {
                var item = routingTable[result.ElementAt(i).Key];
                if (item.SequenceCount <= _neighbourData.ElementAt(i).Value.SequenceCount)
                {
                    item.SequenceNumber = _neighbourData.ElementAt(i).Value.SequenceNumber;
                    if (_neighbourData.ElementAt(i).Value.SequenceCount % 2 == 0)
                    {
                        if (item.Distance > result.ElementAt(i).Value && result.ElementAt(i).Value >= 0)
                        {
                            item.Distance = result.ElementAt(i).Value;
                            item.NextHop = _neighbourId;
                        }
                    }
                    else
                    {
                        if(_neighbourId == item.NextHop)
                        {
                            item.Distance = result.ElementAt(i).Value;
                            item.NextHop = "-";
                        }
                    }
                }
                /*else if(item.SequenceCount > _neighbourData.ElementAt(i).Value.SequenceCount && _neighbourData.ElementAt(i).Value.SequenceCount % 2 != 0 && item.NextHop == _neighbourId)
                {
                    item.Distance = int.MaxValue;
                    item.NextHop = "-";
                    item.SequenceNumber = _neighbourData.ElementAt(i).Value.SequenceNumber;
                }*/
            }
        }

        public void UpdateLink(RouterReplica _router)
        {
            if(!routingTable.ContainsKey(_router.Id))
            {
                routingTable.Add(_router.Id, _router);
            }
            else
            {
                routingTable[_router.Id].Distance = _router.Distance;
                routingTable[_router.Id].NextHop = _router.NextHop;
                routingTable[_router.Id].SequenceNumber = _router.SequenceNumber;
            }
        }

        public void SetLost(string _id)
        {
            if(routingTable.ContainsKey(_id))
            {
                var replica = routingTable[_id];
                replica.Distance = int.MaxValue;
                replica.NextHop = "-";
                replica.GenerateSequenceNumber(false);
                foreach (var item in routingTable.Where(id => id.Value.NextHop == _id))
                {
                    item.Value.Distance = int.MaxValue;
                    item.Value.NextHop = "-";
                    item.Value.GenerateSequenceNumber(false);
                }
            }
        }

        public override string ToString()
        {
            strBuilder.Clear();
            strBuilder.AppendLine(self.Id);
            strBuilder.Append(string.Format("{0}              {1}               {2}                 {3}", "Name", "Sequence", "Next", "Cost")).Append("\n");
            strBuilder.Append(string.Format("--------------------------------------------------")).Append("\n");
            for (int i = 0; i < routingTable.Count; i++)
            {
                strBuilder.Append(string.Format("{0}           {1}            {2}              {3}", routingTable.ElementAt(i).Key, routingTable.ElementAt(i).Value.SequenceNumber, routingTable.ElementAt(i).Value.NextHop, routingTable.ElementAt(i).Value.Distance)).Append("\n");
            }
            strBuilder.Append(string.Format("--------------------------------------------------")).Append("\n");

            return strBuilder.ToString();
        }

        public void CleanUp()
        {
            for(int i = routingTable.Count - 1; i > -1; i--)
            {
                if (routingTable.ElementAt(i).Value.Distance == int.MaxValue && routingTable.ElementAt(i).Value.SequenceCount % 2 != 0)
                {
                    routingTable.ElementAt(i).Value.StopTimer();
                    routingTable.Remove(routingTable.ElementAt(i).Key);
                }
            }
        }
    }
}

