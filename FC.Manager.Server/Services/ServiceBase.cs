﻿// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Manager.Server.Services
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	public abstract class ServiceBase
	{
		public ServiceBase()
		{
			RPCService.BindMethods(this);
		}

		public bool Alive
		{
			get;
			private set;
		}

		public virtual Task Initialize()
		{
			this.Alive = true;
			return Task.CompletedTask;
		}

		public virtual Task Shutdown()
		{
			this.Alive = false;
			return Task.CompletedTask;
		}
	}
}
