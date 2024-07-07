using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Cache;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using CI.HttpClient.Core;
using UnityEngine;

namespace CI.HttpClient
{
	public class HttpClient
	{
		private const int DEFAULT_BLOCK_SIZE = 10000;

		private const int DEFAULT_TIMEOUT = 10000;

		private const int DEFAULT_READ_WRITE_TIMEOUT = 300000;

		private const bool DEFAULT_KEEP_ALIVE = true;

		private readonly List<HttpWebRequest> _requests;

		private readonly object _lock;

		private static IDispatcher _dispatcher;

		public int DownloadBlockSize { get; set; }

		public int UploadBlockSize { get; set; }

		public int Timeout { get; set; }

		public int ReadWriteTimeout { get; set; }

		public RequestCachePolicy Cache { get; set; }

		public X509CertificateCollection Certificates { get; set; }

		public CookieContainer Cookies { get; set; }

		public ICredentials Credentials { get; set; }

		public bool KeepAlive { get; set; }

		public IDictionary<HttpRequestHeader, string> Headers { get; set; }

		public IDictionary<string, string> CustomHeaders { get; set; }

		public IWebProxy Proxy { get; set; }

		public HttpClient()
		{
			DownloadBlockSize = 10000;
			UploadBlockSize = 10000;
			Timeout = 10000;
			ReadWriteTimeout = 300000;
			KeepAlive = true;
			Headers = new Dictionary<HttpRequestHeader, string>();
			CustomHeaders = new Dictionary<string, string>();
			_requests = new List<HttpWebRequest>();
			_lock = new object();
		}

		public void Abort()
		{
			lock (_lock)
			{
				foreach (HttpWebRequest request in _requests)
				{
					request.Abort();
				}
			}
		}

		public void Delete(Uri uri, Action<HttpResponseMessage<string>> responseCallback)
		{
			CreateDispatcherGameObject();
			QueueWorkItem(delegate
			{
				try
				{
					HttpWebRequest request = CreateRequest(uri);
					new HttpDelete(request, _dispatcher).Delete(responseCallback);
					RemoveRequest(request);
				}
				catch (Exception exception)
				{
					RaiseErrorResponse(responseCallback, exception);
				}
			});
		}

		public void Delete(Uri uri, HttpCompletionOption completionOption, Action<HttpResponseMessage<byte[]>> responseCallback)
		{
			CreateDispatcherGameObject();
			QueueWorkItem(delegate
			{
				try
				{
					HttpWebRequest request = CreateRequest(uri);
					new HttpDelete(request, _dispatcher).Delete(completionOption, responseCallback, DownloadBlockSize);
					RemoveRequest(request);
				}
				catch (Exception exception)
				{
					RaiseErrorResponse(responseCallback, exception);
				}
			});
		}

		public void GetString(Uri uri, Action<HttpResponseMessage<string>> responseCallback)
		{
			CreateDispatcherGameObject();
			QueueWorkItem(delegate
			{
				try
				{
					HttpWebRequest request = CreateRequest(uri);
					new HttpGet(request, _dispatcher).GetString(responseCallback);
					RemoveRequest(request);
				}
				catch (Exception exception)
				{
					RaiseErrorResponse(responseCallback, exception);
				}
			});
		}

		public void GetByteArray(Uri uri, HttpCompletionOption completionOption, Action<HttpResponseMessage<byte[]>> responseCallback)
		{
			CreateDispatcherGameObject();
			QueueWorkItem(delegate
			{
				try
				{
					HttpWebRequest request = CreateRequest(uri);
					new HttpGet(request, _dispatcher).GetByteArray(completionOption, responseCallback, DownloadBlockSize);
					RemoveRequest(request);
				}
				catch (Exception exception)
				{
					RaiseErrorResponse(responseCallback, exception);
				}
			});
		}

		public void Patch(Uri uri, IHttpContent content, Action<HttpResponseMessage<string>> responseCallback, Action<UploadStatusMessage> uploadStatusCallback = null)
		{
			CreateDispatcherGameObject();
			QueueWorkItem(delegate
			{
				try
				{
					HttpWebRequest request = CreateRequest(uri);
					new HttpPatch(request, _dispatcher).Patch(content, responseCallback, uploadStatusCallback, UploadBlockSize);
					RemoveRequest(request);
				}
				catch (Exception exception)
				{
					RaiseErrorResponse(responseCallback, exception);
				}
			});
		}

		public void Patch(Uri uri, IHttpContent content, HttpCompletionOption completionOption, Action<HttpResponseMessage<byte[]>> responseCallback, Action<UploadStatusMessage> uploadStatusCallback = null)
		{
			CreateDispatcherGameObject();
			QueueWorkItem(delegate
			{
				try
				{
					HttpWebRequest request = CreateRequest(uri);
					new HttpPatch(request, _dispatcher).Patch(content, completionOption, responseCallback, uploadStatusCallback, DownloadBlockSize, UploadBlockSize);
					RemoveRequest(request);
				}
				catch (Exception exception)
				{
					RaiseErrorResponse(responseCallback, exception);
				}
			});
		}

		public void Post(Uri uri, IHttpContent content, Action<HttpResponseMessage<string>> responseCallback, Action<UploadStatusMessage> uploadStatusCallback = null)
		{
			CreateDispatcherGameObject();
			QueueWorkItem(delegate
			{
				try
				{
					HttpWebRequest request = CreateRequest(uri);
					new HttpPost(request, _dispatcher).Post(content, responseCallback, uploadStatusCallback, UploadBlockSize);
					RemoveRequest(request);
				}
				catch (Exception exception)
				{
					RaiseErrorResponse(responseCallback, exception);
				}
			});
		}

		public void Post(Uri uri, IHttpContent content, HttpCompletionOption completionOption, Action<HttpResponseMessage<byte[]>> responseCallback, Action<UploadStatusMessage> uploadStatusCallback = null)
		{
			CreateDispatcherGameObject();
			QueueWorkItem(delegate
			{
				try
				{
					HttpWebRequest request = CreateRequest(uri);
					new HttpPost(request, _dispatcher).Post(content, completionOption, responseCallback, uploadStatusCallback, DownloadBlockSize, UploadBlockSize);
					RemoveRequest(request);
				}
				catch (Exception exception)
				{
					RaiseErrorResponse(responseCallback, exception);
				}
			});
		}

		public void Put(Uri uri, IHttpContent content, Action<HttpResponseMessage<string>> responseCallback, Action<UploadStatusMessage> uploadStatusCallback = null)
		{
			CreateDispatcherGameObject();
			QueueWorkItem(delegate
			{
				try
				{
					HttpWebRequest request = CreateRequest(uri);
					new HttpPut(request, _dispatcher).Put(content, responseCallback, uploadStatusCallback, UploadBlockSize);
					RemoveRequest(request);
				}
				catch (Exception exception)
				{
					RaiseErrorResponse(responseCallback, exception);
				}
			});
		}

		public void Put(Uri uri, IHttpContent content, HttpCompletionOption completionOption, Action<HttpResponseMessage<byte[]>> responseCallback, Action<UploadStatusMessage> uploadStatusCallback = null)
		{
			CreateDispatcherGameObject();
			QueueWorkItem(delegate
			{
				try
				{
					HttpWebRequest request = CreateRequest(uri);
					new HttpPut(request, _dispatcher).Put(content, completionOption, responseCallback, uploadStatusCallback, DownloadBlockSize, UploadBlockSize);
					RemoveRequest(request);
				}
				catch (Exception exception)
				{
					RaiseErrorResponse(responseCallback, exception);
				}
			});
		}

		private void QueueWorkItem(WaitCallback action)
		{
			ThreadPool.QueueUserWorkItem(action);
		}

		private HttpWebRequest CreateRequest(Uri uri)
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
			DisableWriteStreamBuffering(httpWebRequest);
			AddCache(httpWebRequest);
			AddCertificates(httpWebRequest);
			AddCookies(httpWebRequest);
			AddCredentials(httpWebRequest);
			AddKeepAlive(httpWebRequest);
			AddHeaders(httpWebRequest);
			AddProxy(httpWebRequest);
			AddTimeouts(httpWebRequest);
			AddRequest(httpWebRequest);
			return httpWebRequest;
		}

		private void DisableWriteStreamBuffering(HttpWebRequest request)
		{
			request.AllowWriteStreamBuffering = false;
		}

		private void AddCache(HttpWebRequest request)
		{
			if (Cache != null)
			{
				request.CachePolicy = Cache;
			}
		}

		private void AddCertificates(HttpWebRequest request)
		{
			if (Certificates != null)
			{
				request.ClientCertificates = Certificates;
			}
		}

		private void AddCookies(HttpWebRequest request)
		{
			if (Cookies != null)
			{
				request.CookieContainer = Cookies;
			}
		}

		private void AddCredentials(HttpWebRequest request)
		{
			if (Credentials != null)
			{
				request.Credentials = Credentials;
			}
		}

		private void AddKeepAlive(HttpWebRequest request)
		{
			request.KeepAlive = KeepAlive;
		}

		private void AddHeaders(HttpWebRequest request)
		{
			if (Headers != null)
			{
				foreach (KeyValuePair<HttpRequestHeader, string> header in Headers)
				{
					switch (header.Key)
					{
					case HttpRequestHeader.Accept:
						request.Accept = header.Value;
						break;
					case HttpRequestHeader.Connection:
						request.Connection = header.Value;
						break;
					case HttpRequestHeader.ContentLength:
						throw new NotSupportedException("Content Length is set automatically");
					case HttpRequestHeader.ContentType:
						throw new NotSupportedException("Content Type is set automatically");
					case HttpRequestHeader.Expect:
						request.Expect = header.Value;
						break;
					case HttpRequestHeader.Date:
						throw new NotSupportedException("Date is automatically set by the system to the current date");
					case HttpRequestHeader.Host:
						throw new NotSupportedException("Host is automatically set by the system to current host information");
					case HttpRequestHeader.IfModifiedSince:
						request.IfModifiedSince = DateTime.Parse(header.Value);
						break;
					case HttpRequestHeader.Range:
					{
						int range = int.Parse(header.Value);
						request.AddRange(range);
						break;
					}
					case HttpRequestHeader.Referer:
						request.Referer = header.Value;
						break;
					case HttpRequestHeader.TransferEncoding:
						throw new NotSupportedException("Transfer Encoding is not currently supported");
					case HttpRequestHeader.UserAgent:
						request.UserAgent = header.Value;
						break;
					default:
						request.Headers.Add(header.Key, header.Value);
						break;
					}
				}
			}
			if (CustomHeaders == null)
			{
				return;
			}
			foreach (KeyValuePair<string, string> customHeader in CustomHeaders)
			{
				request.Headers.Add(customHeader.Key, customHeader.Value);
			}
		}

		private void AddProxy(HttpWebRequest request)
		{
			if (Proxy != null)
			{
				request.Proxy = Proxy;
			}
		}

		private void AddTimeouts(HttpWebRequest request)
		{
			request.Timeout = Timeout;
			request.ReadWriteTimeout = ReadWriteTimeout;
		}

		private void AddRequest(HttpWebRequest request)
		{
			lock (_lock)
			{
				_requests.Add(request);
			}
		}

		private void RemoveRequest(HttpWebRequest request)
		{
			lock (_lock)
			{
				_requests.Remove(request);
			}
		}

		private void RaiseErrorResponse<T>(Action<HttpResponseMessage<T>> action, Exception exception)
		{
			if (action != null)
			{
				_dispatcher.Enqueue(delegate
				{
					action(new HttpResponseMessage<T>
					{
						Exception = exception
					});
				});
			}
		}

		private void CreateDispatcherGameObject()
		{
			if (_dispatcher == null)
			{
				_dispatcher = new GameObject("HttpClientDispatcher").AddComponent<Dispatcher>();
			}
		}
	}
}
