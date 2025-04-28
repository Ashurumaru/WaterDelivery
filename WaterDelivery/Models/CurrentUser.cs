using System;

namespace SchoolApp.Models
{
    public static class CurrentUser
    {
        public static int UserId { get; set; }
        public static string UserName { get; set; }
        public static string Email { get; set; }
        public static string Phone { get; set; }
        public static int UserRoleId { get; set; }
        public static DateTime? LastLoginDate { get; set; }

        public static int? EmployeeId { get; set; }
        public static string FirstName { get; set; }
        public static string MiddleName { get; set; }
        public static string LastName { get; set; }
        public static int? PositionId { get; set; }

        public static string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
                    return UserName;

                return $"{LastName} {FirstName} {MiddleName}".Trim();
            }
        }

        public static bool IsAdmin
        {
            get
            {
                return UserRoleId == 1;
            }
        }
    }
}