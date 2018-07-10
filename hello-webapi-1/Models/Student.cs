using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Serialization;

namespace hello_webapi.Models
{
    [Serializable]
    public class Student
    {
        public int Id{get;set;}
        
        [Required]
        public string Name{get;set;}

        public int Age{get;set;}

        public int Sex{get;set;}
    }
}