using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Validation;

namespace FRTTMO.PaymentCore.Exceptions
{
    public class CustomBusinessException : BusinessException, IBusinessException, IHasValidationErrors
    {
        public List<ValidationResult> _validationErrors { get; }

        public IList<ValidationResult> ValidationErrors => _validationErrors;

        public CustomBusinessException(string code = null, string message = null, string details = null, Exception innerException = null, LogLevel logLevel = LogLevel.Warning) : base(code, message, details, innerException, logLevel)
        {
            _validationErrors = new List<ValidationResult>();
        }

        //public new CustomBusinessException WithData(string name, object value)
        //{
        //    Data[name] = value;
        //    //
        //    var indexExists = -1;
        //    for (int i = 0; i < _validationErrors.Count; i++)
        //    {
        //        if (_validationErrors[i].ErrorMessage == Code)
        //        {
        //            indexExists = i;
        //        }
        //    }
        //    if (indexExists >= 0)
        //    {
        //        var message = _validationErrors[indexExists].ErrorMessage;
        //        var members = (List<string>)_validationErrors[indexExists].MemberNames;
        //        _validationErrors[indexExists] = new ValidationResult(message, members);
        //    }
        //    else
        //    {
        //        _validationErrors.Add(new ValidationResult(Code, new List<string> { value.ToString() }));
        //    }

        //    return this;
        //}
        public CustomBusinessException WithData(string name, object value, params object[] members)
        {
            return WithData(name, ParamTypes.MemberAndJustData, value, members);
        }
        public CustomBusinessException WithData(string name, ParamTypes ParamTypes, object value, params object[] members)
        {
            if (members != null && members.Length > 0) return WithDataMessage(name, ParamTypes, value.ToString(), members);

            //
            if (ParamTypes == ParamTypes.MemberAndJustData || ParamTypes == ParamTypes.JustData)
            {
                Data[name] = value;
            }
            //
            if (ParamTypes == ParamTypes.MemberAndJustData || ParamTypes == ParamTypes.Member)
            {
                if (_validationErrors.Count == 0)
                {
                    _validationErrors.Add(new CustomValidationResult(Code, Code, new List<string> { value.ToString() }));
                }
                else
                {
                    var names = (List<string>)_validationErrors[0].MemberNames;
                    names.Add(value.ToString());
                    _validationErrors[0] = new CustomValidationResult(Code, Code, names);
                }
                
            }
            return this;
        }

        public CustomBusinessException WithDataMessage(string name, ParamTypes ParamTypes = ParamTypes.MemberAndJustData, string message = null, params object[] members)
        {
            message = string.Format(message, members);
            //
            if (ParamTypes == ParamTypes.MemberAndJustData || ParamTypes == ParamTypes.JustData)
            {
                Data[name] = message;
            }
            //
            if (ParamTypes == ParamTypes.MemberAndJustData || ParamTypes == ParamTypes.Member)
            {
                var list = new List<string>();
                foreach (var member in members)
                {
                    list.Add(member.ToString());
                }
                _validationErrors.Add(new CustomValidationResult(Code, Code, list));
            }
            return this;
        }

    }
    public class CustomValidationResult : ValidationResult
    {
        public CustomValidationResult(string code, string errorMessage, IEnumerable<string> memberNames) : base(errorMessage, memberNames)
        {
            Code = code;
        }

        public string Code { get; }
    }

    public enum ParamTypes
    {
        MemberAndJustData = 1,
        Member,
        JustData
    }
}
