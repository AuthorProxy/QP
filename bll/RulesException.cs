﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{

    public class RuleViolation
    {
        public RuleViolation()
        {
            Critical = false;
        }

        public LambdaExpression Property { get; set; }
        public string Message { get; set; }
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
        public bool Critical { get; set; }
    }

    [Serializable]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2240")]
    public class RulesException : Exception
    {
        public readonly IList<RuleViolation> Errors = new List<RuleViolation>();

        private readonly static Expression<Func<object, object>> thisObject = x => x;

        public void CriticalErrorForModel(string message)
        {
            Errors.Add(new RuleViolation { Property = thisObject, Message = message, Critical = true });
        }

        public void ErrorForModel(string message)
        {
            Errors.Add(new RuleViolation { Property = thisObject, Message = message });
        }
			

        public void Error(string propertyName, string propertyValue, string message)
        {
            Errors.Add(new RuleViolation { PropertyName = propertyName, PropertyValue = propertyValue, Message = message });
        }

        public static void Wrap(Exception ex)
        {
            RulesException newEx = new RulesException();
            StringBuilder sb = new StringBuilder(ex.Message);
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                sb.Append(" ");
                sb.Append(ex.Message);
            }
            newEx.ErrorForModel(sb.ToString());
            throw newEx;       
        }

        public bool IsEmpty
        {
            get
            {
                return Errors.Count == 0;          
            }
        }	
    }

    [Serializable]
    /// Strongly-typed version permits lambda expression syntax to reference properties
    public class RulesException<T> : RulesException
    {
        public void ErrorFor<P>(Expression<Func<T, P>> property, string message)
        {
            Errors.Add(new RuleViolation { Property = property, Message = message });
        }
    }
}
