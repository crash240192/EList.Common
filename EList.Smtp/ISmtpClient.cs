using EList.Smtp.Models;
using System;
using System.Threading.Tasks;

namespace EList.Smtp
{
    public interface ISmtpClient
    {
        [Obsolete("Consider using 'await SendMessageAsync'")]
        void SendMessage(Message message);

        [Obsolete("Consider using 'await SendMessageAsync'")]
        void SendMessage(string correlationId, Message message);
        Task SendMessageAsync(string correlationId, Message message);
    }
}
