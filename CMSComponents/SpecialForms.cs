﻿using Carrotware.CMS.Core;
using Carrotware.Web.UI.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Xml.Serialization;

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

	public class SearchForm : IDisposable {
		protected HtmlHelper _helper;

		public SearchForm(HtmlHelper helper, PagePayload page, object formAttributes = null) {
			_helper = helper;

			var frmID = new TagBuilder("input");
			frmID.MergeAttribute("type", "hidden");
			frmID.MergeAttribute("name", "form_type");
			frmID.MergeAttribute("value", "SearchForm");

			var frmBuilder = new TagBuilder("form");
			frmBuilder.MergeAttribute("action", page.TheSite.SiteSearchPath);
			frmBuilder.MergeAttribute("method", "GET");

			//frmBuilder.MergeAttribute("action", page.ThePage.FileName);
			//frmBuilder.MergeAttribute("method", "POST");

			var frmAttribs = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(formAttributes);
			frmBuilder.MergeAttributes(frmAttribs);

			string frmTag = frmBuilder.ToString(TagRenderMode.StartTag)
				//+ Environment.NewLine
				//+ helper.AntiForgeryToken().ToString()  // only put in if using a post
				//+ Environment.NewLine
				//+ frmID.ToString(TagRenderMode.SelfClosing)
							+ Environment.NewLine;

			_helper.ViewContext.Writer.Write(frmTag);
		}

		public HtmlHelper<SiteSearch> GetModelHelper() {
			SiteSearch model = new SiteSearch();
			if (_helper.ViewData["CMS_searchform"] != null) {
				model = _helper.ViewData["CMS_searchform"] as SiteSearch;
			} else {
				model = new SiteSearch();
			}

			return new HtmlHelper<SiteSearch>(_helper.ViewContext, new WrapperForHtmlHelper<SiteSearch>(model));
		}

		public void Dispose() {
			_helper.ViewContext.Writer.Write("</form>");
		}
	}

	//=======
	public class SiteSearch {

		[StringLength(128)]
		public string query { get; set; }
	}

	//======================

	//public class ContactForm : IDisposable {
	//	protected HtmlHelper _helper;

	//	public ContactForm(HtmlHelper helper, PagePayload page, object formAttributes = null) {
	//		_helper = helper;

	//		var frmID = new TagBuilder("input");
	//		frmID.MergeAttribute("type", "hidden");
	//		frmID.MergeAttribute("name", "form_type");
	//		frmID.MergeAttribute("value", "ContactForm");

	//		var frmBuilder = new TagBuilder("form");
	//		frmBuilder.MergeAttribute("action", page.ThePage.FileName);
	//		frmBuilder.MergeAttribute("method", "POST");

	//		var frmAttribs = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(formAttributes);
	//		frmBuilder.MergeAttributes(frmAttribs);

	//		string frmTag = frmBuilder.ToString(TagRenderMode.StartTag)
	//						+ Environment.NewLine
	//						+ _helper.AntiForgeryToken().ToString()
	//						+ Environment.NewLine
	//						+ frmID.ToString(TagRenderMode.SelfClosing);

	//		_helper.ViewContext.Writer.Write(frmTag);
	//	}

	//	public HtmlHelper<ContactInfo> GetModelHelper() {
	//		ContactInfo model = new ContactInfo();

	//		//var lst = ModelBinders.Binders.Values.Where(x => x is GenericModelBinder).Cast<GenericModelBinder>().ToList();
	//		////.Where(x => x.BinderType is ContactInfo).ToList();

	//		//if (!lst.Any()) {
	//		//	var bind = new GenericModelBinder();
	//		//	bind.BinderType = typeof(ContactInfo);
	//		//	ModelBinders.Binders.Add(bind.BinderType, bind);
	//		//}

	//		if (_helper.ViewData["CMS_contactform"] != null) {
	//			model = _helper.ViewData["CMS_contactform"] as ContactInfo;

	//		} else {
	//			model = new ContactInfo();
	//		}

	//		return new HtmlHelper<ContactInfo>(_helper.ViewContext, new WrapperForHtmlHelper<ContactInfo>(model, _helper.ViewData));
	//	}

	//	public void Dispose() {
	//		_helper.ViewContext.Writer.Write("</form>");
	//	}
	//}

	//======================

	internal class FormRouteValue {

		internal FormRouteValue() {
			this.controller = String.Empty;
			this.action = String.Empty;
			this.carrotedit = null;
		}

		internal FormRouteValue(string c, string a)
			: this() {
			this.controller = c;
			this.action = a;
		}

		internal FormRouteValue(string c, string a, bool ce)
			: this() {
			this.controller = c;
			this.action = a;
			this.carrotedit = ce;
		}

		internal string controller { get; set; }
		internal string action { get; set; }
		internal bool? carrotedit { get; set; }
	}

	//======================

	public class AjaxContactForm : IDisposable {
		protected AjaxHelper _helper;
		protected MvcForm frm = null;
		protected ContactInfo _model = null;
		protected ContactInfoSettings _settings = null;

		public AjaxContactForm(AjaxHelper ajaxHelper, PagePayload page, AjaxOptions ajaxOptions, object formAttributes = null) {
			_helper = ajaxHelper;

			if (page == null) {
				page = PagePayload.GetCurrentContent();
			}

			if (ajaxOptions == null) {
				ajaxOptions = new AjaxOptions();
				ajaxOptions.InsertionMode = InsertionMode.Replace;
			}
			if (String.IsNullOrEmpty(ajaxOptions.HttpMethod)) {
				ajaxOptions.HttpMethod = "POST";
			}
			if (String.IsNullOrEmpty(ajaxOptions.UpdateTargetId)) {
				ajaxOptions.UpdateTargetId = "frmContact";
			}
			if (String.IsNullOrEmpty(ajaxOptions.OnFailure)) {
				ajaxOptions.OnFailure = "__OnAjaxRequestFailure";
			}

			string formAction = "Contact.ashx";

			//try #3
			//RouteValueDictionary dic = new RouteValueDictionary();
			//dic.Add("controller", "CmsAjaxForms");
			//dic.Add("action", formAction);
			//if (SecurityData.AdvancedEditMode) {
			//	dic.Add(SiteData.AdvancedEditParameter, true);
			//}
			//frm = ajaxHelper.BeginRouteForm("Default", dic, ajaxOptions, formAttributes);

			//try #2
			if (SecurityData.AdvancedEditMode) {
				frm = ajaxHelper.BeginRouteForm("Default", new { controller = "CmsAjaxForms", action = formAction, carrotedit = true }, ajaxOptions, formAttributes);
			} else {
				frm = ajaxHelper.BeginRouteForm("Default", new { controller = "CmsAjaxForms", action = formAction }, ajaxOptions, formAttributes);
			}

			/*
			//try #1
			string formAction = "Contact.ashx";
			FormRouteValue frv = new FormRouteValue("CmsAjaxForms", formAction);

			if (SecurityData.AdvancedEditMode) {
				frv = new FormRouteValue("CmsAjaxForms", formAction, true);
			}

			frm = ajaxHelper.BeginRouteForm("Default", frv, ajaxOptions, formAttributes);
			 */
		}

		public HtmlHelper<ContactInfo> GetModelHelper(string partialName, IValidateHuman validateHuman) {
			_model = InitContactInfo(partialName);

			_settings.UseValidateHuman = true;
			_settings.ValidateHumanClass = validateHuman.GetType().AssemblyQualifiedName;
			if (!String.IsNullOrEmpty(validateHuman.AltValidationFailText)) {
				_settings.ValidationFailText = validateHuman.AltValidationFailText;
			}

			return InitHelp();
		}

		public HtmlHelper<ContactInfo> GetModelHelper(ContactInfoConfig config) {
			_model = InitContactInfo(config.PostPartialName);

			if (config.ValidateHuman != null) {
				_settings.UseValidateHuman = true;
				_settings.ValidateHumanClass = config.ValidateHuman.GetType().AssemblyQualifiedName;
				if (!String.IsNullOrEmpty(config.ValidateHuman.AltValidationFailText)) {
					_settings.ValidationFailText = config.ValidateHuman.AltValidationFailText;
				}
			}

			_settings.DirectEmailKeyName = config.DirectEmailKeyName;
			_settings.NotifyEditors = config.NotifyEditors;

			return InitHelp();
		}

		public HtmlHelper<ContactInfo> GetModelHelper(string partialName) {
			_model = InitContactInfo(partialName);

			return InitHelp();
		}

		protected ContactInfo InitContactInfo(string partialName) {
			ContactInfo model = new ContactInfo();
			_settings = new ContactInfoSettings();

			if (_helper.ViewData["CMS_contactform"] != null) {
				model = _helper.ViewData["CMS_contactform"] as ContactInfo;
			} else {
				model = new ContactInfo();
			}

			_settings.Uri = CarrotCakeHtml.CmsPage.ThePage.FileName;
			_settings.PostPartialName = partialName;
			model.Settings = _settings;

			return model;
		}

		protected HtmlHelper<ContactInfo> InitHelp() {
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ContactInfoSettings));
			string sXML = String.Empty;
			using (StringWriter stringWriter = new StringWriter()) {
				xmlSerializer.Serialize(stringWriter, _settings);
				sXML = stringWriter.ToString();
				sXML = CMSConfigHelper.EncodeBase64(sXML);
			}

			_model.Settings = _settings;
			_model.EncodedSettings = sXML;

			var hlp = new HtmlHelper<ContactInfo>(_helper.ViewContext, new WrapperForHtmlHelper<ContactInfo>(_model, _helper.ViewData));

			string frmTag = Environment.NewLine
						+ hlp.AntiForgeryToken().ToString()
						+ Environment.NewLine
						+ hlp.HiddenFor(x => x.EncodedSettings).ToString()
						+ Environment.NewLine;

			_helper.ViewContext.Writer.Write(frmTag);

			return hlp;
		}

		public void Dispose() {
			//_helper.ViewContext.Writer.Write("</form>");
			if (frm != null) {
				frm.Dispose();
			}
		}
	}

	//=======

	public class ContactInfo {

		public ContactInfo() {
			ReconstructSettings();
		}

		public void ReconstructSettings() {
			this.Settings = null;

			if (!String.IsNullOrEmpty(this.EncodedSettings)) {
				string sXML = CMSConfigHelper.DecodeBase64(this.EncodedSettings);
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(ContactInfoSettings));
				using (StringReader stringReader = new StringReader(sXML)) {
					this.Settings = (ContactInfoSettings)xmlSerializer.Deserialize(stringReader);
				}

				if (this.Settings != null && !String.IsNullOrEmpty(this.Settings.ValidateHumanClass)) {
					Type objType = Type.GetType(this.Settings.ValidateHumanClass);
					Object obj = Activator.CreateInstance(objType);
					this.ValidateHuman = (IValidateHuman)obj;
					this.ValidateHuman.AltValidationFailText = this.Settings.ValidationFailText;
				}
			}
		}

		public Guid Root_ContentID { get; set; }
		public DateTime CreateDate { get; set; }

		[StringLength(32)]
		[Display(Name = "IP")]
		public string CommenterIP { get; set; }

		[StringLength(256)]
		[Required]
		[Display(Name = "Name")]
		public string CommenterName { get; set; }

		[StringLength(256)]
		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string CommenterEmail { get; set; }

		[StringLength(256)]
		//[Required]
		[Display(Name = "URL")]
		public string CommenterURL { get; set; }

		[StringLength(4096)]
		[Required]
		[Display(Name = "Comment")]
		public string PostCommentText { get; set; }

		public string EncodedSettings { get; set; }
		public ContactInfoSettings Settings { get; set; }
		public IValidateHuman ValidateHuman { get; set; }
		public string ValidationValue { get; set; }
		public bool IsSaved { get; set; }

		public void SendMail(PostComment pc, ContentPage page) {
			HttpRequest request = HttpContext.Current.Request;

			//TODO: NotifyEditors wire up
			if (this.Settings.NotifyEditors || !String.IsNullOrEmpty(this.Settings.DirectEmailKeyName)) {
				List<string> emails = new List<string>();

				if (this.Settings.NotifyEditors && page != null) {
					emails.Add(page.CreateUser.Email);

					if (page.EditUser.UserId != page.CreateUser.UserId) {
						emails.Add(page.EditUser.Email);
					}

					if (page.CreditUserId.HasValue) {
						emails.Add(page.CreditUser.Email);
					}
				}

				if (!String.IsNullOrEmpty(this.Settings.DirectEmailKeyName)) {
					emails.Add(ConfigurationManager.AppSettings[this.Settings.DirectEmailKeyName].ToString());
				}

				string mailSubject = "Comment Form " + request.ServerVariables["HTTP_HOST"];

				string strHTTPHost = String.Empty;
				try { strHTTPHost = request.ServerVariables["HTTP_HOST"] + String.Empty; } catch { strHTTPHost = String.Empty; }

				string strHTTPPrefix = "http://";
				try {
					strHTTPPrefix = request.ServerVariables["SERVER_PORT_SECURE"] == "1" ? "https://" : "http://";
				} catch { strHTTPPrefix = "http://"; }

				strHTTPHost = String.Format("{0}{1}", strHTTPPrefix, strHTTPHost).ToLower();

				string sBody = "Name:   " + pc.CommenterName
					+ "\r\nEmail:   " + pc.CommenterEmail
					+ "\r\nURL:   " + pc.CommenterURL
					+ "\r\n-----------------\r\nComment:\r\n" + HttpUtility.HtmlEncode(pc.PostCommentText)
					+ "\r\n=================\r\n\r\nIP:   " + pc.CommenterIP
					+ "\r\nSite URL:   " + String.Format("{0}{1}", strHTTPHost, request.ServerVariables["script_name"])
					+ "\r\nSite Time:   " + SiteData.CurrentSite.Now.ToString()
					+ "\r\nUTC Time:   " + DateTime.UtcNow.ToString();

				string sEmail = String.Join(";", emails);

				EmailHelper.SendMail(null, sEmail, mailSubject, sBody, false);
			}
		}
	}

	//========

	public class ContactInfoSettings {

		public ContactInfoSettings() {
			this.ValidationFailText = "Failed to validate as a human.";
		}

		public string PostPartialName { get; set; }
		public string Uri { get; set; }
		public bool UseValidateHuman { get; set; }
		public string ValidateHumanClass { get; set; }
		public string ValidationFailText { get; set; }
		public string DirectEmailKeyName { get; set; }
		public bool NotifyEditors { get; set; }
	}

	//========

	public class ContactInfoConfig {

		public ContactInfoConfig() {
			this.ValidateHuman = null;
			this.NotifyEditors = false;
			this.PostPartialName = String.Empty;
			this.DirectEmailKeyName = String.Empty;
		}

		public ContactInfoConfig(string partialName)
			: this() {
			this.PostPartialName = partialName;
		}

		public ContactInfoConfig(string partialName, IValidateHuman validateHuman)
			: this(partialName) {
			this.ValidateHuman = validateHuman;
		}

		public IValidateHuman ValidateHuman { get; set; }
		public string PostPartialName { get; set; }
		public string DirectEmailKeyName { get; set; }
		public bool NotifyEditors { get; set; }
	}

	//======================

	public class FormHelper {

		public static Object ParseRequest(Object obj, HttpRequestBase request) {
			Type type = obj.GetType();

			PropertyInfo[] props = type.GetProperties();

			foreach (var p in props) {
				if (request.Form[p.Name] != null) {
					string val = request.Form[p.Name];
					object o = null;

					if (val != null) {
						Type tp = p.PropertyType;
						tp = Nullable.GetUnderlyingType(tp) ?? tp;

						if (tp == typeof(Guid)) {
							o = new Guid(val.ToString());
						} else {
							o = Convert.ChangeType(val, tp);
						}

						p.SetValue(obj, o);
					}
				}
			}

			return obj;
		}

		//========================

		public static T ParseRequest<T>(T obj, HttpRequestBase request) {
			Type type = typeof(T);

			PropertyInfo[] props = type.GetProperties();

			foreach (var p in props) {
				if (request.Form[p.Name] != null) {
					string val = request.Form[p.Name];
					object o = null;

					if (val != null) {
						Type tp = p.PropertyType;
						tp = Nullable.GetUnderlyingType(tp) ?? tp;

						if (tp == typeof(Guid)) {
							o = new Guid(val.ToString());
						} else {
							o = Convert.ChangeType(val, tp);
						}

						p.SetValue(obj, o);
					}
				}
			}

			return obj;
		}
	}

	//==================================================

	public class GenericModelBinder : IModelBinder {
		//public T BindModel<T>(ControllerContext controllerContext, ModelBindingContext bindingContext) {
		//	var request = controllerContext.HttpContext.Request;

		//	T obj = Activator.CreateInstance<T>();

		//	obj = FormHelper.ParseRequest<T>(obj, request);

		//	return obj;
		//}

		public Type BinderType { get; set; }

		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
			this.BinderType = bindingContext.ModelType;
			object model = Activator.CreateInstance(this.BinderType);
			var request = controllerContext.HttpContext.Request;

			model = FormHelper.ParseRequest(model, request);

			return model;
		}
	}
}