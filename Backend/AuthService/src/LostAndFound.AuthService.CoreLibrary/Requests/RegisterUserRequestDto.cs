﻿namespace LostAndFound.AuthService.CoreLibrary.Requests
{
    /// <summary>
    /// User data for account registration
    /// </summary>
    public class RegisterUserRequestDto
    {
        /// <summary>
        /// Valid and unique email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Unique username with minimal length 6
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password with minimal length 6
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Confirmation of password
        /// </summary>
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
