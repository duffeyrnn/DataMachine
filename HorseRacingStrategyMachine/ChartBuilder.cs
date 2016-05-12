using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HorseRacingStrategyMachine
{
    public class ChartBuilder : ViewModelBase
    {
        private static object syncCreatorLock = new Object();
        private static object syncUpdateLock = new Object();
        private PlotModel _priceChart;
        public PlotModel PriceChart
        {
            get
            {
                return _priceChart;
            }
            set
            {
                _priceChart = value;
                OnPropertyChanged("PriceChart");
            }
        }

        public CandleStickSeries CandleStickSeries { get; set; }
        public LineSeries TwentySMAAverage { get; set; }
        public LineSeries FiftySMAAverage { get; set; }
        public LineSeries AverageDifference { get; set; }
        public LineSeries SignalCrossLine { get; set; }
        public Dictionary<double, double> MatchedVolume { get; set; }
        public List<double> profitLoss;
        public DateTime _raceStartTime;
        public BetfairTickList tickList;

        public ChartBuilder(DateTime raceStartTime)
        {
            lock (syncCreatorLock)
            {
                PriceChart = new PlotModel();
                PriceChart.Axes.Add(new DateTimeAxis() { Position = AxisPosition.Bottom, Key = "Time" });
                PriceChart.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, Key = "Value", StartPosition = 0.2, EndPosition = 1 });
                PriceChart.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, Key = "AverageDifference", StartPosition = 0, EndPosition = 0.19 });

                CandleStickSeries = new CandleStickSeries()
                {
                    IncreasingColor = OxyColor.FromRgb(0, 197, 49),
                    DecreasingColor = OxyColor.FromRgb(255, 95, 95),
                    Background = OxyColors.Black,
                };

                TwentySMAAverage = new LineSeries()
                {
                    StrokeThickness = 1,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.DeepPink,
                    YAxisKey = "Value",
                    Title = "20SMA"
                };

                FiftySMAAverage = new LineSeries()
                {
                    StrokeThickness = 1,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.AntiqueWhite,
                    YAxisKey = "Value",
                    Title = "50SMA",
                };

                AverageDifference = new AreaSeries()
                {
                    StrokeThickness = 1,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.AntiqueWhite,
                    YAxisKey = "AverageDifference",
                    Title = "AverageDifferences"
                };

                SignalCrossLine = new LineSeries()
                {
                    StrokeThickness = 1,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.AntiqueWhite,
                    YAxisKey = "AverageDifference",
                    Title = "Signal Cross"
                };

                MatchedVolume = new Dictionary<double, double>();

                PriceChart.Series.Add(CandleStickSeries);
                PriceChart.Series.Add(TwentySMAAverage);
                PriceChart.Series.Add(FiftySMAAverage);
                PriceChart.Series.Add(AverageDifference);
                PriceChart.Series.Add(SignalCrossLine);

                PriceChart.LegendOrientation = LegendOrientation.Horizontal;
                PriceChart.LegendPlacement = LegendPlacement.Outside;
                PriceChart.LegendPosition = LegendPosition.TopCenter;
                PriceChart.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);

                profitLoss = new List<double>();
                _raceStartTime = raceStartTime;
                tickList = new BetfairTickList();
            }
        }

        public void AddDataPoint(HorseRaceDataMap incomingDataPoint, double incomingTwentyAverage, double incomingFifyAverage)
        {
            var currentTimeForXAxis = DateTimeAxis.ToDouble(incomingDataPoint.DateTime);
            CandleStickSeries.Items.Add(new HighLowItem(currentTimeForXAxis, incomingDataPoint.High, incomingDataPoint.Low, incomingDataPoint.Open, incomingDataPoint.Close));
            TwentySMAAverage.Points.Add(new DataPoint(currentTimeForXAxis, incomingTwentyAverage));
            FiftySMAAverage.Points.Add(new DataPoint(currentTimeForXAxis, incomingFifyAverage));
            AverageDifference.Points.Add(new DataPoint(currentTimeForXAxis, incomingTwentyAverage - incomingFifyAverage));
            SignalCrossLine.Points.Add(new DataPoint(currentTimeForXAxis, 0));
            try
            {
                MatchedVolume.Add(currentTimeForXAxis, incomingDataPoint.GrossVol);
            }
            catch
            { }
        }

        private DateTime fiveMinutesBeforeRace;
        private string runnerName;
        private double profitThreshold = 2;

        public void SimulateTrades(string _runnerName, Dictionary<StrategyParameters, List<TradeModel>> strategiesToTest)
        {
            var simulatedTrades = new List<TradeModel>();
            double runnerProfit = 0;
            runnerName = _runnerName;
            var numberOfAveragePoints = AverageDifference.Points.Count;

            var closingIndex = AverageDifference.Points.Count;

            for (var i = 1; i < closingIndex; i++)
            {
                var previousPoint = AverageDifference.Points[i - 1];
                var currentPoint = AverageDifference.Points[i];
                var currentVolume = MatchedVolume[currentPoint.X];
                fiveMinutesBeforeRace = _raceStartTime.AddMinutes(-5);
                if (isBeforeFiveMinutesToRaceStart(currentPoint, fiveMinutesBeforeRace))
                {
                    if (currentPoint.Y < 0 && previousPoint.Y > 0)
                    {
                        //var tradeModel = SimulateTrade(currentPoint, Side.BACK, currentVolume, i);
                        //simulatedTrades.Add(tradeModel);
                        //runnerProfit += tradeModel.pnl;
                        //AssessTrade(tradeModel);
                    }

                    else if (currentPoint.Y > 0 && previousPoint.Y < 0)
                    {
                        //var tradeModel = SimulateTrade(currentPoint, Side.LAY, currentVolume, i);
                        //simulatedTrades.Add(tradeModel);
                        //runnerProfit += tradeModel.pnl;
                        //AssessTrade(tradeModel);
                    }
                }
            }
        }

        private void AssessTrade(TradeModel tradeModel, Dictionary<StrategyParameters, List<TradeModel>> strategiesToTest)
        {
            foreach (var strategy in strategiesToTest)
            {
                //if (IsTradeValidForStrategy(tradeModel, strategy.Key))
                //{
                //    strategiesToTest[strategy.Key].Add(tradeModel);
                //}
            }
        }

        private bool IsTradeValidForStrategy(TradeModel tradeModel, StrategyParameters strategyToTest, double runnerPnl)
        {
            var strategyPassParameters = new List<bool>();
            //MinVol
            strategyPassParameters.Add(strategyToTest.hasMinVolume ? tradeModel.VolumeAtTradeOpen > strategyToTest.minimumVolume : true);
            strategyPassParameters.Add(strategyToTest.hasMinBeforeRaceOff ? tradeModel.TradeOpenTime > tradeModel.RaceStartTime.AddMinutes(-strategyToTest.minutesBeforeRaceOff) : true);
            strategyPassParameters.Add(strategyToTest.RunnerProfitTarget ? runnerPnl < profitThreshold : true);
            //Amend tradeModelto change closing odds to the target odds.
            strategyPassParameters.Add(strategyToTest.isProfitTargetBeforeTenMinutes ? tradeModel.ProfitTargetAchieved(strategyToTest.tickTargetPreTenMinutes) : true); 
            if (tradeModel.OpeningSide == Side.BACK)
            {
                strategyPassParameters.Add(strategyToTest.canBack ? tradeModel.OpeningSide == Side.BACK : true);
                strategyPassParameters.Add(strategyToTest.compareCandlesticksBack ? tradeModel.AreOpeningCandlesticksValid : true);
            }
            else if (tradeModel.OpeningSide == Side.LAY)
            {
                strategyPassParameters.Add(strategyToTest.canLay ? tradeModel.OpeningSide == Side.LAY : false);
                strategyPassParameters.Add(strategyToTest.compareCandlesticksLay ? tradeModel.AreOpeningCandlesticksValid : false);
            }

            return strategyPassParameters.All(x => x == true);
            
        }


        private TradeModel SimulateTrade(DataPoint currentPoint, Side openingSide, double currentVolume, int openingIndexForLoop, DateTime raceStartTime)
        {
            var currentCandlestick = CandleStickSeries.Items.FirstOrDefault(x => x.X == currentPoint.X);
            var previousCandlestick = CandleStickSeries.Items[CandleStickSeries.Items.IndexOf(currentCandlestick) - 1];
            double bestPriceThroughoutTrade = currentCandlestick.Close;

            TradeModel tradeModel = new TradeModel(runnerName, DateTimeAxis.ToDateTime(currentCandlestick.X), raceStartTime, openingSide, currentCandlestick, previousCandlestick, currentVolume);
            for (var j = openingIndexForLoop + 1; j < AverageDifference.Points.Count; j++)
            {
                var previousClosing = AverageDifference.Points[j - 1];
                var currentClosing = AverageDifference.Points[j];
                var currentTime = DateTimeAxis.ToDateTime(CandleStickSeries.Items.FirstOrDefault(x => x.X == currentClosing.X).X);
                var currentPrice = CandleStickSeries.Items.FirstOrDefault(x => x.X == currentClosing.X).Close;
                if (openingSide == Side.BACK)
                {
                    bestPriceThroughoutTrade = currentPrice < bestPriceThroughoutTrade ? currentPrice : bestPriceThroughoutTrade;
                    if (currentClosing.Y >= 0 && previousClosing.Y < 0 || DateTimeAxis.ToDateTime(currentClosing.X) > fiveMinutesBeforeRace)
                    {
                        var closingCandlestick = CandleStickSeries.Items.FirstOrDefault(x => x.X == currentClosing.X);
                        tradeModel.CloseTrade(DateTimeAxis.ToDateTime(closingCandlestick.X), closingCandlestick.Close, bestPriceThroughoutTrade);
                        break;
                    }
                }
                else if (openingSide == Side.LAY)
                {
                    bestPriceThroughoutTrade = currentPrice > bestPriceThroughoutTrade ? currentPrice : bestPriceThroughoutTrade;
                    if (currentClosing.Y <= 0 && previousClosing.Y > 0 || DateTimeAxis.ToDateTime(currentClosing.X) > fiveMinutesBeforeRace)
                    {
                        var closingCandlestick = CandleStickSeries.Items.FirstOrDefault(x => x.X == currentClosing.X);
                        tradeModel.CloseTrade(DateTimeAxis.ToDateTime(closingCandlestick.X), closingCandlestick.Close, bestPriceThroughoutTrade);
                        break;
                    }
                }
            }

            if (!tradeModel.IsTradeClosed)
            {
                var closingCandlestick = CandleStickSeries.Items.Last();
                tradeModel.CloseTrade(DateTimeAxis.ToDateTime(closingCandlestick.X), closingCandlestick.Close, bestPriceThroughoutTrade);
            }
            return tradeModel;
        }

        private bool isBeforeFiveMinutesToRaceStart(DataPoint currentPoint, DateTime fiveMinutesBeforeRace)
        {
            return DateTimeAxis.ToDateTime(currentPoint.X) < fiveMinutesBeforeRace;
        }
        
        public void ExportChart(string fileName)
        {
            BetfairTickList betfairTickList = new BetfairTickList();
            var tickList = betfairTickList.BetfairPrices;
            var highOpenCandlePoint = CandleStickSeries.Items.OrderByDescending(x => x.Open).ThenBy(x => x.High).First().High;
            var highCloseCandlePoint = CandleStickSeries.Items.OrderByDescending(x => x.Close).ThenBy(x => x.High).First().High;
            var highestHigh = highOpenCandlePoint > highCloseCandlePoint ? highOpenCandlePoint : highCloseCandlePoint;
            var highestHighIndex = tickList.IndexOf(highestHigh);

            var lowOpenCandlePoint = CandleStickSeries.Items.OrderBy(x => x.Open).ThenBy(x => x.Low).First().Low;
            var lowCloseCandlePoint = CandleStickSeries.Items.OrderBy(x => x.Close).ThenBy(x => x.Low).First().Low;
            var lowestLow = lowOpenCandlePoint > lowCloseCandlePoint ? lowOpenCandlePoint : lowCloseCandlePoint;
            var lowestLowIndex = tickList.IndexOf(lowestLow);

            PriceChart.Axes.First(x => x.Key == "Value").Minimum = tickList[lowestLowIndex - 3];
            PriceChart.Axes.First(x => x.Key == "Value").Maximum = tickList[highestHighIndex + 3];

            using (var stream = File.Create(fileName))
            {
                var pngExporter = new OxyPlot.Wpf.PngExporter() { Width = 2048, Height = 1024, Resolution = 96 };
                pngExporter.Export(PriceChart, stream);
            }
        }
    }
}


                //if (strategyUnderTest.RunnerProfitTarget)
                //{
                //    isProfitMet = IsRunnerAboveProfit(simulatedTrades.Sum(x => x.pnl));
                //}

                //if (!isProfitMet)
                //{

                                            //if (strategyUnderTest.canBack)
                            //{
                            //    isValidCandlestick = true;

                            //    if (strategyUnderTest.compareCandlesticksBack)
                            //    {
                            //        var currentCandlestick = CandleStickSeries.Items.FirstOrDefault(x => x.X == currentPoint.X);
                            //        var indexOfCurrent = CandleStickSeries.Items.IndexOf(currentCandlestick);
                            //        isValidCandlestick = IsCandlesticksValid(currentCandlestick, CandleStickSeries.Items[CandleStickSeries.Items.IndexOf(currentCandlestick) - 1]);
                            //    }

                            //    if (isValidCandlestick)

 //
  //if (strategyUnderTest.canLay)
  //                          {
  //                              isValidCandlestick = true;

  //                              if (strategyUnderTest.compareCandlesticksLay)
  //                              {
  //                                  var currentCandlestick = CandleStickSeries.Items.FirstOrDefault(x => x.X == currentPoint.X);
  //                                  var indexOfCurrent = CandleStickSeries.Items.IndexOf(currentCandlestick);
  //                                  isValidCandlestick = IsCandlesticksValid(currentCandlestick, CandleStickSeries.Items[CandleStickSeries.Items.IndexOf(currentCandlestick) - 1]);
  //                              }

//var timeUntilRaceOff = _raceStartTime - currentTime;
//                                        if (timeUntilRaceOff.TotalMinutes > 10)
//                                        {
//                                            var currentPrice = CandleStickSeries.Items.FirstOrDefault(x => x.X == currentClosing.X).Close;
//                                            if (strategyUnderTest.isProfitTargetBeforeTenMinutes)
//                                            {
//                                                if (currentPrice >= targetPrice)
//                                                {
//                                                    tradeModel.BackOdds = CandleStickSeries.Items.FirstOrDefault(x => x.X == currentClosing.X).Close;
//                                                    tradeModel.TradeCloseTime = DateTimeAxis.ToDateTime(CandleStickSeries.Items.FirstOrDefault(x => x.X == currentClosing.X).X);
//                                                    simulatedTrades.Add(tradeModel);
//                                                    break;
//                                                }
//                                            }
//                                        }


//if (strategyUnderTest.hasMinVolume)
//{
//    for (var i = 1; i < numberOfAveragePoints; i++)
//    {
//        var currentPoint = AverageDifference.Points[i];
//        var currentVolume = MatchedVolume[currentPoint.X];
//        if (isVolumeValid(currentVolume, strategyUnderTest.minimumVolume))
//        {
//            startingIndexForVolume = AverageDifference.Points.IndexOf(currentPoint);
//            break;
//        }
//    }
//}

//var startingIndexForBeforeRaceTime = 1;
//if (strategyUnderTest.hasMinBeforeRaceOff)
//{
//    for (var i = 1; i < numberOfAveragePoints; i++)
//    {
//        var currentPoint = AverageDifference.Points[i];
//        if (IsTimeOfDataPointValidBeforeRace(currentPoint.X, strategyUnderTest.minutesBeforeRaceOff))
//        {
//            startingIndexForBeforeRaceTime = AverageDifference.Points.IndexOf(currentPoint);
//            break;
//        }
//    }
//}
