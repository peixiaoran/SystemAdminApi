namespace SystemAdmin.Common.Utilities
{
    /// <summary>
    /// 请假工时计算（工作时段 8:00-12:00、13:00-17:00）
    /// </summary>
    public static class LeaveHoursCalculator
    {
        /// <summary>
        /// 按自然年拆分并累计工时
        /// </summary>
        public static Dictionary<int, decimal> CalcHoursByYear(DateTime start, DateTime end)
        {
            var hoursByYear = new Dictionary<int, decimal>();

            for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                var dayHours = Overlap(start, end, date.AddHours(8), date.AddHours(12))
                               + Overlap(start, end, date.AddHours(13), date.AddHours(17));

                if (dayHours > 0)
                {
                    hoursByYear[date.Year] = hoursByYear.GetValueOrDefault(date.Year) + dayHours;
                }
            }

            return hoursByYear;
        }

        /// <summary>
        /// 指定年度内的工时
        /// </summary>
        public static decimal CalcHoursInYear(DateTime start, DateTime end, int year) =>
            CalcHoursByYear(start, end).GetValueOrDefault(year);

        /// <summary>
        /// 总工时
        /// </summary>
        public static decimal CalcTotalHours(DateTime start, DateTime end) =>
            CalcHoursByYear(start, end).Values.Sum();

        private static decimal Overlap(DateTime start, DateTime end, DateTime periodStart, DateTime periodEnd)
        {
            var from = start > periodStart ? start : periodStart;
            var to = end < periodEnd ? end : periodEnd;
            return to > from ? Math.Round((decimal)(to - from).TotalHours, 2) : 0m;
        }
    }
}
