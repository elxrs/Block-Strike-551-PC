using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace U3D
{
	public class AggregateException : Exception
	{
		private List<Exception> m_innerExceptions;

		public ReadOnlyCollection<Exception> InnerExceptions
		{
			get
			{
				return new ReadOnlyCollection<Exception>(m_innerExceptions);
			}
		}

		public AggregateException(params Exception[] es)
			: base("Target invocation caused an exception, check InnerExceptions")
		{
			m_innerExceptions = new List<Exception>(es);
		}

		public void AddException(Exception e)
		{
			m_innerExceptions.Add(e);
		}

		public override string ToString()
		{
			if (InnerExceptions.Count == 1)
			{
				return InnerExceptions[0].ToString();
			}
			return string.Format("[AggregateException: InnerExceptions={0}]", InnerExceptions);
		}
	}
}
