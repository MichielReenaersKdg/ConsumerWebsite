﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS.BL.Domain.Analyses;

namespace SS.BL.Domain.Users
{
    public class User
    {
        [Key]
        public long Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        [Index(IsUnique = true)]
        [MaxLength(100)]
        public string Email { get; set; }
        public DateTime DateRegistered { get; set; }
        public string AvatarUrl { get; set; }
        public Organisation Organisation { get; set; }
        public ICollection<ClassifiedInstance> ClassifiedInstances { get; set; }
    }
}
