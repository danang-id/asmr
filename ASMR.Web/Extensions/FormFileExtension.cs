﻿using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ASMR.Core.Entities;
using ASMR.Web.Constants;

namespace ASMR.Web.Extensions
{
    public static class FormFileExtension
    {
        private const int ImageMinimumBytes = 512;

        public static bool IsImage(this IFormFile formFile)
        {
            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            if (!string.Equals(formFile.ContentType, "image/jpg", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(formFile.ContentType, "image/jpeg", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(formFile.ContentType, "image/pjpeg", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(formFile.ContentType, "image/gif", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(formFile.ContentType, "image/x-png", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(formFile.ContentType, "image/png", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            //-------------------------------------------
            //  Check the image extension
            //-------------------------------------------
            var postedFileExtension = Path.GetExtension(formFile.FileName);
            if (!string.Equals(postedFileExtension, ".jpg", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(postedFileExtension, ".png", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(postedFileExtension, ".gif", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(postedFileExtension, ".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            //-------------------------------------------
            //  Attempt to read the file and check the first bytes
            //-------------------------------------------
            try
            {
                if (!formFile.OpenReadStream().CanRead)
                {
                    return false;
                }
                //------------------------------------------
                //   Check whether the image size exceeding the limit or not
                //------------------------------------------ 
                if (formFile.Length < ImageMinimumBytes)
                {
                    return false;
                }

                var buffer = new byte[ImageMinimumBytes];
                formFile.OpenReadStream()
                    .Read(buffer, 0, ImageMinimumBytes);
                var content = System.Text.Encoding.UTF8.GetString(buffer);
                if (Regex.IsMatch(content,
                    @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy", 
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                formFile.OpenReadStream().Position = 0;
            }

            //-------------------------------------------
            //  Try to instantiate new Bitmap, if .NET will throw exception
            //  we can assume that it's not a valid image
            //-------------------------------------------
            try
            {
                using var bitmap = new System.Drawing.Bitmap(formFile.OpenReadStream());
            }
            catch (Exception)
            {
                // TODO: Return true for now. There is a problem with Blob Form uploading.

                //return false;
                return true;
            }
            finally
            {
                formFile.OpenReadStream().Position = 0;
            }

            return true;
        }

        public static async Task<MediaFile> SaveResourceWithRandomNameAsync(this IFormFile formFile)
        {
            try
            {
                if (!Directory.Exists(ResourceContants.DirectoryPath))
                {
                    Directory.CreateDirectory(ResourceContants.DirectoryPath);
                }
            
                var timeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                var fileName = $"{timeStamp}_{Path.GetRandomFileName()}.resource";
                var filePath = Path.Combine(ResourceContants.DirectoryPath, fileName);
                var absoluteFilePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
                using (var fileStream = File.Create(absoluteFilePath))
                {
                    await formFile.CopyToAsync(fileStream);
                }
                
                return new MediaFile {
                    Name = formFile.FileName,
                    MimeType = formFile.ContentType,
                    Location = filePath
                };
                    
            }
            catch (Exception)
            {
                return null;
            } 
        }
    }
}
