using System;

namespace Synapse.Core
{
	public class DynamicValue
	{
		public string Name { get; set; }
		public string Path { get; set; }

		public override string ToString()
		{
			return string.Format( "[{0}]::[{1}]", Name, Path );
		}
	}
}