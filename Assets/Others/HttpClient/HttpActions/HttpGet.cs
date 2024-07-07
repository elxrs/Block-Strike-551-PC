using System;
using System.Net;

namespace CI.HttpClient.Core
{
	public class HttpGet : HttpBase
	{
		public HttpGet(HttpWebRequest request, IDispatcher dispatcher)
		{
			_request = request;
			_dispatcher = dispatcher;
		}

		public void GetString(Action<HttpResponseMessage<string>> responseCallback)
		{
			try
			{
				SetMethod(HttpAction.Get);
				HandleStringResponseRead(responseCallback);
			}
			catch (Exception exception)
			{
				RaiseErrorResponse(responseCallback, exception);
			}
		}

		public void GetByteArray(HttpCompletionOption completionOption, Action<HttpResponseMessage<byte[]>> responseCallback, int blockSize)
		{
			try
			{
				SetMethod(HttpAction.Get);
				HandleByteArrayResponseRead(responseCallback, completionOption, blockSize);
			}
			catch (Exception exception)
			{
				RaiseErrorResponse(responseCallback, exception);
			}
		}
	}
}
