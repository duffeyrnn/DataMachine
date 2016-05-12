using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HorseRacingStrategyMachine
{
    public class HorseRaceDataMap
    {
        public DateTime DateTime { get; set; }
        public DateTime MarketOpen { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double? MACDVal { get; set; }
        public double? NinePerVal { get; set; }
        public double? DiffVal { get; set; }
        public double? RSI { get; set; }
        public double GrossVol { get; set; }
    }
}
