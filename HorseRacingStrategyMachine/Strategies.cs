using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HorseRacingStrategyMachine
{
    public class Strategies
    {
        public List<StrategyParameters> GetStrategiesForTest()
        {
            var returnStrategies = new List<StrategyParameters>();

            var minimumVols = new List<double>();
            for (int i = 5000; i <= 40000; i += 5000)
            {
                minimumVols.Add(i);
            }

            var timeBeforeRaceStarts = new List<int>();
            for (int i = 10; i <= 30; i += 5)
            {
                timeBeforeRaceStarts.Add(i);
            }

            var tickTargetBeforeTenMinutes = new List<int>();
            for (int i = 3; i < 6; i++)
            {
                tickTargetBeforeTenMinutes.Add(i);
            }

            var compareCandlesticksForLays = new List<bool>();
            compareCandlesticksForLays.Add(true);
            compareCandlesticksForLays.Add(false);

            var compareCandlesticksForBacks = new List<bool>();
            compareCandlesticksForBacks.Add(true);
            compareCandlesticksForBacks.Add(false);

            var isMinVols = new List<bool>();
            isMinVols.Add(true);
            isMinVols.Add(false);

            var canLay = new List<bool>();
            canLay.Add(true);
            canLay.Add(false);

            var canBack = new List<bool>();
            canBack.Add(true);
            canBack.Add(false);

            var isTimeBeforeRace = new List<bool>();
            isTimeBeforeRace.Add(true);
            isTimeBeforeRace.Add(false);

            var isRunnerProfitTarget = new List<bool>();
            isRunnerProfitTarget.Add(true);
            isRunnerProfitTarget.Add(false);

            var isTickTargetBeforeTenMinutes = new List<bool>();
            isTickTargetBeforeTenMinutes.Add(true);
            isTickTargetBeforeTenMinutes.Add(false);

            foreach (var isMinVol in isMinVols)
            {
                foreach (var minVol in minimumVols)
                {
                    foreach (var isTimeBefore in isTimeBeforeRace)
                    {
                        foreach (var timeBeforeRace in timeBeforeRaceStarts)
                        {
                            foreach (var lay in canLay)
                            {
                                foreach (var back in canBack)
                                {
                                    foreach (var isCandlestickLay in compareCandlesticksForLays)
                                    {
                                        foreach (var isCandlestickBacks in compareCandlesticksForBacks)
                                        {
                                            foreach (var profitTarget in isRunnerProfitTarget)
                                            {
                                                foreach (var isTickTarget in isTickTargetBeforeTenMinutes)
                                                {
                                                    foreach (var tickTarget in tickTargetBeforeTenMinutes)
                                                    {
                                                        var strategyToAdd = new StrategyParameters()
                                                        {
                                                            hasMinVolume = isMinVol,
                                                            minimumVolume = minVol,
                                                            hasMinBeforeRaceOff = isTimeBefore,
                                                            minutesBeforeRaceOff = timeBeforeRace,
                                                            canLay = lay,
                                                            compareCandlesticksLay = isCandlestickLay,
                                                            canBack = back,
                                                            compareCandlesticksBack = isCandlestickBacks,
                                                            RunnerProfitTarget = profitTarget,
                                                            isProfitTargetBeforeTenMinutes = isTickTarget,
                                                            tickTargetPreTenMinutes = tickTarget
                                                        };
                                                        returnStrategies.Add(strategyToAdd);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var noMinVols = returnStrategies.Where(x => !x.hasMinVolume).ToList();
            var strategiesToRemove = new List<StrategyParameters>();
            foreach (var noMinVol in noMinVols)
            {
                var matchingStrategies = noMinVols.Where(x => x.canBack == noMinVol.canBack && x.canLay == noMinVol.canLay && x.compareCandlesticksBack == noMinVol.compareCandlesticksBack && x.compareCandlesticksLay == noMinVol.compareCandlesticksLay && x.hasMinBeforeRaceOff == noMinVol.hasMinBeforeRaceOff && x.isProfitTargetBeforeTenMinutes == noMinVol.isProfitTargetBeforeTenMinutes && x.minutesBeforeRaceOff == noMinVol.minutesBeforeRaceOff && x.RunnerProfitTarget == noMinVol.RunnerProfitTarget && x.tickTargetPreTenMinutes == noMinVol.tickTargetPreTenMinutes).Skip(1);
                foreach (var matchStrat in matchingStrategies)
                {
                    if (!strategiesToRemove.Contains(matchStrat))
                    {
                        strategiesToRemove.Add(matchStrat);
                    }
                }
            }
            foreach (var toRemove in strategiesToRemove)
            {
                returnStrategies.Remove(toRemove);
            }

            strategiesToRemove.Clear();

            var noProfitTargets = returnStrategies.Where(x => !x.isProfitTargetBeforeTenMinutes).ToList();
            foreach (var noProfitTarget in noProfitTargets)
            {
                var matchingStrategies = noProfitTargets.Where(x => x.canBack == noProfitTarget.canBack && x.canLay == noProfitTarget.canLay && x.compareCandlesticksBack == noProfitTarget.compareCandlesticksBack && x.compareCandlesticksLay == noProfitTarget.compareCandlesticksLay && x.hasMinBeforeRaceOff == noProfitTarget.hasMinBeforeRaceOff && x.hasMinVolume == noProfitTarget.hasMinVolume && x.minutesBeforeRaceOff == noProfitTarget.minutesBeforeRaceOff && x.RunnerProfitTarget == noProfitTarget.RunnerProfitTarget && x.minimumVolume == noProfitTarget.minimumVolume).Skip(1);
                foreach (var matchStrat in matchingStrategies)
                {
                    if (!strategiesToRemove.Contains(matchStrat))
                    {
                        strategiesToRemove.Add(matchStrat);
                    }
                }
            }
            foreach (var toRemove in strategiesToRemove)
            {
                returnStrategies.Remove(toRemove);
            }

            strategiesToRemove.Clear();

            var noMinTimeBefores = returnStrategies.Where(x => !x.hasMinBeforeRaceOff).ToList();
            foreach (var noMinTimeBefore in noMinTimeBefores)
            {
                var matchingStrategies = noMinTimeBefores.Where(x => x.canBack == noMinTimeBefore.canBack && x.canLay == noMinTimeBefore.canLay && x.compareCandlesticksBack == noMinTimeBefore.compareCandlesticksBack && x.compareCandlesticksLay == noMinTimeBefore.compareCandlesticksLay && x.isProfitTargetBeforeTenMinutes == noMinTimeBefore.isProfitTargetBeforeTenMinutes && x.hasMinVolume == noMinTimeBefore.hasMinVolume && x.minutesBeforeRaceOff == noMinTimeBefore.minutesBeforeRaceOff && x.RunnerProfitTarget == noMinTimeBefore.RunnerProfitTarget && x.minimumVolume == noMinTimeBefore.minimumVolume).Skip(1);
                foreach (var matchStrat in matchingStrategies)
                {
                    if (!strategiesToRemove.Contains(matchStrat))
                    {
                        strategiesToRemove.Add(matchStrat);
                    }
                }
            }
            foreach (var toRemove in strategiesToRemove)
            {
                returnStrategies.Remove(toRemove);
            }

            strategiesToRemove.Clear();

            var cantLayThereforeNoLayCandlesticks = returnStrategies.Where(x => !x.canLay && x.compareCandlesticksLay);
            foreach (var matchStrat in cantLayThereforeNoLayCandlesticks)
            {
                if (!strategiesToRemove.Contains(matchStrat))
                {
                    strategiesToRemove.Add(matchStrat);
                }
            }

            foreach (var toRemove in strategiesToRemove)
            {
                returnStrategies.Remove(toRemove);
            }

            strategiesToRemove.Clear();

            var cantBackThereforeNoBackCandlesticks = returnStrategies.Where(x => !x.canBack && x.compareCandlesticksBack);
            foreach (var matchStrat in cantBackThereforeNoBackCandlesticks)
            {
                if (!strategiesToRemove.Contains(matchStrat))
                {
                    strategiesToRemove.Add(matchStrat);
                }
            }

            foreach (var toRemove in strategiesToRemove)
            {
                returnStrategies.Remove(toRemove);
            }

            strategiesToRemove.Clear();

            var cantBackOrLay = returnStrategies.Where(x => !x.canBack && !x.canLay);
            foreach (var matchStrat in cantBackOrLay)
            {
                if (!strategiesToRemove.Contains(matchStrat))
                {
                    strategiesToRemove.Add(matchStrat);
                }
            }

            foreach (var toRemove in strategiesToRemove)
            {
                returnStrategies.Remove(toRemove);
            }

            return returnStrategies;
        }
    }
}
