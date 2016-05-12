using CsvHelper;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HorseRacingStrategyMachine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataCleaner dataCleaner;

        public MainWindow()
        {
            this.DataContext = this;
            dataCleaner = new DataCleaner();
            Main();
            InitializeComponent();
        }

        private void Main()
        {
            //DeleteAllFilesInFolder("Results");
            DeleteAllFilesInFolder("EveningResults");
            ProcessEveningBetsOnly();
            string baseFolder = @"C:\Users\Casson306\Documents\PriceAnalysis\";
            var dateList = new DateList();

            foreach (var date in dateList.DatesToProcess)
            {
                string folderPath = baseFolder + date + @"\";

                DirectoryInfo dir = new DirectoryInfo(folderPath);
                string folderName = dir.Name;

                Strategies strategies = new Strategies();
                var allStrategyCombinations = strategies.GetStrategiesForTest();

                string filePathForModellingOutcomes = folderPath + @"Results\";
                System.IO.Directory.CreateDirectory(filePathForModellingOutcomes);
                string modellingOutcomesFile = filePathForModellingOutcomes + "ModellingOutcomes.csv";

                using (StreamWriter writer = new StreamWriter(modellingOutcomesFile, true))
                {
                    writer.WriteLine("Strategy,PandL");
                };

                string[] fileEntries = Directory.GetFiles(folderPath);

                List<KeyValuePair<string, string>> toDelete = new List<KeyValuePair<string, string>>();

                Dictionary<StrategyParameters, List<TradeModel>> strategyModelling = new Dictionary<StrategyParameters, List<TradeModel>>();

                foreach (var strategy in allStrategyCombinations)
                {
                    strategyModelling.Add(strategy, new List<TradeModel>());
                }

                foreach (var file in fileEntries)
                {
                    using (var sr = new StreamReader(@file))
                    {
                        var reader = new CsvReader(sr);
                        try
                        {
                            var records = reader.GetRecords<HorseRaceDataMap>().ToList();
                            if (dataCleaner.CleanData(records))
                            {
                                var priceChart = new ChartBuilder(records.FirstOrDefault().MarketOpen);
                                for (var i = 50; i < records.Count(); i++)
                                {
                                    var currentRecord = records[i];
                                    var previousRecord = records[i - 1];
                                    var previousTwentySet = records.Skip(i - 19).Take(20).ToList();
                                    var previousFiftySet = records.Skip(i - 49).Take(50).ToList();
                                    var currentTwentyAverage = previousTwentySet.Average(x => x.Close);
                                    var currentFiftyAverage = previousFiftySet.Average(x => x.Close);
                                    priceChart.AddDataPoint(currentRecord, currentTwentyAverage, currentFiftyAverage);
                                }

                                    //var simulatedTrades = priceChart.SimulateTrades(System.IO.Path.GetFileName(@file), strategyModelling);
                                    
                                
                                //simulatedTrades.ForEach(x => strategyUnderTest.Value.Add(x));

                                string fileName = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\PriceAnalysis\\" + System.IO.Path.GetFileNameWithoutExtension(@file) + ".png";
                                if (!System.IO.File.Exists(fileName))
                                {
                                    priceChart.ExportChart(fileName);
                                }
                            }
                            else
                            {
                                toDelete.Add(new KeyValuePair<string, string>(@file, "Not clean data"));
                            }
                        }
                        catch (Exception e)
                        {
                            toDelete.Add(new KeyValuePair<string, string>(@file, e.Message));
                        }
                    }
                }
                if (toDelete.Any())
                {
                    foreach (var file in toDelete.Select(x => x.Key))
                    {
                        File.Move(@file, @"C:\Users\Casson306\Desktop\Exceptions\" + System.IO.Path.GetFileName(@file));
                    }
                }

                foreach (var strategy in strategyModelling)
                {
                    var pnl = Math.Round(strategy.Value.Sum(x => x.pnl), 2);
                    using (StreamWriter writer = new StreamWriter(modellingOutcomesFile, true))
                    {
                        writer.WriteLine(string.Format("{0},{1}", strategy.Key.strategyName, pnl));
                    }

                    if (pnl > 0)
                    {
                        using (StreamWriter writer = new StreamWriter(filePathForModellingOutcomes + strategy.Key.strategyName + ".csv", true))
                        {
                            writer.WriteLine("RunnerName,TradeOpenTime,TradeCloseTime,BackOdds,LayOdds,PNL");

                            foreach (var trade in strategy.Value)
                            {
                                writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5}", trade.RunnerName, trade.TradeOpenTime, trade.TradeCloseTime, trade.BackOdds, trade.LayOdds, Math.Round(trade.pnl, 2)));
                            }
                        }
                    }
                }
            }
            AddDailyStrategyPnL();
        }

        private void AddDailyStrategyPnL()
        {
            string baseFolder = @"C:\Users\Casson306\Documents\PriceAnalysis\";
            string resultsFolder = @"\Results\ModellingOutcomes.csv";
            
            var dateList = new DateList();

            Strategies strategies = new Strategies();
            var allStrategyCombinations = strategies.GetStrategiesForTest();
            
            var strategyPnlPerDay = new List<SingleStrategyDayPnL>();

            foreach (var date in dateList.DatesToProcess)
            {
                using (var sr = new StreamReader(baseFolder + date + resultsFolder))
                {
                    var reader = new CsvReader(sr);
                    var records = reader.GetRecords<StrategyModelMap>().ToList();
                    var parsedDate = DateTime.Parse(date);
                    foreach (var record in records)
                    {
                        strategyPnlPerDay.Add(new SingleStrategyDayPnL() { Date = parsedDate, PnL = record.PandL, StrategyName = record.Strategy });
                    }
                }
            }

            var grouping = strategyPnlPerDay.GroupBy(x => x.StrategyName).Where(x => x.All(z => z.PnL > 0) || x.Sum(z => z.PnL) > 40);

            using (StreamWriter writer = new StreamWriter(@"C:\Users\Casson306\Desktop\AllStrategyByDay.csv", true))
            {
                var titleLine = new StringBuilder();
                titleLine.Append("Strategy");

                foreach (var date in dateList.DatesToProcess)
                {
                    titleLine.Append("," + date);
                }
                writer.WriteLine(titleLine);
                
                foreach (var strategy in grouping)
                {
                    var sb = new StringBuilder();
                    sb.Append(strategy.Key);
                    var pnlList = strategy.Select(x => x.PnL);
                    foreach (double pnl in pnlList)
                    {
                        sb.Append("," + Math.Round(pnl, 2));
                    }
                    writer.WriteLine(sb);
                }
            }
        }

        private void ProcessEveningBetsOnly()
        {
            string baseFolder = @"C:\Users\Casson306\Documents\PriceAnalysis\";
            var dateList = new DateList();

            foreach (var date in dateList.DatesToProcess)
            {
                string folderPath = baseFolder + date + @"\";

                DirectoryInfo dir = new DirectoryInfo(folderPath);
                string folderName = dir.Name;

                Strategies strategies = new Strategies();
                var allStrategyCombinations = strategies.GetStrategiesForTest();

                string filePathForModellingOutcomes = folderPath + @"EveningResults\";
                System.IO.Directory.CreateDirectory(filePathForModellingOutcomes);
                string modellingOutcomesFile = filePathForModellingOutcomes + "ModellingOutcomesForEveningBets.csv";

                using (StreamWriter writer = new StreamWriter(modellingOutcomesFile, true))
                {
                    writer.WriteLine("Strategy,PandL");
                };

                string[] fileEntries = Directory.GetFiles(folderPath);

                List<KeyValuePair<string, string>> toDelete = new List<KeyValuePair<string, string>>();

                Dictionary<StrategyParameters, List<TradeModel>> strategyModelling = new Dictionary<StrategyParameters, List<TradeModel>>();

                foreach (var strategy in allStrategyCombinations)
                {
                    strategyModelling.Add(strategy, new List<TradeModel>());
                }

                foreach (var file in fileEntries)
                {
                    using (var sr = new StreamReader(@file))
                    {
                        var reader = new CsvReader(sr);
                        try
                        {
                            var records = reader.GetRecords<HorseRaceDataMap>().ToList();
                            if (dataCleaner.CleanData(records))
                            {
                                var priceChart = new ChartBuilder(records.FirstOrDefault().MarketOpen);
                                var openingPointTime = records[50].DateTime.TimeOfDay;
                                var getFirstTimePoint = new TimeSpan(openingPointTime.Hours, openingPointTime.Minutes, openingPointTime.Seconds);
                                var eveningStart = new TimeSpan(17, 0, 0);
                                if (getFirstTimePoint > eveningStart)
                                {
                                    for (var i = 50; i < records.Count(); i++)
                                    {
                                        var currentRecord = records[i];
                                        var previousRecord = records[i - 1];
                                        var previousTwentySet = records.Skip(i - 19).Take(20).ToList();
                                        var previousFiftySet = records.Skip(i - 49).Take(50).ToList();
                                        var currentTwentyAverage = previousTwentySet.Average(x => x.Close);
                                        var currentFiftyAverage = previousFiftySet.Average(x => x.Close);
                                        priceChart.AddDataPoint(currentRecord, currentTwentyAverage, currentFiftyAverage);
                                    }

                                    foreach (var strategyUnderTest in strategyModelling)
                                    {
                                        //var simulatedTrades = priceChart.SimulateTrades(System.IO.Path.GetFileName(@file), strategyUnderTest.Key);
                                        //simulatedTrades.ForEach(x => strategyUnderTest.Value.Add(x));
                                    }

                                    string fileName = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\PriceAnalysis\\Graphs\\" + System.IO.Path.GetFileNameWithoutExtension(@file) + ".png";
                                    if (!System.IO.File.Exists(fileName))
                                    {
                                        priceChart.ExportChart(fileName);
                                    }
                                }
                            }
                            else
                            {
                                toDelete.Add(new KeyValuePair<string, string>(@file, "Not clean data"));
                            }
                        }
                        catch (Exception e)
                        {
                            toDelete.Add(new KeyValuePair<string, string>(@file, e.Message));
                        }
                    }
                }
                if (toDelete.Any())
                {
                    foreach (var file in toDelete.Select(x => x.Key))
                    {
                        File.Move(@file, @"C:\Users\Casson306\Desktop\Exceptions\" + System.IO.Path.GetFileName(@file));
                    }
                }

                foreach (var strategy in strategyModelling)
                {
                    var pnl = Math.Round(strategy.Value.Sum(x => x.pnl), 2);
                    using (StreamWriter writer = new StreamWriter(modellingOutcomesFile, true))
                    {
                        writer.WriteLine(string.Format("{0},{1}", strategy.Key.strategyName, pnl));
                    }

                    if (pnl > 0)
                    {
                        using (StreamWriter writer = new StreamWriter(filePathForModellingOutcomes + strategy.Key.strategyName + ".csv", true))
                        {
                            writer.WriteLine("RunnerName,TradeOpenTime,TradeCloseTime,BackOdds,LayOdds,PNL");

                            foreach (var trade in strategy.Value)
                            {
                                writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5}", trade.RunnerName, trade.TradeOpenTime, trade.TradeCloseTime, trade.BackOdds, trade.LayOdds, Math.Round(trade.pnl, 2)));
                            }
                        }
                    }
                }
            }
        }

        private void DeleteAllFilesInFolder(string folderTarget)
        {
            string baseFolder = @"C:\Users\Casson306\Documents\PriceAnalysis\";
            string resultsFolder = @"\" + folderTarget + @"\";
            var dateList = new DateList();

            foreach (var date in dateList.DatesToProcess)
            {
                var folderPath = baseFolder + date + resultsFolder;
                if (System.IO.Directory.Exists(folderPath))
                {
                    string[] fileEntries = Directory.GetFiles(baseFolder + date + resultsFolder);
                    foreach (var file in fileEntries)
                    {
                        File.Delete(@file);
                    }
                }
            }
        }
    }

    public class SingleStrategyDayPnL
    {
        public string StrategyName { get; set; }
        public DateTime Date { get; set; }
        public double PnL { get; set; }
    }
}