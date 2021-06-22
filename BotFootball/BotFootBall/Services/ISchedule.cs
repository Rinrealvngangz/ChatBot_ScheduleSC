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
        public void DisPlayScheduleByStep(WaterfallStepContext stepContext, CancellationToken cancellation);
        public void DisPlayScheduleByText(ITurnContext<IMessageActivity> stepContext, CancellationToken cancellation);
    }
}
