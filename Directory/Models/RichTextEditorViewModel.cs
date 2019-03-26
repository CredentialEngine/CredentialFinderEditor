using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Web.Mvc;

namespace CTI.Directory.Models
{
    public class RichTextEditorViewModel
    {
        [AllowHtml]
        [Display( Name = "Message" )]
        public string Message
        {
            get;
            set;
        }
    }
}