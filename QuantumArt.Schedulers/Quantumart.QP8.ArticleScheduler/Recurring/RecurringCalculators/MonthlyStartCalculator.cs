﻿using System;
using System.Linq;

namespace Quantumart.QP8.ArticleScheduler.Recurring.RecurringCalculators
{
    public class MonthlyStartCalculator : RecurringStartCalculatorBase
    {
        private readonly int _interval;
        private readonly int _recurrenceFactor;
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;
        private readonly TimeSpan _startTime;

        public MonthlyStartCalculator(int interval, int recurrenceFactor, DateTime startDate, DateTime endDate, TimeSpan startTime)
        {
            _interval = interval;
            _recurrenceFactor = recurrenceFactor;
            _startDate = startDate;
            _endDate = endDate;
            _startTime = startTime;

            CalculateNearestStartDateFunc = GetNearestStartDate;
        }

        private DateTime? GetNearestStartDate(DateTime dateTime)
        {
            return Optimize(new Tuple<DateTime, DateTime>(_startDate.Date, _endDate.Date), dateTime.Date)
                .GetEveryFullMonthLimitedByFactor(_recurrenceFactor) // получаем полные месяца, но только те, которые ограничены recurrenceFactor
                .GetAllDaysFromRange() // получаем даты
                .GetEveryNDayGroupedByMonth(_interval) // получить только те дни в каждом месяце которые соответствуют interval
                .Where(d => _startDate.Date <= d.Date && _endDate.Date >= d.Date) // только те даты что в диапазоне
                .Select(d => d.Add(_startTime)) // получаем точное время старта
                .GetNearestPreviousDateFromList(dateTime);
        }
    }
}
