using System.Threading.Tasks;
using DnsClient.Internal;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Play.Identity.Contracts;
using Play.Identity.Service.Entities;
using Play.Identity.Service.Exceptions;

namespace Play.Identity.Service.Consumers
{
    public class DebitGilConsumer : IConsumer<DebitGil>
    {
        private readonly ILogger<DebitGilConsumer> logger;
        private readonly UserManager<ApplicationUser> userManager;

        public DebitGilConsumer(UserManager<ApplicationUser> userManager, ILogger<DebitGilConsumer> logger)
        {
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<DebitGil> context)
        {
            logger.LogInformation(
                "Debiting amount {Gil} of gil from user {UserId} with CorrelationId {CorrelationId}",
                context.Message.Gil,
                context.Message.UserId,
                context.Message.CorrelationId
            );

            var message = context.Message;

            var user = await userManager.FindByIdAsync(message.UserId.ToString());

            if (user == null)
            {
                throw new UnknownUserException(message.UserId);
            }

            if (user.MessageIds.Contains(context.MessageId.Value))
            {
                await context.Publish(new GilDebited(message.CorrelationId));
                return;
            }

            user.Gil -= message.Gil;

            if (user.Gil < 0)
            {
                logger.LogError("Could not the total amount {Gil} gil from user {UserId} with CorrelationId {CorrelationId}. Error {ErrorMessage}");

                throw new InsufficientFundsException(message.UserId, message.Gil);
            }

            user.MessageIds.Add(context.MessageId.Value);

            await userManager.UpdateAsync(user);

            var gilDebitedTask = context.Publish(new GilDebited(message.CorrelationId));
            var userUpdatedTask = context.Publish(new UserUpdated(user.Id, user.Email, user.Gil));

            await Task.WhenAll(userUpdatedTask, gilDebitedTask);
        }
    }
}