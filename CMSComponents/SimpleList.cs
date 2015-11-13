﻿using Carrotware.CMS.Core;
using Carrotware.CMS.Interface;
using System;
using System.Collections.Generic;
using System.Text;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.UI.Components {

	public abstract class SimpleList : BaseToolboxComponent {

		public SimpleList() {
			this.CssClass = String.Empty;
			this.CssSelected = "selected";
			this.ElementId = "list";

			this.CmsPage = PagePayload.GetContentFromViewData();
		}

		public override bool EnableEdit {
			get {
				return true;
			}
		}

		public PagePayload CmsPage { get; set; }

		[Widget(WidgetAttribute.FieldMode.TextBox)]
		public string ElementId { get; set; }

		[Widget(WidgetAttribute.FieldMode.TextBox)]
		public string CssClass { get; set; }

		[Widget(WidgetAttribute.FieldMode.TextBox)]
		public string CssItem { get; set; }

		[Widget(WidgetAttribute.FieldMode.TextBox)]
		public string CssSelected { get; set; }

		public List<SiteNav> NavigationData { get; set; }

		public override void LoadData() {
			base.LoadData();

			this.NavigationData = new List<SiteNav>();

			try {
				string sFoundVal = this.GetParmValue("ElementId", "list");

				if (!String.IsNullOrEmpty(sFoundVal)) {
					this.ElementId = sFoundVal;
				}
			} catch (Exception ex) { }

			try {
				string sFoundVal = this.GetParmValue("CssClass", String.Empty);

				if (!String.IsNullOrEmpty(sFoundVal)) {
					this.CssClass = sFoundVal;
				}
			} catch (Exception ex) { }

			try {
				string sFoundVal = this.GetParmValue("CssItem", String.Empty);

				if (!String.IsNullOrEmpty(sFoundVal)) {
					this.CssItem = sFoundVal;
				}
			} catch (Exception ex) { }

			try {
				string sFoundVal = this.GetParmValue("CssSelected", "selected");

				if (!String.IsNullOrEmpty(sFoundVal)) {
					this.CssSelected = sFoundVal;
				}
			} catch (Exception ex) { }
		}

		protected virtual void TweakData() { }

		public override string ToHtmlString() {
			LoadData();
			TweakData();

			string sItemCSS = String.Empty;
			if (!String.IsNullOrEmpty(this.CssItem)) {
				sItemCSS = String.Format(" {0} ", this.CssItem).Trim();
			}
			string sThis1CSS = sItemCSS;

			StringBuilder output = new StringBuilder();

			if (!String.IsNullOrEmpty(this.CssClass)) {
				output.AppendLine("<ul id=\"" + this.ElementId + "\" class=\"" + this.CssClass + "\">");
			} else {
				output.AppendLine("<ul id=\"" + this.ElementId + "\" >");
			}

			if (this.NavigationData != null) {
				foreach (var n in this.NavigationData) {
					if (SiteData.IsFilenameCurrentPage(n.FileName)
								|| (n.NavOrder == 0 && SiteData.IsCurrentLikelyHomePage)
								|| ControlUtilities.AreFilenamesSame(n.FileName, this.CmsPage.ThePage.FileName)) {
						sThis1CSS = String.Format(" {0} {1} ", this.CssItem, this.CssSelected).Trim();
					} else {
						sThis1CSS = String.Format(" {0} ", this.CssItem).Trim();
					}

					if (!String.IsNullOrEmpty(sThis1CSS)) {
						output.Append("<li class=\"" + sThis1CSS + "\">");
					} else {
						output.Append("<li>");
					}

					output.Append(" <a href=\"" + n.FileName + "\">" + n.NavMenuText + "</a> </li>" + Environment.NewLine);
				}
			}

			output.AppendLine("</ul>");

			return output.ToString();
		}
	}
}