using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HorseRacingStrategyMachine
{
    public class TradeModel
    {
        public string RunnerName { get; private set; }
        public DateTime TradeOpenTime { get; private set; }
        public DateTime TradeCloseTime { get; private set; }
        public DateTime RaceStartTime { get; private set; }
        public double BackOdds { get; private set; }
        public double LayOdds { get; private set; }
        public Side OpeningSide{ get; private set; }
        public HighLowItem TradeOpenCandlestick { get; private set; }
        public HighLowItem CandlestickBeforeTradeOpen { get; private set; }
        public double VolumeAtTradeOpen { get; private set; }
        public double BestPriceReachedDuringTrade { get; private set; }
        public bool IsTradeClosed { get; private set; }
        const double openingStake = 50;
        private BetfairTickList tickList;
        public double pnl
        {
            get
            {
                var netWinOutcome = (openingStake * (BackOdds - 1)) - (openingStake * (LayOdds - 1));
                var netLossOutcome = (-openingStake) + openingStake;
                var netPosition = netWinOutcome + netLossOutcome;
                var hedgeStake = (netPosition / LayOdds);
                var winHedge = (openingStake * (BackOdds - 1)) - (openingStake * (LayOdds - 1)) - (hedgeStake * (LayOdds - 1));
                if (winHedge > 0)
                {
                    return winHedge * 0.95;
                }
                return winHedge;
            }
        }

       public TradeModel(string runnerName, DateTime tradeOpenTime, DateTime raceStartTime, Side openingSide, HighLowItem currentCandlestick, HighLowItem previousCandlestick, double currentVolume)
        {
            RunnerName = runnerName;
            TradeOpenTime = tradeOpenTime;
            RaceStartTime = raceStartTime;
            OpeningSide = openingSide;

            TradeOpenCandlestick = currentCandlestick;
            CandlestickBeforeTradeOpen = previousCandlestick;

            if (OpeningSide == Side.BACK)
            {
                BackOdds = currentCandlestick.Close;
            }
            else if (OpeningSide == Side.LAY)
            {
                LayOdds = currentCandlestick.Close;
            }

            VolumeAtTradeOpen = currentVolume;
            tickList = new BetfairTickList();
        }

        public void CloseTrade (DateTime tradeCloseTime, double closingOdds, double bestPriceAchieved)
        {
            TradeCloseTime = tradeCloseTime;
            if (OpeningSide == Side.BACK)
            {
                LayOdds = closingOdds;
            }
            else if (OpeningSide == Side.LAY)
            {
                BackOdds = closingOdds;
            }

            IsTradeClosed = true;
            BestPriceReachedDuringTrade = bestPriceAchieved;
        }

        public void AmendClosingPriceToProfitTarget()
        {

        }
        public bool AreOpeningCandlesticksValid
        {
            get
            {
                if (TradeOpenCandlestick.High == CandlestickBeforeTradeOpen.High && TradeOpenCandlestick.Low == CandlestickBeforeTradeOpen.Low && TradeOpenCandlestick.Open == CandlestickBeforeTradeOpen.Open && TradeOpenCandlestick.Close == CandlestickBeforeTradeOpen.Close)
                {
                    return false;
                }
                else if (TradeOpenCandlestick.High == CandlestickBeforeTradeOpen.Low && TradeOpenCandlestick.Low == CandlestickBeforeTradeOpen.High && TradeOpenCandlestick.Open == CandlestickBeforeTradeOpen.Close && TradeOpenCandlestick.Close == CandlestickBeforeTradeOpen.Open)
                {
                    return false;
                }
                return true;
            }
        }

        public bool ProfitTargetAchieved(int tickTarget)
        {
            if (OpeningSide == Side.BACK)
            {
                var indexOfBackOdds = tickList.BetfairPrices.IndexOf(BackOdds);
                var targetPrice = tickList.BetfairPrices[indexOfBackOdds - tickTarget];
                return BestPriceReachedDuringTrade <= targetPrice;
            }
            else if (OpeningSide == Side.LAY)
            {
                var indexOfLayOdds = tickList.BetfairPrices.IndexOf(LayOdds);
                var targetPrice = tickList.BetfairPrices[indexOfLayOdds + tickTarget];
                return BestPriceReachedDuringTrade >= targetPrice;
            }
            return false;
        }

    }

    public enum Side
    {
        BACK,
        LAY
    }
}
