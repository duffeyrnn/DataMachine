using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HorseRacingStrategyMachine
{
    public class StrategyParameters
    {
        public string strategyName
        {
            get
            {
                return string.Format("MINVOL{0}{1} MINSBEFORERACE{2}{3} CANBACK{4} CANDLESTICKBACK{5} CANLAY{6} CANDLESTICKLAY{7} RUNNERPROFITTARGET{8} PROFITBEFORETENMINS{9}{10}", 
                    hasMinVolume, minimumVolume, 
                    hasMinBeforeRaceOff, minutesBeforeRaceOff, canBack,
                    compareCandlesticksBack, canLay, compareCandlesticksLay, 
                    RunnerProfitTarget, isProfitTargetBeforeTenMinutes, tickTargetPreTenMinutes);
            }
        }

        //Coding Puzzle

        //Want to get a list of StrategyParameter objects so that every combination of the properties above are created, except for the combinations that would make no sense. Eg hasMinVol = false and minimumVolume = 15,000.
        //Could create by nested foreach loops, but wondered if there was a better way?

        //All possible combinations == 23,040 but want to get the number much lower!

        public bool hasMinVolume { get; set; }
        //if hasMinVolume is true, minimumVolume can be 15,000, 20,000, 25,000, 30,000, 35,000, 40,000
        public double minimumVolume { get; set; }

        public bool hasMinBeforeRaceOff { get; set; }
        //if hasMinBeforeRaceOff is true, minutesBeforeRaceOff can be 10, 15, 20, 25, 30
        public int minutesBeforeRaceOff { get; set; }

        public bool canLay { get; set; }
        public bool canBack { get; set; }
        
        //If canLay == false, don't create strategy with a compareCandlestickLay == true.
        //If canBack == false, don't create strategy with a compareCandlestickBack == true.

        public bool compareCandlesticksBack { get; set; }
        public bool compareCandlesticksLay { get; set; }

        public bool RunnerProfitTarget { get; set; }

        public bool isProfitTargetBeforeTenMinutes { get; set; }
        //if isProfitTargetBeforeTenMinutes is true, tickTargetPreTenMinutes can be 3,4,5
        public int tickTargetPreTenMinutes { get; set; }
    }
}
