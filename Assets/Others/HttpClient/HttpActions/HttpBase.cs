using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CI.HttpClient.Core
{
	public abstract class HttpBase
	{
		protected HttpWebRequest _request;

		protected HttpWebResponse _response;

		protected IDispatcher _dispatcher;

		protected void SetMethod(HttpAction httpAction)
		{
			_request.Method = httpAction.ToString().ToUpper();
		}

		protected void SetContentHeaders(IHttpContent content)
		{
			_request.ContentLength = content.GetContentLength();
			_request.ContentType = content.GetContentType();
		}

		protected void HandleRequestWrite(IHttpContent content, Action<UploadStatusMessage> uploadStatusCallback, int blockSize)
		{
			using (Stream stream = _request.GetRequestStream())
			{
				if (content.ContentReadAction == ContentReadAction.Multi)
				{
					WriteMultipleContent(stream, content, uploadStatusCallback, blockSize);
				}
				else
				{
					WriteSingleContent(stream, content, uploadStatusCallback, blockSize, content.GetContentLength(), 0L);
				}
			}
		}

		private void WriteMultipleContent(Stream stream, IHttpContent content, Action<UploadStatusMessage> uploadStatusCallback, int blockSize)
		{
			long contentLength = content.GetContentLength();
			long num = 0L;
			int num2 = 0;
			MultipartContent multipartContent = content as MultipartContent;
			foreach (IHttpContent item in multipartContent)
			{
				byte[] bytes = Encoding.UTF8.GetBytes("Content-Type: " + item.GetContentType());
				stream.Write(multipartContent.BoundaryStartBytes, 0, multipartContent.BoundaryStartBytes.Length);
				num += multipartContent.BoundaryStartBytes.Length;
				stream.Write(bytes, 0, bytes.Length);
				num += bytes.Length;
				stream.Write(multipartContent.CRLFBytes, 0, multipartContent.CRLFBytes.Length);
				num += multipartContent.CRLFBytes.Length;
				stream.Write(multipartContent.CRLFBytes, 0, multipartContent.CRLFBytes.Length);
				num += multipartContent.CRLFBytes.Length;
				num += WriteSingleContent(stream, item, uploadStatusCallback, blockSize, contentLength, num);
				stream.Write(multipartContent.CRLFBytes, 0, multipartContent.CRLFBytes.Length);
				num += multipartContent.CRLFBytes.Length;
				num2++;
			}
			if (num2 == 0)
			{
				stream.Write(multipartContent.BoundaryStartBytes, 0, multipartContent.BoundaryStartBytes.Length);
				num += multipartContent.BoundaryStartBytes.Length;
			}
			stream.Write(multipartContent.BoundaryEndBytes, 0, multipartContent.BoundaryEndBytes.Length);
			num += multipartContent.BoundaryEndBytes.Length;
			RaiseUploadStatusCallback(uploadStatusCallback, contentLength, multipartContent.CRLFBytes.Length * 2 + multipartContent.BoundaryEndBytes.Length, num);
		}

		private long WriteSingleContent(Stream stream, IHttpContent content, Action<UploadStatusMessage> uploadStatusCallback, int blockSize, long overallContentLength, long totalContentUploadedOverall)
		{
			long contentLength = content.GetContentLength();
			int num = 0;
			int num2 = 0;
			byte[] array = null;
			Stream stream2 = null;
			if (content.ContentReadAction == ContentReadAction.Stream)
			{
				array = new byte[blockSize];
				stream2 = content.ReadAsStream();
			}
			else
			{
				array = content.ReadAsByteArray();
			}
			while (num2 != contentLength)
			{
				num = 0;
				if (content.ContentReadAction == ContentReadAction.Stream)
				{
					int num3 = 0;
					while ((num3 = stream2.Read(array, num3, blockSize - num3)) > 0)
					{
						num += num3;
					}
					if (num > 0)
					{
						stream.Write(array, 0, num);
					}
				}
				else
				{
					num = ((blockSize <= array.Length - num2) ? blockSize : (array.Length - num2));
					stream.Write(array, num2, num);
				}
				num2 += num;
				totalContentUploadedOverall += num;
				RaiseUploadStatusCallback(uploadStatusCallback, overallContentLength, num, totalContentUploadedOverall);
			}
			return num2;
		}

		private void RaiseUploadStatusCallback(Action<UploadStatusMessage> uploadStatusCallback, long contentLength, long contentUploadedThisRound, long totalContentUploaded)
		{
			if (uploadStatusCallback != null)
			{
				_dispatcher.Enqueue(delegate
				{
					uploadStatusCallback(new UploadStatusMessage
					{
						ContentLength = contentLength,
						ContentUploadedThisRound = contentUploadedThisRound,
						TotalContentUploaded = totalContentUploaded
					});
				});
			}
		}

		protected void HandleStringResponseRead(Action<HttpResponseMessage<string>> responseCallback)
		{
			HttpWebResponse httpWebResponse = (_response = (HttpWebResponse)_request.GetResponse());
			using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
			{
				if (responseCallback != null)
				{
					RaiseResponseCallback(responseCallback, streamReader.ReadToEnd(), httpWebResponse.ContentLength, httpWebResponse.ContentLength);
				}
			}
		}

		protected void HandleByteArrayResponseRead(Action<HttpResponseMessage<byte[]>> responseCallback, HttpCompletionOption completionOption, int blockSize)
		{
			using (Stream stream = (_response = (HttpWebResponse)_request.GetResponse()).GetResponseStream())
			{
				if (responseCallback == null)
				{
					return;
				}
				long num = 0L;
				int num2 = 0;
				int num3 = 0;
				List<byte> list = new List<byte>();
				byte[] array = new byte[blockSize];
				do
				{
					num3 = stream.Read(array, num2, blockSize - num2);
					num2 += num3;
					if (num2 == blockSize || num3 == 0)
					{
						num += num2;
						byte[] array2 = new byte[num2];
						Array.Copy(array, array2, num2);
						if (completionOption == HttpCompletionOption.AllResponseContent)
						{
							list.AddRange(array2);
						}
						if (completionOption == HttpCompletionOption.StreamResponseContent || num3 == 0)
						{
							RaiseResponseCallback(responseCallback, (completionOption != 0) ? array2 : list.ToArray(), (completionOption != 0) ? num2 : num, num);
						}
						num2 = 0;
					}
				}
				while (num3 > 0);
			}
		}

		private void RaiseResponseCallback<T>(Action<HttpResponseMessage<T>> responseCallback, T data, long contentReadThisRound, long totalContentRead)
		{
			_dispatcher.Enqueue(delegate
			{
				responseCallback(new HttpResponseMessage<T>
				{
					OriginalRequest = _request,
					OriginalResponse = _response,
					Data = data,
					ContentLength = _response.ContentLength,
					ContentReadThisRound = contentReadThisRound,
					TotalContentRead = totalContentRead,
					StatusCode = _response.StatusCode,
					ReasonPhrase = _response.StatusDescription
				});
			});
		}

		protected void RaiseErrorResponse<T>(Action<HttpResponseMessage<T>> action, Exception exception)
		{
			if (action != null)
			{
				_dispatcher.Enqueue(delegate
				{
					action(new HttpResponseMessage<T>
					{
						OriginalRequest = _request,
						OriginalResponse = _response,
						Exception = exception,
						StatusCode = GetStatusCode(exception, _response),
						ReasonPhrase = GetReasonPhrase(exception, _response)
					});
				});
			}
		}

		private HttpStatusCode GetStatusCode(Exception exception, HttpWebResponse response)
		{
			if (response != null)
			{
				return response.StatusCode;
			}
			if (exception.Message.Contains("The remote server returned an error:"))
			{
				int result = 0;
				Match match = Regex.Match(exception.Message, "\\(([0-9]+)\\)");
				if (match.Groups.Count == 2 && int.TryParse(match.Groups[1].Value, out result))
				{
					return (HttpStatusCode)result;
				}
			}
			return HttpStatusCode.InternalServerError;
		}

		private string GetReasonPhrase(Exception exception, HttpWebResponse response)
		{
			if (response != null)
			{
				return response.StatusDescription;
			}
			if (exception.Message.Contains("The remote server returned an error:"))
			{
				Match match = Regex.Match(exception.Message, "\\([0-9]+\\) (.+)");
				if (match.Groups.Count == 2)
				{
					return match.Groups[1].Value;
				}
			}
			return "Unknown";
		}
	}
}
