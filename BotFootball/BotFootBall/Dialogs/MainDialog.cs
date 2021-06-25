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
using System.Globalization;

namespace BotFootBall.Dialogs
{
    public class MainDialog :  ComponentDialog
    {
        private DateTime dateTime;
        private readonly ISchedule _schedule;
        private readonly IStandingService _standingService;
        public MainDialog(ScheduleDayDialog scheduleDayDialog, ISchedule schedule ,IStandingService standingService) : base(nameof(MainDialog))
        {
            _schedule = schedule;
            _standingService = standingService;
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
            string filter = string.Empty;
            const string GROUP = "group_";
            int day;
            int month;
            if (rs.StartsWith("XH_"))
            {
                filter =  rs.Split('_')[1].ToUpper();
                rs = "xếp hạng";
            }else if (rs.Length > 0 && char.IsNumber(rs[0]))
            {
                string[] formats = { "M/dd" };      
                bool checkMonth = Int32.TryParse(rs.Split('/')[0], out month);
                bool checkDay = Int32.TryParse(rs.Split('/')[1], out day);
                bool checkDate = DateTime.TryParseExact(rs, formats, new CultureInfo("en-US"),
                                   DateTimeStyles.None, out dateTime);
                bool checkRangeDayMonth = day <= 11 && month == 7 || day >= 11 && month == 6;
                bool checkOutRangeDayMonth = day >= 11 && month == 7 || day <= 11 && month == 6 || month != 7 || month != 6;
                if (checkRangeDayMonth && checkDay && checkMonth && checkDate)
                {
                    rs = "ngày thi đấu";
                }
                else if(checkOutRangeDayMonth)
                {
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, "Chọn ngày thi đấu trong khoảng (06/11 - 07/11).", cancellationToken);

                }
                if (checkDate== false)
                {
                    return await stepContext.ReplaceDialogAsync(InitialDialogId, "format không đúng ngày tháng, dùng help xem lệnh.", cancellationToken);
                }
            }
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
                case "xếp hạng":
                    string group = GROUP.ToUpper() + filter;
                    
                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment( await _standingService.GetBotStading(group)), cancellationToken);
                    break;
                case "ngày thi đấu":
                    _schedule.DisPlayScheduleByStep( dateTime, stepContext, cancellationToken);
                    break;
                default:
                   return await stepContext.ReplaceDialogAsync(InitialDialogId, "Xin lỗi,Tôi không hiểu", cancellationToken);

            }
            return await stepContext.NextAsync(null, cancellationToken);

        }
       
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
          
            //return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
            return await stepContext.ReplaceDialogAsync(InitialDialogId,null,cancellationToken);
        }

    

    }
}
