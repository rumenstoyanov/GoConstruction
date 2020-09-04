using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GoApi.Data.Attributes
{
    public class NoSpacesAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return Regex.IsMatch(value.ToString(), @"^[a-zA-Z0-9]*$", RegexOptions.IgnoreCase);
        }
    }
}
