﻿using ClientApp.Models;

namespace ClientApp.DTO
{
    public class RegisterDTO
    {
        public Account account { get; set; } = null;

        public Customer customer { get; set; } = null;

        public Employee employee { get; set; } = null;
    }
}
