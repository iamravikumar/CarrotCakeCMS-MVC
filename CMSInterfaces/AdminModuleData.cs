﻿using System;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 2 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.Interface {

	public class AdminModuleData : IAdminModule {
		public Guid SiteID { get; set; }

		public Guid ModuleID { get; set; }

		public string ModuleName { get; set; }

		public virtual string QueryStringFragment { get; set; }

		public virtual string QueryStringPattern { get; set; }

		public virtual void LoadData(IAdminModule data) { }
	}
}