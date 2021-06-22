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
using Microsoft.Extensions.Logging;
using BotFootBall.Dialogs.Schedule;

namespace BotFootBall.Dialogs
{
    public class MainDialog : ComponentDialog 
    {
      
        public MainDialog( ScheduleDayDialog scheduleDayDialog ) : base(nameof(MainDialog))
        {
           AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
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
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("Chọn lịch thi đấu muốn xem ?"), cancellationToken);

            List<string> operationList = new List<string> { "Lịch euro hôm nay", "Lịch euro trong tuần", "Lịch euro ngày mai" };

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                // Use LINQ to turn the choices into submit actions
                Actions = operationList.Select(choice => new AdaptiveSubmitAction
                {
                    Title = choice,
                    Data = choice,  // This will be a string
                }).ToList<AdaptiveAction>(),
            };
            // Promt
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    // Convert the AdaptiveCard to a JObject
                    Content = JObject.FromObject(card),
                }),
                Choices = ChoiceFactory.ToChoices(operationList),
                Style = ListStyle.None
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
           stepContext.Values["Operation"] = ((FoundChoice)stepContext.Result).Value;
            string operation = (string)stepContext.Values["Operation"];
            if(operation.Equals("Lịch euro hôm nay"))
            {
                return await stepContext.BeginDialogAsync(nameof(ScheduleDayDialog),null,cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("You have selected - " + operation), cancellationToken);
            }
           
            return await stepContext.NextAsync(null, cancellationToken);
        }
       
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Restart the main dialog with a different message the second time around
            var promptMessage = "What else can I do for you?";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(promptMessage), cancellationToken);
            //return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
            return await stepContext.CancelAllDialogsAsync( cancellationToken);
        }


       
    }
}
