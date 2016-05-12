using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HorseRacingStrategyMachine
{
    public interface IStrategy
    {
        string strategyName { get; }

        bool hasMinVolume { get; set; }
        double minimumVolume { get; set; }

        bool hasMinBeforeRaceOff { get; set; }
        int minutesBeforeRaceOff { get; set; }

        bool hascutOffBeforeRaceOff { get; set; }
        int cutOffBeforeRaceOff { get; set; }

        bool hasCompareCandlesticks { get; set; }
        bool compareCandlesticks { get; set; }
    }
}
