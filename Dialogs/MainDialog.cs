using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace WebChatSSO.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly ILogger Logger;

        public MainDialog(IConfiguration configuration, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            Logger = logger;

            AddDialog(new SignInDialog(configuration));
            AddDialog(new SignOutDialog(configuration));
            AddDialog(new DisplayTokenDialog(configuration));
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                var text = innerDc.Context.Activity.Text.ToLowerInvariant();
                DialogTurnResult dialogResult;

                text = text.Replace(" ", string.Empty);

                // Top level commands
                if (text == "signin" || text == "login")
                {
                    dialogResult = await innerDc.BeginDialogAsync(nameof(SignInDialog), null, cancellationToken);
                }
                else if (text == "signout" || text == "logout")
                {
                    dialogResult = await innerDc.BeginDialogAsync(nameof(SignOutDialog), null, cancellationToken);
                }
                else if (text == "token" || text == "gettoken")
                {
                    dialogResult = await innerDc.BeginDialogAsync(nameof(DisplayTokenDialog), null, cancellationToken);
                }
                else
                {
                    await innerDc.Context.SendActivityAsync(MessageFactory.Text($"Echo: {text}"), cancellationToken);
                    dialogResult = new DialogTurnResult(DialogTurnStatus.Complete);
                }

                return dialogResult;
            }

            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }
    }
}
