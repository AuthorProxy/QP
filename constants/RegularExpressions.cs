﻿using System;

namespace Quantumart.QP8.Constants
{
	/// <summary>
	/// Шаблоны регулярных выражений
	/// </summary>
	public static class RegularExpressions
	{
        public const string Email = @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
        public const string Ip = @"^(((\d{1,2})|(1\d{2})|(2[0-4]\d)|(25[0-5]))\.){3}((\d{1,2})|(1\d{2})|(2[0-4]\d)|(25[0-5]))$";
        public const string Url = @"^http(s)?://(?:[a-zA-Zа-яА-Я0-9_\-]{1,63}|(?:(?!\d+\.|-)[a-zA-Zа-яА-Я0-9_\-]{1,63}(?<!-)\.)+(?:[a-zA-Zа-яА-Я]{2,}))(:[0-9]{1,5})?(/[a-zA-Z0-9а-яА-Я-_ ./?%&=]*)?$";
		public const string InvalidEntityName = @"[!-,\.\/:-@{-~\[\]]";
		public const string InvalidFieldName = @"[!-,\/:-@{-~\[\]]";
		public const string InvalidUserName = @"[""#%-\)\+,\/:-\?{-~\[\]]";
		public const string InvalidFolderName = @"[<>:""\/\\|\?\*]";
		public const string NetName = @"^[_a-zA-Z][_a-zA-Z0-9]*$";
		public const string FullQualifiedNetName = @"^[_a-zA-Z][_a-zA-Z0-9]*(\.[_a-zA-Z][_a-zA-Z0-9]*)*$";
        public const string AbsoluteWebFolderUrl = @"^http(s)?://(?:[a-zA-Zа-яА-Я0-9_\-]{1,63}|(?:(?!\d+\.|-)[a-zA-Zа-яА-Я0-9_\-]{1,63}(?<!-)\.)+(?:[a-zA-Zа-яА-Я]{2,}))(:[0-9]{1,5})?(/[a-zA-Z0-9а-яА-Я-_/]*)?$";
		public const string ExternalCssUrl = @"^http(s)?://(?:[a-zA-Zа-яА-Я0-9_\-]{1,63}|(?:(?!\d+\.|-)[a-zA-Zа-яА-Я0-9_\-]{1,63}(?<!-)\.)+(?:[a-zA-Zа-яА-Я]{2,}))(:[0-9]{1,5})?(/[a-zA-Z0-9а-яА-Я-_/]*)?\.(css|CSS)$";
        public const string RelativeWebFolderUrl = @"^/([a-zA-Z0-9а-яА-Я-_/]*$)";
		public const string AbsoluteWindowsFolderPath = @"^[a-zA-Z]:\\([a-zA-Z0-9а-яА-Я-_\\\.]*)$";
		public const string RelativeWindowsFolderPath = @"^([a-zA-Z0-9а-яА-Я-_\\\.]*)$";
        public const string DomainName = @"^(?:[a-zA-Zа-яА-Я0-9_\-]{1,63}|(?:(?!\d+\.|-)[a-zA-Zа-яА-Я0-9_\-]{1,63}(?<!-)\.)+(?:[a-zA-Zа-яА-Я]{2,}))(:[0-9]{1,5})?$";
		public const string FileName = @"^[\w\- ]+[\w\-. ]*$";
		public const string RgbColor = @"^[a-fA-F0-9]{6}$";
	}
}
