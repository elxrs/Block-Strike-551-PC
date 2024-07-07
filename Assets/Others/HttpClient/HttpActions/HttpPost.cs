using System;
using System.Net;

namespace CI.HttpClient.Core
{
	public class HttpPost : HttpBase
	{
		public HttpPost(HttpWebRequest request, IDispatcher dispatcher)
		{
			_request = request;
			_dispatcher = dispatcher;
		}

		public void Post(IHttpContent content, Action<HttpResponseMessage<string>> responseCallback, Action<UploadStatusMessage> uploadStatusCallback, int uploadBlockSize)
		{
			try
			{
				SetMethod(HttpAction.Post);
				SetContentHeaders(content);
				HandleRequestWrite(content, uploadStatusCallback, uploadBlockSize);
				HandleStringResponseRead(responseCallback);
			}
			catch (Exception exception)
			{
				RaiseErrorResponse(responseCallback, exception);
			}
		}

		public void Post(IHttpContent content, HttpCompletionOption completionOption, Action<HttpResponseMessage<byte[]>> responseCallback, Action<UploadStatusMessage> uploadStatusCallback, int downloadBlockSize, int uploadBlockSize)
		{
			try
			{
				SetMethod(HttpAction.Post);
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
