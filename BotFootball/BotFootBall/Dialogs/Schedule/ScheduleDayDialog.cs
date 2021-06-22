using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using BotFootBall.Services;
using BotFootBall.Models;

namespace BotFootBall.Dialogs.Schedule 
{
    public class ScheduleDayDialog : ComponentDialog
    {
        private readonly ISchedule _schedule;
       public ScheduleDayDialog(ISchedule schedule): base(nameof(ScheduleDayDialog))
        {
            _schedule = schedule;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]{
                  InitScheduleStepAsyc,
                  MoreScheduleStepAsyc,
                  CompleteStepAsyc,
            }));
            InitialDialogId = nameof(WaterfallDialog);
        }
        private async Task<DialogTurnResult> InitScheduleStepAsyc(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             _schedule.GetJsonSchedule();
            List<MatchesModel> Listmatches  = _schedule.GetScheduleDay();
            string response = string.Empty;
             foreach(var item in Listmatches)
            {
                response += item.Group+'\n';
                response += $"Home {item.HomeTeam} vs ";
                response += $"Away {item.AwayTeam} ";
                response += $"({item.UctDate})\n";
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(response), cancellationToken);
            }
            return await stepContext.NextAsync(null, cancellationToken);
        }

      
        private async Task<DialogTurnResult> MoreScheduleStepAsyc(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.NextAsync(null, cancellationToken);
        }
        private async Task<DialogTurnResult> CompleteStepAsyc(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        
    }
}
