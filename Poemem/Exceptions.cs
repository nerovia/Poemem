using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poemem
{
	class HandleException : Exception
	{
		public HandleException(string message, Exception? innerException = null) : base(message, innerException) { }

		public override string ToString()
		{
			return Message;
		}
	}
}
