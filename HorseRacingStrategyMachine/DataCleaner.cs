using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HorseRacingStrategyMachine
{
    public class DataCleaner
    {
        private const int minimumRecordPointCount = 300;
        private const int maximumSecondsGapBetweenRecords = 60;
        public bool CleanData(List<HorseRaceDataMap> records)
        {
            var minRecordPass = CheckMinimumDataPointCount(records);
            var minGapPass = CheckGapBetweenDataPoints(records);

            if (minGapPass && minRecordPass)
            {
                return true;
            }
            return false;  
        }

        private bool CheckMinimumDataPointCount(IEnumerable<HorseRaceDataMap> records)
        {
            return records.Count() > minimumRecordPointCount;
        }

        private bool CheckGapBetweenDataPoints(List<HorseRaceDataMap> records)
        {
            var recordCount = records.Count();
            for (int i = 1; i < recordCount; i++)
            {
                if ((records[i].DateTime - records[i-1].DateTime).TotalSeconds > maximumSecondsGapBetweenRecords)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
