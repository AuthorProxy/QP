﻿using System;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.ArticleScheduler;

namespace Quantumart.QP8.ArticleScheduler.Recurring
{
    internal class RecurringTaskScheduler
    {
        private readonly IArticleRecurringSchedulerService _bllService;
        private readonly IOperationsLogWriter _operationsLogWriter;

        public RecurringTaskScheduler(IArticleRecurringSchedulerService bllService, IOperationsLogWriter operationsLogWriter)
        {
            if (bllService == null)
            {
                throw new ArgumentNullException(nameof(bllService));
            }

            if (operationsLogWriter == null)
            {
                throw new ArgumentNullException(nameof(operationsLogWriter));
            }

            _bllService = bllService;
            _operationsLogWriter = operationsLogWriter;
        }

        public void Run(RecurringTask task)
        {
            if (task == null)
            {
                return;
            }

            var currentDateTime = _bllService.GetCurrentDBDateTime();
            var taskRange = Tuple.Create(task.StartDate + task.StartTime, task.EndDate + task.StartTime);
            var taskRangePos = taskRange.Position(currentDateTime);

            var calc = CreateRecuringStartCalc(task);

            // получить ближайшую дату начала показа статьи
            var showRangeStartDt = calc.GetStart(currentDateTime);
            if (!showRangeStartDt.HasValue)
            {
                // если вычислитель вернул null - значит ничего не делаем
                return;
            }

            // Определяем время окончания показа статьи
            var showRangeEndDt = showRangeStartDt.Value + task.Duration;
            if (taskRange.Position(showRangeEndDt) > 0)
            {
                // Если необходимо то ограничить время окончания показа статьи правой границей диапазона задачи
                showRangeEndDt = taskRange.Item2;
            }

            // диапазон показа статьи
            Article article;
            var showRange = Tuple.Create(showRangeStartDt.Value, showRangeEndDt);

            // определить положение текущей даты относительно диапазона показа статьи
            var showRangePos = showRange.Position(currentDateTime);
            if (showRangePos == 0)
            {
                // внутри диапазона показа
                article = _bllService.ShowArticle(task.ArticleId);
                if (article != null && !article.Visible)
                {
                    _operationsLogWriter.ShowArticle(article);
                }
            }
            else if (showRangePos > 0 && taskRangePos == 0)
            {
                // за диапазоном показа но внутри диапазона задачи
                article = _bllService.HideArticle(task.ArticleId);
                if (article != null && article.Visible)
                {
                    _operationsLogWriter.HideArticle(article);
                }
            }
            else if (showRangePos > 0 && taskRangePos > 0)
            {
                // за диапазоном показа и за диапазоном задачи
                article = _bllService.HideAndCloseSchedule(task.Id);
                if (article != null && article.Visible)
                {
                    _operationsLogWriter.HideArticle(article);
                }
            }
        }

        /// <summary>
        /// Создать вычислитель даты начала диапазона показа статьи в зависимости от типа задачи
        /// </summary>
        private IRecuringStartCalc CreateRecuringStartCalc(RecurringTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            switch (task.TaskType)
            {
                case RecurringTaskTypes.Daily:
                    return new DailyStartCalc(task.Interval, task.StartDate, task.EndDate, task.StartTime);
                case RecurringTaskTypes.Weekly:
                    return new WeeklyStartCalc(task.Interval, task.RecurrenceFactor, task.StartDate, task.EndDate, task.StartTime);
                case RecurringTaskTypes.Monthly:
                    return new MonthlyStartCalc(task.Interval, task.RecurrenceFactor, task.StartDate, task.EndDate, task.StartTime);
                case RecurringTaskTypes.MonthlyRelative:
                    return new MonthlyRelativeStartCalc(task.Interval, task.RelativeInterval, task.RecurrenceFactor, task.StartDate, task.EndDate, task.StartTime);
                default:
                    throw new ArgumentException("Неопределенный тип расписания циклической задачи: " + task.TaskType);
            }
        }
    }
}