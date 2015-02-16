using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SampleSmtpClient
{
    public class SmtpClientWrapper : IDisposable
    {
        private SmtpClient _smtpClient;

        public SmtpClientWrapper(SmtpClient smtpClient)
        {
            this._smtpClient = smtpClient;
        }

        public async Task SendMailAsync(MailMessage message, CancellationToken ct)
        {
            try
            {
                ct.ThrowIfCancellationRequested();
                var task = _smtpClient.SendMailAsync(message);
                using (ct.Register(_smtpClient.SendAsyncCancel))
                {
                    try
                    {
                        await task;
                    }
                    catch (OperationCanceledException exception)
                    {
                        if (exception.CancellationToken == ct)
                        {
                            Trace.TraceWarning("Operation has been canceled.");
                            return;
                        }

                        throw;
                    }
                }
            }
            catch (Exception exception)
            {
                Trace.TraceError("Unexpected exception occured; Exception details: {0}", exception.ToString());
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool p)
        {
            if (_smtpClient != null)
            {
                _smtpClient.Dispose();
                _smtpClient = null;
            }
        }

    }
}
