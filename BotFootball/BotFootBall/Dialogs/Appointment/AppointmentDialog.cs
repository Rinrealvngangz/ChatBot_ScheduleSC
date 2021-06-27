using BotFootBall.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Activity = Microsoft.Bot.Schema.Activity;
using System.Net.Http;

namespace BotFootBall.Dialogs.Appointment
{
    public class AppointmentDialog : ComponentDialog
    {
        private readonly IHttpClientFactory _factory;
        private readonly TimersManage _timerManage;
        private readonly ISchedule _schedule;
        private static string vnTimeZoneKey = "SE Asia Standard Time";
        private static TimeZoneInfo vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById(vnTimeZoneKey);
        public AppointmentDialog(ISchedule schedule,TimersManage timerManage, IHttpClientFactory factory) : base(nameof(AppointmentDialog))
        {
            _schedule = schedule;
            _timerManage = timerManage;
            _factory = factory;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new TextPrompt("appointmentSchedule"));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]{
                  InitScheduleStepAsyc,
                  AcptScheduleStepAsyc,
                  CompleteStepAsyc,
            }));
            InitialDialogId = nameof(WaterfallDialog);
        }
        private  Task<DialogTurnResult> InitScheduleStepAsyc(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return CardPromSchedule(stepContext, cancellationToken);
        }

        private async Task<DialogTurnResult> AcptScheduleStepAsyc(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["Operation"] = ((FoundChoice)stepContext.Result).Value;
            string operation = (string)stepContext.Values["Operation"];
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(operation), cancellationToken);
            switch (operation.ToLower())
            {
                case "ngày mai":
                    DateTime tomorrow = DateTime.UtcNow.AddDays(1);
                   
                    return await _schedule.AppointmentSchedule(tomorrow, stepContext, cancellationToken);
                
                case "hôm nay":
                    DateTime today = DateTime.UtcNow;
                    return await _schedule.AppointmentSchedule(today, stepContext, cancellationToken);
                
                case "hủy":
                  
                    return await stepContext.EndDialogAsync(null, cancellationToken);     
                default:
                    await stepContext.Context.SendActivityAsync(
                                     MessageFactory.Text("Tôi không hiểu, mong bạn thông cảm."), cancellationToken);
                    break;

            }

            return await stepContext.NextAsync(null, cancellationToken);
          
        }

        private async Task<DialogTurnResult> CompleteStepAsyc(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             var rs = stepContext.Result;
            if (!rs.Equals("Hủy"))
            {
                DateTime currentTime = FormatTimeZoneVietNam(DateTime.UtcNow.AddMinutes(-3));
                TimeSpan time = Convert.ToDateTime(rs) - currentTime;
                int seconds = Convert.ToInt32(time.TotalSeconds);
                _timerManage.AddTimer(_factory, stepContext.Context.Activity.GetConversationReference(), seconds);
                await stepContext.Context.SendActivityAsync(
                                      MessageFactory.Text($"Đặt lịch hẹn {stepContext.Result} thành công."), cancellationToken);
            }
       

            await stepContext.Context.SendActivityAsync(
                                MessageFactory.Text("Hủy, nếu bạn không đặt lịch nữa."), cancellationToken);
            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
        }

        private async Task<DialogTurnResult> CardPromSchedule(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(
            MessageFactory.Text("Chọn ngày muốn đặt hẹn."), cancellationToken);

            List<string> operationList = new List<string> { "hôm nay", "ngày mai", "hủy" };

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                // Use LINQ to turn the choices into submit actions
                Actions = operationList.Select(choice => new AdaptiveSubmitAction
                {
                    Title = choice,
                    Data = choice,
                }).ToList<AdaptiveAction>(),
            };
            // Promt
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = JObject.FromObject(card),
                }),
                Choices = ChoiceFactory.ToChoices(operationList),
                Style = ListStyle.None
            }, cancellationToken);
        }

        private DateTime FormatTimeZoneVietNam(DateTime dateTime)
        {

            DateTime ngaygiohientai = TimeZoneInfo.ConvertTimeFromUtc(dateTime, vnTimeZone);
            return ngaygiohientai;
        }


    }
}
