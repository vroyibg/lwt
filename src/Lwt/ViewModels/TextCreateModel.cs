﻿using System;

namespace Lwt.ViewModels
{
    public class TextCreateModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public Guid LanguageId { get; set; }
    }
}