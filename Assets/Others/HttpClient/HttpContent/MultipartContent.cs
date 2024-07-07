using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CI.HttpClient
{
	public class MultipartContent : IHttpContent, IEnumerable, IEnumerable<IHttpContent>
	{
		private const string DEFAULT_SUBTYPE = "form-data";

		private readonly List<IHttpContent> _content;

		private readonly string _boundary;

		private string _contentType;

		private long _contentLength;

		public byte[] BoundaryStartBytes { get; private set; }

		public byte[] BoundaryEndBytes { get; private set; }

		public byte[] CRLFBytes { get; private set; }

		public ContentReadAction ContentReadAction
		{
			get
			{
				return ContentReadAction.Multi;
			}
		}

		public MultipartContent()
		{
			_content = new List<IHttpContent>();
			_boundary = Guid.NewGuid().ToString();
			CreateConentType("form-data");
			CreateDelimiters();
		}

		public MultipartContent(string boundary)
		{
			_content = new List<IHttpContent>();
			_boundary = boundary;
			CreateConentType("form-data");
			CreateDelimiters();
		}

		public MultipartContent(string boundary, string subtype)
		{
			_content = new List<IHttpContent>();
			_boundary = boundary;
			CreateConentType(subtype);
			CreateDelimiters();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private void CreateConentType(string subtype)
		{
			_contentType = "multipart/" + subtype + "; boundary=" + _boundary;
		}

		private void CreateDelimiters()
		{
			CRLFBytes = Encoding.UTF8.GetBytes("\r\n");
			BoundaryStartBytes = Encoding.UTF8.GetBytes("--" + _boundary + "\r\n");
			BoundaryEndBytes = Encoding.UTF8.GetBytes("--" + _boundary + "--\r\n");
		}

		public void Add(IHttpContent content)
		{
			_content.Add(content);
		}

		public long GetContentLength()
		{
			if (_contentLength == 0L)
			{
				long num = 0L;
				if (_content.Count == 0)
				{
					num += BoundaryStartBytes.Length;
				}
				foreach (IHttpContent item in _content)
				{
					num += BoundaryStartBytes.Length;
					num += Encoding.UTF8.GetBytes("Content-Type: " + item.GetContentType()).Length;
					num += CRLFBytes.Length;
					num += CRLFBytes.Length;
					num = ((item.ContentReadAction != ContentReadAction.ByteArray) ? (num + item.ReadAsStream().Length) : (num + item.GetContentLength()));
					num += CRLFBytes.Length;
				}
				num += BoundaryEndBytes.Length;
				_contentLength = num;
			}
			return _contentLength;
		}

		public string GetContentType()
		{
			return _contentType;
		}

		public byte[] ReadAsByteArray()
		{
			throw new NotImplementedException();
		}

		public Stream ReadAsStream()
		{
			throw new NotImplementedException();
		}

		public IEnumerator<IHttpContent> GetEnumerator()
		{
			return _content.GetEnumerator();
		}
	}
}
