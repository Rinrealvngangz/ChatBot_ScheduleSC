using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using AdaptiveCards;
using Newtonsoft.Json.Linq;
using BotFootBall.Dialogs.Schedule;
using BotFootBall.Models;
using BotFootBall.Services;

namespace BotFootBall.Dialogs
{
    public class MainDialog :  ComponentDialog
    {
        private readonly ISchedule _schedule;
        public MainDialog(ScheduleDayDialog scheduleDayDialog, ISchedule schedule) : base(nameof(MainDialog))
        {
            _schedule = schedule;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(scheduleDayDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] {
               IntroStepAsync,
               ActStepAsync,
               FinalStepAsync,

             }));
            InitialDialogId = nameof(WaterfallDialog);


        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // await stepContext.Context.SendActivityAsync(
            //  MessageFactory.Text("Bạn hãy gõ 'help' để biết các lệnh"), cancellationToken);
            if (stepContext.Options != null)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(stepContext.Options.ToString()), cancellationToken);
            
            }
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
               // Prompt = MessageFactory.Text("Tôi có thể giúp gì cho bạn?")

            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string rs = (string)stepContext.Result;
             switch (rs.ToLower())
            {
          
                case "hôm nay":
                  return await stepContext.BeginDialogAsync(nameof(ScheduleDayDialog),null,cancellationToken);
                case "ngày mai":
                    DateTime dt = DateTime.UtcNow.AddDays(1);
                    _schedule.DisPlayScheduleByStep(dt, stepContext, cancellationToken);
                    break;
                case "trong tuần":
                    _schedule.GetDateTimeOfWeeks().ForEach(dt => _schedule.DisPlayScheduleByStep(dt, stepContext, cancellationToken));
                    break;
                default:
                   return await stepContext.ReplaceDialogAsync(InitialDialogId, "Xin lỗi,Tôi không hiểu", cancellationToken);

            }
            return await stepContext.NextAsync(null, cancellationToken);

        }
       
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Restart the main dialog with a different message the second time around
            var promptMessage = "What else can I do for you?";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(promptMessage), cancellationToken);
            //return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
            return await stepContext.ReplaceDialogAsync(InitialDialogId,null,cancellationToken);
        }

    
        private async Task<ResourceResponse> HelpBot(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string help = "Chức năng của bot\n\n'lịch euro hôm nay': xem lịch trong ngày\n\n'" +
                          "lịch euro ngày mai': xem lịch thi đấu ngày mai\n\n"+
                          "lịch euro trong tuần': xem lịch thi đấu trong tuần\n\n";
            return await stepContext.Context.SendActivityAsync(
            MessageFactory.Text(help), cancellationToken);
          
        }

    }
}
