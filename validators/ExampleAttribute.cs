﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.Validators
{
    public class ExampleAttribute : Attribute
    {
        public string Text
        {
            get;
            set;
        }

        public ExampleAttribute(string text)
        {
            Text = text;
        }
    }
}
