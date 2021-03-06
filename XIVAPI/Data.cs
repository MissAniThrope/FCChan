﻿// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace XIVAPI
{
	using System;
	using System.Collections.Generic;
	using System.Text;

	[Serializable]
	public class Data
	{
		public int ID { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Icon { get; set; } = string.Empty;
		public string Url { get; set; } = string.Empty;
	}
}
