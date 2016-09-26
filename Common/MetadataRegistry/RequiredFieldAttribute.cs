using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetadataRegistry
{
	public class RequiredField : Attribute
	{
		private bool _immutable;
		public bool Immutable { get { return _immutable; } }


		public RequiredField()
		{
			_immutable = false;
		}

		public RequiredField( bool immutable )
		{
			_immutable = immutable;
		}
	}
}
