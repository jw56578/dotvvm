﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Configuration
{
	public static class ConfigurationHelper
	{
		/// <summary>
		/// Registers all controls discovered by specified <see cref="IControlRegistrationStrategy"/> in the <see cref="DotvvmMarkupConfiguration"/>.
		/// </summary>
		/// <param name="strategy">A strategy that provides list of control configurations.</param>
		public static void AutoDiscoverControls(this DotvvmMarkupConfiguration configuration, IControlRegistrationStrategy strategy)
		{
			configuration.Controls.AddRange(strategy.GetControls());
		}
	}
}
