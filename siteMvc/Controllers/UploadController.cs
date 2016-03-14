﻿using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class UploadController : QPController
    {
        private readonly IBackendActionLogRepository _logger;

        public UploadController(IBackendActionLogRepository logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public ActionResult UploadChunk(int? chunk, int? chunks, string name, string destinationUrl)
        {
            destinationUrl = HttpUtility.UrlDecode(destinationUrl);
            var fileUpload = Request.Files[0];
            chunk = chunk ?? 0;
            chunks = chunks ?? 1;
            var tempPath = Path.Combine(QPConfiguration.TempDirectory, name);
            var destPath = Path.Combine(destinationUrl, name);
            PathSecurityResult securityResult;

            if (string.IsNullOrEmpty(destinationUrl))
            {
                throw new ArgumentException("Folder Path is empty");
            }

            if (!Directory.Exists(destinationUrl))
            {
                Directory.CreateDirectory(destinationUrl);
            }

            if (chunk == 0 && chunks == 1)
            {
                securityResult = PathInfo.CheckSecurity(destinationUrl);
                if (!securityResult.Result)
                {
                    return Json(new
                    {
                        message = string.Format(PlUploadStrings.ServerError, name, destinationUrl, $"Access to the folder (ID = {securityResult.FolderId}) denied"),
                        isError = true
                    });
                }

                try
                {
                    using (var fs = new FileStream(destPath, FileMode.Create))
                    {
                        var buffer = new byte[fileUpload.InputStream.Length];
                        fileUpload.InputStream.Read(buffer, 0, buffer.Length);
                        fs.Write(buffer, 0, buffer.Length);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { message = string.Format(PlUploadStrings.ServerError, name, destinationUrl, ex.Message), isError = true });
                }
            }
            else
            {
                try
                {
                    using (var fs = new FileStream(tempPath, chunk == 0 ? FileMode.Create : FileMode.Append))
                    {
                        var buffer = new byte[fileUpload.InputStream.Length];
                        fileUpload.InputStream.Read(buffer, 0, buffer.Length);
                        fs.Write(buffer, 0, buffer.Length);
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { message = string.Format(PlUploadStrings.ServerError, name, tempPath, ex.Message), isError = true });
                }

                try
                {
                    var isTheLastChunk = chunk.Value == chunks.Value - 1;
                    if (isTheLastChunk)
                    {
                        securityResult = PathInfo.CheckSecurity(destinationUrl);
                        var actionCode = securityResult.IsSite ? ActionCode.UploadSiteFile : ActionCode.UploadContentFile;

                        if (!securityResult.Result)
                        {
                            return Json(new
                            {
                                message = string.Format(PlUploadStrings.ServerError, name, destinationUrl, $"Access to the folder (ID = {securityResult.FolderId}) denied"),
                                isError = true
                            });
                        }

                        if (System.IO.File.Exists(destPath))
                        {
                            System.IO.File.SetAttributes(destPath, FileAttributes.Normal);
                            System.IO.File.Delete(destPath);
                        }

                        System.IO.File.Move(tempPath, destPath);
                        BackendActionContext.SetCurrent(actionCode, new[] { name }, securityResult.FolderId);

                        var logs = BackendActionLog.CreateLogs(BackendActionContext.Current, _logger);
                        _logger.Save(logs);

                        BackendActionContext.ResetCurrent();
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { message = string.Format(PlUploadStrings.ServerError, name, destinationUrl, ex.Message), isError = true });
                }

                return Json(new { message = $"chunk#{chunk.Value}, of file{name} uploaded", isError = false });
            }

            return Json(new { message = $"file{name} uploaded", isError = false });
        }
    }
}
