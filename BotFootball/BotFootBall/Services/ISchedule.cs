using BotFootBall.Models;
using System.Collections.Generic;
using System;
using Microsoft.Bot.Builder;
using System.Threading;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace BotFootBall.Services
{
  public  interface ISchedule
    {
        public Task<DialogTurnResult> AppointmentSchedule(DateTime dateTime, WaterfallStepContext stepContext, CancellationToken cancellation);
        public void DisPlayScheduleByStep(DateTime dateTime,WaterfallStepContext stepContext, CancellationToken cancellation);
        public List<DateTime> GetDateTimeOfWeeks();
    }
}
