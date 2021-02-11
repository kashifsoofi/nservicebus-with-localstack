namespace Api.Core.Controllers
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NServiceBus;
    using Shared.Core;

    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageSession _messageSession;

        public MessagesController(IMessageSession messageSession)
        {
            _messageSession = messageSession;
        }

        [HttpPost]
        public async Task<ActionResult<string>> Post()
        {
            var guid = Guid.NewGuid();
            var message = new RequestDataMessage
            {
                DataId = guid,
                String = "String property value"
            };

            await _messageSession.Send(message)
                .ConfigureAwait(false);

            return guid.ToString();
        }

        [Route("bigmessage")]
        [HttpPost]
        public async Task<ActionResult<string>> BigMessage()
        {
            var guid = Guid.NewGuid();
            var message = new RequestDataMessage
            {
                DataId = guid,
                String = RandomString(256000, false)
            };

            await _messageSession.Send(message)
                .ConfigureAwait(false);

            return guid.ToString();
        }

        [HttpPost("callback")]
        public async Task<IActionResult> Callback()
        {
            var guid = Guid.NewGuid();
            var message = new RequestDataMessage
            {
                DataId = guid,
                String = "String property value"
            };

            var response = await _messageSession.Request<DataResponseMessage>(message)
                .ConfigureAwait(false);

            return new OkObjectResult(new
            {
                RequestId = guid,
                ResponseId = response.Id,
            });
        }

        private static string RandomString(int size, bool lowerCase)
        {
            var builder = new StringBuilder();
            var random = new Random();
            for (var i = 0; i < size; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }
    }
}
