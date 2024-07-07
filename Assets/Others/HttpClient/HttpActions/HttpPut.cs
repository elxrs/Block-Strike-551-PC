using System;
using System.Net;

namespace CI.HttpClient.Core
{
	public class HttpPut : HttpBase
	{
		public HttpPut(HttpWebRequest request, IDispatcher dispatcher)
		{
			_request = request;
			_dispatcher = dispatcher;
		}

		public void Put(IHttpContent content, Action<HttpResponseMessage<string>> responseCallback, Action<UploadStatusMessage> uploadStatusCallback, int uploadBlockSize)
		{
			try
			{
				SetMethod(HttpAction.Put);
				SetContentHeaders(content);
				HandleRequestWrite(content, uploadStatusCallback, uploadBlockSize);
				HandleStringResponseRead(responseCallback);
			}
			catch (Exception exception)
			{
				RaiseErrorResponse(responseCallback, exception);
			}
		}

		public void Put(IHttpContent content, HttpCompletionOption completionOption, Action<HttpResponseMessage<byte[]>> responseCallback, Action<UploadStatusMessage> uploadStatusCallback, int downloadBlockSize, int uploadBlockSize)
		{
			try
			{
				SetMethod(HttpAction.Put);
				SetContentHeaders(content);
				HandleRequestWrite(content, uploadStatusCallback, uploadBlockSize);
				HandleByteArrayResponseRead(responseCallback, completionOption, downloadBlockSize);
			}
			catch (Exception exception)
			{
				RaiseErrorResponse(responseCallback, exception);
			}
		}
	}
}
